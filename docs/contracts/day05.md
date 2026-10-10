# Gün 05 ortak sözleşmesi

## Felice
- Amacı oyuncudan açıklama almaktır.
- Oyuncunun mektup iddiası gerçek kanıt gösterimi sayılmaz.
- evidenceShown oyun tarafından yönetilir.
- Modelin ikna değerlendirmesi kanıt durumundan ayrıdır.
- open_exit bir öneridir; kapı önkoşulları C# tarafından doğrulanır.
- İzinli eylemler: continue_dialogue, request_evidence, open_exit.
- İzinli duygular: angry, suspicious, calm.
- Felice promptuna yalnız karakterin bildiği olaylar aktarılır.

## Companion
- Durumlar: Follow, Wait, MoveToTarget, Interact, Return, Failed.
- Hedefe varıştan sonra yönelme ve sensör kontrolü yapılır.
- Etkileşim başarılıysa oyuncuya dönüş başlar.
- Yol veya görev hatası Failed durumuna götürür.
- Görsel prefab teknik kökün hareket ve sensör ayarlarını değiştirmez.

## Açık karar
- Kafka ile böceğin zihinsel bağının açılışta kurulması:
  Bulut ve Güngör tarafından birlikte değerlendirilecek.