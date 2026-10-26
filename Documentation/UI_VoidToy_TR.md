# Void Toy Arcade — UI uygulaması

Kaynak tasarım: [Cube Invasion — Tasarım Dili](https://claude.ai/artifact/28HFcJ1WJcJWu6RZnEHb2W) canvas'ı. GDD §14 paleti (void #101A2D, kobalt/turkuaz oyuncu, altın ödül, pembe tehlike) teal/yeşil casual görünümün yerine geçti. Menü akışı, ekonomi, kayıt ve oynanış değişmedi.

## Ne değişti

- **Savaş HUD'u:** üst bant üç parçalı. Solda `KÜP x/y` chip'i, ortada büyük sayaç ve altında hedef etiketi (`3 HEDEF`, boss/kota metni), sağda altın chip'i ve pause. Sayaç 5 saniyede turuncu, 3 saniyede kırmızı zemine geçer; çevresindeki halo atar. Tam ekran alarm katmanı yoktur. Seçim aşamasında sayaç yeşil "temizlendi" zeminiyle donar.
- **Can barı** oyuncunun başının üstünde dünya konumundan izlenir; renk %50 altında turuncu, %25 altında pembe. Ayrı `CAN 100/100` yazısı gizlendi, kalkan bilgisi güçlendirme satırına taşındı.
- **SES butonu savaştan kaldırıldı.** Ses Ayarlar sayfasında kalır.
- **Joystick** kenarı yumuşatılmış 256 px disk + ince halka. Eski 64 px pikselli doku kullanılmaz.
- **Rota kartları:** "A/B kapısı" yerine `SAĞ KAPI` / `İLERİ KAPI`. İleri kartı üstte, sağ kartı sağ altta, kapıların ekran yönüyle aynı. Kartta ödül, hedef sayısı + tehdit türü ve 1–3 risk pip'i var. Kart çerçevesi ve dünya içindeki kapı ödül ailesinin rengini alır (can turkuaz-yeşil, güç kobalt, altın/elit altın, kalkan/mıknatıs açık turkuaz, patlama turuncu). Seçim yine yalnız fiziksel kapıdan geçerek yapılır; kartlara dokunulmaz.
- **Sonuç penceresi:** başlık kazanınca altın `TEMİZLENDİ!`, kaybedince pembe `KÜP ÇÖKTÜ`. Her durumda tek turkuaz ana buton (kazanınca Sonraki, kaybedince Tekrar Dene), diğerleri koyu ikincil.
- **Ana üs düzeni (canvas 02):** solda profil chip'i, sağda altın chip'i. Altında biyom etiketi, 7 görevlik yatay küp şeridi ve ince ilerleme çubuğu, ortada kaide üstünde Scout vitrini, iki yanda Silah/Pilot kısayolları, "SIRADAKİ GÖREV" kartı ve tam genişlik turkuaz Oyna butonu. Yol sayfası dikey haritayla aynen kalır.
- **Silahlar düzeni (canvas 03):** sol üstte başlık. Altında kaideli vitrin kartı (ad, açıklama, DPS/menzil); her yükseltme ayrı satır: solda seviye bilgisi, sağda altın renkli fiyat butonu. Kilitli silahta satırların yerini altın "Silahı aç" alır. En altta 4'lü koleksiyon.
- **Sonuç penceresi düzeni:** eski kalkan amblemi yerine neden etiketi (`TIME OUT`, `KENAR`, `CAN BİTTİ`, kazanınca `GÖREV TAMAM`). Altında büyük başlık ve koyu özet kutusu.
- **Eski düzenle kaydedilmiş menü** Play başlarken kendini yeni düzenle yeniden kurar (yükseltme bilgi satırları yoksa). `Apply Void Toy Style` menüsü aynı kurulumu sahneye kalıcı yazar.
- **Menüler:** zemin void, paneller #17233D, ana aksiyonlar turkuaz + koyu yazı, seçili bölüm düğümü turkuaz, tamamlanan yeşil, açık kobalt, kilitli gri; seçili silah kartı kobalt.
- **Dünya:** arena üstü #C9D6EA, yan yüzler #22315A, aktif küpün kenar ışığı turkuaz → 5 sn turuncu → 3 sn pembe. Düşman gövdeleri grafit ailesine çekildi; tepe uzantısı niyet rengini taşır (yakın dövüş pembe, Sniper/Gunner turuncu). Mevcut Original Toy mesh'leri korunur.

## Dosyalar

- `Runtime/VoidToyPalette.cs`: renk token'ları, kontrast için ayrı metin tonları, niyet/ödül/risk eşlemesi.
- `Runtime/VoidToyStyle.cs`: bul-yoksa-oluştur uGUI yardımcıları, AA disk/halka sprite'ları, eski renklerin yeni palete eşlenmesi.
- `Runtime/MobileCombatHud.VoidToy.cs`: HUD yerleşimi, oyuncu üstü can barı, rota kartları.
- `Editor/VoidToyUiBuilder.cs`: `Cube Invasion > UI > Apply Void Toy Style` stili sahneye yazar.

Stil her `Initialize` çağrısında tekrar uygulanır ve idempotenttir; builder çalıştırılmasa da Play'de yeni görünüm gelir. Builder yalnız edit mode görünümünü eşitler. Joystick sprite'ı çalışma anında üretildiği için sahneye kaydedilmez.

## Sınırlar

- Sayaç ve sayılar için tasarımdaki Chakra Petch eklenmedi; tüm metin mevcut Cube Rounded (Fredoka) ile kalır.
- Canvas'taki yeni 3D kadro (Scout Mk-II, grafit istilacı silüetleri, küp kiti varyantları) bu adımda modellenmedi; yalnız mevcut modellerin renkleri değişti.
- Ana üs/Silahlar sayfalarının yerleşimi korunarak yeniden renklendirildi; canvas'taki yeni ana üs kompozisyonu (hero küp vitrini, 10'lu bölüm şeridi) ayrı iş.

## Genişletilmiş kampanya düzeltmeleri — 27 Ekim 2026

Analiz ve tasarımlar canvas'ın "Analiz ve düzeltmeler" satırındadır (12–16).

- **60 bölümde ana üs şeridi:** artık bütün görevleri tek satıra sığdırmıyor; yalnız aktif biyomun 10 bölümünü gösteriyor, sondaki boss karosu daha büyük. Etiket `BİYOM 3 · CIRCUIT ABYSS · 21–30` biçiminde. Kısa katalogda (7 görev) hepsi görünür. `FullCampaignCheck` 10 karonun göründüğünü ve `Home60` görüntüsünü kontrol eder.
- **Hedef etiketi:** uzun boss/Gold Rush hedefleri tek satıra sığacak kadar küçülür.
- **Boss barı:** hedef etiketinin hemen altında, 312×24 ince bar; arenanın ve kapıların üstüne binmez.
- **Kapı tabelaları:** rotalı görevlerde savaş sırasında gizli; rota açılınca oyun fontuyla görünür.
- **Altın:** her yerde Türkçe binlik ayırıcı (`30.645`).
- **Duraklatma:** tam ekran karartma + kart: büyük DEVAM, ses aç/kapat (savaş ekranından kaldırılan SES burada), küp ve kalan süre. Yalnız savaşta açılır.
- **Sayfa başlıkları:** Yol, Pilot, Ayarlar da Silahlar gibi solda beyaz başlık. Pilot geliştirmeleri altın fiyat butonu; rozetler can turkuaz, hız kobalt, alan altın.
- **Biyom kenar ışığı:** küp kenarı biyomun rengini alır (Lava turuncu, Void mor…). Zemin/yan yüz Void Toy materyali renk almadığı için değişmez.
- **Niyet rengi:** Tamirci ve Summoner mor, Bombacı turuncu (önceden pembe).

3D tarafı (Kalkanlı/Tamirci/Bombacı modelleri, tehlike karoları, altın kasası/jackpot sandığı, genişleme silahları ve Summoner/Phaser/Elite restyle) [VoidToy_Art_TR.md](VoidToy_Art_TR.md) içindedir. Birleştirmeden sonra `VoidToyArtBuilder.Reinstall` çalıştırılmalı; `CampaignExpansionBuilder.RefreshActors` eski Summoner/Phaser/Elite'i geri getirir, ardından Reinstall tekrar gerekir.
