# GDD v1.1 — Dallanan küpler ve ödüller

26 Ekim 2026. GDD §26 kapsamındaki iki kapılı rota sistemi, mevcut yedi görevli kampanyaya bağlandı. Seçim kapının içinden yürüyerek yapılır. Karakter, kamera ve sahne aynı kalır; sonraki küpler aşağıdan yükselir. Bu çalışma GDD'nin bütün post-MVP içeriğinin tamamlandığı anlamına gelmez.

| Kapı | Etki | Süre / koşul | Risk |
|---|---|---|---|
| Şifa | +30 can, maksimum canı aşmaz | Kapıdan geçerken | 1 |
| Hasar | +%15 hasar | 2 küp | 2 |
| Hızlı ateş | +%12 atış hızı | 2 küp | 2 |
| Kalkan | 40 hasarı emer; fazlası cana gider | Seçilen küp | 1 |
| Altın | +40 altın | Seçilen küp temizlenince | 3 |
| Patlama | 2 varil; patlama hasarı +%20 | Seçilen küp; oyuncu da etkilenir | 2 |
| Mıknatıs | Altın toplama yarıçapı +%35 | 2 küp; sağlık paketini etkilemez | 1 |
| Elit | Bir elit düşman; +75 altın | Seçilen küp temizlenince | 3 |

Tekrarlanan hasar/atış/mıknatıs ödülleri süreyi yeniler, sınırsız birikmez. Üst sınırlar sırasıyla %35/%25/%35. Kalkan düşüşü ve zaman aşımını engellemez. Başarısızlık ve yeni görev geçici etkileri sıfırlar; kalıcı geliştirmeler korunur. Elit düşmanın canı ×1,35, hızı ×1,10, saldırı hasarı ×1,15 olur; mevcut orijinal model üzerinde altın bir taç taşır.

## Karşılaşmalar

Her kapı, ayrı bir karşılaşma kopyası taşır; ana içerik kataloğu oyun sırasında değiştirilmez. Seed, görev, küp ve silahın temel DPS değeri aynıysa üretim deterministiktir. Can %25 altındayken şifa ağırlığı artar, garanti olmaz. İlk görevin ilk ayrımı öğretim amacıyla şifa/hasardır. Diğer ayrımlarda iki farklı ödül ve en az bir risk 1–2 seçeneği bulunur.

Risk; düşman karışımını, destek sayısını ve siper kullanımını etkiler. Elit altın toplama görevinde sunulmaz. Boss'un zorunlu fazları ve toplam canı, görev küp sayısı ve altın kotası değişmez. Can bütçesi, temel silah DPS'sinin %72'si ve 2,5 saniyelik hareket payıyla sınanır. Geçici güçlendirmeler bu bütçeyi şişirmez. Bu teorik sınır, gerçek oyuncunun her rotayı 20 saniyede bitireceği garantisi değildir.

İniş çevresinde en az 1,8 birim boşluk korunur. Giriş koruması ve telegraph'lar sürer. Kapılarda ödül, risk ve hedef sayısı; HUD'da ayrıca ana tehdit sınıfı gösterilir. A/B bilgi kartları tıklanabilir değildir ve joystick alanını kapatmaz. Savaş sırasında kapılar kapalıdır; hedef tamamlanınca teklifler mevcut cana göre kesinleşir ve görünür olur.

## Düzenleme ve kayıt

Sahnedeki `InvasionPrototype.branchSettings`, `Resources/BranchRouteSettings.asset` varlığına bağlıdır. Seed, DPS payı, hareket payı ve elit can çarpanı Inspector'dan değiştirilebilir. İlk iki ödül mevcut HealthRoute/DamageRoute varlıklarını; diğerleri Resources altındaki Branch varlıklarını kullanır. `Cube Invasion/Content/Install GDD Branch Routes` kurulumu gerektiğinde bu bağlantıyı yeniler. Özelliği boş bırakmak eski prototip rotalarını korur.

Koşullu altın, mevcut atomik küp bankalama işleminin parçasıdır. Disk yazımı başarısızsa tutar korunur; tekrar deneme ödülü bir kez bankalar. Yerel, 256 olayla sınırlı playtest kaydına branch_offered, branch_selected, branch_result ve route_fail eklenir. Seed, teklifler, yön, tepki süresi, can ve güçlendirmeler kaydedilir; ağa gönderilmez.

## Doğrulama

- 10.240 rota çifti: farklı ödüller, güvenli seçenek, sekiz ödülün kapsanması, deterministik üretim, 20 saniyelik can bütçesi, boss/kota koruması ve değişmeyen katalog.
- Play Mode: gerçek silah aralığı, kalkan ve taşan hasar, buff süresi/yenilemesi, yalnız altına uygulanan mıknatıs, iki varil, elit saldırısı, güvenli iniş ve fiziksel kapı geçişi.
- Geçiş sırasında gelecekteki teklifler hazırlanırken seçilen rota ve oyuncu/kamera kimliği korunur.
- Gerçek profil: erken ödeme yok, koşullu bonus kalıcı kayda girer, bozuk depolama ve tekrar deneme çift ödeme üretmez; yeniden yükleme tutarı korur.
- Mevcut P0/P1, yedi görev/iki yön P2, P3 profil/ekonomi ve savaş basıncı kontrolleri geçti. Eski sabit ekonomi fixture'ları rota özelliğini kapatır; beklenen düşman altınını güncel katalogdan hesaplar. Yeni rota ekonomisi ayrı sınanır.
- Unity 6000.4.8f1 Android IL2CPP ARM64 portrait Development APK derlendi. Yeni rota ve mevcut kamera gerilimi kodlarının APK metadata'sında bulunduğu doğrulandı.

[Sonuç kaydı](Validation/BranchRoutes_QA.json) · [Oyun içi görüntü](Validation/BranchRoutes_Gates.png)

Fiziksel Android cihazda FPS, pil/ısı ve gerçek oyuncu zorluk testi yapılmadı. Genişletilmiş biome/boss/görev içerikleri ve GDD'nin post-MVP kapsamı sonraki çalışmalardır. Kullanıcının kamera, orijinal model, lazer, barikat ve tek parmak ateş ritmi kararları korunmuştur. Sahne dosyasına bu çalışmada yalnız branchSettings referansı eklenmiştir.
