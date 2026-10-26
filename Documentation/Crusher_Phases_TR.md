# Crusher — oynanabilir faz hedefleri

GDD §10'daki Crusher akışı, önceki üç ayrı can eritme karşılaşmasının üzerine bağlandı. Claude ile yapılan Void Toy modelleri, materyalleri, animasyon rig'leri, UI yerleşimi ve küçük resimler korunur. Yeni bir model veya UI stili üretilmedi.

## Üç hedef

1. **Zırh:** mevcut LeftArmor / RightArmor parçaları ayrı hedeflerdir; her biri 36 can taşır. Otomatik ateş, görüş hattı açık olan bossun sağlam paneline yönelir. Mermi panelin gerçek dünya konumuyla çarpışır. Fazla hasar diğer panele taşmaz; kırık panel tekrar hasar almaz. Parça, aynı mesh ve materyali kullanan bir kopyayla DOTween üzerinden düşer; model varlığı değiştirilmez. Normal destekler de temizlenmelidir.
2. **Destek:** üç destek birimi mevcut **84 canlık** faz bütçesini paylaşır. Boss gövdesi korunur; otomatik hedefleme ve oyuncu mermileri korunan gövdeyi hedef almaz. Turkuaz bağlantılar yaşayan destekleri gösterir. Boss, 1,2 saniye önceden gösterilen ve yönünü sonradan değiştirmeyen bir itme konisi kullanır. Son destek öldüğünde koruma sistemi devre dışı kalır ve boss bu küpten ayrılır. Bir sonraki küpe yine oyuncunun kapıdan geçmesiyle gidilir.
3. **Çekirdek:** zırh geri gelmez; bossun kalan **96 canı** ve rota destekleri temizlenir. Mevcut radyal/yelpaze saldırıları korunur.

Ortak boss barı **252 → 180 → 96 → 0** ilerler. Her aşama ayrı 20 saniyedir. Kapılar canı veya fazı sıfırlamaz; rota faz atlatmaz. Karakter, kamera, tek parmak kontrolü, lazerler, barikat çarpışmaları ve geri sayım gerilimi korunur. Süre bitmesi hâlâ collapse/fail üretir; yeniden deneme ilk fazı ve 252 canı geri getirir.

Elit rota, desteklerden birinin can payını ve mevcut saldırı/hız üstünlüğünü korur; üç birimin toplam canı yine 84'tür. Rota önizlemeleri korunan boss + üç destek için dört hedef gösterir. Director, bu fazın DPS bütçesinde korunan gövdeyi ikinci kez zorunlu can olarak saymaz. İlk ve son fazın normal destekleri mevcut 20 saniyelik bütçeye göre üretilir.

Varil patlaması son destekle birlikte korunan bossu da kaldırabilir. Patlama başlangıcındaki hedefler bir kez ziyaret edilir; koleksiyon değişimi diğer düşmana çift hasar veya liste sınırı hatası üretmez.

## Kurulum

Sahnedeki `InvasionPrototype.crusherPhases`, `Resources/CrusherPhaseSettings.asset` varlığına bağlıdır. İtme hazırlığı, toparlanması, hasarı, kuvveti ve menzili Inspector'dan düzenlenebilir. `Cube Invasion > Content > Install Crusher Phase Objectives` eksik referansı kurar; görsel üreticileri çalıştırmaz. Ana sahneye yalnız bu referans eklendi, önceki serileştirilmiş değerler korunmuştur.

HUD'nin mevcut hedef alanında ZIRH / KALKAN / ÇEKİRDEK ve kalan hedef gösterilir. Aynı alanı kapatacak tekrar eden boss öğretici satırı ve savaş sırasında kapıdaki kilit metinleri bu modda gizlidir; rota açıldığında kapı metinleri geri görünür. UI boyutları, konumları ve stilleri değiştirilmez.

Yerel playtest kaydına `boss_objective_started`, `boss_armor_broken`, `boss_support_destroyed`, `boss_support_disabled` olayları eklenir. Mevcut kayıt/bankalama ve retry yolu kullanılır; yeni profil şeması veya kayıt sıfırlaması yoktur.

## Doğrulama

- `CrusherBudgetCheck`: dört silah ve 128 seed üzerinde **2.048** boss rota teklifi; zorunlu boss, üç destek önizlemesi ve ≤20 sn teorik can bütçesi.
- `CrusherPhaseCheck`: dört silah × iki kapı yönü = **sekiz tam boss koşusu**. Gerçek lazer/panel isabeti, fazla hasarın tek panelde kalması, kırık parçanın görünürlüğü, korunan gövdenin hedef dışı olması, drone hasarı, koniden kaçış, ortak can sürekliliği, final, timeout ve retry.
- Aynı test, tek varille üç desteğin ve korunan bossun aynı hasar çağrısında kaldırılmasını da sınar.
- Eski P0/P1 ve P2 kampanya kontrolleri; rota üretimi/oynanışı, koşullu altın/kayıt kurtarma ve savaş basıncı kontrolleri çalıştırılır. Eski P2 fixture'ı yalnız kendi oturumunda yeni boss ayarını kapatır; gerçek fazlar yeni entegrasyon testinde sınanır.
- Koruma özeti: model seti, kütüphaneler, içerik kataloğu ve kullanıcının değiştirdiği animasyon inceleme görseli dahil **267 dosyanın** başlangıç/son SHA-256 değerleri eşleşir.

[QA kaydı](Validation/Crusher_Phases_QA.json) · [Zırh](Validation/Crusher_Armor.png) · [Destek](Validation/Crusher_Support.png) · [Çekirdek](Validation/Crusher_Core.png)

Ekran görüntüleri mevcut model animasyonları yerleştikten sonra kontrollü Play Mode'dan alınmıştır. Testler matematiksel bütçe ve programatik karşılaşma doğrulamasıdır; gerçek oyuncu zorluk değerlendirmesi veya fiziksel Android cihaz performans testi değildir. GDD'nin diğer bossları, ek düşman arketipleri, özel mod varyantları ve 60 bölümlük içerik kapsamı bu çalışmada tamamlanmış sayılmaz.
