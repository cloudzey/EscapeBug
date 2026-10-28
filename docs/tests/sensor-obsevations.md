
# CompanionSensors — Görüş Testi

## Negatif senaryo

Sahne: CompanionSensorNegative_Gungor

SensorTestBlocker, bakım pedi yakınında etkinleştirildi.

Gözlenen:
- Target Reachable: True
- Target Distance: 0.2625
- Target Angle: 17.78
- Target Reason: Erişilebilir
- Böcek şalteri etkinleşti.
- D2, insan şalteri beklediği için kapalı kaldı.

Sonuç: Negatif görüş testi doğrulanamadı.

Açık görev:
CompanionSensors raycast başlangıç ve hedef noktaları
kontrol edilerek engelin gerçek görüş hattına
yerleştirilmesi gerekiyor.

Kanıt: screenshots/sensor-negative-unblocked.png
