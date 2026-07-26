# Custom Endpoint Devices Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Let users manually create, persist, edit, delete, and connect to custom
Chromecast receiver endpoints (host:port) across all clients, so casting works
even when zeroconf discovery fails (e.g. server in a container on macOS).

**Architecture:** Custom endpoints are persisted in the server's SQLite DB
(FreeSql + FluentMigrator) and materialized as normal `Receiver` entries in the
in-memory `ServerCastService.AvailableDevices` list (flagged `IsCustom`). All
existing machinery is reused: the `CastDevicesChanged` SignalR event pushes them,
and `ConnectToCastDevice(id)` connects by host:port. CRUD lives in a
transport-agnostic core on `IServerCastService`, exposed over both SignalR
(`CastItHub`) and REST (`PlayerController` + Refit), consumed by WPF, WebUI,
Flutter, watchOS, and the CLI.

**Tech Stack:** .NET 8, FreeSql, FluentMigrator, SignalR, Refit, React + MUI +
RxJS, WPF + MvvmCross, Flutter + signalr_core + bloc, SwiftUI.

**Design doc:** `docs/plans/2026-07-05-custom-endpoint-devices-design.md`

> **Conventions for this project (override defaults):**
> - No unit tests in this plan — the project has little test coverage and the
>   user opted to skip them. Verify by running the apps at the checkpoints.
> - Do NOT build/run projects unless the step says to or the user requests it.
>   Build/verify steps are marked **[CHECKPOINT — user runs]**.
> - Split C# lines over 120 chars across multiple lines; use `[]` collection
>   initializers and latest C# features.

---

## Phase 1 — Backend persistence (SQLite)

### Task 1: `CustomEndpoint` entity

**Files:**
- Create: `CastIt.Domain/Entities/CustomEndpoint.cs`

**Step 1: Create the entity**

Follow the `TinyUrl` entity pattern (`CastIt.Domain/Entities/TinyUrl.cs`).

```csharp
using CastIt.Domain.Interfaces;
using System;

namespace CastIt.Domain.Entities
{
    public class CustomEndpoint : IBaseEntity
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
```

**Step 2: Commit**

```bash
git add CastIt.Domain/Entities/CustomEndpoint.cs
git commit -m "feat: add CustomEndpoint entity"
```

---

### Task 2: FluentMigrator migration

**Files:**
- Create: `CastIt.Server/Migrations/20260705_InitCustomEndpoint.cs`

**Step 1: Create the migration**

Follow `CastIt.Server/Migrations/20211205_InitTinyUrl.cs`. Use the next migration
number after the current highest (`20251213_AddLastPlayedDate` — confirm its
`[Migration(N)]` value and use `N+1`; assume `[Migration(5)]`).

```csharp
using CastIt.Domain.Entities;
using FluentMigrator;

namespace CastIt.Server.Migrations
{
    [Migration(5)]
    public class _20260705_InitCustomEndpoint : Migration
    {
        public override void Up()
        {
            Create.Table(nameof(CustomEndpoint))
                .WithColumn(nameof(CustomEndpoint.Id)).AsInt64().PrimaryKey().Identity()
                .WithColumn(nameof(CustomEndpoint.Name)).AsString(200).NotNullable()
                .WithColumn(nameof(CustomEndpoint.Host)).AsString(200).NotNullable()
                .WithColumn(nameof(CustomEndpoint.Port)).AsInt32().NotNullable()
                .WithColumn(nameof(CustomEndpoint.CreatedAt)).AsDateTime().NotNullable()
                .WithColumn(nameof(CustomEndpoint.UpdatedAt)).AsDateTime().Nullable();
        }

        public override void Down()
        {
            Delete.Table(nameof(CustomEndpoint));
        }
    }
}
```

**Step 2: Verify the migration number is unique** — grep existing migrations:
`grep -rn "\[Migration(" CastIt.Server/Migrations/` and confirm `5` is unused.

**Step 3: Commit**

```bash
git add CastIt.Server/Migrations/20260705_InitCustomEndpoint.cs
git commit -m "feat: add CustomEndpoint migration"
```

---

### Task 3: `AppDataService` CRUD

**Files:**
- Modify: `CastIt.Server/Services/AppDataService.cs`
- Modify: `CastIt.Server/Interfaces/IAppDataService.cs`

**Step 1: Add interface methods** to `IAppDataService`:

```csharp
Task<List<CustomEndpoint>> GetAllCustomEndpoints();
Task<CustomEndpoint> AddCustomEndpoint(string name, string host, int port);
Task UpdateCustomEndpoint(long id, string name, string host, int port);
Task DeleteCustomEndpoint(long id);
```

**Step 2: Register the entity** in the FreeSql config block in the
`AppDataService` constructor (next to the `PlayList`/`FileItem`/`TinyUrl`
`ConfigEntity` calls):

```csharp
_db.CodeFirst.ConfigEntity<CustomEndpoint>(f => f.Property(x => x.Id).IsPrimary(true).IsIdentity(true));
```

**Step 3: Implement the CRUD methods** (mirror the `TinyUrl`/`PlayList`
patterns already in the file):

```csharp
public Task<List<CustomEndpoint>> GetAllCustomEndpoints()
{
    return _db.Select<CustomEndpoint>().ToListAsync();
}

public async Task<CustomEndpoint> AddCustomEndpoint(string name, string host, int port)
{
    var endpoint = new CustomEndpoint
    {
        Name = name,
        Host = host,
        Port = port,
        CreatedAt = DateTime.Now,
    };
    endpoint.Id = await _db.Insert(endpoint).ExecuteIdentityAsync();
    return endpoint;
}

public Task UpdateCustomEndpoint(long id, string name, string host, int port)
{
    return _db.Update<CustomEndpoint>(id)
        .Set(e => e.Name, name)
        .Set(e => e.Host, host)
        .Set(e => e.Port, port)
        .Set(e => e.UpdatedAt, DateTime.Now)
        .ExecuteAffrowsAsync();
}

public Task DeleteCustomEndpoint(long id)
{
    return _db.Delete<CustomEndpoint>().Where(e => e.Id == id).ExecuteAffrowsAsync();
}
```

**Step 4: [CHECKPOINT — user runs]** build backend: `dotnet build CastIt.Server`.

**Step 5: Commit**

```bash
git add CastIt.Server/Services/AppDataService.cs CastIt.Server/Interfaces/IAppDataService.cs
git commit -m "feat: add custom endpoint CRUD to AppDataService"
```

---

## Phase 2 — Shared model + service core

### Task 4: Add `IsCustom` to the shared `Receiver`

**Files:**
- Modify: `CastIt.GoogleCast.Shared/Device/IReceiver.cs`
- Modify: `CastIt.GoogleCast.Shared/Device/Receiver.cs`

**Step 1:** Add `bool IsCustom { get; set; }` to `IReceiver`.

**Step 2:** Add `public bool IsCustom { get; set; }` to `Receiver` (defaults
`false`; discovered devices remain `false` in `DeviceLocator.CreateReceiver`).

**Step 3: Commit**

```bash
git add CastIt.GoogleCast.Shared/Device/IReceiver.cs CastIt.GoogleCast.Shared/Device/Receiver.cs
git commit -m "feat: add IsCustom flag to Receiver"
```

---

### Task 5: Service-core CRUD + merge/dedup in `ServerCastService`

**Files:**
- Modify: `CastIt.Server/Services/ServerCastService.cs`
- Modify: `CastIt.Server/Interfaces/IServerCastService.cs` (or the interface that
  declares `AvailableDevices` / `SetCastRenderer` — confirm exact path)

**Step 1: Add interface methods** to `IServerCastService`:

```csharp
Task<List<IReceiver>> GetCustomEndpoints();
Task AddCustomEndpoint(string name, string host, int port);
Task UpdateCustomEndpoint(long id, string name, string host, int port);
Task DeleteCustomEndpoint(long id);
```

**Step 2: Inject `IAppDataService`** into `ServerCastService` if not already
present (check the constructor).

**Step 3: Add a materializer + merge helper.** A custom endpoint becomes a
`Receiver` with a stable id `endpoint:{dbId}`:

```csharp
private static IReceiver ToReceiver(CustomEndpoint e) => new Receiver
{
    Id = $"endpoint:{e.Id}",
    FriendlyName = e.Name,
    Type = "Custom",
    Host = e.Host,
    Port = e.Port,
    IsCustom = true,
};
```

Merge rule when (re)building `AvailableDevices`: keep all discovered devices,
then add each custom endpoint UNLESS a discovered device already has the same
`Host` + `Port` (dedup — keep discovered's connection state but... see note).
Simplest correct behavior: if a custom endpoint's `host:port` matches a
discovered device, keep the **custom** entry (so edit/delete stays) and copy the
discovered `IsConnected` onto it. Implement a single private method
`RebuildAvailableDevicesWithCustomEndpoints(List<CustomEndpoint> saved)` that the
CRUD methods and startup both call, then fire the existing devices-changed event
(`_server.OnCastDevicesChanged?.Invoke(AvailableDevices)` — confirm the exact
event/trigger used in `ServerCastService.Events.cs`).

**Step 4: Load saved endpoints on startup.** In the service's init path (where
discovery/`AvailableDevices` is first populated — search for where
`AvailableDevices` is filled and `DeviceAdded` is subscribed, ~`ServerCastService.cs:123`),
call `GetAllCustomEndpoints` and merge.

**Step 5: Implement the CRUD methods** — each calls the matching
`AppDataService` method, then rebuilds `AvailableDevices` from
`GetAllCustomEndpoints()` and fires devices-changed:

```csharp
public async Task AddCustomEndpoint(string name, string host, int port)
{
    await _appDataService.AddCustomEndpoint(name, host, port);
    await ReloadCustomEndpoints();
}
// Update/Delete follow the same shape; ReloadCustomEndpoints() re-reads +
// rebuilds AvailableDevices + fires OnCastDevicesChanged.
```

**Step 6:** Confirm `GetCustomEndpoints()` returns the materialized `IReceiver`
list (used by the REST GET).

**Step 7: [CHECKPOINT — user runs]** `dotnet build CastIt.Server`.

**Step 8: Commit**

```bash
git add CastIt.Server/Services/ServerCastService.cs CastIt.Server/Interfaces/IServerCastService.cs
git commit -m "feat: custom endpoint CRUD + merge in ServerCastService"
```

---

## Phase 3 — Transports (SignalR, REST, CLI)

### Task 6: SignalR hub methods

**Files:**
- Modify: `CastIt.Server/Hubs/CastItHub.cs`

**Step 1:** Add three methods delegating to the service (mirror
`ConnectToCastDevice`/`RefreshCastDevices` at `CastItHub.cs:359-371`):

```csharp
public Task AddCustomEndpoint(string name, string host, int port)
    => _castService.AddCustomEndpoint(name, host, port);

public Task UpdateCustomEndpoint(long id, string name, string host, int port)
    => _castService.UpdateCustomEndpoint(id, name, host, port);

public Task DeleteCustomEndpoint(long id)
    => _castService.DeleteCustomEndpoint(id);
```

(The service already fires `CastDevicesChanged`, so no extra push here.)

**Step 2: Commit**

```bash
git add CastIt.Server/Hubs/CastItHub.cs
git commit -m "feat: hub methods for custom endpoints"
```

---

### Task 7: REST endpoints + Refit client

**Files:**
- Modify: `CastIt.Server/Controllers/PlayerController.cs`
- Modify: `CastIt.Cli/Interfaces/Api/ICastItApiService.cs`
- Create (if a request DTO is needed): `CastIt.Server.Shared/.../CustomEndpointRequestDto.cs`
  (place beside the existing `ConnectRequestDto` — find it with
  `grep -rl "class ConnectRequestDto" .`)

**Step 1: Request DTO** `{ string Name; string Host; int Port; }`.

**Step 2: Controller endpoints** (match the `Devices`/`Connect` style at
`PlayerController.cs:50-104`, returning the same response wrapper the other
actions use):

```csharp
[HttpGet("Devices/Custom")]
public async Task<IActionResult> GetCustomEndpoints() { ... CastService.GetCustomEndpoints() ... }

[HttpPost("Devices/Custom")]
public async Task<IActionResult> AddCustomEndpoint(CustomEndpointRequestDto dto) { ... }

[HttpPut("Devices/Custom/{id}")]
public async Task<IActionResult> UpdateCustomEndpoint(long id, CustomEndpointRequestDto dto) { ... }

[HttpDelete("Devices/Custom/{id}")]
public async Task<IActionResult> DeleteCustomEndpoint(long id) { ... }
```

**Step 3: Refit methods** in `ICastItApiService` (match existing signatures like
`GetAllDevices` / `Connect`):

```csharp
[Get("/Player/Devices/Custom")]
Task<AppListResponseDto<Receiver>> GetCustomEndpoints();
[Post("/Player/Devices/Custom")]
Task<EmptyResponseDto> AddCustomEndpoint([Body] CustomEndpointRequestDto dto);
[Put("/Player/Devices/Custom/{id}")]
Task<EmptyResponseDto> UpdateCustomEndpoint(long id, [Body] CustomEndpointRequestDto dto);
[Delete("/Player/Devices/Custom/{id}")]
Task<EmptyResponseDto> DeleteCustomEndpoint(long id);
```

(Confirm whether `ICastItApiService` uses explicit Refit attributes or a
convention/base — match whatever the file already does.)

**Step 4: [CHECKPOINT — user runs]** build backend: `dotnet build CastIt.Server`.

**Step 5: Commit**

```bash
git add CastIt.Server/Controllers/PlayerController.cs CastIt.Cli/Interfaces/Api/ICastItApiService.cs
git commit -m "feat: REST endpoints + Refit client for custom endpoints"
```

---

### Task 8: CLI command

**Files:**
- Create: `CastIt.Cli/Commands/Player/EndpointsCommand.cs`
- Modify: wherever commands are registered (follow how `DevicesCommand` is wired).

**Step 1:** Add an `endpoints` command (pattern: `DevicesCommand.cs`) supporting
list (default), `--add "Name" host port`, `--delete id`, `--update id ...` —
calling the new Refit methods and printing a `ConsoleTable` like `devices` does.

**Step 2: [CHECKPOINT — user runs]** `dotnet build CastIt.Cli`.

**Step 3: Commit**

```bash
git add CastIt.Cli/Commands/Player/EndpointsCommand.cs
git commit -m "feat: CLI endpoints command"
```

---

## Phase 4 — WPF (`CastIt`)

### Task 9: Hub client methods

**Files:**
- Modify: `CastIt/Services/CastItHubClientService.cs`
- Modify: its interface (find with `grep -rl "ConnectToCastDevice" CastIt/Services CastIt/Interfaces`).

**Step 1:** Add `AddCustomEndpoint`/`UpdateCustomEndpoint`/`DeleteCustomEndpoint`
that `InvokeAsync(nameof(...), ...)` (mirror `ConnectToCastDevice` which does
`_connection.InvokeAsync(nameof(ConnectToCastDevice), id)`).

**Step 2: Commit.**

### Task 10: Add/Edit dialog + list affordances

**Files:**
- Create: `CastIt/ViewModels/AddEditEndpointViewModel.cs`
- Create: `CastIt/Views/UserControls/AddEditEndpoint.xaml` (+ `.xaml.cs`)
- Modify: `CastIt/ViewModels/DevicesViewModel.cs`
- Modify: `CastIt/ViewModels/Items/DeviceItemViewModel.cs` (expose `IsCustom`)
- Modify: `CastIt/Views/UserControls/Devices.xaml` (add "+ Add device" button)
- Modify: `CastIt/Views/UserControls/DeviceItem.xaml` (edit/delete buttons bound
  to `Tag.EditCommand`/`Tag.DeleteCommand`, visible when `IsCustom`)

**Step 1:** `DeviceItemViewModel` — map `IsCustom` from the `Receiver`.
**Step 2:** `DevicesViewModel` — add `AddEndpointCommand`, `EditEndpointCommand`,
`DeleteEndpointCommand`; Add/Edit navigates to `AddEditEndpointViewModel`
(MvvmCross modal); on save call the hub client methods.
**Step 3:** Form VM validates Name/Host non-empty, Port default `8009`.
**Step 4:** XAML — add button next to "Refresh devices"; edit/delete icons on the
row gated on `IsCustom` (use a bool→visibility converter already in the project).
**Step 5: [CHECKPOINT — user runs]** build/run WPF, verify add→appears→connect→
edit→delete.
**Step 6: Commit.**

---

## Phase 5 — WebUI (`CastIt.Server/ClientApp`)

### Task 11: Service + model

**Files:**
- Modify: `CastIt.Server/ClientApp/src/models/receiver.model.ts` (add `isCustom: boolean`)
- Modify: `CastIt.Server/ClientApp/src/services/castithub.service.ts`

**Step 1:** Add `isCustom` to `IReceiver`.
**Step 2:** Add `addCustomEndpoint`, `updateCustomEndpoint`, `deleteCustomEndpoint`
using `this.send('AddCustomEndpoint', ...)` etc (mirror `connectToCastDevice`).
**Step 3: Commit.**

### Task 12: UI — add button, form dialog, edit/delete

**Files:**
- Modify: `CastIt.Server/ClientApp/src/components/player/player_devices.tsx`
- Modify: `CastIt.Server/ClientApp/src/components/device/device_item.tsx`
- Create: `CastIt.Server/ClientApp/src/components/device/add_edit_endpoint_dialog.tsx`

**Step 1:** `player_devices.tsx` — "+ Add" `IconButton`/button in the dialog
header next to Refresh; opens the form dialog; on submit calls the service.
**Step 2:** `add_edit_endpoint_dialog.tsx` — MUI `Dialog` with Name/Host/Port
fields (Port default 8009), used for both add and edit.
**Step 3:** `device_item.tsx` — when `isCustom`, render edit + delete `IconButton`s
next to the cast toggle; wire to props callbacks.
**Step 4: [CHECKPOINT — user runs]** `npm run dev` (or the project's script) in
`ClientApp`; verify add/connect/edit/delete.
**Step 5: Commit.**

---

## Phase 6 — Flutter (`CastIt.Client`)

### Task 13: Receiver model + hub streams

**Files:**
- Create: `CastIt.Client/lib/domain/models/receiver.dart` (or match the models
  folder convention — check `lib/domain/models`)
- Modify: `CastIt.Client/lib/domain/services/castit_hub_client_service.dart` (interface)
- Modify: `CastIt.Client/lib/infrastructure/castit_hub_client_service.dart`

**Step 1:** `Receiver` model — `id, friendlyName, type, host, port, isConnected,
isCustom` with `fromJson` (match the server's camelCase JSON).
**Step 2:** In the infrastructure hub client, stop discarding the three
`CastDevice*` events (currently `break` at ~lines 331-336). Expose streams
(`Stream<List<Receiver>> get devices`, `Stream<Receiver> get deviceSet`,
`Stream<void> get deviceDisconnected`) via `StreamController`s, following how
other events are surfaced in this file.
**Step 3:** Add invokes: `connectToCastDevice(String? id)`,
`addCustomEndpoint(name, host, port)`, `updateCustomEndpoint(id, name, host, port)`,
`deleteCustomEndpoint(id)` (mirror existing `invoke` calls like `Play`).
**Step 4: Commit.**

### Task 14: Devices bloc

**Files:**
- Create: `CastIt.Client/lib/application/devices/devices_bloc.dart` (+ state/event
  files following the existing bloc structure, e.g. `server_ws`)
- Register in DI (follow how `ServerWsBloc` is registered — `injection.dart`/`di`).

**Step 1:** Bloc holds `List<Receiver> devices` + selected; listens to the hub
client streams; exposes events: load, connect(id), addEndpoint, editEndpoint,
deleteEndpoint (call the hub client).
**Step 2: Commit.**

### Task 15: Device picker bottom sheet + form

**Files:**
- Create: `CastIt.Client/lib/presentation/shared/devices_bottom_sheet_dialog.dart`
  (follow `change_connection_bottom_sheet_dialog.dart`)
- Create: `CastIt.Client/lib/presentation/shared/add_edit_endpoint_dialog.dart`
- Modify: wherever the "connect/devices" entry point lives in the UI to open the
  new sheet (find the widget that currently opens
  `change_connection_bottom_sheet_dialog`).

**Step 1:** Bottom sheet — `BlocBuilder<DevicesBloc>`, merged list, "+ Add device"
button, Connect per row, edit/delete on rows where `isCustom`.
**Step 2:** Add/Edit form — Name/Host/Port (Port default 8009), dispatches bloc
events.
**Step 3: [CHECKPOINT — user runs]** `flutter run`; verify full flow.
**Step 4: Commit.**

---

## Phase 7 — watchOS (`CastIt.Client/ios/CastIt Watch App`)

### Task 16: Model flag + service invokes

**Files:**
- Modify: `.../Models/Receiver.swift` (add `isCustom: Bool`)
- Modify: `.../Services/SignalRService.swift` (add `addCustomEndpoint` /
  `update` / `delete` invokes — mirror `ConnectToCastDevice` at lines 159-164)
- Optionally: `.../Services/ApiService.swift` (REST equivalents, only if the watch
  prefers REST for these — SignalR is sufficient).

**Step 1:** `Receiver` gains `isCustom` (default false in decoding).
**Step 2:** Add the three invoke methods.
**Step 3: Commit.**

### Task 17: Device picker View + form

**Files:**
- Create: `.../Views/DevicesView.swift`
- Create: `.../Views/AddEditEndpointView.swift`
- Modify: the navigation entry point (where the user would open devices — likely
  from `SettingsView.swift` or the main player view) to present `DevicesView`.

**Step 1:** `DevicesView` reads `PlayerViewModel.devices`/`selectedDevice`, lists
them, Connect via `connectToCastDevice`, "+ Add" button, edit/delete on
`isCustom` rows.
**Step 2:** `AddEditEndpointView` — Name/Host/Port form (Port default 8009);
calls the new service invokes.
**Step 3: [CHECKPOINT — user runs]** build/run the watch app; verify flow.
**Step 4: Commit.**

---

## Final verification (user-run)

1. Server: add an endpoint via CLI (`endpoints --add`), confirm it lands in
   `CastIt.db` and appears in WPF/WebUI without refresh (pushed via
   `CastDevicesChanged`).
2. Connect to it from each client; confirm `_player.ConnectAsync` succeeds by
   host:port.
3. Edit and delete from one client; confirm the change propagates to the others.
4. Restart the server; confirm saved endpoints reappear (loaded from DB on
   startup).
