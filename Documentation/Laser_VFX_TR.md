# Lazer atışları ve namlu efektleri

Oyuncu ve düşman mermileri artık küre mesh'i kullanmaz. Uç kısmı mevcut mermi simülasyonuna bağlı, arkasında kısa ışık izi bulunan lazer atışlarıdır. Hız, hasar, menzil, saçılma, tek parmak ateş ritmi ve hedefleme kuralları değişmedi; bunlar anlık hitscan ışınları değildir.

Her mermi üç küçük, kameraya dönük çizgiyle çizilir: parlak beyaz çekirdek, açık zeminde okunabilen renkli kenar ve yumuşak toplamalı ışık halesi. Oyuncu rengi seçilen silahtan gelir; düşman atışı sıcak kırmızı/turuncudur. Kuyruk çarpışma ucunun arkasında uzar ve havuzdan alınırken sıfırlanır. Mermi çarpışma anında bütün görseliyle kapanır; duvarın ötesinde kalan bağımsız bir trail yoktur.

Oyuncunun namlu efekti animasyonla hareket eden görünür silah mesh'inin ucunu izler. Yaklaşık 95 ms süren ışık jeti, çapraz enerji parlaması ve yumuşak hale kullanır. Twin Blasters iki namludan iki ayrı parlama, Scatter Shot tek ve daha geniş parlama üretir. Gunner ve Sniper'ın ateşinde sağ el/silah rig'inin ucunda düşman parlaması çıkar. Boss'un bir karede attığı fan/radyal mermiler tek enerji boşalımıyla gösterilir.

Efektler mevcut ses ve geri tepme animasyonlarıyla birlikte çalışır. Pause lazer hareketini ve namlu efektlerinin ömrünü dondurur. Küp sonu ve yeni denemede aktif namlu efektleri temizlenir. Namlu ve darbe efektlerinin birleşik aktif üst sınırı 48'dir. Obje havuzları tekrar kullanılır; mermi başına materyal, gerçek zamanlı ışık, bloom veya parçacık sistemi oluşturulmaz.

## Varlıklar

- `Runtime/LaserBoltVisual.cs`: çekirdek, kenar, hale ve kuyruk sıfırlama.
- `Runtime/CombatFeedback.Muzzle.cs`: havuzlanan namlu parlamaları.
- `Runtime/InvasionPrototype.Laser.cs`: görünür silah ve düşman rig'i bağlantıları.
- `Art/VFX/LaserGlow.shader`: ışıklı çekirdek/hale ve renkli kenarın ortak unlit shader'ı.
- `Resources/LaserGlow.mat`, `LaserRim.mat`: build'e dahil edilen iki paylaşılan materyal.

Materyal üretimi: `Cube Invasion > Art > Build Laser VFX Material`. Görsel/mekanik inceleme: `LaserVfxReview.Run` (batch mode, `-quit` olmadan). İnceleme mevcut oyun kamerası ve HUD ile 72 kare üretir.

## Doğrulama

- Runtime/Editor C# derlemesi başarılı.
- Mermide küre mesh'i veya collider bulunmadığı, shader ve üç görsel katman doğrulandı.
- Kuyruk yönü, havuz sıfırlama, pause, oyuncu/düşman parlamaları, twin/scatter davranışı, efekt bütçesi ve havuza geri dönüş kontrolleri geçti.
- Barikatın iki taraftan da mermiyi durdurması, engel/varil/düşman çarpışma sırası, büyük kare süresi, mermi ömrü, hedef görünürlüğü ve tek parmak ateş ritmi kontrolleri geçti.

[Gerçek Unity animasyon önizlemesi](Previews/Laser/Laser_Fire.gif) · [Oyun içi kare](Previews/Laser/Gameplay.png). Önizleme, atışları görünür kılmak için hedeflerin sabit tutulduğu kontrollü bir oyun sahnesi renderıdır. Gerçek Android cihazda görsel ve performans testi henüz yapılmadı.

Android development APK derlemesi başarılı: `CubeInvasion-LaserVFX.apk` (58.6 MiB). P0/P1 animasyon, efekt bütçesi, kamera, kesintisiz geçiş ve karakterin kenardan düşmesi kontrolleri de geçti.
