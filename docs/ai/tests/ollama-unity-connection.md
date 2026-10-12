# Unity – Ollama bağlantı deneyi

Plan günü: Gün 06
Model: llama3.2:3b
Sunucu: http://127.0.0.1:11434
Uç nokta: /api/chat
Streaming: false
Timeout: 60 saniye

## Test sonuçları

| İlk Unity isteği | | | |
| Tırnak ve Türkçe karakter | | | |
| Ollama kapalı | | | |
| Ollama yeniden açıldıktan sonra | | | |

## Gözlemler

- İstek sırasında küpün dönüşü:
- Yanıtın message.content alanından okunması:
- Kapalı sunucuda görülen hata:
- Sunucu yeniden açılınca tekrar istek sonucu:

## Kapsam

Bu çalışma HTTP bağlantısını ve dış yanıt zarfını kapsar.
Modelin ürettiği içerik oyun eylemi olarak uygulanmadı.
Felice yanıtlarının iç JSON doğrulaması ve kapı önkoşulu
kontrolü sonraki aşamaya bırakıldı.