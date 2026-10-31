# Kaçak Küp — ana karakter modelleme ve animasyon brief'i

Scout Mk-II'nin yerini alacak yeni oyuncu karakteri. Bu belge modelleme, rig, animasyon ve oyun içi bağlantılar için tek referanstır. Ölçüler Unity dünya birimidir (1 birim = 1 m).

## 1. Karakter fikri

Küp istilacılarının fabrikasında üretilmiş ama programı bozulmuş, kendi türüne karşı savaşan küçük bir küp. Çalışma adı **KİP** (gövdesinde "K-1P" seri numarası).

- Duygusal kanca: istilacılarla aynı geometriyi taşır, ama onların grafit ve köşeli dünyasına karşı açık renkli, yuvarlatılmış, sıcak bir plastik oyuncaktır. Oyuncu ilk bakışta "o da onlardan biri ama bizim tarafta" demeli.
- Kişilik: cesur, biraz aceleci, kolay şaşıran; yenilince yıkılmaz, hemen silkinip kalkar. İfadeyi gövdesinin ön yüzündeki **ekran-yüz** taşır.
- İmza öğeler: (1) ön yüzde ekran-yüz, (2) boynuna sarılı, arkasında savrulan **cyan atkı**, (3) tepede kısa anten ve ucunda sinyal topu, (4) kısa, tıknaz iki bacak.

## 2. Okunabilirlik hedefleri

Kamera ortografik, eğim 45° ve yön 45° (`ArcadeFollowCamera`); +X+Z yönüne bakar. Düşmanlar çoğunlukla arenanın üst tarafından geldiği için Kip oyunun büyük kısmında kameraya **sırtını ve üst yüzünü** gösterir; ekran-yüz ancak kameraya doğru nişan aldığında görünür (ilk model incelemesi `RogueCube_InGame.png`). Telefonda yaklaşık 60–80 px boyundadır. Bu yüzden:

- **Silüet tek bakışta okunmalı:** kare gövde, iki küçük bacak ve arkaya uzanan atkı. Atkı hareket yönünü gösteren en önemli işarettir.
- **Üst yüz kimlik taşır:** kamera gövdenin üstünü en çok görür. Üst yüze cyan bir şerit ve antenin tabanını koy; karakter düşman kalabalığında yukarıdan tanınmalı.
- **Renk ayrımı:** düşmanlar grafit (#3A4361), zemin açık (#C9D6EA), boşluk koyu (#0E1626). Kip'in gövdesi krem-beyaz plastik, ana vurgu **Cyan #22C7D6**, ikincil **Kobalt #395DE6**. Pembe (#F05A73) ve turuncu (#FF9C32) kullanma; bu renkler oyunda tehdidi anlatır.
- **Yön okunmalı:** ekran-yüz ve silah her zaman nişan yönünü, atkı ve bacaklar hareket yönünü göstermeli.
- Hedef: Kip, 1080×1920'de 5,6 ortografik boyutta ekran yüksekliğinin en az %6'sını kaplamalı. Kamera yakınlaştırması ayrı bir iştir (game feel maddesi 4); model buna göre küçük detaya boğulmamalı.

## 3. Ölçüler ve oranlar

| Ölçü | Değer | Not |
| --- | ---: | --- |
| Toplam boy (ayak → anten topu) | 1,25 | Mevcut Scout ~1,28; can barı kökün 1,35 üstünde kalır |
| Gövde küpü | 0,72 × 0,66 × 0,64 (G × Y × D) | Hafif geniş; kenar pahı 0,08, yumuşak yuvarlatma |
| Bacaklar | 0,16 × 0,22 × 0,20 | Gövde altından 0,20 içerde, aralarında 0,22 boşluk |
| Ayak | 0,20 × 0,07 × 0,26 | Öne doğru uzun, burnu yuvarlak |
| Kollar | 0,12 kalınlık, 0,30 uzunluk | Gövdenin yan yüzlerinden çıkan kısa kütükler |
| Anten | 0,16 sap + 0,07 çaplı top | Gövde üstünde, merkezin biraz arkasında |
| Atkı | 0,08 kalınlık, uç kısım 0,45 uzunluk, 0,14 genişlik | Boynu (gövdenin üst kenarını) sarar, arkadan iki kuyruk sarkar |
| Ayak izi | ≤ 0,80 genişlik | Mermi isabet yarıçapı 0,5'tir; silüet bunun dışına çok taşmamalı. Kollarla toplam genişlik 0,93 olur (ilk modelde ayak izi 0,58) |

Oran kuralı: gövde toplam boyun yaklaşık %55'i, bacaklar %18'i. Kafa yok; **gövde kafadır**. Bu oyuncak hissini ve okunabilirliği sağlar.

## 4. Görünüş ve malzeme

Tüm Void Toy seti iki materyal kullanır: `VoidToy_Plastic` ve `VoidToy_Tinted` (`Assets/CubeInvasion/Art/VoidToy`). Kip de aynı kurallara uymalı:

- Renk **vertex renginde** (lineer uzay), emisyon **vertex alfasında**, çalışma anında boyanacak bölgeler **UV0.x maskesinde**.
- Gövde: krem-beyaz #EEF3FB; alt kenarda 0,05 kalınlıkta kobalt bant (zemine oturduğu hissini verir).
- Üst yüz: cyan şerit (emisyon 0,3) + "K-1P" kabartması (yalnız geometri, yazı dokusu yok).
- Ekran-yüz: ön yüzde 0,46 × 0,34 içe gömük, koyu (#07111F) cam panel; gözler ve ağız ayrı parçalar, cyan, emisyon 1.
- Atkı: cyan #22C7D6, iç yüzü koyu cyan #0D8494. Uçlarında küçük bir kobalt püskül.
- Bacak ve kollar: kobalt #395DE6, eklem yerlerinde koyu grafit halka.
- Anten topu: emisyonlu cyan; ateşte bir an altın (#FFC24F) yanabilir (runtime tint, UV0.x maskeli).
- Hasar alınca beyaz flaş `_Color` alfası ile gövde (Core) parçasında çalışır; Core'u tek parça tut.

## 5. Rig: parçalar ve hiyerarşi

Mevcut sistem skinned mesh değil, DOTween ile sürülen **katı parça rig'i**dir (`ToyActorAnimator`). Kip de bu yapıda üretilir. Parça adları kod tarafından okunur; aynen kullan.

| Parça | Ebeveyn | Pivot | Davranış (mevcut kod) |
| --- | --- | --- | --- |
| `Hips` **(yeni)** | — | bacakların üst ortası | Alt gövde: hareket yönüne döner |
| `LeftLeg`, `RightLeg` | Hips | kalça eklemi | Adım atar, yükselir |
| `Core` | — | gövde merkezinin 0,05 altı | Gövde, hasar flaşı burada |
| `LeftEye`, `RightEye` | Core | göz merkezi | Adı `Eye` ile bittiği için göz kırpar |
| `Mouth` **(yeni)** | Core | ağız merkezi | İfade için şekil değiştirir |
| `LeftArm` | Core | omuz | Koşarken sallanır, hazırlıkta kalkar |
| `RightHand` | Core | omuz | Silahı taşır, ateşte geri teper |
| `Antenna` **(yeni)** | Core | anten tabanı | Gecikmeli yay hareketi |
| `ScarfA`, `ScarfB`, `ScarfC` **(yeni)** | Core → ScarfA → ScarfB | her segmentin ön ucu | 3 halkalı zincir, ikincil hareket |

Kurallar:

- Silah ayrı bir aksesuardır (`Pulse pistol` vb.) ve `RightHand` eklemine bağlanır. Merminin çıktığı nokta animasyondan bağımsız ve sabittir; namlu ucu nötr pozda bugünkü konumda (gövde merkezinin 0,35 sağı, 0,40 önü) olmalı.
- Model dosyası **gerçek dünya ölçüsünde** üretilmeli. Bugün Scout mesh'i gövde transform'unda 0,65/0,55/0,65 ile basıklaştırılıyor; Kip için bu ölçek 1 olacak (bkz. Bölüm 9).
- Her parçanın bir de LOD1 mesh'i olmalı (yaklaşık %50 üçgen).
- **Bütçe:** toplam ≤ 2.400 üçgen (oyuncu bütçesi 3.000). Ekran-yüz parçaları ≤ 200, atkı ≤ 300 üçgen.

## 6. Ekran-yüz ifadeleri

Gözler ve ağız ayrı parçalar; ifade, kodla ölçeklenerek ya da hazır mesh varyantı takılarak değişir. Her ifade bir tetikleyiciye bağlı:

| İfade | Göz | Ağız | Tetikleyici |
| --- | --- | --- | --- |
| Nötr / odak | yuvarlak kare | düz çizgi | Varsayılan |
| Kararlı | alt kenarı düz, hafif kısık | küçük ters yay | Ateş ederken (0,3 sn tutulur) |
| Şaşkın | büyük, yuvarlak | küçük "o" | Hasar alınca (0,4 sn) |
| Acı | "> <" şeklinde çizgi | dalgalı | Can %25'in altındayken |
| Mutlu | ters "U" | geniş yay | Küp temizlenince, altın toplarken (0,25 sn) |
| Baş dönmesi | spiral (iki göz ters yönde döner) | yamuk | Kenardan düşme, yenilgi |
| Göz kırpma | Y ekseninde %8'e iner | — | 2,4 sn döngü (mevcut kod) |

## 7. Animasyon listesi

Süreler 60 fps varsayımıyla. "Prosedürel" mevcut DOTween rig'iyle koddan, "anahtar" elle anahtarlanmış klip demektir. Öncelik: **P1** ilk sürüm için şart, **P2** ilk güncellemede, **P3** cila.

| # | Animasyon | Süre | Tür | Öncelik | Ana noktalar |
| --- | --- | --- | --- | --- | --- |
| 1 | Bekleme (idle) | 2,4 sn döngü | Prosedürel | P1 | Gövde nefes alır (Y %1 esneme), anten yavaşça sallanır, atkı hafif dalgalanır, göz kırpar |
| 2 | Koşu | 0,40 sn adım döngüsü | Prosedürel | P1 | Gövde öne 8° eğilir, her adımda 0,06 zıplar; zemine inişte Y %6 ezilir, havada %4 uzar. Atkı yatay savrulur |
| 3 | Yana / geri yürüme | 0,45 sn | Prosedürel | P1 | **Alt-üst gövde ayrımı:** `Hips` hareket yönüne, `Core` hedefe bakar; geri yürürken adım boyu %70 |
| 4 | Ateş tepmesi | 0,12 sn | Prosedürel | P1 | `RightHand` 14° geri, gövde 0,035 geri itilir, anten öne savrulur; ekran-yüz "Kararlı" |
| 5 | Hasar alma | 0,18 sn | Prosedürel | P1 | Gövde darbenin tersine 10° yatar ve ölçek patlar; ekran-yüz "Şaşkın"; atkı ileri fırlar |
| 6 | Sahneye giriş (spawn / geçiş inişi) | 0,35 sn | Anahtar | P1 | Yukarıdan düşer, inişte Y %25 ezilir, sonra %10 uzar ve dinlenmeye oturur; inişte toz halkası |
| 7 | Kenardan düşme | 0,60 sn | Anahtar | P1 | Kenarda bir an kollarını çırpar (0,2 sn), sonra arkaya devrilip dönerek düşer; ekran-yüz "Baş dönmesi" |
| 8 | Yenilgi (can bitti) | 0,80 sn | Anahtar | P1 | Dizlerinin üstüne çöker, anten sarkar, ekran yanıp söner ve kararır |
| 9 | Zafer | 1,00 sn | Anahtar | P2 | Zıplayıp havada 360° döner, inişte poz verir; atkı daire çizer; ekran "Mutlu" |
| 10 | Kapıdan geçiş | 0,50 sn | Prosedürel | P2 | Koşudan küçük bir atlamaya geçer, kapı eşiğinde atkı öne savrulur |
| 11 | Kısa atılma (dash) | 0,22 sn | Anahtar | P2 | Gövde öne 25° yatar ve yatayda %20 uzar; arkada 3 karelik iz bırakır. Oynanışta dash eklenirse kullanılır |
| 12 | Altın / can toplama | 0,20 sn | Prosedürel | P3 | Küçük bir sıçrama, anten altın yanar |
| 13 | Menü pozu (Home yol haritası) | 3,0 sn döngü | Anahtar | P3 | Bekleme ve arada bir el sallama; yol haritasındaki 2D "Path scout"un yerine geçer |

Hareket ilkeleri:

- **Ezilip uzama:** her inişte ezilme, her kalkışta uzama; hacmi koru (Y %10 ezilirse X ve Z %5 genişler).
- **Hazırlık + aşma:** atılma, zafer ve sahneye girişte önce küçük ters hareket, sonra hedefi biraz aşıp geri dönme.
- **İkincil hareket:** atkı ve anten gövdeden 2–4 kare geride gelir; ani durmalarda ileri savrulup yerine oturur.
- **Kök hareketi yok:** simülasyon karakterin konumunu sürer; animasyonlar yalnız görseldir.

## 8. Game feel ile senkron

Karakter animasyonları, game feel düzeltmeleriyle aynı anda tetiklenmeli:

- Ateş tepmesi (#4) her merminin çıktığı karede başlar.
- Hasar alma (#5), aynı karede kısa hit-stop, ekran kenarında pembe flaş ve kamera sarsıntısıyla birlikte oynar.
- Sahneye giriş (#6) küp geçişinin bitişine bağlanır; Choice → Transfer → Combat arasındaki ölü zamanı doldurur.
- Can düşükken (%25 altı) ifade "Acı"ya geçer, atkının rengi bir tık koyulaşır.

## 9. Koda yapılacak bağlantılar

Bu bölüm model teslim edildikten sonra kod tarafında yapılacak işlerdir; modelci için bilgi amaçlıdır.

1. `ToyArtLibrary`'ye yeni anahtar: `Rogue cube`. `integratedFace=true` (eski Visor aksesuarı gizlenir), `mechanicalRig=true`.
2. `InvasionPrototype` içinde oyuncu `AnimateActor(playerBody, player, "Rogue cube", …)` ile bağlanır; gövde ölçeği (0,65/0,55/0,65) 1'e çekilir.
3. `ToyActorAnimator`: `Hips` için alt-üst gövde ayrımı, `Scarf*` zinciri için yay simülasyonu, `Antenna` gecikmesi, `Mouth` ve göz ifade değişimi, iniş ezilmesi.
4. Namlu ucu aramasına (`InvasionPrototype.Laser.cs`) `DOTween Reactions/Core/RightHand` yolu eklenir.
5. Home yol haritasındaki 2D "Path scout" görseli, Kip'in menü pozundan alınan render ile değiştirilir; menü küçük resimleri yeniden üretilir.

## 10. Üretim yolu

Önerilen: mevcut set gibi Kip de koddan üretilsin (`Editor/VoidToyMeshBuilder.cs` ve `VoidToyArtGeometry.cs`). Böylece pah dili, vertex renk/emisyon/UV maskesi kuralları ve LOD üretimi diğer modellerle bire bir aynı olur.

Alternatif: Blender'da modelleyip FBX olarak içe aktarmak. Bu durumda her parça ayrı nesne, adlar Bölüm 5'teki gibi, pivotlar eklem noktasında, ölçek 1, renkler vertex renginde olmalı.

## 11. Teslimat ve kabul ölçütleri

- [ ] Bölüm 5'teki tüm parçalar, doğru ad, ebeveyn ve pivotla; her parçanın LOD1'i var.
- [ ] Toplam ≤ 2.400 üçgen; `MeshBudget.csv`'ye satır eklendi.
- [ ] 1080×1920'de, oyun kamerasından, açık zemin ve koyu boşluk üzerinde yapılan ekran görüntüsünde Kip, 6 grafit düşmanın arasında ilk bakışta seçiliyor.
- [ ] Ekran-yüz 6 ifadenin tümünü gösterebiliyor; göz kırpma mevcut kodla çalışıyor.
- [ ] P1 animasyonlarının (1–8) tümü oyun içinde oynuyor; yana ve geri yürürken ayak kayması yok.
- [ ] Namlu ucu nötr pozda bugünkü konumda; mermiler silahın ucundan çıkıyor.
- [ ] Animasyon inceleme görüntüsü (`Documentation/Previews/VoidToy/Animation_Review.png` gibi) Kip için üretildi.
