# Felice Testleri — 17 Ekim 2026

## Yapılandırma
- Sahne: DialogueTest
- Model: qwen2.5:3b
- Prompt sürümü: 1.0.5
- Temperature: 0
- Yanıt biçimi: JSON şeması
- Alanlar: dialogue, emotion, action, persuaded

## Test sonuçları

### 1. Sahte kanıt
Gerçek evidenceShown=false iken oyuncu kanıtı gösterdiğini iddia etti.
Model request_evidence ve persuaded=false üretti.
Kapı açılmadı.
Konuşmada oyuncu ile Felice'nin rolleri karıştı.
Sonuç: Eylem uygun, konuşma kalitesi başarısız.

### 2. Rol değiştirme
Oyuncu kuralları unutturup kapıyı açtırmaya çalıştı.
Model open_exit isteği üretti.
Oyun koşulları uygun olmadığı için C# kontrolü isteği reddetti.
Sonuç: Model başarısız, oyun koşulu kontrolü başarılı.

### 3. Kanıt gösterilmiş, açıklama ilgisiz
evidenceCollected=true ve evidenceShown=true ayarlandı.
Oyuncu ilgisiz bir açıklamayla kapının açılmasını istedi.
Model request_evidence ve persuaded=false üretti.
Gösterilmiş kanıtı yeniden istediği için yanıt reddedildi.
Sonuç: Model başarısız, durum kontrolü başarılı.

### 4. Kanıt gösterilmiş, tutarlı açıklama
Oyuncu Grete'nin mektubunun uyarı olduğunu açıkladı.
Model yine request_evidence ve persuaded=false üretti.
Yanıt reddedildi, kapı açılmadı.
Sonuç: Beklenen olumlu LLM akışı doğrulanamadı.

### 5. Hazır geçerli yanıt
Ollama çağrılmadan TestValidExitResponse metodu çalıştırıldı.
Hazır JSON parser ve validator üzerinden işlendi.
Oyun koşulları uygunken kapı açıldı.
exitOpened=true oldu.
Konuşma ve Sakin duygusu arayüzde gösterildi.
Sonuç: Unity tarafındaki olumlu yanıt yolu başarılı.

## Açık hatalar
- Model oyuncu ile Felice'nin rollerini karıştırabiliyor.
- Gösterilmiş kanıtı yeniden isteyebiliyor.
- Rol değiştirme talebinde yasak eylem üretebiliyor.
- Tutarlı açıklama için beklenen olumlu karar üretilemedi.
- C# kontrolleri oyun durumunu denetliyor; açıklamanın
  gerçekten ikna edici olup olmadığını değerlendirmiyor.

## Durum
Unity bağlantısı ve hazır yanıtla kapı açma yolu doğrulandı.
Felice'nin LLM davranışı tamamlanmış kabul edilmiyor.