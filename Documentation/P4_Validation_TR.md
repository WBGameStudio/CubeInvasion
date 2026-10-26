# P4 — doğrulama ve mobil test hazırlığı

P4, GDD §18 ve §21 doğrultusunda öğretici, QA, analitik, denge incelemesi ve performans ölçümüne odaklanır. Android portrait, Unity 6000.4.8f1, kesintisiz küp geçişleri, joystick ve duvarlardan bağımsız otomatik hedefleme korunur.

## Uygulananlar

- İlk görevde hareketle ilerleyen joystick/otomatik ateş ipuçları; ikinci küpte süre ve kenar uyarısı; temizlikte yürüyerek kapı seçimi. Görev tamamlanınca ilk görev öğreticisi kapanır; başarısızlıkta yeniden görünür. Öğretici oyunu durdurmaz ve süreye müdahale etmez.
- Sonraki görevlerin başlangıcında kenar/itici, banka/market, varil, koni, Gold Rush ve boss ipuçları. Metin sahnede `Mobile Combat HUD / Safe area / First run guidance` nesnesindedir; Inspector'dan düzenlenebilir. Dokunmaları engellemez ve pause/fail/menu sırasında gizlenir.
- Uygulama arka plana alınırken oyun duraklatılır ve bekleyen ayarlar yazılır. Geri dönüşte oyuncu devam ettirir.
- Menü içerikleri profil/inceleme/görev/faz değiştiğinde yenilenir. Kullanılmayan eski market/loadout panellerinin metinleri her karede hazırlanmaz; müzik/efekt ayarları değişmediyse UI tekrar doldurulmaz. Bildirimlerin zaman aşımı çalışmaya devam eder.
- Yerel ve çevrimdışı `ci_playtest_v1.json`: en fazla 256 olay; eski olaylar sırayla silinir. Ağ gönderimi yoktur. Ekonomi kaydından bağımsızdır; bozuk veya yazılamayan tanılama dosyası oyunu durdurmaz. Testler kendi profil klasörüne ve ayrı öğretici anahtarına yazar.
- Olaylar: `level_started`, `cube_started`, `cube_cleared`, `cube_failed`, `route_selected`, `level_completed`, `tutorial_moved`, `weapon_unlocked`, `weapon_upgraded`, `goldrush_completed`, `boss_phase_failed`. Savaş olayları görev/küp/silah, aynı denemeye ait run kimliği, süre, can, kalan düşman ve alınan gerçek hasarı taşır. İyileşme alınan hasarı istatistikten silmez. Başarılı banka işleminden sonra clear olayı yazılır; kayıt yeniden denemesi çift clear üretmez. Ekonomi olaylarında `coins` harcamalar için negatiftir.
- Development Android derlemesinde ilk görev başlangıcında 30 ısınma + 300 aktif savaş karesi ölçülür. Pause, kapı seçimi ve sonuç kareleri örneklenmez. `ci_p4_device_profile.json`: kare ve Main Thread medyan/p95, en yüksek GC allocation, ayrılmış Unity belleği, cihaz/GPU, çözünürlük ve hedef FPS. Tamamlanması için yeterli savaş süresi gerekir; ölçüm sonraki denemede devam edebilir. Release sürümünde otomatik profiler yoktur.

## Editör araçları

`Cube Invasion / P4` altında:

- **Install Scene Guidance**: açık oyun sahnesine idempotent kurulum; kamera/karakter/diğer kurulumları yeniden oluşturmaz.
- **Replay First Run Guidance**: ilk görev öğreticisini yeniden açar; ekonomi kaydını değiştirmez.
- **Check Diagnostics and Balance**: FIFO sınırı, tekrar yükleme, bozuk/yazılamayan dosya ve içerik bütünlüğü; `/tmp/cube-p4-balance.csv` üretir.
- **Summarize Local Playtest**: yerel olay örneklemindeki başlangıç, bitiş, fail/timeout, ortalama clear ve silah kullanım sayıları; çıktı persistentDataPath altında `ci_playtest_summary.txt`.
- **Build Android Development APK**: mevcut Android/IL2CPP/ARM64 ayarlarıyla geliştirme APK'sı; işletim sistemi geçici klasöründeki `CubeInvasion-P4` dizinine yazar. AAB tercihini işlem sonunda eski değerine getirir. Debug imzalı yerel test çıktısıdır.

## Denge ve test sınırları

CSV, her küpte toplam HP ve temel Pulse Pistol için **ideal kesintisiz DPS alt sınırını** gösterir. Seyahat, hedef değişimi, mermi uçuşu, ıskalama, saçma dağılımı, kaçınma ve rota güçlendirmesi içermez; gerçek clear süresi değildir. Bu nedenle bu rapora bakılarak düşman HP'si keyfi değiştirilmedi. Canlı oyuncu verisi geldikten sonra içerik ayarlanmalı.

Olay geçmişi sınırlı bir tanılama örneklemidir; benzersiz oyuncu, retention, ilk oturum tamamlama yüzdesi veya güvenilir global KPI üretmez. Sabit içerik kullanıldığı için rastgele encounter seed'i üretilmez. Üretim analitik servisi ve uzun dönem cohort takibi bu teslimde yoktur.

Cihaz kabulü: aynı cihaz ve görev rotasında 60/30 FPS modları ayrı ölçülmeli; düşük/orta Android, çentikli/küçük ekran, arka plana alma, sessiz mod ve uzun oturum test edilmeli. Profiler Development overhead içerir; son performans kararı release'e yakın derlemeyle verilmelidir. Bağlı Android cihaz olmadığı için gerçek cihaz performansı ve dokunmatik ergonomi doğrulanmış sayılmadı.

GDD hedefleri hâlâ test hipotezidir: ilk görev >%85 tamamlama, <0.5 fail/oyuncu; kolay küp 12–15 sn, zor küp 16–19 sn; timeout fail payı %80'i geçerse DPS/okunabilirlik incelemesi. Bunları doğrulamak için gerçek oyuncu testi gerekir.

## Bu teslimde çalıştırılan doğrulamalar

İzole proje kopyasında P0/P1 çekirdek/feel, DOTween animasyon/temizlik, kenar düşüşü ve timeout; P2 yedi görevde iki rota; P3 gerçek oynanış banka/upgrade/retry/save recovery; profil veri kontrolleri; beş UI sayfası ve hareketle ilerleyen öğretici; P4 bozuk/yazılamayan tanılama ve içerik bütünlüğü geçti. Ana kaynakların runtime/editor C# derlemesi de geçti. Android ARM64 IL2CPP Development APK başarıyla üretildi: proje kökünde `CubeInvasion-P4.apk` (git dışında tutulur).

Sonuç özeti `Validation/P4_QA_Result.json`, ideal DPS tablosu `Validation/P4_Balance.csv`, editör örnek performans kaydı `Validation/P4_Profile_Editor.json`, öğretici ekranı `Validation/P4_Onboarding.png`. Editör kaydı cihaz FPS kanıtı değildir. Editör başlangıç loglarında daha önce de görülen UnityEditor.Search.SearchDatabase kaynaklı ArgumentOutOfRangeException bulunuyor; test akışları ve Android derlemesi başarılı çıkış yaptı.
