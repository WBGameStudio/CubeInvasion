# P2 / Content

P2 içerik uygulaması: 6 normal düşman türü, 4 silah, 5 normal görev, Gold Rush ve üç fazlı The Crusher. Toplam 27 oynanacak küp vardır; aday küpler bu sayıya dahil değildir. Unity 6000.4.8f1, Android portrait ve mevcut Built-in renderer korunur.

## Oynama ve düzenleme

`Assets/CubeInvasion/CubeInvasionPrototype.unity` sahnesinde Play'e basınca görev / silah ekranı açılır. Önce görev ve silah seçilir, sonra **Göreve Başla** ile savaş başlar. Silah görev boyunca sabittir. Dört silah P2 içeriğini deneyebilmek için erişime açıktır; satın alma, unlock ve kalıcı upgrade akışı P3 kapsamındadır.

Görev kazanınca sonraki göreve geçilebilir; fail'de aynı görev ve silahla tekrar denenir. Görev/silah ekranına da dönülebilir. Bu butonlar görev menüsüne aittir: küp arası rota/güçlendirme seçimi yalnız karakterin kapıdan geçmesiyle yapılır.

- `Resources/P2Content.asset`: silah hasarı, atış aralığı, saçma sayısı, dağılımı, mermi ömrü; görev başlıkları, renkleri, küp kompozisyonları, siper/varil ve altın kotaları.
- `Art/P2/Models`: 7 yeni mesh, dört uzvu ayrı animasyon alan düşman/boss modelleri ve 7 prefab. Rig'ler gövde ve dört uzuvdan oluşur; hareket, recoil, telegraph, hit ve death mevcut DOTween sürücüsünü kullanır.
- `Art/P2/Audio`: özgün sentezle üretilmiş 32 kısa WAV SFX varyantı, 3 döngüsel müzik WAV dosyası ve düzenlenebilir Audio Bank. Sesler altı eşzamanlı SFX kanalı ile çalınır; müzik ayrı kanaldadır. Pause/mute müziğe de uygulanır; büyük darbeler müziği kısa süre kısar.
- `Cube Invasion > P2 > Install Content In Current Scene`: mevcut sahneyi sıfırlamadan P2 referanslarını ve eksik UI'ı bağlar. Mevcut içerik kataloglarını, model kayıtlarını ve ses bankasını yeniden yazmaz. Temel art üreticisi de P2 model kayıtlarını korur.

## İçerik

| Görev | Küp | Odak |
| --- | ---: | --- |
| İlk Temas | 3 | Grunt, Sprinter ve kapılar |
| Neon Yard | 4 | Gunner serileri, siper ve variller |
| İtki Tesisi | 4 | Pusher konisi ve kenar güvenliği |
| Uzun Menzil | 5 | Sniper lazeri, Bruiser yakın darbesi |
| Son Hat | 5 | En fazla 3 arketipli karışık karşılaşmalar |
| Gold Rush | 3 | 40 / 50 / 60 altın kotası |
| The Crusher | 3 | Zırh, silah sistemi, çekirdek fazları |

Pulse Pistol: 6 hasar / 0,5 sn. Twin Blasters: iki namludan 3,5 hasar / 0,48 sn. Scatter Shot: 5 × 3 hasar / 0,9 sn; 28 derece dağılım, kısa mermi ömrü. Rapid SMG: 2 hasar / 0,14 sn. Scatter'ın etkin hasarı mesafeye bağlıdır; diğer silahların nominal DPS'si bu yüzden doğrudan tek üstünlük sırası değildir.

Pusher 0,85 sn önce konisini gösterir ve kilitlenen yönde iter. Sniper'ın 1,1 sn lazer uyarısı vardır; oyuncu yana kaçarsa atış yeni konuma yeniden kilitlenmez. Bruiser 0,8 sn hazırlıktan sonra 13 hasarlık yakın alan darbesi vurur. Grunt, Sprinter ve Gunner'ın P1 davranışları korunur.

Crusher'ın toplam canı 252'dir (72 / 84 / 96 faz bütçesi). Ortak bar ve sayı kapı geçişinde dolmaz. Faz 1 yaklaşarak alan darbesi, faz 2 yedi mermilik yelpaze, faz 3 dönüşümlü 12 radyal / 9 hedefli mermi kullanır. Silah saldırıları önceden yerde çizgilerle gösterilir. Her faz kendi küpünde 20 sn içinde boss fazı ve minyonlar birlikte temizlenmelidir; kapılar faz atlatmaz.

Gold Rush'ta düşmanların bitmesi tek başına clear değildir. Kota dolmadan kapılar açılmaz. Altınlar küpün erişilebilir iç 4×4 ızgarasında belirir; kota için yeterli altın baştan mevcuttur. Kota zamanında dolarsa clear, süre önce biterse collapse/fail gerçekleşir. Yalnız temizlenen küpün altını bankalanır; tamamlanmamış küpün bekleyen altını fail'de silinir.

## Korunan kurallar

Tek joystick, duvar arkasında da en yakın hedefe kilit, oyuncu mermilerinin siperden geçmesi, Cinemachine'in sabit çapraz açıyla takibi, sahnede düzenlenebilir ana kurulum, aynı karakter/kamera ile animasyonlu küp geçişleri korunur. Kenar düşüşünde küp yerinde kalır; oyuncunun düşüşünden sonra fail görünür. Can bitişi küpü düşürmez. Süre bitişinde aktif küp çöker. Başarılı transferde eski küpün kaldırılma animasyonu devam eder.

## Doğrulama

`PrototypeSmokeCheck.Run`: P0/P1 hareket, hedefleme, kapılar, DOTween, UI, pickup, siper, fail, zaman aşımı ve kenar düşüşü regresyonları. Bu test P2 kataloğunu yalnız test oturumu için devre dışı bırakır; kaydedilmiş sahne değişmez.

`P2SmokeCheck.Run`: dört silahın mermi sayısı/hasarı/ömrü, yeni rig/mesh ve ses bağlantıları, düşman telegraph/karşı hamleleri, boss faz paternleri ve ortak can, altın kotası/zaman sınırı, iki ayrı rota ile yedi görevin tamamı, finalde kapı olmaması, retry ve menü akışı. Testler cüzdan değerini geri yükler. Başarı işareti `CUBE_P2_SMOKE_OK`.

Silah ve karşılaşma verilerinde taban pistol için HP / (12 × 0,78) + 3,5 sn hesabıyla 20 sn bütçesi kontrol edilir. Bu otomatik bütçe kontrolü ve programatik görev tamamlama, gerçek oyuncunun kaçınma/isabet başarısını ölçmez. GDD'deki 10–15 dakika oynama hedefi için kullanıcı oturumlarıyla süre ve zorluk ayarı; Android cihazda dokunma, ses miksajı, ısı ve CPU/GPU performans doğrulaması gerekir. Bu teslimde gerçek cihaz ölçümü yapılmadı.

Son doğrulama: iki rota ile **14 görev sonucu / 54 küp / 40 kapı geçişi** tamamlandı. `CUBE_P2_SMOKE_OK`, `CUBE_P2_VISUAL_OK`, `CUBE_P0_SMOKE_OK`, `CUBE_P1_SMOKE_OK`, `CUBE_FALL_SMOKE_OK` ve animasyon kontrolleri geçti. Ana projenin kendi derleyici response dosyalarıyla runtime/editor C# derlemeleri de hatasız tamamlandı. Yeni varlıkların GUID bağımlılıkları doğrulandı.

Görseller: [görev ekranı](Validation/P2_Loadout.png), [Pusher uyarısı](Validation/P2_Pusher.png), [Gold Rush](Validation/P2_GoldRush.png), [Crusher](Validation/P2_Crusher.png), [yeni model kataloğu](Validation/P2_Model_Catalog.png).

Güncel sahne P3 ilerlemesini de kullanır: yukarıdaki dört silaha doğrudan erişim P2 test moduna aittir; normal oyunda silahlar altınla açılır. [P3 notları](P3_Meta_TR.md).
