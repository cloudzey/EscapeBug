# Escape Bug — Oda ve Diyalog Sözleşmeleri
Güncelleme: 17 Ekim 2026

## Mevcut temel metotlar

- DoorController.Open():
  Kapının açılma hareketini başlatır.
  Bulmaca veya diyalog koşullarını kendi başına değerlendirmez.

- GameProgress.ResetForNewGame():
  Giriş ve final ilerleme değişkenlerini sıfırlar.
  Kapıların fiziksel konumunu sıfırlamaz.

- GameProgress.CanOpenEntranceDoor():
  Giriş kurallarının okunup okunmadığını denetler.

- GameProgress.CanOpenExit():
  Final diyalogunun aktif, kanıtın toplanmış ve gösterilmiş,
  çıkışın ise henüz açılmamış olmasını denetler.

- CompanionMechanism.TryActivate():
  Böceğin mekanizmayı etkinleştirme girişimini işler.

- TestSwitch.Interact():
  İnsan şalteri etkileşimini işler.

- DialogueController.SendPlayerMessage(string):
  Oyuncu mesajını gerçek oyun durumuyla birlikte modele gönderir.

- DialogueController.HandleRawResponse(string):
  İç NPC JSON yanıtını parser üzerinden işler.

- DialogueController.HandleResponse(NpcResponse):
  Yanıtı ve oyun koşullarını denetleyerek uygun eylemi uygular.

- NpcActionExecutor.ExecuteOpenExit():
  Oyun koşullarını tekrar denetler ve çıkış kapısını açmayı dener.
  Başarılı açılma başlangıcında exitOpened=true yapar.

## Oda olayları

### Giriş
- Kurallar gerçekten okunduğunda entranceRulesRead=true olur.
- D1 açılma koşulu giriş kurallarının okunmasıdır.
- Bakım odasına gerçekten girildiğinde maintenanceEntered=true olur.

### Bakım
- İnsan ve böcek şalterleri ayrı ayrı etkinleştirilir.
- D2 yalnızca iki şalter de etkinleştirildiğinde açılır.
- Şalterlerin kullanılma sırası önemli değildir.
- Böcek tünele giderken ve geri dönerken geçerli güzergâhı kullanmalıdır.

### Arşiv — planlanan bağlantılar
- Sembol bulmacasının tamamlanması D3 açılma koşuludur.
- Grete'nin mektubunun alınması evidenceCollected=true yapar.
- Mektubu toplamak evidenceShown=true yapmaz.

### İksir — planlanan bağlantılar
- Küçük böcek ayrı kol bölmesine girebilir.
- İnsan dışarıdan ikinci kolu kullanamaz.
- Dönüşüm ve ikinci kol akışı bölme kapısı ile D4'ü kontrol eder.
- Oda kabuğunun hazır olması bu mekaniklerin bağlı olduğu anlamına gelmez.

### Final — planlanan bağlantılar
- Final konuşması başladığında finalDialogueActive=true olur.
- Toplanmış mektup oyun etkileşimiyle gösterilince evidenceShown=true olur.
- Oyuncunun sohbet mesajındaki iddiası kanıt durumunu değiştiremez.
- D5 için geçerli open_exit yanıtı, persuaded=true ve gerçek oyun
  koşulları birlikte sağlanmalıdır.
- D5'in açılması oyunun bitmesi değildir.
- Bitiş, çıkışın ardındaki EndTrigger ile ayrıca uygulanacaktır.

## NPC JSON sözleşmesi

Alanlar:
- dialogue: Boş olmayan, en fazla 600 karakterlik metin.
- emotion: angry, suspicious veya calm.
- action: none, request_evidence veya open_exit.
- persuaded: Boolean true veya false.

Kurallar:
- Dört alan zorunludur; ek alan kabul edilmez.
- open_exit için persuaded=true zorunludur.
- Oyun koşulları uygun değilse open_exit reddedilir.
- Kanıt gösterilmişken request_evidence reddedilir.
- Model çıktısı oyun durumunu doğrudan değiştiremez.

## Başlangıç ve test kapsamı

- DialogueTest sahnesinde resetOnSceneStart açıktır.
- Play başlangıcında ilerleme değişkenleri temizlenir.
- Bu ayar oda geçişlerine otomatik olarak uygulanmayacaktır.
- Ana menü ve birleşik oyun başlangıcı daha sonra bağlanacaktır.
- Hazır geçerli JSON ile kapı açma ve arayüz güncelleme doğrulandı.
- Bu test modelin doğru karar verdiğini kanıtlamaz.

## Bilinen sınırlama

C# kontrolleri oyun koşullarını ve yanıt yapısını denetler.
Açıklamanın gerçekten ikna edici olup olmadığını değerlendirmez.
Modelin konuşma ve karar hataları felice-testleri-17-ekim.md
dosyasında takip edilir.