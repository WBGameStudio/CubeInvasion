# Cute Space Toys — 3D model ve animasyon yenilemesi

Oyuncu ve yedi düşman, büyük gözlü, yumuşak yüzeyli, yuvarlak oyuncak astronotlar olarak yeniden modellendi. Modeller gerçek Unity mesh asset'leridir; billboard veya model yerine kullanılan görseller değildir. Eski sanat dosyaları korunur; aktif `ToyArtLibrary` yeni modellere bağlanır.

## İçerik

- Scout: mavi gövde, krem yüz çerçevesi ve botlar, turkuaz enerji çekirdeği.
- Grunt: mercan pembe kısa antenli temel düşman.
- Sprinter: sarı, uzun ve hareketli tavşan kulaklı hızlı düşman.
- Gunner: mor, dört ayaklı ve gövdeye bağlı top taşıyan düşman.
- Pusher: turuncu, iki büyük oval avuç kalkanı taşıyan düşman.
- Sniper: mint yeşili, uzun namlulu ve kuyruklu düşman.
- Bruiser: pembe, ayı kulaklı ve pençe detaylı eldivenli düşman.
- Crusher: indigo, taç ve büyük yumrukları olan geniş boss.
- Dört oyuncak silah yeniden modellendi; siper, sağlık kavanozu, altın, patlayıcı kutu ve kapılar dahil 21 çevre/detay modeli yuvarlatıldı.
- Menüdeki oyuncu/silah görselleri yeni gerçek prefab'lardan render edildi; mevcut UI yerleşimleri korunur.

## Asset'ler ve düzenleme

`Assets/CubeInvasion/Art/Cute/` altındaki `Prefabs`, `Meshes`, `Materials` ve `Thumbnails` klasörleri aktif dosyaları içerir. Her karakter prefab'ı düzenleme sırasında birleşik mesh gösterir; Play sırasında `ToyActorAnimator` ayrı parçalı rig oluşturur. Rig, kemikli/skinned bir FBX değil; birbirine bağlı baş, göz, kulak, kol, bacak ve varsa kuyruk parçalarından oluşur. Kaynak geometri `Editor/CuteArtGeometry.cs` ve `CuteMeshBuilder.cs` ile düzenlenebilir.

Unity menüsü: `Cube Invasion > Art`.

- `Build Cute Space Toys`: mesh, materyal ve prefab'ları tekrar üretir. Daha önce üretilmiş dosyaların GUID'lerini korur; bu dosyalardaki elle yapılmış mesh/prefab değişikliklerini yeniden üretir.
- `Install Cute Space Toys In Scene`: üretir ve açık sahnedeki oyuncu/silah bağlantılarını günceller.
- `Refresh Cute Menu Thumbnails`: yeni 3D render'ları mevcut görsel alanlarına bağlar.
- `Render Cute Model Catalog` / `Render Cute Animation Review`: yerel Unity render önizlemelerini `/tmp` altına yazar.

Üretim komutları bilinçli çalıştırılmalıdır. Mevcut kamera konumu, authored transform'lar, atış başlangıcı ve UI yerleşimini yeniden kurmazlar.

## Animasyon

DOTween ile idle nefes alma ve baş hareketi, göz kırpma, yürüyüş/koşuda zıt bacak salınımı ve ayak kaldırma, kulak/kuyruk takibi, ateş geri tepmesi, saldırı hazırlığı, slam, hasar squash ve spawn hareketleri uygulanır. Önceki ölüm küçülme animasyonu yeni rig üzerinde çalışır. Gameplay kökü animasyon tarafından taşınmaz. Tüm hareket saatleri manuel ilerler; pause sırasında donar.

`Previews/Cute/Cute_Actors_Animation.gif` sekiz karakteri, `Scout_Animation.gif` oyuncuyu gösterir. Bunlar Unity'nin gerçek mesh render karelerinden hazırlanmıştır.

## Geometri ve mobil bütçe

Karakterler yaklaşık 27.800–36.700 üçgen; silahlar yaklaşık 8.900–11.400 üçgendir. Kesin değerler `Art/Cute/MeshBudget.csv` dosyasındadır. Renk detayları vertex palette taşınır; rig parçası başına tek materyal/submesh kullanılır. Görünür karakter başına 10–12 parça çizilir; dinamik gölgeler ek geçişler oluşturabilir. Bu sürümde otomatik LOD yoktur.

Mac editöründe 180 karelik örnekte ana thread P95 16,70 ms görüldü; bu Android cihaz performansı veya en yoğun sahne garantisi değildir. Gerçek Android cihazda termal/GPU ölçümü henüz yapılmadı.

## Doğrulama

- Unity 6000.4.8f1 C# runtime ve editor derlemesi.
- Geometri: tek submesh, sonlu normal/palette, rig ile birleşik mesh üçgen sayısı eşleşmesi ve bütçe kontrolü.
- P0/P1 oyun testleri: joystick, en yakın hedef, kapılar, kesintisiz geçiş, pause, timeout ve yalnız oyuncunun kenardan düşmesi.
- Animasyon testleri: yürüyüş/koşu, kökün sabit kalması, pause ve nesne silinince tween temizliği; sekiz rig için göz kırpma ve saldırı/darbe önizlemesi.
- P2: yedi görevin iki rotası, dört silah, telegraph, kota, boss ve retry.

Grafikli batch editör başlangıcında Unity Search veritabanından mevcut bir `ArgumentOutOfRangeException` uyarısı görülebiliyor. Ayrıca bu Mac'te Xcode Metal komut satırı araçlarının eksikliği loglanıyor; gerçek mesh önizlemeleri render edildi. Menü testleri (`CUBE_UI_SMOKE_OK`, `CUBE_REFERENCE_UI_SMOKE_OK`, `CUBE_P4_GUIDANCE_SMOKE_OK`) ve Android development APK derlemesi (`CUBE_P4_ANDROID_BUILD_OK`) başarılıdır. Yeni shader için Android derlemesinde shader/C# hatası yoktur. Test paketi proje kökünde `CubeInvasion-CuteModels.apk` olarak bulunur; APK yaklaşık 39 MB boyutundadır. APK gerçek telefonda çalıştırılmadı.
