# Mobil arayüz / UX uygulaması

Unity sahnesine Imagegen görsellerini kullanan beş ekran eklendi: ana menü, sefer yolu, silah marketi, Scout geliştirmeleri ve ayarlar. Görseller arka plan olarak kullanılır; metinler, fiyatlar, kilitler, yol düğümleri ve bütün kontroller gerçek uGUI bileşenleridir. İşlevler ekran görüntüsüne gömülü değildir.

## Akış

Play açılışında komuta merkezi görünür. Ana eylem bölüm yolunu açar; alttaki beş sekme tüm menülere erişim sağlar. Bölüm yolu kaydırılabilir; yedi görev tamamlandı/açık/kilitli durumlarıyla gösterilir. Düğüme dokunmak görev bilgisini gösterir. Kilitli görev incelenebilir, başlatılamaz. Seçili açık görev **Göreve Başla** ile aynı sahnede oynanır. Küp içi joystick, otomatik hedefleme, kapılardan güçlendirme ve kesintisiz küp geçişleri korunur.

Market, mevcut modellerden hazırlanmış 3D ürün görsellerini kullanır. Açık silaha dokunmak kuşanır; kilitli silah yalnız incelenir ve yeterli altınla satın alınır. Hasar/atış hızı/menzil yükseltmeleri P3'ün kayıt işlemlerini kullanır. Market ve karakter fiyatları gerçek bakiyeye bağlıdır; yetersiz altın veya maksimum seviye butonları kapatır.

## Scout geliştirmeleri

Her biri 5 seviyelidir; yalnız görev menüsünde satın alınır. Fiyatlar `P3MetaSettings.asset` içinde düzenlenebilir.

| Geliştirme | Her seviye | Sonraki fiyat |
| --- | --- | --- |
| Yaşam çekirdeği | +10 maksimum can | 60 × (seviye + 1) |
| Hareket sistemi | Taban hıza +%3 | 70 × (seviye + 1) |
| Toplama alanı | +0,2 birim pickup yarıçapı | 45 × (seviye + 1) |

Görev başlangıcında karakter geliştirmelerinin snapshot'ı alınır. Gerçek başlangıç canı, hareket ve pickup mesafesi etkilenir; can barı/pickup/kapı iyileştirmesi yeni maksimum canı kullanır. Küp değişiminde can yeniden dolmaz.

Kayıt payload şeması 2'ye yükseltildi. Önceki şema-1 cüzdan, silah ve görev verileri korunarak yüklenir; eksik karakter seviyeleri sıfır kabul edilir. Dosya yolu değişmedi. Yeni bir işlemde şema-2 kaydı yazılır; eski sürümlerin yeni kayıtları yanlışlıkla yeniden yazması önlenir. Mevcut kayıt dosyaları testlerde kullanılmaz.

## Ayarlar

Müzik ve efekt sesleri bağımsızdır. Genel sessiz mod, Android titreşim tercihi ve 60/30 FPS hedefi kaydedilir. Ses karışımı ve kare hedefi anında uygulanır. Kaydırıcı sürüklerken disk kaydı kısa süre ertelenir; odak kaybı, arka plana geçiş ve kapanışta bekleyen ayarlar yazılır. Titreşim ses kapalıyken de bağımsız çalışır; gerçek cihazda doğrulanmalıdır.

## Sanat ve sahne düzenleme

- `Art/UI/MenuHero.png`: yerleşik Imagegen ile üretilen robotlu ana menü görseli.
- `Art/UI/CampaignIslands.png`: yerleşik Imagegen ile üretilen ada/harita arka planı.
- `Art/UI/Weapon_0..3.png`, `Scout.png`: mevcut 3D modellerden Unity kamera/ışık ile hazırlanmış saydam önizlemeler.
- `Resources/FrontEndArt.asset`: görseller, önizlemeler ve dilimlenebilir panel/düğüm sprite'ları.
- `Runtime/MenuFrontEnd.cs` ve `.Layout.cs`: beş sayfanın sunumu; `InvasionPrototype.FrontEnd.cs`: ekonomi, görev ve ayar bağlantıları.

Ana sahnedeki `Illustrated Front End` altında beş sayfanın nesneleri vardır. Editörde incelemek için `Cube Invasion > UI > Preview` menüsünden Home, Road Map, Arsenal, Pilot veya Settings seçilebilir. Yeniden kurulum menüsü mevcut nesneleri sıfırlamadan eksik front end'i bağlar. Önceki P2/P3 menü bileşenleri sonuç ve kayıt hatası ekranlarını sağlamaya devam eder; eski loadout panelleri yeni menü açıkken gizlenir.

Üretim promptları ve varlık konumları [UI_Imagegen_Prompts.md](UI_Imagegen_Prompts.md) içinde. Buton basışları, sayfa geçişleri ve cüzdan vurguları native uGUI/DOTween üzerinden çalışır. Arka plan dokunmaları oyun HUD'ına geçmez. Joystick, kamera ve dünya nesneleri menü geçişlerinde yeniden yaratılmaz.

## Doğrulama

`FrontEndSmokeCheck.Run`: beş sayfanın callback'leri, road node inceleme/başlatma kilidi, kaydırılabilir içerik, market satın alma/yükseltme, üç karakter stat'ının gerçek oyuna etkisi, ayrı ses ayarları, titreşim/kare hedefi kaydı, dokunma yüzeyleri, menü arkasına tıklama engeli ve sahne reload'unda para/stat/ayarların geri gelmesi. Ayrı geçici profil ve ayar yedeği kullanılır; gerçek oyuncu kaydı değiştirilmez. Başarı: `CUBE_UI_SMOKE_OK`.

Profil testleri eski şema geçişini, karakter fiyat/sınırlarını ve yazma hatasında para/seviye değişmemesini de kontrol eder. P3 ile P0/P1/kenar düşüşü regresyonları ayrıca çalıştırılır. Ekranlar 540×960 render edilip görsel olarak incelenir. Android cihazda fiziksel dokunma, çentik, titreşim, düşük disk alanı ve GPU performansı ayrıca doğrulanmalıdır.

Son doğrulama: `CUBE_UI_SMOKE_OK`, genişletilmiş `CUBE_P3_STORE_OK`, `CUBE_P3_SMOKE_OK`, P0/P1/kenar düşüşü ve P2'nin iki rota/yedi görev testleri geçti. Ana projenin runtime/editor kaynakları kendi derleyici ayarlarıyla hatasız derlendi. Sahne/sprite/texture GUID bağlantıları doğrulandı.

Uygulanan ekranlar: [ana menü](Validation/UI_Home.png), [bölüm yolu](Validation/UI_Road.png), [üst yol](Validation/UI_RoadTop.png), [market](Validation/UI_Market.png), [karakter](Validation/UI_Pilot.png), [ayarlar](Validation/UI_Settings.png).
