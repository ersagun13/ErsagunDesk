# ErsagunDesk

ErsagunDesk is a Windows desktop HUD developed with **C# / WPF / .NET 10**. Based on the MIT-licensed [OpenWidget](https://github.com/alztrk/OpenWidget) project, it introduces a graphite-and-turquoise identity and Ankara-first Turkish defaults.

[Türkçe](README.tr.md) · [Builds](../../actions) · [Attribution](NOTICE.md)

## Modules
- Clock, date, uptime and game-mode indicator
- Current weather and three-day forecast (Open-Meteo)
- CPU, GPU, VRAM, RAM, disks and network telemetry
- Currency, gold and crypto market data
- Multi-session Windows media controls and application volume
- Persistent clipboard history (text, images and file names)
- Windows hardware inventory and optional custom components and prices
- Quick Windows tools and persistent scratchpad

## Desktop behavior
Compact HUD mode, configurable cards, global `Ctrl+Shift+W` show/hide shortcut, automatic game-mode throttling, multi-monitor/DPI support, single-instance guard, settings, autostart and TR/EN localization.

## Run and build
Windows 10 (2004+) / Windows 11 x64. Install the .NET 10 SDK for development:
```powershell
dotnet restore ErsagunDesk.sln
dotnet test ErsagunDesk.sln
dotnet run --project ErsagunDesk.csproj
powershell -ExecutionPolicy Bypass -File .\scripts\publish-portable.ps1
```
Portable executable builds appear in [GitHub Actions](../../actions).

## Local data and privacy
Settings/cache: `%LOCALAPPDATA%\ErsagunDesk`; clipboard history and notes: `%APPDATA%\ErsagunDesk`. Clipboard history is stored as readable local data: do not copy secrets while it is enabled. Weather, markets and update checks may access external services. Example hardware and unverified starting prices have been removed.

## License
MIT. Original OpenWidget copyright notice is preserved in [LICENSE](LICENSE). See [NOTICE.md](NOTICE.md).

## Planned
- Network Operations widget with interface, IP, DNS, gateway, ping and route tools
- Expanded integration testing, signed releases
