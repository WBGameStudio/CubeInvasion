# GDD — genişletilmiş kampanya

Bu çalışma ilk dikey kesiti 60 bölüm ve 12 silahlı oynanabilir kampanyaya genişletir. Android portrait, Unity 6000.4.8f1, tek joystick, hareket miktarına bağlı otomatik ateş, lazer/siper çarpışması, fiziksel kapı seçimi ve aynı karakter/kamera ile geçiş korunur. Eski Void Toy sanat dosyaları yeniden üretilmez.

## İçerik ve kayıt uyumluluğu

İlk yedi görev aynı indekslerde kalır; eski cüzdan, ownership, geliştirmeler ve tamamlanan görevler korunur. Bu nedenle eski Gold Rush ve Crusher görevleri L6/L7'de giriş karşılaşmaları olarak durur. L10/20/30/40/50/60'ta düzenli boss ritmi; L15/35/55'te Gold Rush vardır. İlk beş normal görevin düşman tanıtımları korunur. Sonraki kampanya, altı biyomun kurallı modüler karşılaşmalarından oluşur; 60 ayrı el yapımı sinematik bölüm değildir.

Kaydı açarken checksum önce doğrulanır; yalnız bilinen 4 silah / 7 görev formatı 12 / 60'a genişletilir. Eski yedi görevi bitirmiş profilde L8 doğrudan açılır; Crusher'ı tekrar oynama zorunluluğu yoktur. Yeni veya bozuk kayıtlar kesilip sıfırlanmaz. Özel evrim ve döngü seviyeleri de aynı atomik kayıtta saklanır.

Düzenleme:

- `Resources/CampaignExpansionSettings.asset`: kampanyayı aç/kapat, seed, genişletilmiş katalog ve 12 silahın açma fiyatları.
- `Resources/CampaignContent.asset`: 60 görev, karşılaşmalar, biyom, boss, özel kural, platform düzeni/tehlikesi ve 12 silahın sayıları. Eski `P2Content.asset` korunur.
- `Resources/CampaignPresentation.asset`: yalnız yeni sanat, market sprite'ları, altı biyom müziği, gerilim katmanı ve yeni SFX.
- `Art/Expansion`: sekiz silah mesh'i, üç yeni düşman ve beş yeni boss için rig'li modeller, enerji düğümü, thumbnail ve sesler. Rig'ler mevcut DOTween hareket/telegraph/vuruş/ölüm sürücüsünü kullanır.
- `Resources/Rendering`: mobil URP renderer/pipeline. Orijinal Void Toy shader ve materyalleri yerinde kalır.

Kurulum menüsü: `Cube Invasion / Full Campaign / Install`. Var olan yeni katalog ve sanatlar otomatik olarak yeniden yazılmaz. Yeni thumbnail'ları yenilemek için `CampaignExpansionBuilder.RefreshWeaponIcons`; ses üretimi için `CampaignSoundBuilder.Build` kullanılır.

## Yeni düşmanlar

- **Summoner — 25 can:** 6 saniyelik bekleme ve 0,8 saniyelik görünür hazırlıktan sonra ömrü boyunca en fazla iki minyon üretir. Minyonlar 5 can taşır ve zorunlu hedef sayısına eklenir. Son 3 saniyede yeni zorunlu minyon doğmaz. Oyuncu, çağrıyı tamamlamadan Summoner'ı öldürebilir.
- **Phaser — 19 can:** 2,5 saniyelik arayla güvenli hedef konumunu 0,7 saniye gösterir, sonra ışınlanır. Çıkıştan sonra 0,6 saniye %30 ek hasar alır.
- **Elite — 70 temel can:** üç atışlı saldırı ve yönü kilitlenen itme saldırısını sırayla kullanır. İki işaret kombinasyonu okunur. Elite Hunt için ayrı 36 canlık mini-elite bütçesi vardır. Mevcut kapı elit çarpanı ayrıca korunur.

Kalkanlı, Tamirci, Bombacı ve önceki altı tür devam eder. Normal karşılaşmalarda üç tür sınırı uygulanır; rota dönüşümleri veya altın takviyesi dördüncü tür ekleyemez.

## Silahlar ve geliştirmeler

| Silah | Davranış |
|---|---|
| Pulse Pistol / Twin Blasters / Scatter Shot / Rapid SMG | Mevcut dört silah korunur. Özel evrim ilk üç tekil atış ailesinde hasarı, Scatter'da saçma sayısını artırır. |
| Pierce Rifle | Hedefleri delerek ilerler; ön kalkanı aşar. Siper yine atışı durdurur. |
| Ricochet Gun | Yakındaki iki ek hedefe sekebilir; her bağlantıda siper kontrolü vardır. |
| Arc Launcher | İlk hedeften yakındaki iki hedefe elektrik zinciri. |
| Rocket Pod | Çarpışmada 0,35 saniye uyarı veren alan patlaması; oyuncu da yakındaysa etkilenir. |
| Flame Sprayer | 2,3 birimlik kısa konide, görüş hattındaki hedefleri yakar. |
| Cryo Beam | Hasar ve kısa yavaşlatma uygular; boss üzerindeki etki kısadır. |
| Orbit Drones | Oyuncunun çevresinde dönen iki drone üzerinden otomatik atış. |
| Nova Cannon | Birden çok hedefe geniş enerji atışı ve sınırlı itme. |

Hasar, atış hızı ve menzile ek olarak özel evrim 0–3 ve döngü 0–5 geliştirmeleri vardır. Özel evrim delinen/seken/zincirlenen hedef sayısını, roket yarıçapını veya yavaşlatma süresini geliştirir. Döngü seviyesi başına %4 atış döngüsü iyileşir. Kontrol için ikinci bir ateş tuşu eklenmez. L3 erişiminden itibaren başlangıç Pistol'ünün ilk hasar geliştirmesi ücretsizdir ve yalnız bir kez alınabilir.

Market 12 seçilebilir kartı yatay kaydırır; bölüm yolu 60 düğüm içerir. Mevcut görsel dil korunarak beş yükseltme satırı sığdırılmıştır. Yeni sekiz thumbnail gerçek yeni mesh'lerden render edilir. Market vitrini ayrıca parmakla döndürülebilen canlı 3D model gösterir; yalnız menüde, 10 FPS ile render edilir ve savaşta kapanır.

## Arenalar, tehlikeler ve özel bölümler

- Classic; gerçek 6×9 dar platform; çoklu kırılabilir siperler; ortası engelli iki koridor; uyarılı tehlike alanları; varil kümeleri; boss platformu.
- Elektrik, buz, diken ve lav alanları uyarı/aktif döngüleri taşır. Hasar alanları oyuncuyu ve düşmanları etkiler; buzda fren mesafesi uzar. Her yerleşimde alternatif kaçış alanı bırakılır.
- Kırılabilir ek siperler 24 can taşır. Oyuncu ve düşman lazerleri bütün siperlerde durur. Mevcut ana siper davranışı korunur.
- Bağımsız Damage Amp pickup'ı 4 saniye +%30 hasar; Magnet Core 3 saniye geniş altın toplama alanı verir. Kapı ödüllerinden ayrı çalışır.
- Survival Flip: görünür beklemeden sonra iki son dalga hedefi. Barrel Party: ek patlayıcılar. Elite Hunt: belirli mini-elite hedefi. Chain Reactor: üç korumalı enerji düğümünü sırayla kırma. Low Gravity: daha güçlü itme ve farklı hareket ağırlığı. Healing Stop: ek sağlık. Gauntlet: beş kısa ve hızlı çatışma.

Hiçbir mod 20 saniye kuralını kaldırmaz. Clear sırasında tehlikeler/hasar kapanır; kapıya yürüyüş güvenlidir. Dar platformun sağ kapısı ve köprüsü gerçek kenara taşınır, yeni platforma varış noktası da buna göre ayarlanır.

## Boss fazları

| Bölüm / boss | Gerçek faz hedefleri |
|---|---|
| L10 Crusher | Mevcut iki zırh paneli → üç destek → çekirdek. L7 giriş sürümü korunur. |
| L20 Furnace King | Yakınındaki üç varili patlat → ateş alanları arasında açılan çekirdeği vur → gövdeyi bitir. |
| L30 Motherboard Hydra | İki baş düğümü → iki ek baş düğümü → yan enerji sütunları → ana çekirdek. |
| L40 Glacier Warden | Buz kalkanı kaynakları → buz alanlarında iki minyon dalgası → çekirdek. |
| L50 Magma Titan | Taş kol kaynağı → görünür güvenli alanda toplam iki saniye kal → dört zayıf nokta → son gövde. |
| L60 Void Architect | İki portal kaynağı → klon kaynakları → üç enerji küpü → gerçek boss; gerçek hedef ölünce sahte hedefler kapanır. |

Kaynak fazlarında boss doğrudan vurularak atlanamaz. Faz başına kendi 20 saniyesi vardır; ortak boss canı geçişte dolmaz. Yeni bossların ilk faz bütçesi 72, orta fazları 84, son çekirdeği 96'dır; üç faz toplamı 252, dört faz toplamı 336 can taşır. Furnace'ın açık çekirdek penceresi ve Void'in son sahte hedefleri rota hesabında ek zaman yükü olarak bütçelenir. Bu bosslarda faz hedefini değiştiren ek elit kapısı sunulmaz; sekiz ödül kampanyanın diğer karşılaşmalarında devam eder.

## Ses, teknik yapı ve analitik

Altı özgün sentez müzik döngüsü ve son 5 saniyede artan gerilim katmanı vardır. Sekiz silah ile summon/teleport/kalkan/iyileştirme için 24 yeni SFX varyantı eklenir. Android haptik desenleri hafif vuruş, orta darbe ve çift güçlü fail olarak ayrılır; cihaz desteği yoksa oynanış değişmez.

Savaş kodu düşman genişletmesi, gelişmiş silahlar, arenalar, boss hedefleri ve pooling dosyalarına ayrılır. Normal düşman rig'leri, ölüm görselleri ve pickup'lar yeniden kullanılır; bosslar seyrek faz geçişlerinde oluşturulur. Mevcut mermi/VFX havuzları korunur. `CubeCombatClock` zaman sınırı ve tek sonuç sırasını; `AdvancedEnemyController` yeni düşmanların karar durumlarını; `WeaponRuntimeStats` geliştirme/atış hesabını; `TemporaryBuffController` pickup sürelerini sahneden bağımsız yürütür. `InvasionPrototype`, mevcut sahne referanslarını koruyan koordinatördür; görsel, çarpışma ve eski AI adaptörlerini bağlar.

Mevcut 256 olaylık tanılama kaydına ek olarak yerel 30 günlük JSONL arşivi tutulur; günlük üst sınır 2 MiB'dır. `Full Campaign / Export Balance Dashboard`, bölüm/silah filtresi, başlangıç/bitiş/fail/timeout, ortalama küp süresi ve CSV dışa aktarımı sunar. Kayıtlar ekonomi dosyası veya ağ servisine bağlı değildir. Harici analitik hesabı/backend kurulmadığı için veri internete gönderilmez; bu arşivden küresel retention iddiası çıkarılmaz.

## Kabul ve açık dış doğrulamalar

- `FullCampaignBudgetCheck.Run`: 60 bölüm × 12 silah × 16 seed; bütün aşamalar ve iki rota. <=20 saniye tahmini bütçe, normal tür sınırı, boss ritmi ve değişmeyen kaynak katalog.
- `FullCampaignCheck.Run`: eski profilin genişlemesi; yeni upgrade kayıtları; 12 kart / 60 düğüm / 5 upgrade; gerçek yeni silah isabetleri; Summoner/Phaser davranışı; tüm 60 görevde gerçek hedef kaynakları ve kapı geçişleri.
- Eski P0/P1/P2/P3, rota, Bombacı, destek, Gold Rush, Crusher ve basınç kontrolleri ayrıca regresyon için kullanılır.

Programatik tamamlamalar oyuncunun doğal kaçınma, nişan/konum ve karar süresini ölçmez. Gerçek Android cihazda 30/60 FPS, GPU, pil/ısı, haptik ve dokunma; farklı ekranlar; gerçek oyuncularla süre/ekonomi/KPI testi hâlâ saha doğrulamasıdır. Harici cihaz/oyuncu olmadan bunlar tamamlandı diye işaretlenmez. Yayın imzası ve mağaza dağıtımı ayrıca yapılmalıdır.

URP sürüm eşlemesi: [Unity Graphics resmi değişiklik kaydı — 17.4 / Unity 6000.4](https://github.com/Unity-Technologies/Graphics/blob/master/Packages/com.unity.render-pipelines.universal/CHANGELOG.md).

Sonuçlar: [QA kaydı](Validation/FullCampaign/QA.json), [market](Validation/FullCampaign/Market.png), [Hydra fazı](Validation/FullCampaign/Boss29.png), [yerel denge paneli](Validation/FullCampaign/Balance_Dashboard.html).

Derleme ortamı: URP 17.4 ve Android JNI modülü etkinleştirildi. Editör shader derlemesi için eksik macOS Metal toolchain kuruldu. Bir editör koşusunda Burst derleyicisi native olarak çöktü; editör regresyonları Burst JIT kapalı tekrarlandı. Son Android APK, Burst AOT açık derlendi ve üretilen ARM64 Burst kütüphanesi paket içinde doğrulandı.
