# Harita Durumu — 16 Ekim 2026

## Tamamlanan iş paketi
Proje defterindeki 15 Ekim bakım odası düzenleme ve teslim çalışmaları.

## Bakım odası

Durum: Temel oynanış ve oda düzeni hazır.
Son görsel tasarım ve diğer odalarla bütünleştirme henüz tamamlanmadı.

### Oynanış
- Böceğin şalteri tünelin sonuna taşındı.
- D2 kapısı, insan ve böcek şalterlerinin ikisi de
  etkinleştirildiğinde açılacak şekilde düzenlendi.
- İnsan şalteri D2'nin yanına yerleştirildi.
- Eski TestDoor ve DoorPanel devre dışı bırakıldı.
- Böceğin duvar köşesinden kestirme yapmasına neden olan
  sabit kapı çerçevelerinin navigasyon katmanları düzeltildi.
- Açık güzergâhta tünelden gidiş ve dönüş doğrulandı.
- Kapalı güzergâh testinde görevin başarısız olduğu doğrulandı.

### Oda organizasyonu
- BugMechanism ve TestSwitch, Mechanisms altında toplandı.
- BugMoveTarget, Anchors altına alındı.
- TunnelEntry oluşturuldu.
- Mevcut CommandPoint konumu kontrol edildi.
- Görsel yönlendirmeler Visuals altında toplandı.
- Player ve Companion, oda prefab'ının dışında bırakıldı.

### Görsel yönlendirme
- PadVisual, böcek şalterinin arka levhası olarak düzenlendi.
- PadVisual'in collider'ı kapatıldı ve katmanı Default yapıldı.
- Tünel girişinden mekanizmaya uzanan turkuaz şeritler eklendi.
- D2 yanına iki şalter gerektiğini açıklayan yazı yerleştirildi.
- Yönlendirme şeritleri collider olmadan düzenlendi.

### Kontroller ve kayıt
- Oyuncu kamerasıyla giriş, tünel önü ve D2 çevresi kontrol edildi.
- D2 yazısının okunabilirliği ve oyuncunun kapıdan geçişi kontrol edildi.
- Oda değişiklikleri mevcut MaintenanceRoom prefab'ına uygulandı.
- Prefab içindeki şalter, hedef ve kapı bağlantıları kontrol edildi.

## Diğer alanlar

| Alan | Durum | Sıradaki iş |
|---|---|---|
| Giriş alanı | Oda kurulumu bekliyor | Zemin, duvarlar, giriş ve D1 bağlantısı |
| Arşiv odası | Oda kurulumu bekliyor | Oda kabuğu, mektup ve bulmaca yerleşimleri |
| İksir odası | Oda kurulumu bekliyor | Ayrılmış bölmeler, küçük geçit ve mekanizma yerleşimleri |
| Final alanı | Oda kurulumu bekliyor | Sahne alanı, karakter konumları ve çıkış yerleşimi |

## Açık işler
- Diğer dört alanın temel geometrisini ve geçişlerini oluşturmak.
- Bakım odasının son materyal, aydınlatma ve dekor çalışmalarını yapmak.
- Şalterlerin etkin durumlarını gösteren görsel geri bildirim eklemek.
- Odalar birleştirildiğinde NavMesh'i bütün haritaya göre yeniden hazırlamak.
- Açılan kapılardan companion geçişini ayrıca doğrulamak.
- Tam oyun rotası kurulduktan sonra uçtan uca oynanış testi yapmak.

## Tasarım kararı
D2 için iki şalter de zorunludur.
Şalterlerden yalnızca birinin etkinleştirilmesi kapıyı açmaz.
Bu karar, eski plandaki yalnızca böcek şalteriyle açılma kuralının yerini alır.