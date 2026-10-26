# Void Toy Arcade — UI uygulaması

Kaynak tasarım: [Cube Invasion — Tasarım Dili](https://claude.ai/artifact/28HFcJ1WJcJWu6RZnEHb2W) canvas'ı. GDD §14 paleti (void #101A2D, kobalt/turkuaz oyuncu, altın ödül, pembe tehlike) teal/yeşil casual görünümün yerine geçti. Menü akışı, ekonomi, kayıt ve oynanış değişmedi.

## Ne değişti

- **Savaş HUD'u:** üst bant üç parçalı. Solda `KÜP x/y` chip'i, ortada büyük sayaç ve altında hedef etiketi (`3 HEDEF`, boss/kota metni), sağda altın chip'i ve pause. Sayaç 5 saniyede turuncu, 3 saniyede kırmızı zemine geçer; çevresindeki halo atar. Tam ekran alarm katmanı yoktur. Seçim aşamasında sayaç yeşil "temizlendi" zeminiyle donar.
- **Can barı** oyuncunun başının üstünde dünya konumundan izlenir; renk %50 altında turuncu, %25 altında pembe. Ayrı `CAN 100/100` yazısı gizlendi, kalkan bilgisi güçlendirme satırına taşındı.
- **SES butonu savaştan kaldırıldı.** Ses Ayarlar sayfasında kalır.
- **Joystick** kenarı yumuşatılmış 256 px disk + ince halka. Eski 64 px pikselli doku kullanılmaz.
- **Rota kartları:** "A/B kapısı" yerine `SAĞ KAPI` / `İLERİ KAPI`. İleri kartı üstte, sağ kartı sağ altta, kapıların ekran yönüyle aynı. Kartta ödül, hedef sayısı + tehdit türü ve 1–3 risk pip'i var. Kart çerçevesi ve dünya içindeki kapı ödül ailesinin rengini alır (can turkuaz-yeşil, güç kobalt, altın/elit altın, kalkan/mıknatıs açık turkuaz, patlama turuncu). Seçim yine yalnız fiziksel kapıdan geçerek yapılır; kartlara dokunulmaz.
- **Sonuç penceresi:** başlık kazanınca altın `TEMİZLENDİ!`, kaybedince pembe `KÜP ÇÖKTÜ`. Her durumda tek turkuaz ana buton (kazanınca Sonraki, kaybedince Tekrar Dene), diğerleri koyu ikincil.
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
