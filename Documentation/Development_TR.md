# GDD incelemesi ve P0 teslimi

Referans: Cube_Invasion_GDD_TR_v1.1.pdf, 24 Ekim 2026. Belge tasarım girdisi olarak okundu. v1.1 ekindeki 31–38. sayfalar, v1.0'ın otomatik sonraki küpe geçişini sağ/ileri seçimle değiştiriyor.

## Tasarım kararları

Çekirdek sözleşme: aktif savaşta 20.0 saniye, tamamlanmamış hedefte kesin çökme/fail. Başarı sonrası sayaç durur; oyuncu seçmeden geçiş yapılmaz. Her rota aynı savaş sayısını korur. Son küpte rota açılmaz.

## Kullanıcı revizyonu — mobil kapı geçişi

Bu kararlar GDD'nin önceki kontrol ve rota seçim kurallarının yerine geçer:

- Hedef platform Android; portrait sabit, otomatik ekran döndürme kapalı. Unity 6000.4.8f1, IL2CPP ve ARM64.
- Ekran joystick'i hareketi ve karakterin bakışını belirler. Otomatik ateş yalnız o yöndedir; düşmana otomatik dönülmez. Joystick bırakıldığında son bakış yönü korunur.
- Mermiler karakterin namlusundan ileri ilerler, yol üzerindeki ilk düşmana çarpar; arkadaki hedefe otomatik isabet etmez.
- Düşmanlar bitene kadar sağ/ileri kapılar kilitlidir. Temizlikten sonra kilit panelleri kalkar ve iki köprü açılır. Süre durmuşken joystick ile yürümeye devam edilir.
- Seçim, gerçek kapı açıklığından dışarı doğru geçişle yapılır. Ekranda rota seçim butonu veya yönü 0.2 sn tutma kuralı yoktur.
- Kapı üstündeki yazı rota verisinden gelir: +30 CAN veya +%15 HASAR / 2 KÜP. Ödül kapı geçildiği anda bir kez uygulanır; köprü sonuna varışta tekrar verilmez.
- Son küpte kapı oluşturulmaz; bölüm biter. Küp temizliği sonrası kapıya yürürken kenar güvenliği vardır; savaşta kapı dışındaki açık kenarlardan düşülebilir.

İlk adım P0: üç küp, iki düşman, tek silah, +30 can / +%15 hasar dalları. Rastgele harita yerine sabit ve tekrar üretilebilir yerleşimler tercih edildi. Düşman hareketi ve ileri mermi segmenti/isabetleri prototip simülasyonunda çözülür; NavMesh veya Rigidbody mermisi henüz kullanılmaz. Kamera sabit çapraz yönünü korur; joystick hareketi kameranın zemindeki ileri/sağ eksenlerine göre hesaplanır. Kapılar dünya +Z ve +X kenarlarında kalır.

Görsel yön: belgenin koyu boşluk, açık savaş zemini, kobalt robot, turkuaz ışık ve pembe tehlike paleti. Küplerin derin yan yüzeyleri, açık kenarları, düşman tepe uzantıları ve robot vizörü çalışma anında oluşturulur. Bunlar prototip modelleridir; bevel'lı final mesh, rig ve animasyon teslimi değildir.

## Kullanıcı revizyonu — arcade takip kamerası

Kamera karakterin sol üst çaprazından, zemine 45° eğim ve 45° yatay açıyla bakar. Ortografik görünüm kullanır. Hedef artık küp merkezi değil karakterin X/Z konumudur; Cinemachine 2.10.4 Brain + Virtual Camera + Transposer (WorldSpace, damping 0.18) ile hareket ve köprü geçişleri izlenir. Kadrajda karakterin önüne 1 birim alan bırakılır. Kamera karakter döndüğünde dönmez, dolayısıyla joystick'in ekran yönleri tutarlıdır. Düşüş sırasında aşağı takip etmez. Portrait görüş genişliği aspect oranına göre ayarlanır.

Android hedefli ayrı prototip kopyasında Play Mode kontrolleri ve 540×960 görsel kontrolü geçti; 45° açı ve köprü geçişinde karakter takibi doğrulandı. Ana proje editörde açık bırakıldı.

Ayarlar `ArcadeFollowCamera` bileşenindedir; tilt, heading, Cinemachine takip damping değeri, görüş genişliği ve ileri kadraj Inspector üzerinden düzenlenebilir.

## Kullanıcı revizyonu — kesintisiz küp akışı

Küp geçişinde dünya, sahne, karakter ve Cinemachine rig yeniden kurulmaz. İlk sağ/ileri adaylar 14 birim aşağıdan, 0.85 saniyelik ease-out yükselişle gelir; ikinci aday 0.15 saniye gecikmeli yükselir. Adaylar yerleşmeden kapılar açılmaz. Savaş erken biterse sayaç durur ve kapılar adayların yerleşmesini bekler.

Seçilen aday nesnesi korunup aktif küp olur. Karakter aynı nesne olarak 0.9 saniyelik köprü geçişini sürdürür. Geçişin %35'inde sonraki adaylar aşağıdan yükselmeye başlar. Seçilmeyen aday düşerek gider; eski aktif küp ise karakter yeni zemine ulaştıktan sonra (%80 geçiş noktasında) düşmeye başlar. Düşüş 1.1 saniye sürer, ardından yalnız ilgili küp nesnesi silinir. Can, güçlendirme ve cüzdan korunur. Yeni karşılaşma ve 20 saniyelik sayaç varışta başlar.

Android hedefli ayrı doğrulama kopyasında Play Mode kontrolleri geçti: aynı karakter/dünya/kamera/sahne kimliği, seçilen mevcut adayın korunması, geçiş sırasında aşağıdan yükselme, güvenli eski zemin kaldırma, düşüş tamamlanınca temizlik ve pause sırasında hareket/animasyonların donması. Sahne yükleme yalnız kullanıcının fail/clear ekranındaki açık tekrar oynama eyleminde kullanılır.

## Dosya sorumlulukları

- `Assets/CubeInvasion/Runtime/InvasionPrototype.cs`: P0 savaş, sayaç, hedefleme, geçiş, kapı geçişi, ödül ömrü, cüzdan ve geçici HUD.
- `Assets/CubeInvasion/Runtime/ArcadeFollowCamera.cs`: eğimli, sabit yönlü ve karakteri takip eden kamera.
- `Assets/CubeInvasion/Runtime/CubeRouteDefinition.cs`: düzenlenebilir rota ödülü, süre, risk ve karşılaşma sayıları.
- `Assets/CubeInvasion/Editor/PrototypeBuilder.cs`: sahne/veri üretimi ve portrait ayarı.
- `Assets/CubeInvasion/Editor/PrototypeSmokeCheck.cs`: gerçek Play Mode entegrasyon kontrolü.

Prototip eski koddan namespace ile ayrıldı. Eski sahneler taşınmadı. Eksik Packages manifesti eklendi; mevcut Cinemachine 2 API'si korunarak 2.10.4 çözümlendi. Gereksiz Visual Scripting bağımlılıkları temizlendi, runtime dosyalarının UnityEditor importları koşullu yapıldı. Paket manifesti ve lock dosyasının git'e girmesi için gitignore düzeltildi. Unity 6 editör geçişinin serileştirme güncellemeleri de projeye dahildir.

## Sınırlar / sonraki iş

1. Mobil cihazda dokunmatik kontrol, safe area, gerçek aspect oranları ve 60 fps profilini doğrula; geçici IMGUI yerine uGUI HUD/joystick kullan.
2. Monolitik P0 simülasyonunu LevelDirector, CubeTimer, GateTraversalController, BuffController, Wallet ve WeaponController bileşenlerine ayır; timeout sınırı, dokunmatik parmak takibi ve kapı açıklığı geçişleri için ayrıntılı deterministik testler ekle.
3. URP geçişini mevcut shader/material uyumluluğuyla birlikte yap; prefab modeller, bevel, hit feedback, gerçek animasyon, SFX ve haptik ekle.
4. İleri ateş mekaniğini koruyarak siperle mermi çarpışması, projectile pool, spawn telegraph, knockback ve Pusher/Sniper/Gunner/Bomber ekle.
5. Beş normal bölüm, dört silah, üç fazlı boss ve Gold Rush; ardından diğer rota ödülleri, risk/encounter director, progression ve analitik.

Şu an kapılar iki başlangıç ödülünü destekler. Kapı yazıları rota verisinden üretilir; yeni ödül türleri için simülasyon genişletilmelidir. Normal altın, P0'da fiziksel pickup yerine temizleme başına 10 olarak verilir. Tam meta ekonomi ve silah kayıt sistemi henüz yoktur. Yeniden deneme yalnız mevcut üç küplük bölümü başlatır.

## Doğrulama notları

6000.4.8f1 (f8b72d3d7343) ile Android platformuna geçiş ve script derlemesi başarılı (`CUBE_ANDROID_PORTRAIT_OK: Android`). Android aktifken gerçek Play Mode entegrasyon kontrolü de başarılı (`CUBE_P0_SMOKE_OK`).

Kontroller: Android hedefi, portrait, pause, joystick ile dönüş/son yönü koruma, ileri merminin öndeki hedefe vurup arkadakine vurmaması, gerçek mermiyle düşman temizliği, savaşta kilitli kapı, clear sonrası iki kapının açılması, yürürken donmuş sayaç, kapı yanında geçmenin rota seçmemesi, sağ kapıdan +30 can, ileri kapıdan iki küplük +%15 hasar, çift ödül engeli, köprü sonunda 20 sn reseti, timeout/fail ve son küpte bölüm bitişi.

Görsel kontrol 540×960 kamera çıktısıyla yapıldı. Mobil APK derlemesi ve gerçek cihazdaki dokunmatik/performans testleri henüz yapılmadı. Önceki çalışmalarda UnityEditor.Search başlangıç indeksleme hatası görüldü; son Android kontrolünde bu hata kaydedilmedi.

## P1 / Feel uygulaması

P0 üzerine ilk P1 uygulaması eklendi. Güncel kapsam, testler, editör performans örneği ve cihaz doğrulama sınırları [P1_Feel_TR.md](P1_Feel_TR.md) içinde. Yukarıdaki P0 notlarında geçen geçici IMGUI ve fiziksel pickup eksikleri bu uygulamada giderildi; tam oyun içeriği ve URP geçişi sonraki aşamada.

## Kullanıcı revizyonu — en yakın düşmana kilit

Joystick hareketi belirler; savaş sırasında karakter hareketten bağımsız olarak en yakın canlı düşmana döner ve otomatik ateş eder. Mesafe zemindeki X/Z düzleminde hesaplanır, görüş hattı kontrolü yapılmaz. Oyuncu mermileri siperlerden geçer; düşman mermilerinde siper davranışı korunur. Hedef ölünce veya başka düşman daha yakına gelince kilit güncellenir. Hedef yoksa ateş edilmez. Bu karar önceki joystick yönünde ateş kuralının yerine geçer.

## P2 öncesi 3D görsel set

Ana primitive placeholder'lar 26 paylaşılan mesh ve 9 inceleme prefab'ından oluşan özel bevel'lı toy sci-fi setle değiştirildi. Kapsam: Scout robot, Pulse Pistol, Grunt/Sprinter/Gunner, arena zemin/yan panelleri, siper, varil, sağlık/altın ve kapı parçaları. `ToyArtLibrary` çalışma başlangıcında Resources'tan yüklenir; sahne ve rota varlıkları yeniden yazılmaz. Önceki düşük seviyeli geometriler yalnız kütüphane bulunamazsa fallback olarak kalır; VFX primitive'leri korunur.

Android hedefli ayrı kopyada P0/P1 kontrolleri ve mesh bağlantı kontrolleri geçti. 540×960 oyun görünümü ve ayrı model kataloğu incelendi. [Katalog](Validation/ToyArt_Catalog.png), [oyun görünümü](Validation/ToyArt_Gameplay.png), [mesh bütçesi](../Assets/CubeInvasion/Art/MeshBudget.csv). Animasyon rig'leri ve gerçek Android cihaz performans testi bu aşamada yoktur.

## DOTween animasyon katmanı

Dört robotun mesh'leri 16 ayrı rig parçasına bölündü; toplam geometri değişmeden parçaların pivotları eklendi. DOTween 1.3.045 ile idle/walk/run/strafe ve saldırı/hasar/doğuş/ölüm tepkileri bağlandı. Küp yükselme/düşme ve pickup'lar da DOTween kullanır. Mevcut karakter root'u, hedef kilidi, sahne, rota ve Cinemachine sistemi korunur. Pause ve destruction cleanup testleri geçti. [Ayrıntılar](DOTween_Animation_TR.md).

### Kenar düşüşü ve sahne kurulumu
- Kenardan çıkınca yalnızca oyuncu 1,15 saniyelik düşüşü tamamlar, ardından fail görünür; aktif küp yerinde kalır. Can bitişi de küpü çökertmez. Süre bitişi aktif küp ile oyuncuyu birlikte düşürür. Başarılı kapı geçişlerindeki eski küpün emekliye ayrılma animasyonu korunur.
- `CubeInvasionPrototype.unity` artık kamera, Cinemachine Brain/Virtual Camera ve takip hedefi, ışık, oyuncu görselleri, Canvas/HUD/joystick ve EventSystem içerir. Runtime bu kayıtlı referansları kullanır. Küpler, düşmanlar ve ödüller `Dynamic gameplay` altında oyun sırasında oluşturulur.
- Kamera açısı ve yumuşatma ayarları `Prototype Camera > ArcadeFollowCamera` üzerinden düzenlenir. Mevcut sahne kurulumunun transform ve UI düzeni Play sırasında yeniden oluşturulmaz.
- Eksik kurulumu mevcut sahneyi sıfırlamadan eklemek için `Cube Invasion > Install Persistent Scene Setup` kullanılabilir.

## P4 — Validate

İlk oyun yönlendirmeleri, yerel sınırlı playtest olayları, UI yenileme optimizasyonu, arka plan pause güvenliği, içerik/denge kontrolü ve Android Development ölçüm desteği eklendi. Kurulum, test sonuçları ve gerçek cihaz/oyuncu testi sınırları için [P4 doğrulama notları](P4_Validation_TR.md).

## Referans UI yeniden tasarımı

Verilen örneklere göre ana bölüm yolu, kaynak şeridi, ikon dock, silah/pilot/ayar yerleşimleri ve HUD/sonuç modalı yeniden kuruldu. [Tasarım, varlıklar ve kontroller](UI_Reference_Redesign_TR.md).

## GDD v1.1 — Dallanan rota sistemi

Sekiz ödül, seçilen kapıya bağlı karşılaşma, risk/tehdit önizlemesi, süreli güçlendirmeler, koşullu altın ve yerel rota olayları eklendi. Seçim yürüyerek yapılır; sahne, karakter ve kamera sürekliliği korunur. Tasarım ayarları, doğrulama ve kapsam sınırları: [Dallanan küpler](Branch_Routes_TR.md).

## GDD — Crusher faz hedefleri

Void Toy görsel seti korunarak iki ayrı zırh hedefi, üç desteğin koruduğu boss ve açık çekirdek fazı bağlandı. Otomatik hedefleme, itme konisi, ortak can bütçesi ve rota sürekliliği güncellendi. [Mekanikler, kurulum ve doğrulama](Crusher_Phases_TR.md).

## GDD — Gold Rush varyantları

Mevcut Coin Sprint'e altı kasa kırma, beş altın taşıyıcısı ve üç anahtarlı Jackpot hedefleri eklendi. Son iki varyant kapılara ayrı ayrı bağlandı; otomatik hedefleme kasaları destekler. [Kurulum, ekonomi ve kontroller](GoldRush_Variants_TR.md).

## GDD — Bombacı karşılaşmaları

Son Hat'ın iki karşılaşmasına Bombacı eklendi. Ölüm sonrası uyarılı patlama, zincirleme hasar, dört silahla isabet ve hedef tamamlanınca güvenli temizlik bağlandı. Görsel/katalog/kayıt düzeni korunur. [Davranış, ayarlar ve kontroller](Bomber_Encounters_TR.md).

## GDD — Kalkanlı ve Tamirci

Son Hat'ın son iki küpüne yönlü kalkan ve kesilebilir iyileştirme eklendi. Mevcut görseller, kamera kurulumu ve Bombacı küpleri korundu. [Davranış, süre bütçesi ve kontroller](Support_Enemies_TR.md).

## GDD — 60 bölüm / tam kampanya genişletmesi

60 bölüm, 12 silah, Summoner/Phaser/davranış birleşimli Elite, beş ek boss, çevresel tehlikeler ve özel bölüm kuralları eklendi. Eski yedi görev ve kayıt indeksleri korunur. Yeni katalog, sanat, ses, URP kurulumu, doğrulamalar ve saha testi sınırları: [Genişletilmiş kampanya](Full_Campaign_TR.md).
