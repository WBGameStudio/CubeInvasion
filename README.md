# Cube Invasion

Unity **6000.4.8f1** ile GDD v1.1'e göre geliştirilen 3D top-down mobil aksiyon prototipi.

## Çalıştırma

1. Unity Hub'dan bu klasörü 6000.4.8f1 ile açın.
2. `Assets/CubeInvasion/CubeInvasionPrototype.unity` sahnesini açıp Play'e basın.
3. Mobilde ekranın sol altındaki joystick ile hareket edin. Karakter hareket yönüne döner ve baktığı yöne otomatik mermi atar. Joystick bırakılınca son bakış yönünü korur. Editörde WASD / ok tuşları da kullanılabilir.
4. Küp temizlenince açılan sağ veya ileri kapıdan joystick ile yürüyerek geçin. Kapının üzerindeki güçlendirme geçişte bir kez verilir; köprü üzerinden sonraki küpe ilerlenir. Rota seçim butonu yoktur.

Sahneyi/verileri yeniden üretmek için `Cube Invasion > Build P0 Playground` menüsünü kullanın. Bu işlem prototip sahnesini, iki rota verisini ve build sahne listesini yeniden yazar. Eski `SampleScene` ve `MainMenu` korunur; build listesinde devre dışıdır.

## Android / portrait

Aktif Unity build target **Android**, ekran yönü **Portrait**. Otomatik ekran döndürme kapalı; runtime da portrait yönünü korur. Android için **IL2CPP + ARM64** ayarlı.

Yeni bir checkout'ta platformu kurmak için `Cube Invasion > Configure Android Portrait` menüsünü çalıştırın. Android Build Support, SDK, NDK ve OpenJDK kurulu olmalıdır. Komut satırı karşılığı:

```sh
"$UNITY_EDITOR" -batchmode -nographics -buildTarget Android -projectPath "$PWD" -executeMethod PrototypeBuilder.ConfigureAndroid -quit -logFile /tmp/cube-android-config.log
```

## İlk prototip

- Üç savaş küpü, her küpte 20 saniye; başarısızlıkta platformun çökmesi.
- Joystick yönünde dönüş ve karakterin baktığı yönde ilerleyen Pulse Pistol mermileri; otomatik hedefe dönme yoktur.
- Grunt ve hızlı Sprinter; oyuncu canı, temas hasarı ve açık kenardan düşme.
- Kesintisiz tek sahne akışı: karakter ve Cinemachine kamera korunur; aday küpler aşağıdan yükselir, geride kalan ve seçilmeyen küpler düşerek yok olur.
- Savaşta kilitli iki güçlendirme kapısı; temizlikte açılır. Sayaç durmuşken kapıya yürünür, geçişte ödül alınır ve 0.9 saniyelik köprü geçişi oynar.
- Sağ: +30 can (100 üst sınır); ileri: iki küp boyunca +%15 hasar.
- Temizlenen her küpün 10 altını yerel cüzdana bankalanır; retry altını korur.
- Karakterin sol üst çaprazından 45° eğim / 45° yatay açıyla bakan ortografik arcade kamera; joystick ekran yönlerine göre çalışır; karakteri Cinemachine Brain + Virtual Camera / Transposer damping (0.18) ile takip eder, karakter dönerken kamera yönünü korur.
- Türkçe geçici HUD, duraklatma, fail ve level clear; uygulama odak kaybında duraklar.
- Robot, silah, farklı düşman silüetleri, ışıklı küp kenarları ve rota işaretleri gerçek 3D primitive geometrilerden çalışma anında üretilir.

Bu P0, nihai mobil UI veya tamamlanmış oyun değildir. Mevcut proje Built-in renderer üzerinde doğrulanmıştır; GDD'deki URP geçişi ayrı bir sonraki adımdır. Üretim kapsamı ve eksikler için [geliştirme notlarına](Documentation/Development_TR.md) bakın.

## Doğrulama

Unity batchmode ile derleme ve sahne üretimi:

```sh
"$UNITY_EDITOR" -batchmode -nographics -projectPath "$PWD" -executeMethod PrototypeBuilder.Build -quit -logFile /tmp/cube-unity-import.log
```

Play Mode entegrasyon kontrolü (`-quit` eklemeyin; kontrol kendisi çıkar):

```sh
"$UNITY_EDITOR" -batchmode -nographics -buildTarget Android -projectPath "$PWD" -executeMethod PrototypeSmokeCheck.Run -logFile /tmp/cube-unity-smoke.log
```

Başarı işareti: `CUBE_P0_SMOKE_OK`. Test cüzdan değerini geri yükler. Grafik modunda ayrıca `/tmp/cube-p0-preview.png` üretir; `-nographics` kullanılırsa görsel üretmez.
