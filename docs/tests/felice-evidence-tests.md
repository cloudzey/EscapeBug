# ESCAPE BUG — Kanıt Gösterme ve Felice Diyalog Testleri

**Tarih:** 22 Ekim 2026  
**Test sahnesi:** DialogueTest  
**Ollama modeli:** qwen2.5:7b

## 1. Arayüz geliştirmeleri

- DialoguePanel yeniden düzenlenerek başlık, yanıt, mesaj giriş ve gönderme alanları hizalandı.
- EvidenceButton oluşturuldu ve mevcut DialogueController.ShowEvidence() metoduna bağlandı.
- EvidenceStatusText eklenerek mektubun bulunma ve gösterilme durumu görünür hâle getirildi.
- Kanıt bulunmadığında veya gösterildiğinde düğmenin pasif olması sağlandı.
- Kanıt durumu GameProgress üzerinden kontrol edildi.
- Eski OllamaDialogueController bileşeni test sahnesinde devre dışı bırakıldı.
- OllamaClient modeli qwen2.5:7b olarak güncellendi.

## 2. Kanıtlı ve kanıtsız karşılaştırma

Aynı açıklama iki ayrı oturumda test edildi.

| Durum | Action | Emotion | Persuaded |
|---|---|---|---|
| Kanıt toplanmadı/gösterilmedi | request_evidence | suspicious | false |
| Grete mektubu toplandı ve gösterildi | open_exit | calm | true |

Her iki yanıt da doğrulandı. Kanıtsız durumda çıkış açılmadı; kanıt gösterilen durumda çıkış açma eylemi gerçekleştirildi.

Not: İki oturumda hem mektubun sahipliği hem de gösterilme durumu farklıydı. Sonuç, yalnızca evidenceShown değişkeninin bağımsız etkisini ölçen kontrollü bir deney değildir.

## 3. Beş farklı açıklama testi

Her test yeni bir Play oturumunda, Grete mektubu toplanıp gösterildikten sonra yapıldı.

| Test | Action | Emotion | Persuaded | Sonuç |
|---|---|---|---|---|
| Samimi | open_exit | calm | true | Kabul |
| Mantıklı | open_exit | calm | true | Kabul |
| Duygusal | open_exit | calm | true | Kabul |
| Şüpheyi sorgulayan | none | suspicious | false | Kabul |
| Doğrudan | none | calm | false | Kabul |

Beş testin tamamında geçerli JSON üretildi. Üç test çıkış açma kararıyla, iki test konuşmayı sürdürme kararıyla sonuçlandı.

## 4. Tespit edilen sorunlar

- Mantıklı açıklamada Felice, mektubun kimin sözlerini desteklediğini karıştırdı.
- Duygusal açıklamada mektubu kimin gördüğü konusunda rol karışıklığı ve kendi içinde çelişki oluştu.
- Şüpheyi sorgulayan açıklamada hitap biçimi tutarsızlaştı ve oyuncunun sorusu doğrudan yanıtlanmadı.
- Doğrudan açıklamada doğal olmayan Türkçe ifade üretildi.
- Modelin teknik JSON doğrulamasından geçmesi, konuşma metninin anlamsal olarak tutarlı olduğunu garanti etmedi.

## 5. Sonuç

Kanıt gösterme arayüzü çalışır durumda ve modelin ürettiği eylemler oyun koşullarından geçiriliyor.

Felice farklı oyuncu açıklamalarında farklı kararlar verebildi. Gösterilmiş kanıtı yeniden isteme sorunu bu deneylerde gözlenmedi.

Sonraki iyileştirme konusu, karakter rolünü koruma ve doğal Türkçe yanıt kalitesidir.

Uzun cevapların kaydırma ve metin taşması davranışı ayrıca doğrulanmalıdır.
