# ErsagunDesk

**ErsagunDesk**, Windows 10/11 için C# / WPF / .NET 10 tabanlı, Türkçe odaklı kişisel masaüstü bilgi ve sistem panelidir. [OpenWidget](https://github.com/alztrk/OpenWidget) altyapısının MIT lisanslı bir türevidir.

[English](README.md) · [GitHub Actions](../../actions) · [Atıf](NOTICE.md)

## Özellikler
- **Saat ve sistem:** Saat, tarih, Windows çalışma süresi ve oyun modu.
- **Hava durumu:** Varsayılan Ankara, anlık durum ve üç günlük Open-Meteo tahmini; şehir seçimi.
- **Telemetri:** CPU, GPU, VRAM, RAM, disk, ağ gecikmesi ve trafiği.
- **Finans:** Döviz, altın, kripto para ve çevrimdışı durum/önbellek.
- **Medya:** Çoklu Windows medya oturumu, oynatıcı ve bağımsız ses denetimi.
- **Pano:** Metin, görsel önizlemesi ve dosya adlarıyla kalıcı geçmiş.
- **Donanım:** Sistemin gerçek bileşenleri, isteğe bağlı manuel bileşenler ve fiyatlar.
- **Hızlı araçlar:** Görev Yöneticisi, ağ araçları, ses ayarları, kalıcı notlar.

**Arayüz:** Grafit/turkuaz tema, kompakt HUD, çoklu monitör ve DPI, sağ tık menüsü, kart açma/kapatma, tek kopya koruması, oyun modu ve `Ctrl+Shift+W` görünürlük kısayolu.

## Kurulum / geliştirme
Windows 10 (2004+) veya Windows 11 x64 gerekir. Geliştirme için .NET 10 SDK yükleyin:
```powershell
dotnet restore ErsagunDesk.sln
dotnet test ErsagunDesk.sln
dotnet run --project ErsagunDesk.csproj
powershell -ExecutionPolicy Bypass -File .\scripts\publish-portable.ps1
```
Başarılı [GitHub Actions](../../actions) çalıştırmasından Windows x64 uygulama paketini indirebilirsiniz.

## Varsayılanlar ve güvenlik
- Türkçe dil, Ankara konumu, Ersagun turkuaz teması.
- Kaynak geliştiricinin örnek bilgisayar bileşenleri ve örnek fiyatları varsayılanlardan çıkarıldı.
- Manuel donanım fiyatları ilk açılışta boş; yalnızca sisteme ait bilgiler algılanır.
- **Pano geçmişi şifresiz yerel dosyada tutulur**: parola ve API anahtarı gibi sırları kopyalarken dikkat edin.
- Yerel ayarlar/önbellek: `%LOCALAPPDATA%\ErsagunDesk`. Pano ve notlar: `%APPDATA%\ErsagunDesk`.

## Sıradaki geliştirmeler
Network Operations paneli (IP, ağ adaptörü, gateway, DNS, ping ve bağlantı kontrolü), daha kapsamlı testler ve imzalı yayın paketleri.

## Lisans
[MIT](LICENSE). Kaynak OpenWidget telif bildirimi korunmuştur, ayrıntılar [NOTICE.md](NOTICE.md).
