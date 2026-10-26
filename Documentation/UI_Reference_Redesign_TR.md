Güncel görünüm: [Void Toy Arcade UI](UI_VoidToy_TR.md). Aşağıdaki tasarım önceki sürümdür.

# Referanslara göre yeniden kurulan UI / UX

Verilen JellyJoy ve Block Jam ekranları stil/yerleşim referansı olarak kullanıldı. Oyuna ait olmayan can yenileme, reklam, premium para veya sahte etkinlik akışları eklenmedi. Eski menü hiyerarşisi, sahne kurulum aracı üzerinden yeniden oluşturuldu; karakter, kamera, dünya akışı ve profil ilerlemesi korunur.

- **Ana üs:** gerçek yedi görevden oluşan kaydırılabilir bölüm yolu; üstte Scout ve gerçek altın, tamamlanan görev çubuğu; iki yanda silah/pilot kısayolları; altta seçili görevi doğrudan başlatan buton.
- **Bölümler:** iri kabartmalı düğümler ve arkalarından geçen rota; seçili görev açıklaması ve kilit durumuna bağlı başlatma.
- **Silahlar:** büyük seçili silah vitrini, yatay dört silah seçimi, okunabilir istatistikler ve üç geniş geliştirme satırı. Kilitli silah seçilince satın alma aksiyonu görünür.
- **Pilot:** karakter vitrini ve ayrı can/hız/toplama değerleri; renkli stat rozetleri ve sağda satın alma butonları.
- **Ayarlar:** çerçeveli ses paneli, büyük sürgüler ve açık/kapalı durumları ayrışan ayar satırları.
- **HUD:** sayaç ve hedef/altın için ayrı kutular; düzenlenmiş can çubuğu, pause ve öğretici alanları.
- **Sonuç:** karartılmış, dokunmaları engelleyen arka katman; sonuç amblemi, deneme özeti ve durumuna göre boşluksuz yerleşen aksiyonlar. Savaş arayüzüne yanlışlıkla dokunulmaz.

Görsel dil: doygun teal/mavi yüzeyler, yeşil ana aksiyonlar, altın başlıklar, kabartmalı düğmeler, beyaz konturlu yuvarlak yazılar ve ikonlu alt dock. Sahne illüstrasyonu veya tam ekran raster arka plan kullanılmaz. Mobil ölçekleme genişliği temel alır; alt dock ve ana oynama butonu alt kenara bağlıdır. 540×960 ve 540×1200 görüntüleri kontrol edilir.

## Imagegen varlıkları ve animasyon

Yerleşik imagegen ile üretilen beş yeni şeffaf PNG: `Assets/CubeInvasion/Art/UI/Reference/{Journey,Arsenal,Pilot,Settings,Coin}.png`. Kaynak PNG'ler projede kalır; Unity sprite importları 512 px ile sınırlandırılır. Önceki `CasualButton.png` nötr yüzeyi dokuz dilimli olarak yeni panel/buton boyutlarında yeniden kullanılır. PNG'lere kodla raster düzenleme yapılmadı. Prompts: `UI_Reference_Imagegen_Prompts.json`.

DOTween: basış/bırakış, seçili dock ikonunda kısa büyüme, sayfa fade/scale geçişi ve sonuç penceresinde pop animasyonu. Animasyonlar pause sırasında UI'nin kullanılabilmesi için unscaled time ile çalışır; disable/cancel sırasında temizlenir.

## Kurulum ve korunan veriler

`Cube Invasion / UI / Rebuild Reference Layout` yalnızca menüyü yeniden kurar ve HUD/sonuç yerleşimini günceller. Ana sahnede karakter/kamera/sabit dünya yeniden oluşturulmaz. Mevcut menü kurulumu da bu aracı çağırır. Rebuild sahne UI düzenlemelerini yeniler; profil dosyasını ve satın alma verilerini değiştirmez.

## Yazı tipi

`CubeRounded-Bold.ttf`, [Google Fonts Fredoka kaynağından](https://github.com/google/fonts/tree/main/ofl/fredoka) türetilmiş statik weight 700 / width 90 örneğidir; aile adı Cube Rounded olarak değiştirildi. Eksik Türkçe precomposed karakterler orijinal harf ve diakritik gliflerinden birleştirildi; ÇĞİÖŞÜçğıöşü kapsamı doğrulandı. Lisans ve değişiklik notu APK'ya Resources üzerinden dahil edilir. Tekrar üretim: `Documentation/Tools/BuildReferenceFont.py source-variable.ttf output.ttf` (fontTools gerekir). Bu araç oyun veya Unity derlemesinin bağımlılığı değildir.

## Doğrulama ve güncel paket

Ana kaynakların runtime/editor C# derlemesi, beş ekran + dokunma animasyonu + işlemler/ayarlar/yeniden yükleme testi, 540×1200 ana aksiyon yerleşimi, rota çizgisi sırası, sonuç etiketleri/karartma katmanı/retry ve P3 banka/kayıt kurtarma regresyonu geçti. Android ARM64 IL2CPP Development derlemesi başarılı: proje kökünde `CubeInvasion-ReferenceUI.apk`. Eski P4 ve HybridCasual APK dosyaları önceki tasarımlardır. Fiziksel Android dokunma/performans testi yapılmadı.

Ekran görüntüleri `Validation/Reference_*.png`; kontrol özeti `Validation/Reference_UI_QA.json`. Unity editör başlangıcındaki önceki SearchDatabase istisnası loglarda bulunuyor; kontroller ve build başarılı çıkış yaptı.
