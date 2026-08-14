# ApexLegendsTracker API

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
dotnet run --project ./ApexLegendsTracker.Api
```

## Endpoints

- `GET /api/v1/health`
- `GET /api/v1/players/{platform}/{playerName}`

Supported `platform` values:

- `PC`
- `PS4`
- `X1`

## Test

```powershell
dotnet test
```
