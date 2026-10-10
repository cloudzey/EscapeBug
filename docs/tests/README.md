# Companion sensör testi

## Kurulum
- Sahne: CompanionTest
- CompanionMotor: NavMesh üzerinde hedefe yürür.
- CompanionSensors: Mesafe, açı ve görüş engelini kontrol eder.
- CompanionController: Varış ve sensör sonucuna göre kapıyı açar.
- Algılama mesafesi: 0.5 m
- En fazla hedef açısı: 60 derece

## Referanslar
- Yürüme hedefi: BugMoveTarget
- Sensör başlangıcı: Companion/SensorOrigin
- Mekanizma kökü: BugMechanism
- Sensör hedefi: BugMechanism/SensorTarget
- Kapı: TestDoor

## Engel deneyi
Böcek yürüme hedefine ulaştı ve mekanizmaya döndü.
SensorTestBlocker görüş çizgisine yerleştirildi.

Sonuç:
- Sensör erişimi false oldu.
- Try Activate Mechanism isteği reddedildi.
- Kapı kapalı kaldı.

Görsel: blocked.png

## Açık görüş deneyi
Böceğin ve hedefin konumu değiştirilmeden engel kapatıldı.

Sonuç:
- Sensör erişimi true oldu.
- Try Activate Mechanism kapıya Open çağrısı gönderdi.
- Kapı açıldı.
- Tekrar kullanım yeni kapı hareketi başlatmadı.

Görsel: clear.png

## Sorumluluk ayrımı
Sensör kapıyı doğrudan açmaz.
Controller sensör sonucunu kullanarak etkileşime izin verir.
Hareket CompanionMotor tarafından yürütülür.