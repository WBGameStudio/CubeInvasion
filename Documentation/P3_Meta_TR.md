# P3 / Meta

P3 uygulaması kalıcı cüzdanı, silah açma ve yükseltmeyi, görev ilerlemesini, kayıt/kurtarmayı ve deneme sonuçlarını ekler. P2'nin 27 küplük içeriği ve mevcut sahne/kamera/hareket düzeni korunur.

## Oynama

İlk açılışta Pulse Pistol ve ilk görev açıktır. Önceki `CI.Wallet` bakiyesi varsa yalnız ilk profil oluşturulurken aktarılır. Diğer silahlar dükkânda incelenir ve yeterli altınla açılır. Kilitli silaha bakmak onu kuşanmaz; kilitli silahla/görevle başlatma engellenir. Görev kazanılınca sıradaki görev açılır; önceki açık görevler yeniden oynanabilir.

| Silah | Açma fiyatı |
| --- | ---: |
| Pulse Pistol | Başlangıç |
| Twin Blasters | 150 altın |
| Scatter Shot | 350 altın |
| Rapid SMG | 600 altın |

Her silahın kendi kalıcı yükseltmeleri vardır:

| Yükseltme | Seviye | Seviye başına etki | Sonraki seviye fiyatı |
| --- | ---: | --- | --- |
| Hasar | 0–10 | Taban hasara +%5 | 35 × (mevcut seviye + 1) |
| Atış hızı | 0–5 | Taban atış hızına +%4 | 50 × (mevcut seviye + 1) |
| Menzil | 0–5 | Mermi ömrüne/menziline +%4 | 30 × (mevcut seviye + 1) |

Artışlar kendi stat'ı içinde additiftir. Atış aralığı hız katsayısına bölünür. Geçici kapı hasar ödülü, kalıcı silah hasarına ayrıca uygulanır ve kalıcı kayda yazılmaz. Silah/güçlendirme seçimleri sadece görev menüsünde yapılır; aktif görevde loadout sabittir. Dükkân kartları gerçek önce/sonra değerlerini, fiyatı ve maksimum seviyeyi gösterir.

Sonuç ekranı bu denemede kazanılan altını, tamamlanan küp sayısını, öldürülen hedefleri, savaş süresini ve toplam cüzdanı ayrı gösterir. Başarıda sonraki göreve geçilebilir. Fail/retry, açık silahı ve kalıcı yükseltmeleri korur; geçici kapı buff'ları yeni denemede sıfırlanır. Bir görevi baştan meşru şekilde tekrar oynamak yeni bir denemedir; aynı clear callback'inin tekrarlanması kazancı çoğaltmaz.

## Kayıt kuralları

- Profil `Application.persistentDataPath/ci_profile_v1.json` dosyasında tutulur. Satın alma, yükseltme, seçim, deneme başlangıcı, her tamamlanan küp ve deneme sonucu ayrı kalıcı işlemlerdir.
- İşlem önce profilin kopyasında hazırlanır. JSON geçici dosyaya yazılıp diske flush edilir, ana dosya değiştirilir, ancak sonra bellekteki profil güncellenir. İşlem başarısızsa altın/ownership/yükseltme veya küp checkpoint'i değişmez.
- Payload checksum ve sürüm doğrulaması yapılır. `.bak` bir önceki geçerli snapshot'ı korur. Ana dosya bozuksa geçerli yedek yüklenir; bozuk ana dosya `.corrupt` olarak saklanır. İki dosya da okunamazsa veya daha yeni sürüm kaydı görülürse dosyalar sıfırlanmaz, işlem engellenir ve yeniden deneme ekranı gösterilir.
- Her deneme benzersiz kimlik taşır. Küp kaydı sıralıdır; aynı deneme/küp ikilisi iki kez ödüllendirilemez. Tamamlanmayan küpün pickup altını fail'de bankalanmaz. Final küpün altını ve görev açılması birlikte kaydedilir.
- Bankalama sırasında depolama hatası olursa sayaç, hareket ve animasyonlar durur. **Kaydı Yeniden Dene**, aynı bekleyen işlemi tekrar dener; başarılı ödül ikinci kez uygulanmaz.
- Uygulama/sahne yeniden açıldığında cüzdan, silahlar, yükseltmeler, seçili görev/loadout ve ilerleme geri gelir. Kesilmiş aktif deneme güvenli biçimde sonlandırılır; tamamlanmış küplerin altını korunur. Aktif savaşın fiziksel konumu/sayaç durumu devam ettirilmez.

## Sahne ve düzenleme

`Resources/P3MetaSettings.asset` fiyat, üst sınır ve katsayıları içerir. `CubeInvasionPrototype.unity` içinde `P3 Meta HUD`, dükkân kontrolleri ve kayıt hatası ekranı sahneye kaydedilmiştir. Ana yönetici üzerindeki P3 referansları mevcut P2 sunumunu tamamlar. Kurulum menüsü: `Cube Invasion > P3 > Install Progression In Current Scene`; mevcut sahne nesnelerini veya ayarlı fiyat asset'ini sıfırlamaz.

Mevcut silah modelleri kullanılır. Satın alma/yükseltmede DOTween cüzdan vurgusu; açma, yükseltme ve reddedilen işlem için altı yeni özgün WAV varyantı eklendi. Sesler mevcut mute/pause/müzik miksajına bağlıdır.

## Doğrulama

`P3ProfileCheck.Run`: ilk legacy aktarımı, tekrar aktarımın engellenmesi, fiyatlar, ownership, stat sınırları, yetersiz altın, idempotent/sıralı cube ödülü, fail/win, kalıcı reload, bozuk ana dosyadan kurtarma, iki bozuk dosyayı koruma, yeni sürüm kaydını koruma, gerçek I/O hata simülasyonu ve sonrasında tekrar deneme.

`P3SmokeCheck.Run`: sıfır altınlı yeni oyuncu, kilitli görev/silah, çift başlatma ve clear, beş tam görev tekrarıyla 285 altın, silah satın alma, üç yükseltmenin gerçek mermi/atış değerleri, kapı geçişleri, kenar fail'i ve bekleyen altının silinmesi, retry, gerçek sahne reload'u ve kesilen deneme, oyun içinde bankalama hatası/yeniden deneme. P3 testleri ayrı geçici profil kullanır; oyuncunun gerçek profilini ve eski cüzdanını değiştirmez.

Başarı işaretleri: `CUBE_P3_STORE_OK`, `CUBE_P3_SMOKE_OK`. P0/P1 ve P2 kontrolleri ayrıca regresyon olarak çalıştırılır. Android hedefi korunmuştur; gerçek cihazda uygulama kapatma/açma, düşük disk alanı, dokunma ve performans doğrulaması bu editör testlerinin yerine geçmez.

Son çalıştırmada `CUBE_P3_STORE_OK`, `CUBE_P3_SMOKE_OK`, P2'nin iki rota/yedi görev kontrolleri, P0/P1 ve kenar düşüşü kontrolleri geçti. Ana projenin runtime/editor kaynakları kendi derleyici ayarlarıyla hatasız derlendi; sahne ve yeni asset GUID bağlantıları doğrulandı.

Görseller: [dükkân](Validation/P3_Arsenal.png), [kilitli silah](Validation/P3_LockedWeapon.png), [yükseltmeler](Validation/P3_UpgradedWeapon.png), [sonuç](Validation/P3_Win.png), [fail](Validation/P3_Fail.png), [kayıt kurtarma](Validation/P3_SaveRecovery.png).
