# Son beş saniye gerilimi

20 saniyelik küp süresi **5 saniyenin altına** indiğinde kamera hafif sarsılmaya başlar. Süre azaldıkça sarsıntının gücü ve frekansı artar. Azami yer değiştirme 0,075 dünya birimidir; kamera açısı, zoom, oyuncu ve takip ankrajı değiştirilmez. Efekt Cinemachine'in Noise aşamasında son kamera konumuna eklenir. Joystick'in yön hesabı ve HUD yerleşimi sabit kalır.

Sayaç en fazla %6,5 oranında hafif nabız atar; rengi kırmızı ile sıcak turuncu arasında değişir. Mevcut alarm sesi başlangıçta yaklaşık 1,25/s, sona yaklaşırken 2,8/s ritmine hızlanır. Ses mevcut mute/SFX ayarlarını kullanır. Kamera hareketi kare başına transform'a eklenmez; kayma veya takip hatası biriktirmez.

Küp temizlenince, kapıya geçince, ölüm/düşüş/çöküşte, menüde, pause sırasında veya süre yeniden 20 saniyeye çıkınca sarsıntı ve sayaç ölçeği sıfırlanır. Resume sonrası yumuşak giriş yapılır. Hasar, mermi hızı, ateş ritmi, AI, küp süresi ve ödüller değişmedi.

## Ayarlar ve kurulum

Ana sahnedeki mevcut Cinemachine Virtual Camera üzerinde `CountdownCameraShake` bileşeni vardır. Inspector'daki `Maximum Displacement`, `Starting Frequency`, `Ending Frequency` değerleri ayarlanabilir. Maximum Displacement=0 kamera hareketini kapatır; sayaç/ses uyarısı devam eder.

`Cube Invasion > Combat > Install Countdown Tension` yalnız mevcut sanal kameraya bileşeni bağlar. Sahne karşılaştırmasında hiçbir transform veya kamera lens/framing bileşeni değişmedi; mevcut sanal kameranın bileşen listesi ve takip bileşeninin bağlantısı güncellendi.

## Doğrulama

`CountdownTensionCheck.Run`: 5 saniye eşiği, artan ve sınırlı genlik, hızlanan ritim, başlık açısının sabitliği, pipeline tekrarlarında kayma oluşmaması, pause, sekiz savaş dışı durum, süre yenilenmesi, karakterin değişmemesi ve sayaç ölçeğinin geri dönmesi kontrolleri geçti. Gerçek oyun kamerasıyla kontrollü bir beş saniyelik render üretildi. P0/P1 kamera, animasyon, küpler arası geçiş ve kenardan düşme kontrolleri de geçti.

[Animasyon önizlemesi](Previews/Countdown/Final_Five_Seconds.gif) · [Son saniye](Previews/Countdown/Last_Second.png). Önizlemede düşman AI'sı ilerletilmeden yalnız geri sayım sunumu gösterilir. Gerçek Android cihazda oynanış/performans testi henüz yapılmadı.

Runtime/Editor C# ve Android development derlemesi başarılı. APK: `CubeInvasion-CountdownTension.apk` (58.6 MiB).
