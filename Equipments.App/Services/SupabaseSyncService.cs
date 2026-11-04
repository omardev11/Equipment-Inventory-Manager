using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Equipments.App.Models;

namespace Equipments.App.Services;

public sealed class SupabaseSyncService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    private readonly HttpClient _httpClient;

    public SupabaseSyncService(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
    }

    public async Task<SupabaseSyncResult> SyncAsync(
        SupabaseSettings settings,
        IEnumerable<EquipmentItem> items,
        CancellationToken cancellationToken = default)
    {
        if (!settings.IsConfigured)
        {
            throw new InvalidOperationException("Supabase ayarları eksik.");
        }

        var normalizedItems = EquipmentRepository.Normalize(items)
            .OrderBy(x => x.SequenceNo)
            .ToList();

        var syncTimestamp = DateTimeOffset.UtcNow;
        var records = normalizedItems.Select(item => SupabaseEquipmentRecord.FromItem(item, syncTimestamp)).ToList();
        var remoteIds = await GetRemoteIdsAsync(settings, cancellationToken);
        var localIds = normalizedItems.Select(x => x.Id).ToHashSet();
        var idsToDelete = remoteIds.Where(id => !localIds.Contains(id)).ToList();

        await UpsertAsync(settings, records, cancellationToken);
        await DeleteMissingAsync(settings, idsToDelete, cancellationToken);

        return new SupabaseSyncResult
        {
            SyncedAtUtc = syncTimestamp,
            UpsertedCount = records.Count,
            DeletedCount = idsToDelete.Count
        };
    }

    private async Task<HashSet<Guid>> GetRemoteIdsAsync(SupabaseSettings settings, CancellationToken cancellationToken)
    {
        using var request = CreateRequest(HttpMethod.Get, $"{settings.GetRestTableUrl()}?select=id", settings);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response);

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var rows = await JsonSerializer.DeserializeAsync<List<RemoteIdRow>>(stream, JsonOptions, cancellationToken) ?? [];
        return rows.Select(x => x.Id).ToHashSet();
    }

    private async Task UpsertAsync(
        SupabaseSettings settings,
        IReadOnlyCollection<SupabaseEquipmentRecord> records,
        CancellationToken cancellationToken)
    {
        var requestUri = $"{settings.GetRestTableUrl()}?on_conflict=id";
        using var request = CreateRequest(HttpMethod.Post, requestUri, settings);
        request.Headers.TryAddWithoutValidation("Prefer", "resolution=merge-duplicates,return=minimal");
        var json = JsonSerializer.Serialize(records, JsonOptions);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response);
    }

    private async Task DeleteMissingAsync(
        SupabaseSettings settings,
        IReadOnlyList<Guid> idsToDelete,
        CancellationToken cancellationToken)
    {
        if (idsToDelete.Count == 0)
        {
            return;
        }

        const int batchSize = 100;
        for (var index = 0; index < idsToDelete.Count; index += batchSize)
        {
            var batch = idsToDelete.Skip(index).Take(batchSize).Select(id => id.ToString()).ToList();
            var filter = Uri.EscapeDataString($"in.({string.Join(",", batch)})");
            var requestUri = $"{settings.GetRestTableUrl()}?id={filter}";

            using var request = CreateRequest(HttpMethod.Delete, requestUri, settings);
            request.Headers.TryAddWithoutValidation("Prefer", "return=minimal");

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            await EnsureSuccessAsync(response);
        }
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string requestUri, SupabaseSettings settings)
    {
        var request = new HttpRequestMessage(method, requestUri);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.TryAddWithoutValidation("apikey", settings.PublishableKey.Trim());
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.PublishableKey.Trim());
        return request;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var content = await response.Content.ReadAsStringAsync();
        throw new InvalidOperationException(
            $"Supabase isteği başarısız oldu. HTTP {(int)response.StatusCode} {response.ReasonPhrase}\n{content}");
    }

    private sealed class RemoteIdRow
    {
        public Guid Id { get; set; }
    }

    private sealed class SupabaseEquipmentRecord
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("sequence_no")]
        public int SequenceNo { get; set; }

        [JsonPropertyName("group_name")]
        public string GroupName { get; set; } = string.Empty;

        [JsonPropertyName("equipment_name")]
        public string EquipmentName { get; set; } = string.Empty;

        [JsonPropertyName("unit")]
        public string Unit { get; set; } = "Adet";

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("serial_no")]
        public string SerialNo { get; set; } = string.Empty;

        [JsonPropertyName("location")]
        public string Location { get; set; } = string.Empty;

        [JsonPropertyName("owner")]
        public string Owner { get; set; } = string.Empty;

        [JsonPropertyName("synced_at")]
        public DateTimeOffset SyncedAt { get; set; }

        public static SupabaseEquipmentRecord FromItem(EquipmentItem item, DateTimeOffset syncedAtUtc)
        {
            return new SupabaseEquipmentRecord
            {
                Id = item.Id,
                SequenceNo = item.SequenceNo,
                GroupName = item.Group,
                EquipmentName = item.Name,
                Unit = item.Unit,
                Quantity = item.Quantity,
                Model = item.Model,
                SerialNo = item.SerialNo,
                Location = item.Location,
                Owner = item.Owner,
                SyncedAt = syncedAtUtc
            };
        }
    }
}

public sealed class SupabaseSyncResult
{
    public DateTimeOffset SyncedAtUtc { get; init; }
    public int UpsertedCount { get; init; }
    public int DeletedCount { get; init; }
}
