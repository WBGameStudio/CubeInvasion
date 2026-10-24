# Cube Invasion — Toy Sci-Fi 3D kit

P2 öncesi mevcut oynanışa bağlanan özgün, kodla modellenmiş düşük poligonlu görsel set. Bu klasörde gerçek Unity Mesh, Material ve Prefab varlıkları bulunur; 2D görsel veya ekran maketi değildir. Blender/FBX kaynağı yoktur; dört robot modelinde DOTween ile sürülen rigid-part transform rig bulunur.

## İçerik

26 temel mesh ve 16 rig parçası (toplam 42 Mesh asset): zırhlı Scout gövdesi, vizör, Pulse Pistol, Grunt, Sprinter, Gunner, tehdit anteni, top namlusu, arena gövde/zemini, siper, varil/bant, sağlık çantası, altın, kapı/köprü/ışık parçaları. 9 prefab ana modelleri renkleri ve aksesuarlarıyla birleştirir. Dört robot prefab’ı Play Mode’da otomatik rig bağlar; poz animasyonları görseldir, root motion üretmez. Bunlar görsel prefab'lardır; düşman AI ve oyuncu kontrolü mevcut InvasionPrototype tarafından yürütülür.

- Scout: kobalt zırh, turkuaz vizör/çekirdek, sırt ünitesi, eller, ayaklar, anten ve silah.
- Grunt: pembe geniş gövde, iki göz, kısa ayaklar ve yan zırh.
- Sprinter: dar kama burun, uzun kızaklar ve çift fin.
- Gunner: mor taret, mekanik taban ve ayrı namlu.
- Arena: bevel'lı 3×3 üst panel düzeni, metal yan paneller, havalandırma ve enerji çizgileri.
- Siper/varil/sağlık/altın: aynı koyu metal ve parlak vurgu dili.

Palet, GDD'nin kobalt/turkuaz oyuncu, pembe tehlike ve sıcak altın yönünü izler. Modeller texture gerektirmez; düz yüzey normalleri, temel UV'ler ve 3 materyal slotu kullanır. Humanoid/skinned rig veya Mecanim klibi içermez. Idle, yürüme, koşu, strafe, geri tepme ve tepkiler DOTween ile bağımsız parça pivotlarında üretilir.

## Oyuna bağlantı

`Resources/ToyArtLibrary.asset`, Shape adını paylaşılan mesh + trim/accent materyallerine eşler. Gövde rengi mevcut oynanış kodundaki renk parametresinden gelir. Root konum/ölçekleri, hasar yarıçapları, AI, kapılar, kamera ve tek sahne geçişi korunur. Mesh'ler çalışma anında yeniden üretilmez. Kütüphane isteğe bağlı olarak InvasionPrototype üzerindeki Art Library alanından da atanabilir.

## Bütçe

[MeshBudget.csv](MeshBudget.csv) her parçanın vertex/üçgen sayısını içerir. Scout gövdesi 748, vizör 88, silah 220 üçgendir; üçü birlikte 1.056 üçgen. Grunt gövdesi 396, Sprinter 288, Gunner 400 üçgen. En büyük tek parça arena yanlarıdır: 792 üçgen. Eş nesneler aynı mesh ve materyalleri paylaşır. Bunlar cihaz FPS garantisi değildir; Android CPU/GPU ve draw call profili ayrıca yapılmalıdır.

## Düzenleme ve yeniden üretim

Kaynak modelleme kodu:

- `Editor/ToyMeshBuilder.cs`: gerçek bevel kutu, çokgen silindir ve prizma üretimi.
- `Editor/ToyArtBuilder.cs`: tasarım parçalarının birleştirilmesi, materyaller, prefabs ve katalog render'ı.
- `Runtime/ToyArtLibrary.cs`: runtime eşleme verisi.

`Cube Invasion > Art > Build Toy Sci-Fi Assets` yalnız bu kitin üretilen mesh/materyal/prefab dosyalarını ve Resources kütüphanesini yeniler. Sahne üreticisini çağırmaz. Kitin üretilmiş dosyalarında el ile değişiklik yaptıysanız yeniden üretim bunları günceller; kalıcı tasarım değişikliklerini kaynak koduna veya ayrı bir prefab varyantına taşıyın. Mevcut asset GUID'leri korunur.

`Cube Invasion > Art > Render Model Catalog` geçici preview scene kullanır; kaydedilmiş oyun sahnesini değiştirmez. Katalog `/tmp/cube-toy-catalog.png` konumuna yazılır.

## Doğrulama

Unity 6000.4.8f1 ve Android aktif hedefiyle ayrı doğrulama kopyasında derleme, P0/P1 Play Mode kontrolleri, robot/siper özel mesh bağlantıları ve 540×960 görsel kontrolü geçti. Model kataloğu ve oyun görünümü `Documentation/Validation/ToyArt_*.png` dosyalarındadır. UnityEditor.Search başlangıç indeksleyici hatası araç logunda görülmüştür; oyun kontrolleri tamamlanmıştır. Gerçek Android cihaz testi sonraki adımdır. DOTween animasyon ayrıntıları `Documentation/DOTween_Animation_TR.md` dosyasındadır.
