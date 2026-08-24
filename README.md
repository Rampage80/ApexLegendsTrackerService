# ApexLegendsTracker WebAPI

RESTful backend for ApexLegendsTracker.

## Prerequisites

- .NET 10 SDK

## Configuration

Set your Apex API key before running:

- `ApexApi__ApiKey`

Optional settings:

- `ApexApi__BaseUrl` (default: `https://api.apexlegendsstatus.com/`)

## Run

```powershell
dotnet run --project ./ApexLegendsTracker.WebAPI
```

## Endpoints

- `GET /api/v1/health`
- `GET /api/v1/players/{platform}/{playerName}`

Supported `platform` values:

- `PC`
- `PS4`
- `X1`

### GET /api/v1/players/{platform}/{playerName}

Looks up an Apex Legends player's stats by platform and player name.

**Path parameters**

| Name | Type | Description |
| --- | --- | --- |
| `platform` | string | One of `PC`, `PS4`, `X1` (case-insensitive). |
| `playerName` | string | Exact in-game player name to search for. |

**Responses**

| Status | Meaning |
| --- | --- |
| `200 OK` | Player found; returns a `PlayerLookupResult`. |
| `400 Bad Request` | `playerName` is empty/whitespace, or `platform` is not one of the supported values. |
| `404 Not Found` | No player with the given name was found on the given platform. |
| `502 Bad Gateway` | The upstream Apex Legends status API returned an error. |

Example:

```powershell
curl "https://localhost:5001/api/v1/players/PC/somePlayerName"
```

### Interactive API docs (Swagger-style)

When running in the `Development` environment, browse the interactive OpenAPI documentation at:

- `/scalar/v1` — interactive UI to explore and try endpoints
- `/openapi/v1.json` — raw OpenAPI document

## Test

```powershell
dotnet test
```
