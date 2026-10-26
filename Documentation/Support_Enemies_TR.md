# Destek düşmanları: Kalkanlı ve Tamirci

GDD §5 düşman listesinden iki yeni davranış, Son Hat görevinin son iki küpüne eklendi. Mevcut görev uzunluğu, Bombacı karşılaşmaları, kayıtlar, Claude ile hazırlanan Void Toy görselleri ve arayüz yerleşimleri korunur. Karşılaşmalar çalışma anındaki kopyada değiştirilir; P2Content kataloğu yeniden yazılmaz.

## Kalkanlı

- 30 temel can. Önündeki 120 derecelik sektöre gelen lazerlerde %65 hasar azaltır. Yan ve arka vuruşlar tam hasar verir; alan patlamaları kalkanı aşar.
- Yerdeki mavi yay korunan yönü gösterir. Mevcut Pusher modeli ve animasyonları kullanılır.
- Dönüş hızı 45 derece/saniye ile sınırlıdır. Yakın saldırısını 0,85 saniye önceden gösterir, saldırı boyunca yönünü sabitler ve ardından 1,2 saniye toparlanır. Oyuncu bu aralıkta yanına geçebilir.

## Tamirci

- 20 temel can. 3,2 birim içindeki yaralı bir müttefike 1,8 saniyelik döngüyle 5 can verir; maksimum can aşılmaz.
- İyileştirmeden önce 0,55 saniye mavi bağlantı görünür. Siper bağlantıyı engeller. Menzil veya görüş kaybolursa, hedef ya da Tamirci ölürse işlem kesilir.
- Kendini, başka Tamircileri ve boss karşılaşmasını iyileştirmez. Yakındaki müttefiği takip eder, oyuncu yaklaşınca uzaklaşır.
- Mevcut Grunt modeli, hareket/telegraph animasyonları ve mavi artı işareti kullanılır.

## Karşılaşma ve süre bütçesi

Son Hat küp 4: Kalkanlı + Grunt + Gunner. Küp 5: Kalkanlı + Tamirci + Grunt. Her küpte en fazla üç düşman türü bulunur. Rota yönetmeni, seçilen silah ve ödül riskine göre sıradan düşman sayısını azaltabilir. Bazı düşük DPS/elit rotalarda Tamirci çıkarılabilir; diğer seçenek Tamirci tanıtımını korur.

Rota hesabı, Kalkanlı'nın tamamına önden ateş edildiğini varsayan etkili canı ve bir iyileştirme darbesini bütçeler. Bu, sınırsız iyileştirmeye karşı tamamlanma garantisi değildir: oyuncu Tamirci'yi önceliklendirmeli veya Kalkanlı'nın yanına geçmelidir. Dört silah ve 128 tohum üzerinde 2.048 rota teklifi 20 saniyelik tahmini bütçe ve tür sınırı için kontrol edilir.

## Sahne ve doğrulama

Ayarlar `Assets/CubeInvasion/Resources/SupportEnemySettings.asset` üzerinden değiştirilebilir; ana sahnedeki InvasionPrototype bileşenine bağlıdır. Yeni görsel işaretler ve iyileştirme bağlantıları dinamik üretilir. Ana kamera, karakter, sahne kurulumu ve statik sanat varlıkları korunur.

Editör kontrolleri: `SupportBudgetCheck.Run`, `SupportEnemyCheck.Run`. Davranış kontrolü ön/arka/alan hasarını, dört gerçek silahı, dönüş sınırını, iyileştirme zamanlamasını, can sınırını, duraklatmayı, siper ve ölüm kesintisini, boss korumasını ve gerçek kapı geçişlerini kapsar. Android geliştirme çıktısı ayrıca üretilir; fiziksel Android cihaz performansı bu kontrollerin kapsamında değildir.
