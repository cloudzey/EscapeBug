# Companion Durum Makinesi — Gün 05

## Durumlar
Follow, Wait, MoveToTarget, Interact, Return, Failed

## Komutlar
- F: Inspector'da atanmış mekanizmaya görev ver.
- Q: Bekle.
- R: Takibe dön.
- Görev sürerken yeni görev, bekle ve takip komutları kabul edilmez.

## Takip Ayarları
- Başlama mesafesi: 1.5 m
- Durma mesafesi: 1 m
- Yol güncelleme kontrol aralığı: 0.25 s
- Oyuncu hedef güncelleme eşiği: 0.2 m

## Görev Ayarları
- Mekanizma durma mesafesi: 0.08 m
- Gidiş ve etkileşim süresi: 20 s
- Dönüş süresi: 20 s

## Testler
- [x] Oyuncu uzaklaşınca takip başlıyor.
- [x] Yakına gelince duruyor.
- [x] Küçük mesafe değişimleri titreme üretmiyor.
- [x] Q ile bekliyor.
- [x] R ile yeniden takip ediyor.
- [x] F ile mekanizmanın InteractionPoint noktasına gidiyor.
- [x] Sensör uygunsa kapı açılıyor.
- [x] Etkileşim sonrasında oyuncuya dönüyor.
- [x] Oyuncuya varınca Follow durumuna geçiyor.
- [x] Görev sırasında tekrar F ikinci görev başlatmıyor.
- [x] Kullanılmış mekanizmaya yeniden komut zararsız.
- [x] Sensör engelinde Failed oluyor ve kapı kapalı kalıyor.
- [x] Engel kaldırılınca Failed durumundan yeniden görev verilebiliyor.
- [x] Tam yol yokken Failed oluyor.
- [x] Süre aşımında Failed oluyor.

## Başarılı Görev Kaydı
Follow -> MoveToTarget -> Interact -> Return -> Follow

## Başarısız Görev Kaydı
[Console'da görülen gerçek sıra ve hata nedeni]

## Son Sahne Durumu
- Tünel açık.
- CornerBlocker etkin.
- SensorTestBlocker kapalı.
- Task Timeout tekrar 20 saniye.