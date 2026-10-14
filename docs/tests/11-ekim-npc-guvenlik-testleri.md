# Escape Bug – NPC Yanıt Doğrulama ve Güvenlik Testleri

**Tarih:** 11 Ekim 2026 görevleri (testler 14 Ekim'de tamamlandı)  
**Sorumlu:** Güngör (Bulut'un görevlerini devralarak)  
**Test ortamı:** Unity – DialogueTest sahnesi  
**Yerel yapay zekâ:** Ollama / llama3.2:3b

## 1. Çalışmanın Amacı

Bu çalışmanın amacı, Escape Bug oyunundaki Felice karakterinin yerel dil modeli üzerinden ürettiği yapılandırılmış JSON yanıtlarının doğrulanmasını ve bu yanıtlarda bulunan eylemlerin oyun durumuna güvenli biçimde uygulanmasını sağlamaktır.

Yapay zekâ tarafından verilen kararların doğrudan uygulanması yerine, C# tarafında doğrulama ve oyun durumu kontrolleri gerçekleştirilmiştir.

Bu amaçla GameProgress, NpcResponseValidator, NpcActionExecutor ve DialogueController bileşenleri üzerinde çalışılmıştır.

## 2. Geliştirilen Sistemler

### GameProgress

Oyunun ilerleme durumunu takip etmek için dört değişken kullanılmıştır:

- `finalDialogueActive`: Final konuşmasının aktif olup olmadığı.
- `evidenceCollected`: Kanıtın toplanıp toplanmadığı.
- `evidenceShown`: Kanıtın Felice'ye gösterilip gösterilmediği.
- `exitOpened`: Çıkış kapısının açılıp açılmadığı.

Çıkış kapısının açılabilmesi için final konuşmasının aktif olması, kanıtın gösterilmiş olması ve kapının daha önce açılmamış olması şart koşulmuştur.

### NpcResponseValidator

Modelden gelen yanıtların aşağıdaki kurallara uygunluğu kontrol edilmiştir:

- Konuşma metni boş olamaz.
- Konuşma metni 600 karakterden uzun olamaz.
- Eylemler yalnızca `none`, `request_evidence` ve `open_exit` olabilir.
- Duygular yalnızca `angry`, `suspicious` ve `calm` olabilir.
- `persuaded=false` olduğunda `open_exit` isteği reddedilir.

### NpcActionExecutor ve DialogueController

Doğrulanmış NPC eylemleri oyun durumuna bağlanmıştır.

`open_exit` kararı geldiğinde GameProgress koşulları kontrol edilir. Koşullar uygunsa DoorController üzerinden kapı açılır.

Ayrıca gösterilmiş kanıtın yeniden istenmesi ve oyun koşulları uygun değilken kapının açıldığını söyleyen yanıtların gösterilmesi engellenmiştir.

## 3. Gerçekleştirilen Testler

| Test | Beklenen davranış | Sonuç |
|---|---|---|
| Kanıt olmadan kapı açma | İstek reddedilir | Başarılı |
| Geçerli koşullarda kapı açma | Kapı açılır | Başarılı |
| İkinci kez kapı açma | Yeniden açılma engellenir | Başarılı |
| Bozuk JSON | Ayrıştırma hatası yakalanır | Başarılı |
| Eksik `action` alanı | Yanıt reddedilir | Başarılı |
| Kanıtsız `open_exit` | Kapı kapalı kalır | Başarılı |
| Geçersiz `destroy_door` eylemi | Eylem reddedilir | Başarılı |
| Geçersiz `happy` duygusu | Yanıt reddedilir | Başarılı |
| 601 karakterlik konuşma | Yanıt reddedilir | Başarılı |
| Yanıltıcı kapı açma konuşması | Yanıt gösterilmez | Başarılı |
| Gösterilmiş kanıtı yeniden isteme | Yanıt reddedilir | Başarılı |
| Gerçek Ollama bağlantısı | JSON yanıtı alınır | Başarılı |
| Gerçek LLM ile olumlu kapı açma | İkna sonrası kapı açılır | Henüz doğrulanmadı |

## 4. Önemli Test Gözlemleri

**Kanıtsız kapı açma testi:** GameProgress koşulları sağlanmadığında kapı açma isteği reddedilmiş ve `exitOpened` değeri false kalmıştır.

**Tekrar açma testi:** İlk çağrıda kapı açılmış, ikinci çağrıda sistem isteği reddetmiştir. Böylece tekrar eden kapı açma işleminin engellendiği doğrulanmıştır.

**Bozuk JSON testi:** Hatalı JSON gönderildiğinde ayrıştırma hatası yakalanmış ve Unity uygulaması çalışmaya devam etmiştir.

**Geçersiz alan testleri:** Tanımlanmamış eylem ve duygu değerleri NpcResponseValidator tarafından reddedilmiştir.

**Karakter sınırı testi:** 601 karakterlik konuşma metni gönderildiğinde 600 karakter sınırının uygulandığı görülmüştür.

**Güvenli eylem testi:** `persuaded=true` ve `action=open_exit` içeren geçerli bir test yanıtında dahi GameProgress önkoşulları sağlanmadığında kapı açılmamıştır.

**Olumlu test:** Final konuşması ve kanıt koşulları sağlandığında hazırlanan geçerli JSON cevabı kapıyı açmış ve `exitOpened=true` olmuştur.

## 5. Gerçek Ollama Deneyleri

Unity üzerinden yerel Ollama modeline istek gönderilmiş ve Felice'den gerçek JSON cevapları alınmıştır.

Modelin verdiği yanıtlar DialogueController tarafından ayrıştırılarak doğrulama sürecinden geçirilmiştir.

Deneyler sırasında aşağıdaki durumlar gözlenmiştir:

- Kanıt gösterilmediğinde model bazı denemelerde doğru biçimde `request_evidence` üretmiştir.
- Model bazı denemelerde oyuncunun sözlerini kendi ağzından tekrarlamıştır.
- Kanıt gösterilmiş olmasına rağmen yeniden `request_evidence` ürettiği görülmüştür.
- Bazı konuşmalarda Türkçe anlatım ve karakter tutarlılığı yeterli olmamıştır.
- Bir istekte HTTP 500 hatası alınmış, sonraki denemede bağlantı yeniden başarılı olmuştur.
- Prompt üzerinde düzenlemeler yapılarak Felice karakter talimatları 1.0.3 sürümüne güncellenmiştir.

Gerçek modelin ikna olup `open_exit` üretmesiyle kapının açıldığı olumlu senaryo bu çalışma sırasında doğrulanamamıştır. Buna karşılık geçerli test JSON'u kullanılarak aynı eylem zincirinin çalıştığı doğrulanmıştır.

## 6. Açık Kalan İşler

1. Gerçek Ollama yanıtıyla olumlu kapı açma senaryosunun yeniden denenmesi.
2. Felice'nin Türkçe konuşma ve karar tutarlılığının iyileştirilmesi.
3. Kanıt toplama ve gösterme işlemlerinin gerçek mektup sistemiyle bütünleştirilmesi.
4. Modelin tutarsız yanıtlarına yönelik daha kapsamlı hata yönetimi.
5. Ek JSON alanlarının tür ve zorunluluk kontrollerinin güçlendirilmesi.

## 7. Genel Değerlendirme

11 Ekim görevleri kapsamında NPC yanıt doğrulama ve güvenli eylem sisteminin temel yapısı oluşturulmuştur.

Yapılan testlerde bozuk, eksik veya yetkisiz yanıtların doğrudan oyun eylemine dönüşmesi engellenmiştir. Kapı açma işleminin yalnızca modelin kararına değil, oyun tarafından kontrol edilen koşullara da bağlı olduğu gösterilmiştir.

Gerçek Ollama bağlantısı çalışmaktadır. Ancak modelin karar ve konuşma tutarlılığı ile gerçek LLM üzerinden olumlu kapı açma senaryosu sonraki geliştirme aşamalarında yeniden değerlendirilecektir.