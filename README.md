# Equipment Inventory Manager

A Windows desktop application for tracking office IT equipment, stock movements, and STF requests. It includes optional Supabase synchronization and a responsive, read-only web viewer.

The user interface is currently in Turkish.

## Features

- Create, edit, search, and filter equipment records
- Track stock receipts and stock-out operations
- Record material movements and STF requests
- Import and export Excel workbooks without third-party spreadsheet packages
- Store data locally as JSON
- Synchronize inventory with Supabase
- View synchronized inventory from a responsive static website

## Technology

- C# and .NET 9
- Windows Forms
- Supabase REST API and PostgreSQL
- HTML, CSS, and vanilla JavaScript

## Project structure

```text
Equipments.App/   Windows Forms desktop application
supabase/         Database schema and row-level security baseline
viewer/           Static read-only inventory viewer
```

## Requirements

- Windows 10 or later
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Visual Studio 2022 is optional
- A Supabase project is optional; the desktop application also works with local JSON data

## Build and run

```powershell
dotnet restore .\Equipment-Inventory-Manager.sln
dotnet build .\Equipment-Inventory-Manager.sln --configuration Release
dotnet run --project .\Equipments.App\Equipments.App.csproj
```

Runtime data is created under `Equipments.App/bin/<configuration>/net9.0-windows/data` during development. It is intentionally excluded from Git.

## Supabase setup

1. Create a Supabase project.
2. Run [`supabase/equipments.sql`](supabase/equipments.sql) in the Supabase SQL editor.
3. Open **Bulut Ayarları** in the desktop application.
4. Enter the project URL, publishable key, and table name (`equipments`).

The committed schema allows anonymous read access for the viewer but does not grant anonymous writes. For production synchronization, authenticate users and add a write policy for the appropriate role, or place writes behind a trusted API or Edge Function.

## Web viewer setup

1. Copy `viewer/config.example.js` to `viewer/config.js`.
2. Replace the placeholder project URL and publishable key.
3. Serve the `viewer` directory with any static web server.

For example:

```powershell
cd viewer
python -m http.server 8080
```

Then open `http://localhost:8080`.

`viewer/config.js` is ignored so a live project configuration is not accidentally committed. A Supabase publishable key is designed for client-side use, but access must still be protected with Row Level Security.

## Data and privacy

This repository contains source code only. Real inventory records, serial numbers, employee assignments, spreadsheets, Supabase configuration, release archives, and compiled binaries are excluded.

