Güncel görünüm: [Void Toy Arcade UI](UI_VoidToy_TR.md). Aşağıdaki tasarım önceki sürümdür.

Güncel tasarım ve yerleşim: [Referans UI yeniden tasarımı](UI_Reference_Redesign_TR.md). Bu dosyanın önceki görünümü aşağıda tarihçe olarak tutulur.

# Hybrid casual UI — 25 Ekim 2026

İllüstrasyon arka planları menülerden kaldırıldı. Düz açık zemin, beyaz yuvarlatılmış kartlar, koyu okunabilir başlıklar, yeşil ana aksiyonlar, mavi geliştirme butonları ve sarı seçili bölüm düğümü kullanılır. Ana menü mevcut Scout model görselini öne çıkarır. Road path, silah marketi, karakter geliştirme ve ayarlar aynı işlemleri sürdürür. Eski arka plan PNG'leri silinmedi; aktif FrontEndArt referanslarından çıkarıldı.

`Assets/CubeInvasion/Art/UI/CasualButton.png` yerleşik **imagegen** ile üretilmiş şeffaf, nötr beyaz buton yüzeyidir. Unity'de sprite dikdörtgeni ve dokuz dilimli kenarlar tanımlanır; PNG kodla düzenlenmez. Farklı boyutlarda köşeler sabit kalır, yüzey yeşil/mavi renklendirilir. Tipografi projedeki Liberation Sans fontunu kullanır. Sahnedeki ana kurulum korunur; UI ve yeni Scout showcase nesnesi sahnede kayıtlıdır.

DOTween: buton basışında 0.08 sn / %95 ölçek, bırakma/iptalde 0.22 sn OutBack dönüş; sayfalarda 0.18 sn fade ve 0.24 sn scale geçişi. Animasyonlar oyun süresinden bağımsızdır. Pasif buton basış tween'i başlatmaz; pointer exit/cancel/disable sırasında basış bırakılır. Hover tek başına tween üretmez. Eski cüzdan geri bildirimi korunur.

Kurulum: `Cube Invasion / UI / Apply Hybrid Casual Style`. Mevcut nesneleri ve callback'leri koruyarak yeniden uygulanabilir. Standart front end kurulumu da bu stili uygular. HUD ve sonuç/pause butonları aynı font ve buton dilini kullanır; savaş sayacı açık kart üzerinde koyu, son 5 saniyede kırmızıdır.

Doğrulama: FrontEndSmokeCheck, beş sayfayı, kilitli görev guard'ını, satın alma ve upgrade tutarlarını, pilot geliştirmelerini, ses/30–60 FPS ayarlarını, yeniden yüklemeyi ve öğreticiyi kontrol eder. Ayrıca düz arka plan, nine-slice üretilmiş sprite, geçerli font ve dokunma ile DOTween başlangıcı doğrulanır. 540×960 ekran görüntüleri `Documentation/Validation/Hybrid_*.png` altındadır. Gerçek cihaz dokunma/performans testi ayrıca gerekir.

## Son üretim prompt'u

Yerleşik imagegen kullanıldı; CLI/API fallback kullanılmadı. Önce yatay taslaklar denendi, üretimde aşağıdaki kare yüzey seçildi:

> Use case ui-mockup. Production UI sprite for a modern hybrid casual mobile game. ONE blank WHITE rounded SQUARE tile, straight-on orthographic view, on actual transparent background. Square 1024x1024 composition. Tile occupies 90% of canvas, centered. Rounded corners with radius approximately 120 pixels. Top, bottom, left and right edges are straight, NOT oval, NOT capsule. Broad flat neutral white face, extremely subtle soft white highlight at top, narrow light gray extrusion beneath giving only 5% depth. This asset will be nine-sliced into rectangular buttons in Unity: keep all four corners consistent and all four central edges straight, blank homogeneous center. Clean smooth alpha silhouette, absolutely no dust, no speckles or particles, no shadows beyond the immediate tile edge, no text, no icons, no background, no scenery. Matte molded plastic, not metallic. This is a square UI tile asset, not an image of an entire interface.

Android ARM64 IL2CPP Development derlemesi geçti. Güncel test paketi proje kökünde `CubeInvasion-HybridCasual.apk`; önceki `CubeInvasion-P4.apk` eski görünümü içerir. Kontrol özeti `Validation/Hybrid_QA.json`. Editör başlangıcında daha önce görülen UnityEditor.Search.SearchDatabase istisnası logda bulunuyor; UI testleri ve build başarılı çıkış yaptı.
