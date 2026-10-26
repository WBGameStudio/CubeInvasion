# Void Toy Arcade — yeni 3D model ve arena seti

Aktif model kütüphanesi, tasarım tuvalindeki "Void Toy Arcade" diline göre yeniden üretildi (artboard'lar "10 · Karakterler ve silahlar" ve "11 · Küpler, çöküş, biyomlar"). Kamera, sahne yerleşimi, kontrol, karşılaşma, profil ve UI akışı korunur. Original Toy, Cute ve Prism varlıkları silinmedi.

## Tasarım

- Kural: oyuncu açık renkli, parlak plastik. Düşmanlar tek bir grafit (#3A4361) gövde ailesi; aralarındaki farkı silüet ve tek bir imza uzantı taşır. Niyet ışığı emisyonlu: pembe = yakın dövüş, turuncu = menzilli.
- Scout Mk-II: kobalt küp gövde, beyaz küre kafa, koyu vizör ve turkuaz gözler, göğüste turkuaz çekirdek, kafada anten topu. Silah sağ elde; mermi çıkış ankrajı rig'e bağlı değil.
- Grunt: kalın küp, köşelere sarılan chevron vizör, kısa boynuzlar, tepede pembe ışık.
- Sprinter: ince, öne eğik gövde, kızak ayaklar, sırt yüzgeci ve iki pembe kulak kanadı.
- Pusher: önde büyük kalkan levha; pembe çizgiler şok konisinin çıktığı yeri gösterir. Yanlarda göz yarıkları.
- Bruiser: geniş omuz pedleri, küçük kafa, tek dev yumruk.
- Sniper: dört ince bacak, uzun namlu, turuncu lens ve namlu ucu.
- Gunner: alçak, dört ayaklı taret, çift namlu, turuncu gövde şeritleri.
- The Crusher: iki pembe çerçeveli zırh paneli (ayrı mesh), pembe çekirdek küre, altın taç, büyük sağ yumruk.
- Silahlar: beyaz gövde, kobalt üst plaka, koyu kabza, aile renginde namlu ve yan şerit. Pulse Pistol turkuaz, Twin Blasters kobalt, Scatter Shot turuncu geniş ağız, Rapid SMG mor bobin halkaları.
- Arena kiti: açık zemin (#C9D6EA) 4×4 karo ızgarası (2,25 m), koyu yan yüzler (#22315A) ve panel detayı, ayrı "Edge light" şeridi. Siper, patlayıcı varil ve bandı, sağlık kutusu ve haçı, altın, elite tacı, kapı direkleri/başlığı, kilit bariyeri, köprüler ve ödül işareti de aynı pah dilinde.

Bunlar konsept resimler değil, Unity içinde koddan üretilmiş gerçek mesh, material ve prefab varlıklarıdır: `Assets/CubeInvasion/Art/VoidToy`. Tüm set iki materyal kullanır (`VoidToy_Plastic`, `VoidToy_Tinted`). Renkler vertex renginde (lineer uzay), emisyon vertex alfasında, çalışma anında renklenen bölgeler UV0.x maskesindedir. `CubeInvasion/Void Toy Plastic` shader'ı kendi ışığını hesaplar; URP'de ve önizleme sahnelerinde aynı izometrik "üst açık, yanlar kademeli" görünümü verir. Palet `Runtime/VoidToyPalette.cs` token'larından okunur.

## Çalışma anı bağlantıları

- Kütüphane anahtarları değişmedi; `InvasionPrototype.Arena()`, kapılar, köprüler ve pickup'lar kod değişmeden yeni modelleri alır.
- `Edge light`, ödül işareti, kapılar, kilit bariyeri ve köprüler `useRuntimeTint` ile gelir: `Shape()` ve geri sayım (`TintArenaEdges`) `_Color` verdiğinde yalnız maskeli parçalar boyanır (turkuaz → 5 sn turuncu → 3 sn pembe).
- Kapı direkleri `RouteView` tarafından ödül rengindeki `Mat(tint)` ile yeniden boyanır; bu durumda yalnız silüet bu setten gelir.
- Düşman vurulma parlaması Core parçasında `_Color` alfası ile çalışır.
- Düşman namlu parlaması: Sniper ve Crusher `DOTween Reactions/RightHand`, Gunner `DOTween Reactions/Head/RightHand` eklemini kullanır. Bunun için `InvasionPrototype.Laser.cs` aramasına iki yol eklendi.
- `ToyActorAnimator`: adı `Armor` ile biten eklemler sabit kalır. Boss zırh panelleri gövdeyle birlikte tepki verir ama sallanmaz; ileride kırılınca düşürülebilir.

## Animasyon

DOTween'le çalışan mevcut mekanik rig kullanılır; skinned mesh yoktur. Scout 7 parçalıdır: Core, Head, VisorEye, LeftArm, RightHand (silahlı kol, geri teper) ve iki bacak. Düşmanlar 5–9 parça: bacaklar adımlar, `*Eye` eklemleri göz kırpar, `Ear`/`Tail` sallanır, `*Hand` hazırlıkta kalkar ve ateşte geri teper. Kök hareketi yoktur; mermi çıkış ankrajı sabittir.

## Geometri bütçesi

| Model | Üçgen | Bütçe | Rig parçası |
| --- | ---: | ---: | ---: |
| Scout Mk-II | 1.352 | 3.000 | 7 |
| Grunt | 528 | 3.000 | 5 |
| Sprinter | 616 | 3.000 | 7 |
| Gunner | 616 | 3.000 | 8 |
| Pusher | 440 | 3.000 | 6 |
| Sniper | 440 | 3.000 | 7 |
| Bruiser | 572 | 3.000 | 8 |
| The Crusher | 1.168 | 6.000 | 9 |
| Pulse Pistol / Twin / Scatter / Rapid | 264 / 396 / 308 / 440 | 600 | — |
| Deep sides / Arena surface | 792 / 748 | 1.200 | — |
| Diğer props | 44–352 | 600 | — |

Bir actor, rig parçası kadar render parçası çizer. Tüm parçalar aynı materyali paylaşır; GPU instancing açıktır. LOD üretilmedi; bu bütçelerde gerekmiyor. Tam döküm: `Assets/CubeInvasion/Art/VoidToy/MeshBudget.csv`.

## Yeniden üretim ve geri dönüş

Unity 6000.4.8f1, `Cube Invasion > Art > Void Toy` menüsü:

- `Build Void Toy Models`: mesh, rig, materyal, prefab ve `VoidToyLibrary.asset` anlık görüntüsünü üretir; aktif kütüphaneye dokunmaz.
- `Build And Install Void Toy Models`: üretir, ardından `Resources/ToyArtLibrary.asset` içindeki 31 anahtarı değiştirir. Sahnedeki oyuncu ve silah mesh'lerini değiştirir, ayrı vizörü gizler. Kamera, transform ve UI değişmez.
- `Reinstall Built Void Toy Models`: anlık görüntüden yeniden kurar.
- `Restore Original Toy Models`: `OriginalToyRestore` ile ilk Toy setine döner, vizörü açar ve Void Toy'a özgü `Elite crown` anahtarını kaldırır. Cute ve Prism setlerinin kendi kurulum menüleri değişmedi.
- `Render Void Toy Model Catalog`, `Render Void Toy Arena Kit`, `Render Void Toy Animation Review`: gerçek Unity renderları.

Batch: `-executeMethod VoidToyArtBuilder.BuildInstallAndReview`.

Model doğrulaması şunları kontrol eder: üçgen bütçesi, normal ve vertex renkleri, emisyon ışığı, renk maskesi, rig geometrisinin eksiksizliği, göz kırpma eklemi, Scout'un 7 parçası ve boss'un iki ayrı zırh paneli. Animasyon incelemesi şunları doğrular: sekiz karakterde göz kırpma ve bacak hareketi; sabit kök, mermi ankrajı ve zırh paneli; namlu eklemleri. `PrototypeSmokeCheck` animasyon incelemesi, Void Toy aktifken bu incelemeyi çalıştırır.

Önizlemeler: [model kataloğu](Previews/VoidToy/Model_Catalog.png), [arena kiti](Previews/VoidToy/Arena_Kit.png), [animasyon kareleri](Previews/VoidToy/Animation_Review.png).

## Kampanya genişlemesi turu

Genişleme içeriğindeki ödünç modeller Void Toy diliyle değiştirildi. Beş yeni boss bu turun dışında.

- **Özel düşmanlar** (temel kütüphane): Shielder (önde büyük kalkan levha, mavi yay, pembe göz şeridi), Healer (mor halkalı anten, mor sırt ışığı ve vizör) ve Bomber (kadrodaki tek küre gövde, turuncu bel bandı, fitil topu). `InvasionPrototype.Content.cs` bu anahtarlar kütüphanede rig'li olarak varsa Grunt/Pusher yerine onları kullanır. Healer'ın havada duran sağlık haçı ve Bomber'ın sırt varili yalnız özel model yokken eklenir.
- **Tehlike karoları**: Spike tile (ayrı yükselen diken mesh'i ile), Electric coil, Lava tile ve Void Toy "Frozen patch". `InvasionPrototype.HazardTiles.cs` her `HazardZone` altına karoyu yerleştirir. Uyarı halkası korunur. Durum ışık şeritlerinin rengiyle okunur: bekleme sönük, uyarıda yanıp söner, aktifken tam parlak. Elektrik aktifken titrer; dikenler uyarıda biraz görünür, aktifken tam çıkar. `Arenas.cs` içine yalnız iki çağrı satırı eklendi.
- **Gold Rush**: altın kasa ve üç anahtar kilitli jackpot sandığı. `InvasionPrototype.GoldRush.cs`, anahtarlar kurulu değilse "Low cover" ile devam eder. Coin rozetleri ve anahtar sayacı aynı kalır.
- **Genişleme silahları ve düşmanları**: 8 silah (beyaz gövde, kobalt üst plaka, `CampaignExpansionSettings.Colors` aile renginde namlu ve şerit), Summoner (camgöbeği çağırma çanağı), Phaser (mor faz halkaları, turuncu menzilli namlu) ve Elite (altın taç, pembe vizör, omuz pedleri). Bu anahtarlar `Art/Expansion/ExpansionArt.asset` içinde yerinde değiştirilir; çalışma anında temel kütüphaneden sonra birleştirildiği için onlar geçerli olur. Sıra korunur. Market küçük resimleri `CampaignExpansionBuilder.RefreshWeaponIcons` ile yeni modellerden yeniden renderlanır.

| Model | Üçgen | Rig parçası |
| --- | ---: | ---: |
| Shielder / Healer / Bomber | 660 / 1.044 / 704 | 5 / 7 / 5 |
| Summoner / Phaser / Elite | 616 / 968 / 704 | 7 / 5 / 7 |
| Altın kasa / jackpot sandığı | 452 / 396 | — |
| Spike tile (+ dikenler) / Electric coil / Lava tile / Frozen patch | 616 (+792) / 712 / 396 / 176 | — |
| Genişleme silahları | 308–496 | — |

Geri dönüş: `Restore Original Toy Models`, Void Toy'a özgü anahtarları temel kütüphaneden kaldırır. İlk kurulumda alınan `Art/VoidToy/ExpansionArtBackup.asset` yedeğiyle ExpansionArt kayıtlarını geri koyar ve market küçük resimlerini yeniler. Not: `CampaignExpansionBuilder.RefreshActors` çalıştırılırsa Summoner/Phaser/Elite eski genişleme modellerine döner. Void Toy için `Reinstall Built Void Toy Models` yeterli.

Önizleme: [genişleme kataloğu](Previews/VoidToy/Expansion_Catalog.png).

## Açık noktalar

- Rapid SMG mermisi P2 içeriğinde yeşil tanımlı; namlusu tasarımdaki mor ailede. Mermi rengi ayrıca eşitlenmeli.
- Telegraph sırasında niyet ışığının parlaması ve boss zırhının kırılınca düşmesi runtime işi olarak bekliyor. Modeller buna hazır: emisyon vertex alfasında, zırh ayrı eklemde.
- Gerçek Android cihaz testi yapılmadı.
