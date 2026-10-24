# DOTween animasyon katmanı

## Kurulum ve kaynak

Resmî ücretsiz DOTween 1.3.045 dağıtımı: https://dotween.demigiant.com/download.php (12 Ekim 2026 sürümü). Vendor dosyaları ve orijinal readme `Assets/Plugins/Demigiant/DOTween` altında korunur. Kullanılan API belgeleri: https://dotween.demigiant.com/documentation.php, lisans: https://dotween.demigiant.com/license.php.

Stok DOTween Physics2D modülünün derlenmesi için Unity'nin `com.unity.modules.physics2d` 1.0.0 yerleşik paketi eklendi. Pro sürümü veya ücretli bağımlılık yoktur. Scene builder çalıştırılmadı; kullanıcının sahne ve kamera ayarları korunur.

## Rig ve locomotion

- Scout: Core + LeftArm, RightArm, LeftLeg, RightLeg.
- Grunt / Sprinter: Core + iki bacak/kızak.
- Gunner: Core + dört ayak, çapraz fazlı yürüyüş.

Bu bir rigid-part, transform tabanlı rig'dir; FBX humanoid skeleton, skinned mesh veya Mecanim klibi değildir. Birleşik mesh'lerdeki geometri parçalara aynen paylaştırılır; toplam üçgen sayısı değişmez. Orijinal birleşik renderer kapatılır, parçalar onun materyallerini paylaşır. Aynı mesh iki kez çizilmez. 26 temel model + 16 rig parçası = 42 Mesh asset bulunur.

`ToyActorAnimator`, DOTween float tween'leriyle idle/gait fazını sürer; faz eklem rotasyonlarına, hafif gövde salınımına ve yönlü eğime uygulanır. Hız <0.12 ise idle, <2.8 birim/sn yürüyüş, üstünde koşudur. Joystick analog hızı korunur; klavye tam hızda koşu üretir. Gerçek root hızından poz seçilir, dolayısıyla köprüde de adımlama devam eder.

Nişan yönü hareketten bağımsızdır. Root'un ileri eksenine göre hareket yönü hesaplanır; yana/geriye giderken adımlar o doğrultuya yönelir. Ateş sırasında kol salınımı azaltılır. Animasyonlar root position/rotation yazmaz, yalnız `DOTween Gait / DOTween Reactions` görsel çocuklarını ve eklem pivotlarını oynatır. Kapılar, hasar yarıçapı, mermi hedefi ve Cinemachine anchor'ı gameplay root'unu kullanmaya devam eder.

Dört robot prefab'ı Play Mode'da kendi rig'ini otomatik bağlayabilir. Oyun içinde `InvasionPrototype` saati sürer; bağımsız prefab örnekleri kendi LateUpdate'inde idle/movement animasyonunu sürer. Düzenleme modundaki katalog statiktir.

## Tepkiler ve ortam

- Idle nefesi ve yürüme/koşuda dönüşümlü kol/bacak hareketi.
- Ateşte kısa görsel recoil; oyuncu ve Gunner için.
- Vurulmada squash/punch scale; hasar yanıp sönmesi bütün oyuncu rig'ini kapsar.
- Düşman doğuşunda kısa scale-in; ölümde görsel rig ayrılır, dönüp küçülerek silinir. Objective ve loot aynı karede çözülür, görsel ölüm clear'ı geciktirmez.
- Telegraph sırasında hafif hazırlık hareketi; önceden var olan saldırı süreleri değişmez.
- Küpler DOTween OutCubic ile yükselir; emekli küpler InQuad ile düşer ve hafif döner.
- Açılan kapılar kısa bir scale punch yapar; loot DOTween ile yükselip alçalır ve döner; varil vuruşu kısa scale tepkisi verir.

Köprü geçişinin mevcut 0.9 sn root hareketi ve fail çöküşü çekirdek akışın kontrolünde kalır. Animasyon süresi 20 sn savaş sayacını durdurmaz veya uzatmaz.

## Saat ve temizleme

Tweener/Sequence'ler `UpdateType.Manual` olarak kurulur. Yalnız bu oyunun sahip olduğu tween instance'ları `ManualUpdate(dt,dt)` ile sürülür. Global `DOTween.PauseAll`, `KillAll` veya oyun genelinde Time.timeScale değişikliği yapılmaz. Böylece başka kullanıcı sistemlerinin tween'leri etkilenmez.

Pause sırasında rig ve dünya saatleri ilerlemez. Tween'ler actor OnDestroy veya dünya temizliğinde Kill edilir. Geçici görseller ve pickup/cube tween'leri `SetLink` ile nesne ömrüne bağlanır. Kayıtlı materyal/mesh varlıkları yok edilmez veya animasyonla değiştirilmez.

## Kontroller ve görsel çıktı

Unity 6000.4.8f1 / Android aktif hedefli ayrı doğrulama kopyasında:

- P0 ve P1 regresyonları geçti: en yakın hedefe ateş, siperden geçen oyuncu mermisi, kapılar, buff, 20 sn timeout, üç küp ve sürekli aynı karakter/kamera/sahne.
- Dört robotun rig parça üçgen toplamı birleşik mesh ile eşleşti.
- Scout'un dört eklemi, dönüşümlü yürüyüş, run/idle geçişi, pause'da sabit poz, recoil ve root'un değişmemesi doğrulandı.
- Actor silinince gait/idle tween'lerinin kapandığı doğrulandı.
- Küp animasyonlarının pause ile durması ve düşüş sonunda silinmesi mevcut kontrollerle doğrulandı.

İşaretler: `CUBE_ANIMATION_SMOKE_OK`, `CUBE_ANIMATION_CLEANUP_OK`, `CUBE_P0_SMOKE_OK`, `CUBE_P1_SMOKE_OK`.

[96 kare / 24 FPS rig önizlemesi](Validation/DOTween_Locomotion.gif): idle → walk → run + auto fire. Kareler Unity Camera ile gerçek rig'den render edildi.

[Editör profil örneği](Validation/DOTween_Profile_Editor.json). Bu Mac editör ölçümüdür; Android CPU/GPU/ısı ve gerçek cihaz FPS onayı değildir. UnityEditor.Search başlangıç indeksleyicisinden bilinen hata görüldü; animasyon ve oyun kontrolleri tamamlandı.

İleri kalite adımları: iki parçalı diz eklemleri, zemin/ayak kilidi, karaktere özel animasyon eğrileri, ses-adım senkronu ve gerçek cihaz performans testi. Mevcut set stilize robotlar için procedural rig animasyonudur.
