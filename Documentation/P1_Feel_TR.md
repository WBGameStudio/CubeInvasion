# P1 / Feel — ilk uygulama

GDD §22 P1 kapsamı: düşman saldırıları, varil, can/altın, UI ve SFX/VFX. Mevcut P0 commit'i üzerinde ilerlenmiştir. Kullanıcının sahne, rota asset'leri, proje ayarları, Cinemachine ayarları ve eski oyun scriptleri korunmuştur. Sahne üreticisi çalıştırılmamıştır.

## Oynanış

- Grunt: 0.5 sn çember uyarısı, yakın alan saldırısı (5 hasar). Uyarıdan uzaklaşınca darbe boşa gider.
- Sprinter: 0.65 sn yön uyarısı, sabit doğrultuda 0.32 sn hamle (7 hasar). Hamle sırasında oyuncuyu yeniden takip etmez.
- Gunner: risk ≥2 rotada ilk Grunt yerine görünür; toplam hedef sayısı korunur. 1 sn çizgi uyarısı ardından 3 mermi (6 hasar), her mermi arasında 0.13 sn. Silüetinde ayrı namlu, mor renk vardır.
- Doğuşlarda 0.65 sn saldırı güvenliği ve halka efekti; oyuncunun küpe giriş güvenliği 1.3 sn.
- İkinci küpten itibaren iki varil, alçak siper ve +20 can bulunur. Varil 12 HP, 0.45 sn uyarı fitili, 2.2 birim yarıçap, düşmana 30 / oyuncuya 10 alan hasarı uygular. Yakındaki varil de kendi uyarı fitiliyle tetiklenir.
- Siper oyuncu/düşman hareketini ve düşman mermilerini engeller; kullanıcı revizyonuyla oyuncunun hedef kilidi ve mermileri siperden etkilenmez. Bu P1'de siper çarpışması basit düzlemsel yarıçap yaklaşımıdır; NavMesh veya tam fizik navigasyonu değildir.
- Düşman ölümü 3 altın pickup bırakır. Aktif savaşta toplanan altın bekletilir. Clear anında kalan pickup'lar mıknatısla toplanır; 10 temizlik altınıyla birlikte banka güncellenir. Fail anında yalnız aktif küpün bekleyen altını kaybolur.
- Can pickup'ı +20 verir, 100 üstüne çıkmaz. Tam canda savaş sırasında yerde kalır.
- Hasarda kısa görsel yanıp sönme, 0.55 sn tekrar hasar koruması ve itme vardır. Açık kenardan düşme kuralı korunur.

En yakın düşmana otomatik kilit ve ateş, kapıdan güçlendirme alma, 20 saniye kuralı, aynı karakter/kamera/sahneyle devam etme ve küplerin yükselip düşmesi korunmuştur.

## Sunum ve mobil UI

`MobileCombatHud` çalışma anında Canvas oluşturur. Canvas Scaler + Screen.safeArea kullanır; can ve süre barları, hedef, küp, banka/bekleyen altın ve buff bilgisi gösterilir. Son 5 saniye alarm ve renk uyarısı vardır. Joystick parmak kimliğine bağlıdır; ikinci dokunuş hareketi bıraktıramaz. Pause, odak kaybı ve geçişte input sıfırlanır. Pause / ses / retry dışında rota seçim butonu yoktur. Ses tercihi yerel kayda alınır.

`CombatFeedback`: shot, hit, hurt, explosion, gold, heal, clear, alarm, fail ve gate için 10 sentezlenmiş prototip ses. En fazla 6 ses kanalı kullanılır; yoğunlukta sıradan atış/vuruş sesleri atlanır, kritik sesler öncelik alır. Sonradan lisanslı/özgün final seslerle değiştirilebilir. Müzik ve cihaz haptikleri bu teslimde yoktur.

Muzzle flash, vuruş kıvılcımı, düşman hit flash, ölüm/patlama/ödül halkaları ve son saniye vurguları eklendi. Efektler en fazla 48 aktif nesneyle sınırlıdır. Mermiler ve efektler havuza döner. Renk bazlı materyal önbelleği, sürekli mermi üretiminde yeni materyal açılmasını engeller. Efektler oyun saatini durdurmaz; timeout sözleşmesi korunur.

## Dosyalar

- `Runtime/InvasionPrototype.Feel.cs`: P1 saldırıları, etkileşimler, hasar/pickup ve sunum bağlantıları; çekirdek akış partial sınıf üzerinden korunur.
- `Runtime/MobileCombatHud.cs`: Canvas, safe area, joystick ve Türkçe HUD.
- `Runtime/CombatFeedback.cs`: sınırlı ses kanalları ve efekt havuzları.
- `Runtime/P1PerformanceProbe.cs`: editör/development build profil örnekleyicisi.
- `Editor/P1ProfileMenu.cs`: Play Mode sırasında 300 karelik profil başlatır.
- `Editor/PrototypeSmokeCheck.cs`: P0 regresyonları ve P1 entegrasyon kontrolleri.

## Doğrulama

Unity 6000.4.8f1, Android aktif hedefiyle ayrı prototip kopyasında derlendi ve Play Mode kontrolleri geçti. Ana sahne/ayar varlıklarına test sırasında dokunulmadı.

P0: kamera ve ekran yönü, hareketten bağımsız en yakın hedefe ateş, kapılar, tek ödül, üç küp, 20 sn fail, pause ve sürekli aynı nesnelerle geçiş.

P1: Canvas/safe area, gerçek pointer event'leriyle joystick, parmak kimliği, Gunner ve Sprinter uyarı/saldırı sırası, mermi havuzu, siper arkasındaki düşmana kilit ve ateş, düşman mermisi için siper, can üst sınırı, bekleyen altın, gecikmeli/zincirleme patlama, düşmanın hedef sayısından çıkarılması, oyuncuya alan hasarı, tekrar hasar koruması, ölümcül mermi/fail temizliği ve efekt bütçesi.

Başarı işaretleri: `CUBE_P0_SMOKE_OK`, `CUBE_P1_SMOKE_OK`, `CUBE_P1_PROFILE_OK`.

540×960 görsel çıktı: [P1_Preview.png](Validation/P1_Preview.png).

180 karelik editör örneklemesi: [P1_Profile_Editor.json](Validation/P1_Profile_Editor.json). Yaklaşık 16.70 ms medyan, 16.71 ms P95 kare süresi; en yüksek kare GC tahsisi yaklaşık 17.8 KB. Bu, 60 FPS hedefli Mac editör örneğidir. Main Thread sayacı beklemeleri ve editör yükünü de içerir; saf oyun CPU süresi veya Android GPU performansı değildir. Gerçek cihaz kabul kapısı açık kalır.

Editör logunda UnityEditor.Search başlangıç indeksleyici hatası ve yerel Xcode Metal Toolchain eksikliği görüldü; C# derlemesi/oyun kontrolleri ve görsel çıktı tamamlandı. Bu yerel araç sorunları mobil cihaz performans testi yerine geçmez.

## Sonraki kabul kapısı

Android APK/AAB derlemesi ve gerçek cihazda joystick/safe area, ses miksajı, pil/ısı, GC ve CPU/GPU profilini doğrulamak. Ardından hareket animasyonları/final modeller, özel ses varlıkları, müzik/haptik, balans ve P2 içerik (ek düşman/silah, bölümler, Gold Rush, boss). UI görseli P1 işlevsel temelidir; nihai sanat teslimi değildir.
