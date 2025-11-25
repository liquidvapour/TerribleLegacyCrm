# TerribleLegacyCrm

TerribleLegacyCrm is a tiny WinForms CRM toy app that talks to a local SQLite database. It is intentionally rough around the edges and meant for experimenting with messy legacy-style WinForms code.

## What it does

- Launches a Windows Forms UI (`MainCrazyForm`) for managing customers and their notes.
- Stores data in `data/terriblecrm.db` via the bundled `System.Data.SQLite` package.
- Lets you add, edit, soft-delete, and search customers; add notes; and attach a throwaway “deal” record.
- Creates tables on startup if they do not exist (`customers`, `notes`, `deals`).

## Prerequisites

- Windows (WinForms target).
- .NET 10.0 SDK or newer (project targets `net10.0-windows`).

## Getting started

```powershell
# Restore and build
dotnet build .\src\TerribleLegacyCrm\TerribleLegacyCrm.csproj

# Run from the repo root so the app finds data/terriblecrm.db
dotnet run --project .\src\TerribleLegacyCrm\TerribleLegacyCrm.csproj
```

The app will create `data/terriblecrm.db` if it is missing. To start over, delete that file and rerun the app.

After building you can also launch the exe directly from `src/TerribleLegacyCrm/bin/Debug/net10.0-windows/TerribleLegacyCrm.exe`.

## Data model (quick reference)

- `customers`: `Id`, `Name`, `Email`, `Phone`, `Status`, `Deleted` (soft delete flag).
- `notes`: `Id`, `CustomerId`, `NoteText`, `CreatedBy`, `CreatedOn`.
- `deals`: `Id`, `CustomerId`, `Title`, `Amount`, `Stage` (UI does not display deals yet).

## Known quirks

- The UI uses lots of global state and minimal validation; SQL is string-concatenated.
- Soft-deleted customers disappear from the grid but remain in the database.
- Searching is case-sensitive for names and may not update the cached grid consistently.
- Deals are written to the database but not visible in the UI.
