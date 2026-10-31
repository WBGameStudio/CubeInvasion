# Savaş baskısı ve tek parmak ateş ritmi

Oyuncu onaylı zorluk paketi mevcut sahne, profil, model, kamera ve menü akışı korunarak uygulandı. Unity 6000.4.8f1 / Android / portrait.

## Tek parmak kontrolü

Otomatik hedefleme ve otomatik ateş korunur; yeni düğme, ikinci parmak veya parmağı kaldırma şartı yoktur. Joystick'in merkezine uzaklığı ateş ritmini belirler:

- Merkezin %25'i içinde: seçilen silahın normal atış aralığı; hafif yürürken de odak atışı yapılabilir.
- %25–85 arasında: atış aralığı kademeli artar.
- %85 ve üzeri: atış aralığı normalin 1,4 katıdır. Koşarken ateş devam eder, hasar üretimi yaklaşık %29 azalır. (Önceki 2,1 kat oyun testinde hareketi fazla cezalandırıyordu: koşarken 2,3 sn boyunca hiç düşman ölmedi.)

Örnek: temel Pulse Pistol odakta 0,5 saniyede, tam koşuda yaklaşık 0,7 saniyede bir ateş eder. Hız değişimi bekleyen atışın toparlanmasını anında etkiler; joystick'i merkeze çekmek sayacı sıfırlamaz. Silah/kalıcı atış hızı geliştirmeleri bu ritmin temel hızına uygulanır. Hareketsiz kalmanın karşılığı daha hızlı ateş, bedeli düşman saldırılarına açık kalmaktır. Parmağı hafifçe merkeze yaklaştırarak yavaş yürüme de mümkündür.

Joystick üstünde `ODAK ATEŞİ`, `KOŞU ATIŞI`, ara hızda `YAVAŞLA · HIZLI ATEŞ`, tüm hedefler kapalıysa `ATEŞ HATTI BUL` gösterilir. İlk kullanım yönlendirmesi aynı parmak basılıyken yavaşlamayı anlatır. Pause atış sayacını ve animasyonları dondurur.

## Barikat ve hedefleme

Oyuncu ve düşman mermileri, barikat mesh'inin boyutlarından alınan yatay sınırları kullanır. Çarpışma önceki–sonraki konum boyunca taranır; düşük kare hızında barikatın öte tarafına atlama engellenir. Barikat döndürüldüğünde veya farklı eksenlerde ölçeklendiğinde sınırlar dönüşümü izler.

- En yakın **açık ateş hattındaki** düşman seçilir. Barikat arkasındaki yakın hedef, açıkta duran daha uzak hedefe tercih edilmez.
- Mermi önce barikata çarparsa arkasındaki düşmana veya patlayıcı varile hasar vermez.
- Düşman ya da varil barikattan önceyse ilk çarpışma ona uygulanır.
- Silah ucu yakından barikata uzansa bile mermi barikatın ötesinde oluşturulmaz.
- Hem oyuncu hem düşman mermilerinin ömrü, büyük bir kare süresinde de menzili sınırlar.

Hedefleme silahın menzilini uzatmaz: kısa menzilli silahla hedefe yaklaşmak gerekir. Düşmanlar doğrudan yol kapalıysa siperin köşelerinden dolaşır. Saldırı hazırlıkları görünür kalır; kararlı Sprinter atılışı uyarıldığı yönü takip eder ve sipere takılabilir.

## Karşılaşma dengesi

- Düşman hareket hızı ×1,20; saldırı sonrası toparlanma süresi ×0,80. Uyarı süreleri kısaltılmadı.
- Düşman saldırı hasarı ×1,30. Darbe sonrası 0,55 saniyelik dokunulmazlık ve başlangıç koruması korunur. Çevresel varil patlamasının hasarı değişmedi.
- Düşman canları ve boss'un 72/84/96 canlık üç fazı artırılmadı.
- İlk iki görevde altı kişilik karışık gruplar; diğer savaş görevlerinde dört–altı, Gold Rush'ta dört–beş, boss fazlarında üç–dört düşman. Yan şeritlerdeki düşmanlar tek bir kuyruk yerine farklı yönlerden baskı kurar.
- Siperler kübe göre sağa/sola kayar ve ateş hattı değiştirmeyi gerektirir.
- 27 küp içinde yalnızca dört belirli savaş kübünde +20 sağlık kutusu bulunur. Kutular yan taraftaki riskli alanlardadır. Küp temizlenince altınlar otomatik toplanır, sağlık kutusu uzaktan ücretsiz alınmaz; kapıdan çıkmadan önce yürüyüp alınabilir. Can kapısı seçimi önem kazanır.
- Küp süresi 20 saniyedir; kesintisiz geçiş, yükselen sonraki küpler, timeout çöküşü ve yalnız karakterin kenardan düşmesi davranışı korunur.

Tüm değerler `Assets/CubeInvasion/Resources/CombatBalance.asset` üzerinden değiştirilebilir. Grup/siper/sağlık kutusu düzeni `P2Content.asset` içindeki karşılaşmalardadır. Mevcut silah değerleri, kalıcı ilerleme ve geliştirme fiyatları korunur. Otomatik göç `pressureRevision` ile tek sefer yapılır; sonraki kurulumda elle ayarlanmış karşılaşmalar tekrar yazılmaz.

## Ölçümün kapsamı

İlk, daha küçük gruplu denemede temel tabancalı sabit karakter ilk küpte 3,53 saniyede hiç hasar almadan bütün düşmanları indirebildi. Grup baskısı artırıldıktan sonra aynı deterministik denemede bu 8,65 saniye ve 36,4 can kaybına çıktı. Bu test her görevin yalnızca ilk kübünü, meta geliştirmesiz silahı ve hareketsiz karakteri değerlendirir; insan oyuncu zorluk testi veya Android performans ölçümü değildir. Gold Rush'ta düşmanları öldürmek tek başına küpü bitirmez.

Yeniden kurulum: `Cube Invasion > Combat > Install Pressure And Fire Rhythm`. Ana sahnede ayar varlığı ve ateş ritmi etiketi kaydedilmiştir; kamera ve oyuncu transformları taşınmaz. Mekanik test: `CombatPressureSmokeCheck.Run` (batch mode, `-quit` olmadan).

## Doğrulama sonucu

C# runtime/Editor derlemesi; hedef görünürlüğü; büyük kare süresinde mermi/barikat ve varil çarpışma sırası; döndürülmüş/ölçeklenmiş siper; mermi ömrü; tek basılı parmakla koşu/odak geçişi; pause; hasar dokunulmazlığı; dört yönden siper çevresinde ilerleme ve siper kenarına yaslanan oyuncuya ulaşma kontrolleri geçti. Uzaktan sağlık kutusu otomatik alınmıyor, temizlenmiş küpte yürüyerek alınabiliyor.

P0/P1 kamera, animasyon, küp geçişi ve düşme; P2 yedi görevin iki rotası, silahlar, boss fazları ve retry; ana menü, market, pilot, ayarlar, yol ve yeni onboarding kontrolleri geçti. Başlangıç silahı can/DPS bütçesi 20 saniyelik küplerde doğrulandı; bu rapor insan oyuncunun her kombinasyonla kesin kazanabileceğini garanti etmez.

[Oyun içi önizleme](Previews/CombatPressure/Gameplay.png) · [Yönlendirme](Previews/CombatPressure/Onboarding.png) · [Kontrol özeti](Previews/CombatPressure/QA.json) · [Can/DPS bütçeleri](Previews/CombatPressure/Balance.csv) · [Sabit karakter denemesi](Previews/CombatPressure/Hands_Off_Probe.csv).

Android development APK: `CubeInvasion-CombatPressure.apk`, 47.3 MiB. Gerçek Android cihazda oynanış/performans testi henüz yapılmadı.
