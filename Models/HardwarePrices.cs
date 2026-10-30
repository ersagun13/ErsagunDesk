using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace ErsagunDesk.Models;

public class HardwarePrices
{
    public string Source { get; set; } = "Amazon.com.tr";
    public string LastUpdated { get; set; } = DateTime.Today.ToString("yyyy-MM-dd");
    public string Currency { get; set; } = "₺";
    public Dictionary<string, decimal> Items { get; set; } = new();

    public static HardwarePrices Load(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var prices = JsonSerializer.Deserialize<HardwarePrices>(json, options);
                if (prices != null) return prices;
            }

        }
        catch { }

        return GetDefaults();
    }

    public void Save(string path)
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(path, json);
        }
        catch { }
    }

    public static HardwarePrices GetDefaults()
    {
        return new HardwarePrices
        {
            Source = "Amazon.com.tr",
            LastUpdated = DateTime.Today.ToString("yyyy-MM-dd"),
            Currency = "₺",
            Items = new Dictionary<string, decimal>()
        };
    }
}
