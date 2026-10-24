# Cube Invasion

Unity **6000.4.8f1** ile GDD v1.1'e göre geliştirilen 3D top-down mobil aksiyon prototipi.

## Çalıştırma

1. Unity Hub'dan bu klasörü 6000.4.8f1 ile açın.
2. `Assets/CubeInvasion/CubeInvasionPrototype.unity` sahnesini açıp Play'e basın.
3. Mobilde ekranın sol altındaki joystick ile hareket edin. Karakter hareket ederken veya dururken en yakın düşmana dönüp otomatik ateş eder. Duvar ve siperler oyuncunun hedef kilidini ve mermisini engellemez. Editörde WASD / ok tuşları da kullanılabilir.
4. Küp temizlenince açılan sağ veya ileri kapıdan joystick ile yürüyerek geçin. Kapının üzerindeki güçlendirme geçişte bir kez verilir; köprü üzerinden sonraki küpe ilerlenir. Rota seçim butonu yoktur.

Sahneyi/verileri yeniden üretmek için `Cube Invasion > Build P0 Playground` menüsünü kullanın. Bu işlem prototip sahnesini, iki rota verisini ve build sahne listesini yeniden yazar. Eski `SampleScene` ve `MainMenu` korunur; build listesinde devre dışıdır.

## Android / portrait

Aktif Unity build target **Android**, ekran yönü **Portrait**. Otomatik ekran döndürme kapalı; runtime da portrait yönünü korur. Android için **IL2CPP + ARM64** ayarlı.

Yeni bir checkout'ta platformu kurmak için `Cube Invasion > Configure Android Portrait` menüsünü çalıştırın. Android Build Support, SDK, NDK ve OpenJDK kurulu olmalıdır. Komut satırı karşılığı:

```sh
"$UNITY_EDITOR" -batchmode -nographics -buildTarget Android -projectPath "$PWD" -executeMethod PrototypeBuilder.ConfigureAndroid -quit -logFile /tmp/cube-android-config.log
```

## P0 temeli ve P1 Feel

- Üç savaş küpü, her küpte 20 saniye; başarısızlıkta platformun çökmesi.
- Joystick ile serbest hareket; en yakın düşmana otomatik kilit ve ateş. Oyuncu mermileri duvar/siperden geçer.
- Uyarılı Grunt saldırısı, yönü kilitlenen Sprinter hamlesi ve riskli rotada 3 mermilik seri atan Gunner.
- 2. küpten itibaren siper, gecikmeli/zincirleme patlayan variller ve +20 can pickup’ı.
- Vuruş flaşı, muzzle/impact/patlama efektleri, kısa hasar dokunulmazlığı ve itme.
- Kesintisiz tek sahne akışı: karakter ve Cinemachine kamera korunur; aday küpler aşağıdan yükselir, geride kalan ve seçilmeyen küpler düşerek yok olur.
- Savaşta kilitli iki güçlendirme kapısı; temizlikte açılır. Sayaç durmuşken kapıya yürünür, geçişte ödül alınır ve 0.9 saniyelik köprü geçişi oynar.
- Sağ: +30 can (100 üst sınır); ileri: iki küp boyunca +%15 hasar.
- Düşmanlar 3 altın bırakır. Pickup altını aktif küpte bekletilir; temizlenince mıknatısla toplanıp 10 temizlik altınıyla bankalanır. Fail, aktif küpün altınını kaybettirir; önceki banka korunur.
- Karakterin sol üst çaprazından 45° eğim / 45° yatay açıyla bakan ortografik arcade kamera; joystick ekran yönlerine göre çalışır; karakteri Cinemachine Brain + Virtual Camera / Transposer damping (0.18) ile takip eder, karakter dönerken kamera yönünü korur.
- Safe area uyumlu Türkçe uGUI HUD, çoklu dokunmaya dayanıklı Canvas joystick, can/süre barları, pause, ses anahtarı ve sonuç ekranı.
- Çalışma anında üretilen 10 temel SFX; 6 ses kanalı, 48 aktif efekt sınırı ve mermi/efekt havuzları. Uygulama odak kaybında duraklar.
- Scout robot, 3 düşman, silah, panelli küpler, kapılar ve pickup’lar özel bevel mesh’lerle gösterilir. 26 temel mesh + 16 rig parçası + 9 görsel prefab `Assets/CubeInvasion/Art` altındadır. Oynanış sırasında hazır, paylaşılan mesh varlıkları kullanılır.

Bu P1 ilk uygulamasıdır; tamamlanmış oyun veya cihaz performans onayı değildir. Mevcut proje Built-in renderer üzerinde doğrulanmıştır; GDD'deki URP geçişi ayrı bir sonraki adımdır. Üretim kapsamı ve eksikler için [geliştirme notlarına](Documentation/Development_TR.md) bakın.

## Doğrulama

Unity batchmode ile derleme ve sahne üretimi:

```sh
"$UNITY_EDITOR" -batchmode -nographics -projectPath "$PWD" -executeMethod PrototypeBuilder.Build -quit -logFile /tmp/cube-unity-import.log
```

Play Mode entegrasyon kontrolü (`-quit` eklemeyin; kontrol kendisi çıkar):

```sh
"$UNITY_EDITOR" -batchmode -nographics -buildTarget Android -projectPath "$PWD" -executeMethod PrototypeSmokeCheck.Run -logFile /tmp/cube-unity-smoke.log
```

Başarı işaretleri: `CUBE_P0_SMOKE_OK`, `CUBE_P1_SMOKE_OK` ve `CUBE_P1_PROFILE_OK`. Test cüzdan değerini geri yükler. Grafik modunda ayrıca `/tmp/cube-p0-preview.png` üretir; `-nographics` kullanılırsa görsel üretmez.

P1 uygulama kapsamı ve doğrulama: [P1 geliştirme notları](Documentation/P1_Feel_TR.md).

Play Mode sırasında `Cube Invasion > Profile P1 (Play Mode)` menüsü 300 karelik bir JSON örneklemesi yazar. Çıktı yolu Console'da `CUBE_P1_PROFILE_OK` ile gösterilir. Development build'de `P1PerformanceProbe.Capture()` cihaz ölçümü için kullanılabilir. Editör ölçümü mobil 60 FPS garantisi değildir.

## 3D model seti

Ana placeholder modeller özel düşük poligonlu toy sci-fi mesh'lerle değiştirildi. [Varlık kılavuzuna](Assets/CubeInvasion/Art/README.md) bakın. [Model kataloğu](Documentation/Validation/ToyArt_Catalog.png) ve [oyun içi görünüm](Documentation/Validation/ToyArt_Gameplay.png).

Mevcut sahne, rota ve kamera ayarları korunur. `ToyArtLibrary` Resources üzerinden yüklenir; görsel prefab'lar bağımsız inceleme/yeniden kullanım içindir. Dört robot modelinde DOTween ile idle, yürüme/koşu ve tepki animasyonları vardır; humanoid/skinned Animator rig kullanılmaz.

## DOTween animasyonları

DOTween 1.3.045 resmî ücretsiz paketi `Assets/Plugins/Demigiant/DOTween` altındadır. Analog joystick düşük hızda yürüyüş, tam hızda koşu üretir. Karakter en yakın düşmana bakarken yana/geri adımlayabilir. Gövde nefesi, kol/bacak döngüsü, ateş geri tepmesi, vurulma, spawn/death, küp yükselme/düşme, kapı açılışı ve pickup hover/spin animasyonları eklidir. Pause tüm bu saatleri durdurur. Gameplay root'u animasyonla taşınmaz; Cinemachine takibi korunur.

[Animasyon önizlemesi](Documentation/Validation/DOTween_Locomotion.gif) · [Teknik notlar ve kontroller](Documentation/DOTween_Animation_TR.md). Testte ek başarı işaretleri: `CUBE_ANIMATION_SMOKE_OK`, `CUBE_ANIMATION_CLEANUP_OK`.

## P2 içerik

Play açılışında görev ve silah seçimi bulunur: 6 düşman arketipi, 4 silah, 5 normal görev, Gold Rush ve 3 fazlı boss. Küpler arasında güçlendirme kapılarından yürüyerek geçilir. Yeni modeller, DOTween rig'leri, WAV ses varyantları ve müzik döngüleri projeye dahildir. İçerik ve doğrulama ayrıntıları: [P2_Content_TR.md](Documentation/P2_Content_TR.md).

## P3 kalıcı ilerleme

Güncel sahnede silahlar altınla açılır; hasar, atış hızı ve menzil yükseltmeleri kalıcıdır. Görev kazanıldıkça sonraki görev açılır. Sonuç ekranında deneme kazancı ile toplam cüzdan ayrı gösterilir. Yerel profil/yedek, legacy altın aktarımı, fail/retry ve yazma hatasından kurtarma ayrıntıları: [P3_Meta_TR.md](Documentation/P3_Meta_TR.md).

## Görselli mobil menüler

Ana menü, kaydırılabilir bölüm yolu, 3D ürün görselli market, kalıcı Scout geliştirmeleri ve ses/titreşim/FPS ayarları sahneye bağlıdır. Arka planlar yerleşik Imagegen ile üretildi; kontroller native uGUI'dir. [UI / UX notları](Documentation/UI_UX_TR.md) · [Görsel üretim promptları](Documentation/UI_Imagegen_Prompts.md).
