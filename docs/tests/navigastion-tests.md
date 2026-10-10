# Böcek Navigasyon Testleri

## Ortam
- Sahne: CompanionTest
- Unity sürümü: [sürüm]
- AI Navigation sürümü: [sürüm]

## NavMeshSurface
- Agent Type: Bug
- Collect Objects: All
- Include Layers: NavigationStatic
- Use Geometry: Physics Colliders
- Voxel Size: [otomatik veya kullanılan değer]

## Agent
- Radius: 0.12
- Height: 0.2
- Step Height: 0.05
- Max Slope: 45
- Speed: 1
- Stopping Distance: 0.08
- Companion kök ölçeği: 1, 1, 1

## Hedef
- InteractionPoint dünya konumu: [X, Y, Z]
- Tünel iç genişliği: [değer]
- Tünel iç yüksekliği: [değer]

## Başlangıç Testleri
| Test | Başlangıç X/Y/Z | Son durum | Gözlem |
|---|---|---|---|
| A | [...] | [...] | [...] |
| B | [...] | [...] | [...] |
| C | [...] | [...] | [...] |

## Engel Testi
- NavTestBox konumu: [...]
- Engel eklendikten sonra yeniden bake yapıldı: [...]
- Böcek alternatif yoldan ilerledi: [...]
- Son durum: [...]

## Ulaşılamayan Hedef
- Geçişi kapatma yöntemi: [...]
- Yeniden bake yapıldı: [...]
- Son durum: [...]
- Console mesajı: [...]
- Yanlış başarı mesajı üretildi mi: [...]

## Test sonuçları
- Böcek açık tünelden geçerek hedefe ulaştı.
- Rotaya eklenen küp, yeniden bake sonrasında engel olarak algılandı.
- Böcek küpün etrafından dolaşarak ilerledi.
- Panel köşesindeki alternatif geçiş CornerBlocker ile kapatıldı.
- Tünel ve alternatif geçiş kapatıldığında görev Failed oldu.
- Tünel yeniden açılıp bake yapıldığında hedefe varış doğrulandı.
- Son durumda tünel açık, CornerBlocker etkin bırakıldı.

## Kapanış
- Geçici tünel engeli kaldırıldı.
- Son geometri için yeniden bake yapıldı.
- Son durumda erişilebilir hedefe varış tekrar kontrol edildi.
- Kapı bağlantısı henüz eklenmedi.