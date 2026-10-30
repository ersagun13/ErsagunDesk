using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Windows;
using ErsagunDesk.Models;

namespace ErsagunDesk.Services;

public class HardwareScanner
{
    private readonly HardwareConfig _config;
    private readonly PriceService _priceService;

    public HardwareScanner(HardwareConfig config, PriceService priceService)
    {
        _config = config;
        _priceService = priceService;
    }

    public HardwareSpecs Scan()
    {
        var specs = new HardwareSpecs();

        if (_config.HardwareVisibility.CoreSystem)
        {
            specs.CoreSystem.Add(GetCpuItem());
            specs.CoreSystem.Add(GetGpuItem());
            specs.CoreSystem.Add(GetMotherboardItem());
            specs.CoreSystem.Add(GetMemoryItem());
        }

        // 2. Storage
        if (_config.HardwareVisibility.Storage)
        {
            specs.Storage.AddRange(GetStorageItems());
        }

        // 3. Peripherals
        if (_config.HardwareVisibility.Peripherals)
        {
            specs.Peripherals.Add(GetMonitorItem());
            if (!string.IsNullOrWhiteSpace(_config.Peripherals.Keyboard)) specs.Peripherals.Add(GetKeyboardItem());
            if (!string.IsNullOrWhiteSpace(_config.Peripherals.Mouse)) specs.Peripherals.Add(GetMouseItem());
            if (!string.IsNullOrWhiteSpace(_config.Peripherals.Headset)) specs.Peripherals.Add(GetHeadsetItem());
            if (!string.IsNullOrWhiteSpace(_config.Peripherals.Microphone)) specs.Peripherals.Add(GetMicrophoneItem());
        }

        // Manually described components are shown only when actually configured.
        if (_config.HardwareVisibility.CoolingAndPower)
        {
            if (!string.IsNullOrWhiteSpace(_config.CoolingAndPower.PcCase))
                specs.CoolingAndPower.Add(new HardwareItem(
                    "Kasa", "IconCase", "Kasa", _config.CoolingAndPower.PcCase,
                    "Kullanıcı tanımlı", _priceService.GetPriceFormatted("case"), "", ""));
            if (!string.IsNullOrWhiteSpace(_config.CoolingAndPower.CpuCooler))
                specs.CoolingAndPower.Add(new HardwareItem(
                    "Soğutma", "IconCooler", "Soğutucu", _config.CoolingAndPower.CpuCooler,
                    "Kullanıcı tanımlı", _priceService.GetPriceFormatted("cooler"), "", ""));
            if (!string.IsNullOrWhiteSpace(_config.CoolingAndPower.Psu))
                specs.CoolingAndPower.Add(new HardwareItem(
                    "Güç", "IconPsu", "PSU", _config.CoolingAndPower.Psu,
                    "Kullanıcı tanımlı", _priceService.GetPriceFormatted("psu"), "", ""));
        }

        if (_config.HardwareVisibility.Accessories)
        {
            void AddAccessory(string label, string icon, string? value, string priceKey)
            {
                if (!string.IsNullOrWhiteSpace(value))
                    specs.Accessories.Add(new HardwareItem(
                        "Aksesuar", icon, label, value, "Kullanıcı tanımlı",
                        _priceService.GetPriceFormatted(priceKey), "", ""));
            }
            AddAccessory("Monitör Kolu", "IconArm", _config.Accessories.MonitorArm, "monitor_arm");
            AddAccessory("Mikrofon Kolu", "IconArm", _config.Accessories.MicArm, "mic_arm");
            AddAccessory("Gamepad", "IconGamepad", _config.Accessories.Gamepad, "gamepad");
            AddAccessory("Ağ Adaptörü", "IconNetwork", _config.Accessories.EthernetAdapter, "ethernet_adapter");
            AddAccessory("Ağ Kablosu", "IconCable", _config.Accessories.EthernetCable, "ethernet_cable");
        }

        decimal manualTotal = 0;
        foreach (var item in _config.ManualHardware)
        {
            var category = HardwareCategory.Normalize(item.Category);
            var (target, enabled) = category switch
            {
                HardwareCategory.CoreSystem => (specs.CoreSystem, _config.HardwareVisibility.CoreSystem),
                HardwareCategory.Storage => (specs.Storage, _config.HardwareVisibility.Storage),
                HardwareCategory.Peripherals => (specs.Peripherals, _config.HardwareVisibility.Peripherals),
                HardwareCategory.CoolingAndPower => (specs.CoolingAndPower, _config.HardwareVisibility.CoolingAndPower),
                _ => (specs.Accessories, _config.HardwareVisibility.Accessories)
            };

            if (!enabled) continue;
            manualTotal += item.Price;
            target.Add(new HardwareItem(
                category,
                "IconMonitor",
                "Manuel Donanım",
                item.Name,
                "Amazon.com.tr",
                item.Price > 0 ? $"{item.Price:N0} ₺" : "- ₺",
                item.AmazonUrl,
                "amazon.com.tr ↗"));
        }

        RemoveHiddenHardware(specs);
        ApplyConfiguredLinks(specs);

        // Summary Values
        var total = _priceService.GetTotalPriceValue() + manualTotal;
        specs.TotalPriceFormatted = $"{total.ToString("N0", CultureInfo.GetCultureInfo("tr-TR"))} ₺";
        specs.LastUpdatedText = _priceService.GetLastUpdatedFormatted();
        specs.PriceSource = _priceService.GetSource();

        return specs;
    }

    private void RemoveHiddenHardware(HardwareSpecs specs)
    {
        var hidden = _config.HiddenHardware.ToHashSet(StringComparer.OrdinalIgnoreCase);
        specs.CoreSystem.RemoveAll(item => hidden.Contains(GetHardwareKey(item)));
        specs.Storage.RemoveAll(item => hidden.Contains(GetHardwareKey(item)));
        specs.Peripherals.RemoveAll(item => hidden.Contains(GetHardwareKey(item)));
        specs.CoolingAndPower.RemoveAll(item => hidden.Contains(GetHardwareKey(item)));
        specs.Accessories.RemoveAll(item => hidden.Contains(GetHardwareKey(item)));
    }

    public static string GetHardwareKey(HardwareItem item) => $"{item.Category}:{item.Value}";

    private void ApplyConfiguredLinks(HardwareSpecs specs)
    {
        ApplyConfiguredLinks(specs.CoreSystem);
        ApplyConfiguredLinks(specs.Storage);
        ApplyConfiguredLinks(specs.Peripherals);
        ApplyConfiguredLinks(specs.CoolingAndPower);
        ApplyConfiguredLinks(specs.Accessories);
    }

    private void ApplyConfiguredLinks(List<HardwareItem> items)
    {
        for (var index = 0; index < items.Count; index++)
        {
            var item = items[index];
            var key = GetHardwareKey(item);
            if (_config.HardwareLinks.TryGetValue(key, out var link))
            {
                items[index] = item with { Url = link, LinkDisplay = "amazon.com.tr ↗" };
            }
        }
    }

    private HardwareItem GetCpuItem()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Name, NumberOfCores, NumberOfLogicalProcessors FROM Win32_Processor");
            foreach (ManagementObject obj in searcher.Get())
            {
                var name = obj["Name"]?.ToString()?.Trim();
                if (string.IsNullOrWhiteSpace(name)) continue;
                var cores = obj["NumberOfCores"]?.ToString() ?? "?";
                var threads = obj["NumberOfLogicalProcessors"]?.ToString() ?? "?";
                return new HardwareItem("Sistem", "IconCpu", "İşlemci", name,
                    $"{cores} Çekirdek • {threads} İzlek", _priceService.GetPriceFormatted("cpu"));
            }
        }
        catch { }
        return new HardwareItem("Sistem", "IconCpu", "İşlemci", "Algılanamadı",
            "WMI verisi alınamadı", _priceService.GetPriceFormatted("cpu"));
    }

    private HardwareItem GetGpuItem()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Name, AdapterRAM FROM Win32_VideoController");
            string bestGpu = string.Empty;
            ulong maxRam = 0;

            foreach (ManagementObject obj in searcher.Get())
            {
                var name = obj["Name"]?.ToString()?.Trim() ?? "";
                if (string.IsNullOrEmpty(name)) continue;

                if (name.Contains("Microsoft", StringComparison.OrdinalIgnoreCase) ||
                    name.Contains("Basic Display", StringComparison.OrdinalIgnoreCase)) continue;

                ulong ram = 0;
                if (obj["AdapterRAM"] != null)
                {
                    ulong.TryParse(obj["AdapterRAM"].ToString(), out ram);
                }

                if (string.IsNullOrEmpty(bestGpu) || ram > maxRam ||
                    name.Contains("NVIDIA", StringComparison.OrdinalIgnoreCase) ||
                    name.Contains("GeForce", StringComparison.OrdinalIgnoreCase) ||
                    name.Contains("Radeon", StringComparison.OrdinalIgnoreCase) ||
                    name.Contains("RTX", StringComparison.OrdinalIgnoreCase) ||
                    name.Contains("Arc", StringComparison.OrdinalIgnoreCase))
                {
                    bestGpu = name;
                    maxRam = ram;
                }
            }

            if (!string.IsNullOrEmpty(bestGpu))
            {
                string cleanGpu = bestGpu
                    .Replace("NVIDIA ", "")
                    .Replace("AMD ", "")
                    .Trim();

                double vramGb = maxRam > 0 ? Math.Round((double)maxRam / (1024 * 1024 * 1024)) : 0;
                string vramText = vramGb > 0 ? $"{vramGb:F0} GB VRAM" : "Ekran Kartı";

                return new HardwareItem(
                    Category: "Sistem",
                    IconKey: "IconGpu",
                    Label: "Ekran Kartı",
                    Value: cleanGpu,
                    SubValue: vramText,
                    PriceFormatted: _priceService.GetPriceFormatted("gpu")
                );
            }
        }
        catch { }

        return new HardwareItem("Sistem", "IconGpu", "Ekran Kartı", "Algılanamadı", "WMI verisi alınamadı", _priceService.GetPriceFormatted("gpu"));
    }

    private HardwareItem GetMotherboardItem()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Manufacturer, Product FROM Win32_BaseBoard");
            foreach (ManagementObject obj in searcher.Get())
            {
                var mfg = obj["Manufacturer"]?.ToString()?.Trim() ?? "";
                var prod = obj["Product"]?.ToString()?.Trim() ?? "";

                if (mfg.Contains("Micro-Star", StringComparison.OrdinalIgnoreCase)) mfg = "MSI";
                if (mfg.Contains("ASUSTeK", StringComparison.OrdinalIgnoreCase)) mfg = "ASUS";

                var full = string.IsNullOrEmpty(mfg) ? prod : $"{mfg} {prod}";
                full = full.Trim();

                return new HardwareItem("Sistem", "IconMotherboard", "Anakart", full, "Anakart", _priceService.GetPriceFormatted("motherboard"));
            }
        }
        catch { }
        return new HardwareItem("Sistem", "IconMotherboard", "Anakart", "Sistem Anakartı", "Mainboard", _priceService.GetPriceFormatted("motherboard"));
    }

    private HardwareItem GetMemoryItem()
    {
        try
        {
            ulong totalBytes = 0;
            int stickCount = 0;
            uint speed = 0;
            using var searcher = new ManagementObjectSearcher("SELECT Capacity, Speed FROM Win32_PhysicalMemory");
            foreach (ManagementObject obj in searcher.Get())
            {
                if (obj["Capacity"] != null && ulong.TryParse(obj["Capacity"].ToString(), out var cap))
                {
                    totalBytes += cap;
                    stickCount++;
                }
                if (speed == 0 && obj["Speed"] != null && uint.TryParse(obj["Speed"].ToString(), out var sp))
                {
                    speed = sp;
                }
            }

            if (totalBytes > 0)
            {
                double totalGb = Math.Round((double)totalBytes / (1024 * 1024 * 1024));
                string stickText = stickCount > 1 ? $"{stickCount}x Modül" : "Tek Modül";
                string speedText = speed > 0 ? $"{speed} MHz" : "DDR";

                return new HardwareItem(
                    Category: "Sistem",
                    IconKey: "IconRam",
                    Label: "RAM",
                    Value: $"{totalGb:F0} GB RAM ({stickText})",
                    SubValue: speedText,
                    PriceFormatted: _priceService.GetPriceFormatted("ram")
                );
            }
        }
        catch { }

        return new HardwareItem("Sistem", "IconRam", "RAM", "Algılanamadı", "WMI verisi alınamadı", _priceService.GetPriceFormatted("ram"));
    }

    private List<HardwareItem> GetStorageItems()
    {
        var items = new List<HardwareItem>();
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Model, Size, MediaType FROM Win32_DiskDrive");
            foreach (ManagementObject obj in searcher.Get())
            {
                var model = obj["Model"]?.ToString()?.Trim();
                if (string.IsNullOrWhiteSpace(model)) continue;
                var mediaType = obj["MediaType"]?.ToString()?.Trim() ?? "Disk";
                string capacity = "";
                if (ulong.TryParse(obj["Size"]?.ToString(), out var bytes) && bytes > 0)
                    capacity = $"{(double)bytes / 1_000_000_000:F0} GB";
                var isSsd = model.Contains("SSD", StringComparison.OrdinalIgnoreCase)
                    || model.Contains("NVMe", StringComparison.OrdinalIgnoreCase)
                    || mediaType.Contains("SSD", StringComparison.OrdinalIgnoreCase);
                var label = isSsd ? "SSD" : "Disk";
                var priceKey = isSsd ? "ssd_nvme" : "ssd_sata";
                items.Add(new HardwareItem("Depolama", "IconStorage", label,
                    model, string.IsNullOrEmpty(capacity) ? mediaType : $"{capacity} • {mediaType}",
                    _priceService.GetPriceFormatted(priceKey)));
            }
        }
        catch { }

        if (items.Count == 0)
            items.Add(new HardwareItem("Depolama", "IconStorage", "Disk",
                "Algılanamadı", "WMI verisi alınamadı", _priceService.GetPriceFormatted("ssd_sata")));
        return items;
    }

    private HardwareItem GetMonitorItem()
    {
        if (!string.IsNullOrEmpty(_config.Peripherals.Monitor))
        {
            return new HardwareItem("Çevre Birimleri", "IconMonitor", "Monitör", _config.Peripherals.Monitor, "Kullanıcı tanımlı", _priceService.GetPriceFormatted("monitor"));
        }

        int resX = (int)SystemParameters.PrimaryScreenWidth;
        int resY = (int)SystemParameters.PrimaryScreenHeight;
        string resText = $"{resX}x{resY} Çözünürlük";

        try
        {
            using var searcher = new ManagementObjectSearcher(@"root\wmi", "SELECT UserFriendlyName FROM WmiMonitorID");
            foreach (ManagementObject obj in searcher.Get())
            {
                if (obj["UserFriendlyName"] is ushort[] nameBytes)
                {
                    var chars = nameBytes.Where(b => b != 0).Select(b => (char)b).ToArray();
                    var name = new string(chars).Trim();
                    if (!string.IsNullOrEmpty(name))
                    {
                        return new HardwareItem("Çevre Birimleri", "IconMonitor", "Monitör", name, resText, _priceService.GetPriceFormatted("monitor"));
                    }
                }
            }
        }
        catch { }

        return new HardwareItem("Çevre Birimleri", "IconMonitor", "Monitör", "Birincil Ekran", resText, _priceService.GetPriceFormatted("monitor"));
    }

    private HardwareItem GetKeyboardItem() =>
        new("Çevre Birimleri", "IconKeyboard", "Klavye",
            _config.Peripherals.Keyboard ?? "Tanımlanmadı", "Kullanıcı tanımlı",
            _priceService.GetPriceFormatted("keyboard"));

    private HardwareItem GetMouseItem() =>
        new("Çevre Birimleri", "IconMouse", "Fare",
            _config.Peripherals.Mouse ?? "Tanımlanmadı", "Kullanıcı tanımlı",
            _priceService.GetPriceFormatted("mouse"));

    private HardwareItem GetHeadsetItem() =>
        new("Çevre Birimleri", "IconHeadset", "Kulaklık",
            _config.Peripherals.Headset ?? "Tanımlanmadı", "Kullanıcı tanımlı",
            _priceService.GetPriceFormatted("headset"));

    private HardwareItem GetMicrophoneItem() =>
        new("Çevre Birimleri", "IconMic", "Mikrofon",
            _config.Peripherals.Microphone ?? "Tanımlanmadı", "Kullanıcı tanımlı",
            _priceService.GetPriceFormatted("microphone"));

}
