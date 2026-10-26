# Gold Rush — dört hedef türü

GDD §11 kapsamındaki Coin Sprint, Crate Breaker, Golden Swarm ve Jackpot aynı üç küplük özel göreve bağlandı. İlk küp altın toplama, ikinci küp altı kasa kırma; ikinci küp temizlendikten sonraki iki kapıdan biri Altın Sürüsü, diğeri Jackpot sunar. Kapının güçlendirme ödülü ayrıca korunur. Kontrol hâlâ tek joystick ve yürüyerek kapı seçimidir.

## Hedefler ve ödüller

| Tür | Tamamlama | Varsayılan değerler |
|---|---|---|
| Coin Sprint | Mevcut altın kotasını topla | İlk küpte 40; erişilebilir grid korunur |
| Crate Breaker | Sayılan altı kasayı kır | Her kasa 6 can / 8 altın |
| Golden Swarm | Beş altın taşıyıcısını indir | Her biri 12 can / 12 altın |
| Jackpot | Üç anahtar taşıyıcısını indir, açılan sandığı kır | Sandık 24 can / 60 altın; normal düşman altını ayrıca |

Kasa hedefinde düşmanları öldürmek, Sürü hedefinde kota kadar altın toplamak veya Jackpot'ta yalnız üç anahtarı almak clear üretmez. Jackpot sandığı anahtarlar tamamlanmadan hasar almaz ve otomatik hedeflemeye girmez. Açıldıktan sonra otomatik hedef olur. Kasa/sandık için ikinci joystick, ateş butonu veya dokunma gerekmez.

Oyuncu en yakın görüş hattı açık düşman veya açık kasaya döner. Oyuncu lazeri siper, düşman, varil ve kasa arasında yolundaki ilk objeye çarpar. Kilitli sandık hasar almaz; düşmanlar anahtarları açar. Varil patlamaları açık kasaları kırabilir; bir patlama hedefi iki kez sayamaz.

Ödüller pickup olarak çıkar, tamamlanan küpün mevcut bankalama işleminde kalıcılaşır. Kasadan altın düşmesi tek başına bankaya yazmaz. Başarısız küpün bekleyen altını kaybolur. Kayıt hatası/retry sırasında hedef ilerlemesi ve bekleyen tutar korunur; tekrarlanan tamamlama veya kayıt denemesi çift ödeme üretmez. Geçici görev durumları yeni küp ve yeni denemede sıfırlanır. Profil şeması ve kalıcı geliştirmeler değişmez.

## Rotalar ve süre bütçesi

Rota üretimi, kasa canını normal düşman bütçesine ekler; Sürü için beş taşıyıcı, Jackpot için üç muhafız + sandık hesaplanır. Sabit hedef sayıları HP bütçesi uğruna azaltılmaz. Kasa modunun gerektiğinde yalnız normal destek sayısı azalır. Son iki kapı farklı hedef türlerini garanti eder; seed/silah verisi aynıysa seçimler deterministiktir.

Mevcut hedef hapı KASA KIR, ALTIN SÜRÜSÜ, ANAHTAR veya SANDIĞI KIR gösterir. Kapı bilgi kartlarında hedef türü ve düşman sayısı yazılır. Görev açıklaması çalışma anında yeni akışı anlatır. UI boyutları, konumları, fontları ve stilleri değiştirilmedi. Yeni kasa; mevcut Low cover gövdesi ve Gold pickup parçalarıyla kurulur. Model kütüphanesine veya görsel varlıklara yazılmaz.

## Düzenleme

`InvasionPrototype.goldRushVariants`, `Resources/GoldRushVariantSettings.asset` varlığına bağlıdır. Kasa/sandık/taşıyıcı canları ve altın ödülleri Inspector'dan düzenlenebilir. `Cube Invasion > Content > Install Gold Rush Variants` yalnız eksik ayarı ve sahne referansını kurar. Özellik kapatılırsa önceki kota odaklı Gold Rush çalışır.

Mevcut görev sayısı, üç küplük uzunluk, görev/silah açılma sırası, içerik kataloğu, Crusher fazları, kamera, lazerler, tek parmak atış ritmi, süre gerilimi ve kesintisiz geçişler korunur. Ana sahneye bu çalışmada yalnız goldRushVariants referansı eklendi. Görsel set, ana kütüphaneler, içerik kataloğu, Crusher ayarı ve kullanıcının önizlemesi dahil 268 dosya başlangıç SHA-256 değerleriyle kontrol edilir.

Yerel sınırlı playtest kaydı varyant başlangıcı, kırılan kasa, öldürülen taşıyıcı, alınan anahtar ve açılan jackpot olaylarını tutar. Rota özeti hedef türünü de içerir; ağa gönderim yoktur.

## Doğrulama

- `GoldRushBudgetCheck`: dört silah, 128 seed, **2.048 teklif**; farklı son hedefler, zorunlu sayılar, ≤20 sn teorik can bütçesi ve değişmeyen içerik kataloğu.
- `GoldRushVariantCheck`: dört silahla Sürü ve Jackpot finallerini kapsayan **sekiz tam görev / 24 küp**. Gerçek lazerlerle kasa ve sandık isabeti, hedeflerin birbirinin yerine geçememesi, kilit/üç anahtar sırası, kapı sürekliliği, tam altın tutarları, varil, timeout ve retry.
- Aynı test, siper arkasındaki kasanın otomatik hedef dışı kalmasını ve gerçek süpürülmüş lazerin kasadan önce sipere çarpmasını sınar.
- `GoldRushEconomyCheck`: izole gerçek profil, erken bankalama olmaması, disk yazma hatası, kurtarma, tekrar deneme, kalıcı tutar ve hedef olayları.
- Mevcut çekirdek/feel, iki yönlü yedi görev, Crusher'ın sekiz koşusu, rota, savaş basıncı ve ana menü kontrolleri de çalıştırılır. Eski P2 fixture'ı yeni Gold Rush ayarını yalnız kendi test oturumunda kapatır.

[QA kaydı](Validation/GoldRush_Variants_QA.json) · [Kasalar](Validation/GoldRush_Crates.png) · [Kapılar](Validation/GoldRush_Gates.png) · [Jackpot](Validation/GoldRush_Jackpot.png)

Programatik tamamlama ve teorik DPS bütçesi gerçek oyuncu zorluk testi değildir. Fiziksel Android cihazda dokunmatik, ısı/pil ve FPS testi yapılmadı. GDD'nin diğer özel modları, yeni düşman/silah/boss ve 60 bölüm kapsamı bu çalışmada tamamlanmış sayılmaz.
