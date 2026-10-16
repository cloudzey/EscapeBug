# ESCAPE BUG — Giriş Sistemi Teknik Raporu

**Tarih:** 16 Ekim 2026  
**Geliştirici:** Bulut  
**Proje:** ESCAPE BUG — Hafıza Gümrüğü  
**Test Sahnesi:** `Assets/_Project/Scenes/EntranceTest.unity`  
**Durum:** Bağımsız giriş mekanikleri test edildi. Oda entegrasyonu bekleniyor.

---

## 1. Çalışmanın Amacı

Bu çalışmanın amacı, oyuncunun Hafıza Gümrüğü giriş alanındaki kurallar kartıyla etkileşime girmesini, kurallar ekranı açıkken kontrollerin kilitlenmesini ve girişten bakım odasına geçişin koşullara bağlı olarak yönetilmesini sağlamaktır.

Geliştirme, mevcut oyuncu hareketi, kamera, etkileşim ve kapı sistemleri korunarak gerçekleştirilmiştir.

## 2. Giriş Test Sahnesi

`EntranceTest` adlı bağımsız test sahnesi kullanılmıştır.

Sahnede mevcut Player prefabı kullanılarak aşağıdaki sistemler hazırlanmıştır:

- Oyuncu hareketi ve yer çekimi
- Üçüncü şahıs takip kamerası
- Fareyle kamera kontrolü
- E tuşuyla nesne etkileşimi
- Geçici zemin ve kurallar kartı
- Kurallar paneli
- D0 ve D1 test kapıları
- Bakım odasına geçiş tetikleyicisi
- GameProgress ilerleme yöneticisi

Giriş odasının görsel prefabı henüz hazır olmadığından geçici geometriler kullanılmıştır.

## 3. Kurallar Kartı ve Etkileşim Sistemi

Kurallar kartı, mevcut `IInteractable` arayüzü üzerinden geliştirilmiştir.

`PlayerInteraction` sistemi hedef algılama, görüş açısı, mesafe ve görüş hattı kontrollerini gerçekleştirmektedir.

`RulesCard.cs`, `IInteractable` arayüzünü uygulamakta ve oyuncunun etkileşimi sonucunda `RulesPanelController` üzerinden kurallar ekranını açmaktadır.

Kurallar paneli:

- E tuşuyla yapılan kart etkileşimi sonucunda açılır.
- KAPAT butonuyla kapatılabilir.
- Escape tuşuyla kapatılabilir.
- Kapatıldığında `GameProgress.entranceRulesRead` değerini `true` yapar.

Kuralların okunması, panelin kapatılmasıyla tamamlanmış kabul edilmektedir.

## 4. Ortak Kontrol Kilidi

Oyuncu kontrollerini yönetmek amacıyla `PlayerControlLocks.cs` oluşturulmuştur.

Sistem, kilit talep eden nesneleri bir `HashSet` içerisinde tutmaktadır. Aynı nesnenin tekrar kilit istemesi ikinci bir kayıt oluşturmaz.

Kontrol kilidi aktifken:

- Oyuncunun yatay hareket girdisi sıfırlanır.
- Fareyle kamera döndürme engellenir.
- Arka plandaki nesnelerle etkileşim engellenir.
- Yer çekimi ve CharacterController hareket hesaplamaları çalışmaya devam eder.
- Kamera takip sistemi çalışmayı sürdürür.

`PlayerMotor.cs`, `ThirdPersonCamera.cs` ve `PlayerInteraction.cs` mevcut işlevleri korunarak güncellenmiştir.

Kilidi talep eden sistem kendi kilidini kaldırır. Başka bir sistemin kilidi devam ediyorsa genel kilit durumu açık kalır.

**Test durumu:** Kurallar panelinin açılıp kapanması doğrulanmıştır. Diyalog, mektup ve dönüşüm gibi birden fazla sistemin eşzamanlı kilit talep ettiği entegrasyon testi henüz yapılmamıştır.

## 5. Kapı ve İlerleme Sistemi

### D0 — Dış Giriş Kapısı

- Oyun başlangıcında açık konumdadır.
- Oyuncunun bakım alanına geçişi kaydedilince kapanır.
- `D0ReturnDoor.cs` kullanılarak kapanma animasyonu gerçekleştirilir.
- Kapanma işlemi aynı oyun oturumunda tekrar başlatılmaz.

### D1 — Girişten Bakım Odasına Geçiş

- Başlangıçta kapalıdır.
- Kurallar okunmadan açılmaz.
- `entranceRulesRead = true` olduğunda `EntranceDoorGate.cs`, mevcut `DoorController.Open()` metodunu çağırır.
- Kapı kanadı yukarı doğru hareket ederek geçişi açar.
- Açılma işlemi tekrar tetiklenmez.

### Bakım Odası Geçişi

`MaintenanceEntryTrigger.cs`, oyuncunun D1 arkasındaki geçiş alanına girmesini algılar.

Geçiş sırasında:

1. Nesnenin oyuncu olduğu kontrol edilir.
2. Kuralların okunmuş olması şartı aranır.
3. `GameProgress.maintenanceEntered = true` yapılır.
4. D0 kapısının kapanması tetiklenir.

## D0–D5 Kapı Durumları

Bu bölüm, oyun tasarımında planlanan kapıların açılma koşullarını ve mevcut geliştirme durumlarını kaydetmektedir.

16 Ekim tarihinde yalnızca D0 ve D1 kapı mekanikleri geliştirilmiş ve test edilmiştir.

D0: Bakım odasına geçiş sonrasında kapanır. Geliştirildi ve test edildi.

D1: Kurallar kartı okunduktan sonra açılır. Geliştirildi ve test edildi.

D2: İnsan ve böcek şalterlerinin etkinleşmesiyle açılır. Önceki çalışmalarda geliştirildi ve test edildi.

D3: Arşivdeki üç sembollü kilidin çözülmesiyle açılması planlanmaktadır. Henüz geliştirilmedi.

D4: Doğru iksir, dönüşüm ve ikinci kol mekanikleri tamamlandığında açılması planlanmaktadır. Henüz geliştirilmedi.

D5: Final diyaloğunda gerekli önkoşulların sağlanması ve LLM tarafından geçerli bir open_exit kararı verilmesiyle açılması planlanmaktadır. D5'in kapı mekaniği henüz geliştirilmedi.

Not: D5 ile ilişkili bazı LLM ve diyalog altyapıları daha önce ayrı testlerde çalışılmıştır. Bunlar D5 kapısının tamamlandığı veya test edildiği anlamına gelmez. 

## 7. GameProgress ve Başlangıç Durumları

Mevcut `GameProgress.cs` genişletilerek giriş bölümü için iki yeni değişken eklenmiştir:

| Değişken | Başlangıç | Anlamı |
|---|---|---|
| `entranceRulesRead` | `false` | Kuralların okunup okunmadığı |
| `maintenanceEntered` | `false` | Bakım alanına geçişin gerçekleşip gerçekleşmediği |
| `finalDialogueActive` | `false` | Final diyaloğunun aktifliği |
| `evidenceCollected` | `false` | Kanıtın toplanması |
| `evidenceShown` | `false` | Kanıtın NPC'ye gösterilmesi |
| `exitOpened` | `false` | Final çıkışının açılması |

`CanOpenEntranceDoor()` metodu D1 koşulunu, mevcut `CanOpenExit()` metodu ise final çıkışının önkoşullarını kontrol eder.

**Reset davranışı:** Yeni bir test oturumu varsayılan `false` değerleriyle başlar. `PlayerControlLocks` içindeki kilit kayıtları da yeni çalışma başlangıcında temizlenir. Kalıcı kayıt/yükleme sistemi ve tam oyun sıfırlama akışı henüz geliştirilmemiştir.

## 8. Gerçekleştirilen Testler

| Test | Sonuç |
|---|---|
| Oyuncunun hareket etmesi | Başarılı |
| Kameranın oyuncuyu takip etmesi | Başarılı |
| Fareyle kamera kontrolü | Başarılı |
| Kurallar kartının algılanması | Başarılı |
| Kurallar panelinin açılması | Başarılı |
| KAPAT butonunun çalışması | Başarılı |
| Panelin tekrar açılabilmesi | Başarılı |
| Kurallar sonrası D1'in açılması | Başarılı |
| Bakım geçişinin kaydedilmesi | Başarılı |
| Geçiş sonrasında D0'ın kapanması | Başarılı |
| Eşzamanlı çoklu kontrol kilidi | Henüz test edilmedi |
| Gerçek giriş odasında fiziksel geri dönüş engeli | Entegrasyon bekleniyor |

Konsol üzerinden doğrulanan olay sırası:

```text
Kapı açıldı.
Oyuncu bakım odasına geçti. D0 geri dönüşü kilitlenebilir.
D0 kapandı. Geri dönüş kilitlendi.
```

Test sırasında bu akışla ilişkili bir derleme hatası gözlenmemiştir.

## 9. Eklenen ve Güncellenen Dosyalar

**Yeni scriptler:**

- `PlayerControlLocks.cs`
- `RulesPanelController.cs`
- `RulesCard.cs`
- `EntranceDoorGate.cs`
- `MaintenanceEntryTrigger.cs`
- `D0ReturnDoor.cs`

**Güncellenen scriptler:**

- `PlayerMotor.cs`
- `ThirdPersonCamera.cs`
- `PlayerInteraction.cs`
- `GameProgress.cs`

**Sahne:**

- `EntranceTest.unity`

**Yeniden kullanılan sistemler:**

- `IInteractable.cs`
- `DoorController.cs`
- Mevcut Player prefabı
- Mevcut kamera sistemi

## 10. Bekleyen İşler

- Güngör tarafından hazırlanan `EntranceRoom` prefabının teslim alınması.
- Geçici kapıların ve kurallar kartının gerçek oda geometrisine bağlanması.
- D0 ve D1 kapılarının gerçek konumlarının düzenlenmesi.
- D0 kapandıktan sonra fiziksel geri dönüşün engellendiğinin doğrulanması.
- Diyalog, mektup ve dönüşüm sistemleriyle eşzamanlı kontrol kilidi testlerinin yapılması.
- Giriş ve bakım odası bağlantısının gerçek prefablarla test edilmesi.
- Oda birleştirme aşamasında tek bir `GameProgress`, Player ve ana kamera kullanılmasının sağlanması.

Bu işler tamamlanmadan giriş sistemi `MainGame` sahnesine taşınmayacaktır.

## 11. Sonuç

16 Ekim çalışmaları kapsamında giriş kartı etkileşimi, kontrol kilidi, D1 açılma koşulu, bakım geçişinin kaydedilmesi ve D0 kapanma mekanizması bağımsız test sahnesinde başarıyla çalıştırılmıştır.

Giriş bölümünün teknik prototipi hazırlanmıştır. Gerçek oda prefabının entegrasyonu ve çoklu sistem testleri sonraki çalışmalara bırakılmıştır.
