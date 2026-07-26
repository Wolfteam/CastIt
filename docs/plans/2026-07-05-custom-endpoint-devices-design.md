# Custom Endpoint Devices — Design

**Date:** 2026-07-05
**Status:** Approved (design)

## Problem

The server discovers Chromecast devices via zeroconf/mDNS (`DeviceLocator` →
`_googlecast._tcp.local.`). This discovery does not work on macOS when the
server runs inside a container, so no devices appear and the user cannot cast.

We need a way for the user to **manually create a receiver** (a host:port
endpoint), **persist it**, and have it appear in every frontend's device picker
alongside discovered devices, with **connect / edit / delete**.

## Approach (chosen: Option A — server-side merge)

Custom endpoints are persisted in the server's existing SQLite DB and, at
startup and on every change, **materialized as normal `Receiver` entries** in
the in-memory `ServerCastService.AvailableDevices` list (flagged `IsCustom`).
Everything downstream reuses existing machinery:

- The server pushes them via the existing `CastDevicesChanged` SignalR event.
- Connecting reuses the existing `ConnectToCastDevice(id)` path — a custom
  endpoint is a real entry in `AvailableDevices`, so `_player.ConnectAsync(receiver)`
  connects by `host:port` with no new connect code.
- All clients get saved endpoints "for free" as extra items in the list they
  already render, and endpoints are **shared across every client** because they
  live in the server DB.

Rejected alternatives: (B) a separate custom-device channel/event — doubles the
client wiring; (C) client-side per-frontend storage — no sharing, 4× storage
code, contradicts "store it in the sqlite db".

## Data model

New entity `CustomEndpoint` in `CastIt.Domain/Entities`:

| Field       | Type       | Notes                     |
|-------------|------------|---------------------------|
| `Id`        | `long`     | PK, identity              |
| `Name`      | `string`   | Friendly name             |
| `Host`      | `string`   | IP / hostname             |
| `Port`      | `int`      | Default `8009` at the UI  |
| `CreatedAt` | `DateTime` |                           |
| `UpdatedAt` | `DateTime?`|                           |

- FluentMigrator `[Migration(5)]` creating the `CustomEndpoint` table (same
  pattern as `20211205_InitTinyUrl`).
- Registered in the FreeSql config in `AppDataService` with identity PK.

When materialized into `AvailableDevices`, a `CustomEndpoint` becomes:

```
Receiver {
  Id = $"endpoint:{dbId}",
  FriendlyName = Name,
  Type = "Custom",
  Host = Host,
  Port = Port,
  IsCustom = true
}
```

**Shared model change:** add `bool IsCustom` (default `false`) to
`IReceiver` / `Receiver`, and mirror it in the TS `IReceiver`, Swift `Receiver`,
and the new Flutter model. This flag is all a frontend needs to decide whether a
row shows edit/delete.

**Dedup rule:** if a discovered device shares a custom endpoint's `host:port`,
keep the custom entry so edit/delete stays available (reflecting connected state).

## Backend

### Transport-agnostic core (`IServerCastService`)

All CRUD logic lives here so both transports are thin wrappers (no duplication):

- `GetCustomEndpoints()`
- `AddCustomEndpoint(name, host, port)`
- `UpdateCustomEndpoint(id, name, host, port)`
- `DeleteCustomEndpoint(id)`

Each persists via `AppDataService`, rebuilds `AvailableDevices`, and fires the
existing `CastDevicesChanged`.

### `AppDataService` (FreeSql)

Add `GetAllCustomEndpoints`, `AddCustomEndpoint`, `UpdateCustomEndpoint`,
`DeleteCustomEndpoint`, and register the entity in the FreeSql `ConfigEntity`
block.

### SignalR (`CastItHub`)

Three new methods → delegate to the service:
`AddCustomEndpoint(name, host, port)`, `UpdateCustomEndpoint(id, name, host, port)`,
`DeleteCustomEndpoint(id)`.

### REST (`PlayerController`)

New endpoints matching the existing `Devices` / `Connect` style:

- `GET Devices/Custom`
- `POST Devices/Custom`
- `PUT Devices/Custom/{id}`
- `DELETE Devices/Custom/{id}`

Plus matching Refit methods in `ICastItApiService`.

### Connect flow

**Unchanged.** `ConnectToCastDevice("endpoint:{id}")` resolves from
`AvailableDevices` and connects by host:port through the existing path.

## Frontends

Shared UX: picker = **merged list + persistent "+ Add device" button** next to
Refresh. Custom rows (`isCustom == true`) get **edit** and **delete**; discovered
rows stay Connect-only. Add/Edit form = **Name**, **Host**, **Port** (default
`8009`).

- **WPF** (`CastIt`) — `DevicesViewModel` gains Add/Edit/Delete commands; new
  `AddEditEndpointViewModel` + dialog; `DeviceItem.xaml` shows edit/delete gated
  on `IsCustom`; wire methods through `CastItHubClientService`.
- **WebUI** (`CastIt.Server/ClientApp`) — "+ Add" button + form `Dialog` in
  `player_devices.tsx`; edit/delete in `device_item.tsx` when `isCustom`; add
  `addCustomEndpoint / updateCustomEndpoint / deleteCustomEndpoint` to
  `castithub.service.ts`.
- **Flutter** (`CastIt.Client`) — largest lift, built from scratch: new receiver
  model with `IsCustom`; stop discarding the three `CastDevice*` events in
  `castit_hub_client_service.dart` and expose them as streams; new devices bloc
  (list + selected); device-picker bottom sheet (follow
  `change_connection_bottom_sheet_dialog` pattern) with merged list, add button,
  edit/delete on custom rows; add `connectToCastDevice` + the three CRUD invokes.
- **watchOS** (`CastIt.Client/ios/CastIt Watch App`) — data layer already exists
  (`PlayerViewModel.devices`, `connectToCastDevice`, `RefreshCastDevices`); build
  the missing device-picker View, add the Add/Edit/Delete form + service invokes,
  add `IsCustom` to `Receiver.swift`.
- **CLI** (`CastIt.Cli`) — extend the `devices` command family so custom
  endpoints can be listed/added/edited/deleted from the terminal (calling the new
  Refit methods). The existing `devices` list already shows them since they are
  merged into `AvailableDevices`.

## Testing

- No unit tests: the project has little test coverage, so verification is manual
  by running each app (add → appears via `CastDevicesChanged` → connect → edit →
  delete → survives server restart).

## Notes / decisions

- Entity named `CustomEndpoint` (no "cast" framing).
- Materialized `Receiver.Id` prefix: `endpoint:{id}`.
- CRUD exposed over **both** SignalR and REST over one shared service core; CLI
  is a fifth consumer via REST.
- Adding an endpoint saves it blindly; reachability is validated on Connect
  (existing failure handling applies).
