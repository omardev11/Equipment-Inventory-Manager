(function () {
  const config = window.SUPABASE_VIEWER_CONFIG || {};
  const state = {
    items: [],
    filteredItems: []
  };

  const searchInput = document.getElementById("search-input");
  const ownerFilter = document.getElementById("owner-filter");
  const groupFilter = document.getElementById("group-filter");
  const locationFilter = document.getElementById("location-filter");
  const tableBody = document.getElementById("table-body");
  const mobileCards = document.getElementById("mobile-cards");
  const viewerState = document.getElementById("viewer-state");
  const lastSyncLabel = document.getElementById("last-sync-label");

  const totalCount = document.getElementById("total-count");
  const missingModelCount = document.getElementById("missing-model-count");
  const missingSerialCount = document.getElementById("missing-serial-count");
  const locationCount = document.getElementById("location-count");

  if (!config.supabaseBaseUrl || !config.publishableKey || !config.equipmentTable) {
    viewerState.textContent = "config.js içindeki Supabase bilgilerini tamamlaman gerekiyor.";
    return;
  }

  searchInput.addEventListener("input", applyFilters);
  ownerFilter.addEventListener("input", applyFilters);
  groupFilter.addEventListener("change", applyFilters);
  locationFilter.addEventListener("change", applyFilters);

  loadItems();

  async function loadItems() {
    viewerState.textContent = "Veriler yükleniyor...";

    try {
      const endpoint = `${config.supabaseBaseUrl.replace(/\/+$/, "")}/rest/v1/${config.equipmentTable}?select=id,sequence_no,group_name,equipment_name,unit,quantity,model,serial_no,location,owner,synced_at,updated_at&order=sequence_no.asc`;
      const response = await fetch(endpoint, {
        headers: {
          apikey: config.publishableKey,
          Authorization: `Bearer ${config.publishableKey}`
        }
      });

      if (!response.ok) {
        const body = await response.text();
        throw new Error(`Supabase HTTP ${response.status}: ${body}`);
      }

      const rows = await response.json();
      state.items = Array.isArray(rows) ? rows : [];
      hydrateFilters(state.items);
      applyFilters();
      updateLastSync(state.items);
    } catch (error) {
      console.error(error);
      viewerState.textContent = `Veriler alınamadı. ${error instanceof Error ? error.message : "Supabase ayarlarını ve tablo politikalarını kontrol et."}`;
    }
  }

  function hydrateFilters(items) {
    refillSelect(groupFilter, uniqueValues(items.map((item) => item.group_name)));
    refillSelect(locationFilter, uniqueValues(items.map((item) => item.location)));
  }

  function refillSelect(select, values) {
    const current = select.value;
    select.innerHTML = '<option value="">Tümü</option>';
    values.forEach((value) => {
      const option = document.createElement("option");
      option.value = value;
      option.textContent = value;
      select.appendChild(option);
    });

    if (values.includes(current)) {
      select.value = current;
    }
  }

  function applyFilters() {
    const search = searchInput.value.trim().toLowerCase();
    const owner = ownerFilter.value.trim().toLowerCase();
    const group = groupFilter.value;
    const location = locationFilter.value;

    state.filteredItems = state.items.filter((item) => {
      const matchesSearch =
        !search ||
        contains(item.group_name, search) ||
        contains(item.equipment_name, search) ||
        contains(item.model, search) ||
        contains(item.serial_no, search) ||
        contains(item.location, search) ||
        contains(item.owner, search);

      const matchesOwner = !owner || contains(item.owner, owner);
      const matchesGroup = !group || item.group_name === group;
      const matchesLocation = !location || item.location === location;

      return matchesSearch && matchesOwner && matchesGroup && matchesLocation;
    });

    render(state.filteredItems);
    updateStats(state.filteredItems);
  }

  function render(items) {
    if (!items.length) {
      tableBody.innerHTML = '<tr><td colspan="9" class="empty-state">Gösterilecek kayıt bulunamadı.</td></tr>';
      mobileCards.innerHTML = '<div class="empty-state">Gösterilecek kayıt bulunamadı.</div>';
      viewerState.textContent = "Filtreye uyan kayıt yok.";
      return;
    }

    viewerState.textContent = `${items.length} kayıt gösteriliyor.`;

    tableBody.innerHTML = items
      .map(
        (item) => `
          <tr>
            <td>${escapeHtml(item.sequence_no)}</td>
            <td>${escapeHtml(item.group_name)}</td>
            <td>${escapeHtml(item.equipment_name)}</td>
            <td>${escapeHtml(item.unit)}</td>
            <td>${escapeHtml(item.quantity)}</td>
            <td>${escapeHtml(item.model)}</td>
            <td>${escapeHtml(item.serial_no)}</td>
            <td>${escapeHtml(item.location)}</td>
            <td>${escapeHtml(item.owner)}</td>
          </tr>`
      )
      .join("");

    mobileCards.innerHTML = items
      .map(
        (item) => `
          <article class="mobile-card">
            <h3>${escapeHtml(item.equipment_name || "-")}</h3>
            <p><b>Grup:</b> ${escapeHtml(item.group_name || "-")}</p>
            <p><b>Model:</b> ${escapeHtml(item.model || "-")}</p>
            <p><b>Seri No:</b> ${escapeHtml(item.serial_no || "-")}</p>
            <p><b>Lokasyon:</b> ${escapeHtml(item.location || "-")}</p>
            <p><b>Sorumlu:</b> ${escapeHtml(item.owner || "-")}</p>
            <p><b>Miktar:</b> ${escapeHtml(item.quantity || "-")} ${escapeHtml(item.unit || "")}</p>
          </article>`
      )
      .join("");
  }

  function updateStats(items) {
    totalCount.textContent = items.length;
    missingModelCount.textContent = items.filter((item) => !item.model).length;
    missingSerialCount.textContent = items.filter((item) => !item.serial_no).length;
    locationCount.textContent = uniqueValues(items.map((item) => item.location)).length;
  }

  function updateLastSync(items) {
    const syncValues = items
      .map((item) => item.synced_at)
      .filter(Boolean)
      .sort()
      .reverse();

    if (!syncValues.length) {
      lastSyncLabel.textContent = "Son güncelleme bilgisi yok";
      return;
    }

    lastSyncLabel.textContent = `Son güncelleme: ${formatMogadishu(syncValues[0])}`;
  }

  function uniqueValues(values) {
    return [...new Set(values.filter(Boolean))].sort((a, b) => a.localeCompare(b, "tr"));
  }

  function contains(value, search) {
    return String(value || "").toLowerCase().includes(search);
  }

  function formatMogadishu(value) {
    return new Intl.DateTimeFormat("tr-TR", {
      timeZone: "Africa/Mogadishu",
      year: "numeric",
      month: "2-digit",
      day: "2-digit",
      hour: "2-digit",
      minute: "2-digit",
      second: "2-digit"
    }).format(new Date(value));
  }

  function escapeHtml(value) {
    return String(value ?? "")
      .replaceAll("&", "&amp;")
      .replaceAll("<", "&lt;")
      .replaceAll(">", "&gt;")
      .replaceAll('"', "&quot;")
      .replaceAll("'", "&#39;");
  }
})();
