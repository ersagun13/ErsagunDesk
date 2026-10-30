using System.IO;
using System.Collections.Generic;
using System.Text.Json;

namespace ErsagunDesk.Models;

public class CoolingAndPowerConfig
{
    public string PcCase { get; set; } = string.Empty;
    public string CpuCooler { get; set; } = string.Empty;
    public string Psu { get; set; } = string.Empty;
}

public class PeripheralsConfig
{
    public string? Keyboard { get; set; } = string.Empty;
    public string? Mouse { get; set; } = string.Empty;
    public string? Mousepad { get; set; }
    public string? Headset { get; set; } = string.Empty;
    public string? Microphone { get; set; } = string.Empty;
    public string? Monitor { get; set; }
}

public class AccessoriesConfig
{
    public string MonitorArm { get; set; } = string.Empty;
    public string MicArm { get; set; } = string.Empty;
    public string Gamepad { get; set; } = string.Empty;
    public string EthernetAdapter { get; set; } = string.Empty;
    public string EthernetCable { get; set; } = string.Empty;
}

public class WidgetConfig
{
    public double Opacity { get; set; } = 1.0;
    public double OffsetX { get; set; } = 28;
    public double OffsetY { get; set; } = 28;
    public double? CustomLeft { get; set; }
    public double? CustomTop { get; set; }
    public double Width { get; set; } = 360;
    public double Height { get; set; } = 780;
    public bool AlwaysOnTop { get; set; } = false;
    public bool PinToDesktop { get; set; } = false;
    public bool AutoGameMode { get; set; } = true;
    public bool IsLocked { get; set; } = false;
    public bool IsCompact { get; set; } = false;
    public bool OnlyShowOnDesktop { get; set; } = false;
    public string Language { get; set; } = "tr";
    public string City { get; set; } = "Ankara";
    public string AccentTheme { get; set; } = "Ersagun";
}

public class WidgetSelectionConfig
{
    public bool ShowHeader { get; set; } = true;
    public bool ShowWeather { get; set; } = true;
    public bool ShowClipboard { get; set; } = true;
    public bool ShowTelemetry { get; set; } = true;
    public bool ShowFinance { get; set; } = true;
    public bool ShowHardware { get; set; } = true;
    public bool ShowMedia { get; set; } = true;
    public bool ShowQuickActions { get; set; } = true;
}

public class HardwareVisibilityConfig
{
    public bool CoreSystem { get; set; } = true;
    public bool Storage { get; set; } = true;
    public bool Peripherals { get; set; } = true;
    public bool CoolingAndPower { get; set; } = false;
    public bool Accessories { get; set; } = false;
}

public class ManualHardwareItem
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = HardwareCategory.Accessories;
    public decimal Price { get; set; }
    public string AmazonUrl { get; set; } = string.Empty;
}

public static class HardwareCategory
{
    public const string CoreSystem = "CoreSystem";
    public const string Storage = "Storage";
    public const string Peripherals = "Peripherals";
    public const string CoolingAndPower = "CoolingAndPower";
    public const string Accessories = "Accessories";

    public static string Normalize(string? category) => category switch
    {
        "Sistem" or CoreSystem => CoreSystem,
        "Depolama" or Storage => Storage,
        "Çevre Birimleri" or Peripherals => Peripherals,
        "Soğutma ve Güç" or CoolingAndPower => CoolingAndPower,
        "Aksesuar" or Accessories => Accessories,
        _ => Accessories
    };
}

public class TelemetryWidgetConfig
{
    public bool Enabled { get; set; } = true;
    public double OffsetX { get; set; } = 415;
    public double OffsetY { get; set; } = 20;
    public double Width { get; set; } = 260;
}

public class FinanceWidgetConfig
{
    public bool Enabled { get; set; } = true;
    public double OffsetX { get; set; } = 415;
    public double OffsetY { get; set; } = 205;
    public double Width { get; set; } = 260;
}

public class HardwareConfig
{
    public CoolingAndPowerConfig CoolingAndPower { get; set; } = new();
    public PeripheralsConfig Peripherals { get; set; } = new();
    public AccessoriesConfig Accessories { get; set; } = new();
    public WidgetConfig Widget { get; set; } = new();
    public WidgetSelectionConfig Widgets { get; set; } = new();
    public TelemetryWidgetConfig TelemetryWidget { get; set; } = new();
    public FinanceWidgetConfig FinanceWidget { get; set; } = new();
    public HardwarePrices Prices { get; set; } = HardwarePrices.GetDefaults();
    public HardwareVisibilityConfig HardwareVisibility { get; set; } = new();
    public List<ManualHardwareItem> ManualHardware { get; set; } = new();
    public List<string> HiddenHardware { get; set; } = new();
    public Dictionary<string, string> HardwareLinks { get; set; } = new();

    public static HardwareConfig Load(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var config = JsonSerializer.Deserialize<HardwareConfig>(json, options);
                if (config != null)
                {
                    foreach (var item in config.ManualHardware)
                    {
                        item.Category = HardwareCategory.Normalize(item.Category);
                    }

                    // Sync legacy flags if present
                    if (!config.TelemetryWidget.Enabled) config.Widgets.ShowTelemetry = false;
                    if (!config.FinanceWidget.Enabled) config.Widgets.ShowFinance = false;
                    return config;
                }
            }
        }
        catch { }

        return new HardwareConfig();
    }

    public void Save(string path)
    {
        try
        {
            // Sync legacy
            TelemetryWidget.Enabled = Widgets.ShowTelemetry;
            FinanceWidget.Enabled = Widgets.ShowFinance;

            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            var json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(path, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving config to {path}: {ex.Message}");
        }
    }
}
