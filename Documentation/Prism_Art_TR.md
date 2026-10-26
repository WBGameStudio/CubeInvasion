# Prizma Muhafızları — yeni 3D model seti

Oyundaki aktif model kütüphanesi, yuvarlak hayvan robotları yerine seramik zırhlı, küp biçimli mekanik muhafızlarla değiştirildi. Mevcut kamera, sahne yerleşimi, kontrol, karşılaşma, profil ve UI akışı korunur. Eski Cute varlıkları silinmedi.

## Tasarım

- Scout: mavi/beyaz seramik zırh, siyah ekran yüzü, LED gözler, göğüs reaktörü ve sırt iticileri.
- Grunt: kırmızı standart muhafız; Sprinter: ince sarı gövde ve geriye yatık kanatlar.
- Gunner: dört bacak, omuz üstü çift namlu; Pusher: iki enerji kalkanı.
- Sniper: uzun hassas atış kolu ve arka dengeleyici; Bruiser: büyük enerji yumrukları.
- Crusher: geniş ağır zırh, büyük eldivenler, omuz reaktörleri ve üç parçalı altın taç.
- Dört silahın gövde, enerji hücresi, namlu bilezikleri ve renkleri yeniden modellendi. Siper, patlayıcı kapsül ve sağlık kutusu da yenilendi.

Bunlar konsept resimler değil, Unity içinde üretilmiş gerçek mesh, material ve prefab varlıklarıdır. Yüzeylerde analitik yumuşak normaller, yuvarlatılmış kenarlar, tek materyalli vertex renkleri ve hafif enerji parlaması kullanılır. Modeller `Assets/CubeInvasion/Art/Prism` içindedir. Ana menü ve silah marketinin model küçük resimleri de gerçek modellerden yeniden renderlandı.

## Animasyon

DOTween ile bağımsız baş, göz, kol, el, uyluk ve diz parçaları hareket eder; Gunner dört bacaklıdır. Bu set skinned FBX veya humanoid Animator klipleri yerine hiyerarşik mekanik parça rig'i kullanır.

- Idle: küçük gövde nefesi, baş hareketi ve göz kırpma.
- Yürüyüş/koşu: dönüşümlü adımlar, bükülen dizler, ayak kaldırma, gövde eğimi ve kol salınımı; duruşlar arasında yumuşak geçiş.
- Ateş: el/bilek geri tepmesi, ateş ederken daha az kol salınımı; görünen silah sağ ele bağlıdır.
- Saldırı: hazırlık çömelmesi, kolları germe, darbe ve toparlanma.
- Hasar/spawn: kısa esneme ve giriş animasyonu.
- Ölüm: görsel zırh devrilip küçülerek temizlenir. Oynanış kökü hemen kaldırılabilir.

Mermi çıkış ankrajı animasyon hiyerarşisine taşınmaz. Silah değiştirilince eldeki mesh ve materyal güncellenir. Pause animasyon saatlerini dondurur; root motion uygulanmaz.

## Geometri ve mobil sınırlar

| Model | Yakın üçgen | LOD1 üçgen | Rig parçası |
| --- | ---: | ---: | ---: |
| Scout | 62.352 | 22.300 | 12 |
| Grunt | 58.736 | 21.076 | 12 |
| Sprinter | 63.536 | 22.804 | 14 |
| Gunner | 78.720 | 28.168 | 16 |
| Pusher | 66.576 | 24.068 | 12 |
| Sniper | 64.752 | 23.248 | 13 |
| Bruiser | 70.928 | 25.364 | 12 |
| Crusher | 84.128 | 30.116 | 12 |

Karakter rig'leri çalışma anında iki seviyeli LODGroup kullanır: ekrandaki göreli yükseklik %8 altına inince LOD1, %1,2 altında culling. LOD1 üçgenleri yaklaşık %64 azaltır; parça sayısını/draw call sayısını azaltmaz. Yüksek modelde karakter başına 12–16 render parçası vardır. Silahlar 14.656–21.152 üçgendir; hafif mesh'leri de üretilmiştir ancak aktif eldeki silahta otomatik LOD uygulanmaz. Prop modeller tek mesh'tir. Tam döküm: `Assets/CubeInvasion/Art/Prism/MeshBudget.csv`.

Bu değerler cihazda 60 FPS garantisi değildir. Önizleme ve Editor performans kaydı Apple M3 üzerinde alındı; gerçek Android cihaz ölçümü ayrıca gerekir.

## Yeniden üretim ve kontrol

Unity 6000.4.8f1 menüleri:

- `Cube Invasion > Art > Build Prism Vanguard`: mesh, rig, LOD, materyal, prefab ve kütüphane üretir.
- `Install Prism Vanguard In Scene`: açık oyun sahnesindeki oyuncu/silah görsellerini değiştirir; kamera ve gameplay transformlarını korur.
- `Refresh Prism Menu Thumbnails`: ana menü/marketin ilgili küçük resimlerini yeniler.
- `Render Prism Model Catalog` ve `Render Prism Animation Review`: gerçek Unity renderları üretir.

Otomatik animasyon incelemesi sekiz karakterde göz kırpma, diz hareketi, iki LOD seviyesi, silah değişimi ve değişmeyen kök/mermi çıkış transformlarını doğrular. Model doğrulaması üçgen bütçesi, normal/vertex renkleri, eksiksiz rig ve LOD geometri eşleşmesini kontrol eder.

Önizlemeler: [model kataloğu](Previews/Prism/Model_Catalog.png), [animasyon](Previews/Prism/Animation_Review.gif), [oyun içi](Previews/Prism/Gameplay.png). Editor performans kaydı: [JSON](Previews/Prism/Editor_Performance.json).

## Tamamlanan doğrulamalar

- Unity 6000.4.8f1: runtime ve Editor C# derlemesi başarılı.
- Gerçek Unity renderları: sekiz modelde 144 karelik idle/yürüme/koşu/ateş/saldırı/hasar incelemesi başarılı. Kök ve ateş ankrajları sabit; eklem hareketleri, silah değişimi ve LOD geometri bütünlüğü doğrulandı.
- P0/P1: animasyon, pause, takip kamerası, kesintisiz küp geçişi, ölüm temizliği ve karakterin kenardan düşmesi kontrolleri geçti.
- P2: yedi görevin her iki rota üzerinden tamamlanması, dört silah, saldırı hazırlıkları, kota, boss fazları ve retry kontrolleri geçti.
- UI: ana menü, uzun portrait ekran, kaydırılan yol, silah satın alma/geliştirme, karakter geliştirme, ayarlar, onboarding ve sonuç akışı kontrolleri geçti.

Kontroller izole proje kopyasında yapıldı; kullanıcının oyun profiline dokunulmadı. Editor başlangıcında Unity Search veritabanından gelen mevcut `ArgumentOutOfRangeException`, bu doğrulamaları durdurmadı.

Android development APK derlemesi başarılı: `CubeInvasion-PrismModels.apk` (46.0 MiB). APK fiziksel Android cihazda henüz denenmedi.
