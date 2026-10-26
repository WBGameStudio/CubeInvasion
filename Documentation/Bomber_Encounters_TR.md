# Bombacı ve patlama zincirleri

GDD §5'teki Bomber / Bombacı, mevcut düşman sistemine eklendi. Eski enum değerleri değişmedi; Bomber sona eklendi. Sağlık 16'dır. Mevcut Grunt rig'i ve patlayıcı varil parçası birleştirilir: sırtındaki turuncu patlayıcı silüeti ayırt eder. Görsel kütüphane, modeller, materyaller, UI yerleşimi ve bölüm sırası değiştirilmez.

## Davranış

Bombacı yaklaşır ve önceden gösterilen yakın saldırı yapar. Öldüğünde hedef sayısından hemen düşer, normal altınını bırakır ve yerinde **0,8 saniye fitilli**, **2,2 birim yarıçaplı** bir patlayıcı kalır. Uyarı halkası ve alarm kaçış fırsatı verir. Patlayıcı konumu sabittir; oyuncuyu takip etmez.

Patlama temel 10 hasar verir. Oyuncu için mevcut düşman hasar çarpanı uygulanır; varsayılan ayarda 13'tür. Aktif Patlama rotası bu patlama hasarına da etki eder. İtme kuvveti 1,8 ile sınırlıdır. Diğer düşmanlar, açık altın kasaları ve yakındaki variller de etkilenir. Hasarı yeterliyse ikinci bir Bombacı ölür ve **kendi tam fitilini** başlatır; zincir anlık ve kaçınılmaz hasara dönüşmez. Variller mevcut görünür fitillerini kullanır.

Duraklatma fitili durdurur. Fail, retry, yeni görev ve küp temizliği artık patlayıcı bırakmaz. Son zorunlu düşman ölünce normal clear koşulu hemen sağlanır: ceset patlayıcısı 20 saniyelik hedefe ek bekleme borcu yaratmaz ve başarıdan sonra oyuncuya zarar vermez. Aynı hedef patlamada iki kez vurulmaz veya iki kez ödüllendirilmez.

## Karşılaşmalar

Varsayılan hedef **Son Hat (mevcut beşinci görev)**:

- Küp 1: mevcut karşılaşma korunur.
- Küp 2: **2 Bombacı + 1 Bruiser / Ezici**; variller etkin.
- Küp 3: **2 Bombacı + 1 Gunner + 1 Grunt**; variller etkin.
- Küp 4–5: mevcut karşılaşmalar korunur.

Rota ödülleri, elit güçlendirmesi ve risk kuralları korunur. Bombacı bulunan dalda Altın rotasının ek desteği yine Bombacı olur; yeni bir dördüncü arketip eklenmez. Hasar bütçesi Bombacı için doğru 16 canı kullanır. Dört silahın rotaları, 20 saniyelik teorik can bütçesi ve en fazla üç arketip için sınanır.

Karşılaşmalar görev başlangıcında oluşturulan bir **çalışma zamanı kopyasında** genişletilir. Kaynak `P2Content.asset` dosyası, yedi görev, beş küplük Son Hat, açılma sırası ve profil şeması korunur. Kayıt göçü veya ilerleme sıfırlaması gerekmez.

## Düzenleme

`InvasionPrototype.bomberSettings`, `Resources/BomberSettings.asset` varlığına bağlıdır. Hedef görev indeksi, fitil, yarıçap ve hasar Inspector'dan düzenlenir. `Cube Invasion > Content > Install Bomber Encounters` eksik referansı kurar; görsel üreticileri çalıştırmaz. `enabled` kapalıyken eski görev karşılaşmaları kullanılır. Sonraki çalışmalarda düşman elle kullanılabilsin diye Bomber enum'u ve davranışı kalır.

Sahneye bu adımda yalnız bomberSettings referansı eklenir. Kullanıcının görsel seti, kütüphaneleri, içerik kataloğu, Crusher/Gold Rush ayarları ve önizlemesi dahil 269 dosya başlangıç SHA-256 değerleriyle karşılaştırılır. Mevcut kamera, joystick, lazer, barikat, geri sayım gerilimi ve sahne sürekliliği korunur.

Yerel kayıt, `bomber_fuse_started` ve `bomber_exploded` olaylarını mevcut sınırlı playtest günlüğüne ekler; ağ gönderimi yoktur.

## Doğrulama

- `BomberBudgetCheck`: dört silah, 128 seed, iki küp, iki kapı = **2.048 teklif**. ≤20 sn teorik bütçe, ≤3 arketip, Bombacı girişinin ve kaynak kataloğun korunması.
- `BomberCheck`: 16 HP, mevcut rig, dört gerçek silahın lazer isabeti; fitilden önce hasar olmaması; kaçış; içeride kalınca ölçekli hasar; pause; zincir Bombacının tam fitili; son saniyede hedeften sonra ek bekleme olmaması; fail temizliği; gerçek kapı geçişi; genişletilmiş beş küplük Son Hat'ın tamamlanması.
- Mevcut P0/P1, yedi görevin iki yönü, sekiz Gold Rush koşusu, sekiz Crusher koşusu ve savaş basıncı testleri çalıştırılır. Eski P2 fixture'ı yeni Bombacı ayarını yalnız kendi test oturumunda kapatır; gerçek genişletilmiş görev ayrı entegrasyon testinde sınanır.

[QA kaydı](Validation/Bomber_QA.json) · [Oyun içi görünüm](Validation/Bomber_Gameplay.png)

Teorik bütçe ve programatik bitiriş gerçek oyuncu zorluk testi değildir. Fiziksel Android cihaz performansı, ısı/pil ve dokunmatik testi yapılmadı. GDD'nin Shielder, Healer, Summoner ve Phaser düşmanları ile diğer özel mod/biome/boss kapsamı sonraki işlerdir.
