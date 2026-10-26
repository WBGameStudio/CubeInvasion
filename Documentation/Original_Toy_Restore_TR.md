# İlk Toy Sci-Fi modellerine dönüş

İlk üretilen köşeli, düşük poligonlu Toy Sci-Fi seti tekrar aktif edildi. Cute veya Prism seti aktif değildir; bu varlıklar silinmedi.

- Aktif kütüphanede oyuncu, düşmanlar, dört silah, siper, varil, sağlık kutusu, arena ve kapılar dahil 33 özgün modelin ilk mesh, materyal ve rig bağlantıları geri yüklendi.
- P2 düşmanlarının ve ek silahlarının ilk sürümleri de geri geldi.
- Sahnedeki oyuncu, silah ve ayrı vizörün özgün üç materyalli kurulumu geri bağlandı; vizör yeniden görünür.
- Ana menü ve marketteki Scout/dört silah küçük resimleri gerçek eski modellerden yeniden renderlandı. Diğer UI tasarımı ve yerleşimi korundu.
- DOTween animasyonları eski rig'lerle devam ediyor. Eski Gunner'ın ayrı namlusu ve Sniper'ın gövdeye bağlı silahı için lazer parlamasının bağlantısı uyarlandı.
- Yeni lazer atışları, namlu parlamaları, barikat çarpışmaları, zorluk paketi, joystick ateş ritmi, kayıtlar ve yükseltmeler korunuyor.
- Kamera, oyuncu konumu/ölçeği ve UI transformları değiştirilmedi. Sahne karşılaştırmasında hiçbir transform veya kamera bileşeni değişmedi.

Özgün eşlemeler `Assets/CubeInvasion/Art/OriginalToyLibrary.asset` dosyasında saklanır. Aktif kütüphane `Resources/ToyArtLibrary.asset` dosyasıdır. Tekrar bağlama menüsü: `Cube Invasion > Art > Restore Original Toy Models`. Bu araç, özgün varlıkları yeniden modellemez veya diğer setleri silmez; mevcut özgün mesh/materyalleri tekrar bağlar ve küçük resimleri yeniler. Özel ek kütüphane anahtarları korunur.

Doğrulama: runtime/Editor C# derlemesi, eski rig'lerle idle/yürüme/koşu/geri tepme, pause, kamera, küp geçişi, karakter düşüşü, lazer görselleri, twin/scatter namlu efektleri, barikat/varil çarpışma sırası, tek parmak ateş ritmi ve siper çevresinde ilerleme kontrolleri geçti.

[Oyun içi görünüm](Previews/OriginalToy/Gameplay.png) · [Eski modellerle yeni lazerler](Previews/OriginalToy/Laser_Fire.gif). Gerçek Android cihaz testi henüz yapılmadı.

Menü/market/yol/onboarding kontrolleri ve Android development derlemesi de geçti. APK: `CubeInvasion-OriginalToy.apk` (58.6 MiB).
