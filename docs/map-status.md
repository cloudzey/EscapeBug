# ESCAPE BUG — Harita Durumu

**Son güncelleme:** 17 Ekim 2026  
**Kapsam:** 15 Ekim bakım odası çalışmaları ve 16–17 Ekim giriş/arşiv odası çalışmaları  
**Sorumlu:** Bulut

---

## 1. Tamamlanan İş Paketleri

### 15 Ekim — Bakım Odası Düzenleme ve Teslim

Bakım odasının temel oynanış mekanikleri, sahne organizasyonu ve görsel yönlendirmeleri düzenlendi.

### 16–17 Ekim — Giriş ve Arşiv Odaları

- EntranceRoom oda kabuğu ve temel dekorları oluşturuldu.
- EntranceRoom prefabı mevcut giriş mekanikleriyle birleştirildi.
- ArchiveRoom oda kabuğu ve temel dekorları oluşturuldu.
- Üç mektubun ve sembol kilidinin görsel yerleşimi tamamlandı.
- ArchiveRoom prefabı oluşturuldu.
- Bağımsız sahnelerde hareket, kamera ve erişilebilirlik testleri gerçekleştirildi.

**Not:** Oda kabuğunun tamamlanması, o odadaki tüm bulmacaların veya birleşik harita entegrasyonunun tamamlandığı anlamına gelmez.

---

## 2. Bakım Odası — MaintenanceRoom

**Durum:** Temel oynanış ve oda düzeni hazır. Son görsel tasarım ve diğer odalarla bütünleştirme henüz tamamlanmadı.

### 2.1. Oynanış

- Böceğin şalteri tünelin sonuna taşındı.
- D2 kapısı, insan ve böcek şalterlerinin ikisi de etkinleştirildiğinde açılacak şekilde düzenlendi.
- İnsan şalteri D2'nin yanına yerleştirildi.
- Eski TestDoor ve DoorPanel devre dışı bırakıldı.
- Böceğin duvar köşesinden kestirme yapmasına neden olan sabit kapı çerçevelerinin navigasyon katmanları düzeltildi.
- Açık güzergâhta tünelden gidiş ve dönüş doğrulandı.
- Kapalı güzergâh testinde görevin başarısız olduğu doğrulandı.

### 2.2. Oda Organizasyonu

- BugMechanism ve TestSwitch, Mechanisms altında toplandı.
- BugMoveTarget, Anchors altına alındı.
- TunnelEntry oluşturuldu.
- Mevcut CommandPoint konumu kontrol edildi.
- Görsel yönlendirmeler Visuals altında toplandı.
- Player ve Companion, oda prefabının dışında bırakıldı.

### 2.3. Görsel Yönlendirme

- PadVisual, böcek şalterinin arka levhası olarak düzenlendi.
- PadVisual'in collider'ı kapatıldı ve katmanı Default yapıldı.
- Tünel girişinden mekanizmaya uzanan turkuaz şeritler eklendi.
- D2 yanına iki şalter gerektiğini açıklayan yazı yerleştirildi.
- Yönlendirme şeritleri collider olmadan düzenlendi.

### 2.4. Kontroller ve Kayıt

- Oyuncu kamerasıyla giriş, tünel önü ve D2 çevresi kontrol edildi.
- D2 yazısının okunabilirliği ve oyuncunun kapıdan geçişi kontrol edildi.
- Oda değişiklikleri mevcut MaintenanceRoom prefabına uygulandı.
- Prefab içindeki şalter, hedef ve kapı bağlantıları kontrol edildi.

### 2.5. Tasarım Kararı

D2 için iki şalter de zorunludur.

Şalterlerden yalnızca birinin etkinleştirilmesi kapıyı açmaz.

Bu karar, eski plandaki yalnızca böcek şalteriyle açılma kuralının yerini alır.

---

## 3. Giriş Odası — EntranceRoom

**Durum:** Oda kabuğu, temel nesneler ve giriş etkileşim sistemi hazır. Bağımsız giriş testi başarılı.

**Yaklaşık boyut:** 6 × 5 metre

### 3.1. Oda Geometrisi

- Floor oluşturuldu.
- Dört duvar oluşturuldu.
- D0 dış giriş açıklığı hazırlandı.
- D1 bakım odası geçiş açıklığı hazırlandı.
- Duvar ve zemin collider'ları düzenlendi.
- Ana yürüyüş güzergâhı açık bırakıldı.

### 3.2. Yerleştirilen Nesneler

- Counter
- CounterBody
- RulesCardVisual
- Vestiyer
- VestiyerBody
- Hook_01
- Hook_02
- Hook_03

Banko, D0 ile D1 arasındaki ana geçişi engellemeyecek şekilde yerleştirildi.

Kurallar kartı bankonun üzerine konumlandırıldı.

### 3.3. Referans Noktaları

- PlayerStart
- RulesPoint
- DoorD0
- DoorD1

### 3.4. Teknik Bağlantılar

EntranceTest sahnesinde daha önce hazırlanan giriş mekanikleri EntranceRoom prefab örneğiyle birleştirildi.

- Mevcut Player prefabı kullanıldı.
- ThirdPersonCamera sistemi yeniden kullanıldı.
- RulesCardVisual üzerine mevcut RulesCard etkileşim bileşeni bağlandı.
- Kurallar panelinin açılması ve kapanması sağlandı.
- Kontrol kilidi sistemi kullanıldı.
- Kurallar okunduktan sonra D1'in açılması sağlandı.
- MaintenanceEntryTrigger üzerinden bakım alanına geçiş kaydedildi.
- Geçiş sonrasında D0'ın kapanması sağlandı.

Giriş mekanikleri için mevcut DoorController ve GameProgress sistemleri kullanıldı.

### 3.5. Test Sonuçları

- [x] Oyuncu giriş odasında hareket edebiliyor.
- [x] Kamera oyuncuyu takip ediyor.
- [x] Kurallar kartına yaklaşılabiliyor.
- [x] E tuşuyla kurallar paneli açılıyor.
- [x] Panel KAPAT veya Escape ile kapanıyor.
- [x] Kurallar tamamlandığında D1 açılıyor.
- [x] Bakım geçişi GameProgress üzerinden kaydediliyor.
- [x] Bakım geçişinden sonra D0 kapanıyor.

### 3.6. Açık İşler

- Nihai materyal, aydınlatma ve dekorların düzenlenmesi.
- EntranceRoom ile MaintenanceRoom'un birleşik haritada hizalanması.
- Birleşik haritada D0'ın fiziksel geri dönüşü engellemesinin tekrar test edilmesi.
- Diğer UI sistemleriyle eşzamanlı kontrol kilidi testlerinin yapılması.

---

## 4. Arşiv Odası — ArchiveRoom

**Durum:** Oda kabuğu, temel dekorlar, mektup modelleri ve kilit yerleşimi hazır. Bağımsız yürüyüş testi başarılı.

**Yaklaşık boyut:** 8 × 7 metre

### 4.1. Oda Geometrisi

- Floor oluşturuldu.
- Kuzey ve güney duvarları oluşturuldu.
- D2 giriş açıklığı hazırlandı.
- D3 çıkış açıklığı hazırlandı.
- Duvar ve zemin collider'ları oluşturuldu.

D2 bakım odasından giriş, D3 ise iksir odasına çıkış için ayrıldı.

### 4.2. Yerleştirilen Nesneler

- ReadingDesk
- DeskTop
- Dört masa ayağı
- Shelf_01
- Shelf_02
- LetterInvite
- LetterGrete
- LetterFelice
- LockPanel
- SymbolSlot_01
- SymbolSlot_02
- SymbolSlot_03

### 4.3. Mektup Yerleşimleri

| Mektup | Konum | Tarih | Sembol |
|---|---|---|---|
| LetterInvite | Okuma masası | 3 Mart | Anahtar |
| LetterGrete | Birinci raf | 12 Mart | Göz |
| LetterFelice | İkinci raf | 21 Mart | Güve |

Bu tarihler ve semboller hikâye defterindeki bulmaca tasarımına aittir.

Mektuplar şu anda fiziksel görsel nesnelerdir. Okuma ve envanter mekanikleri henüz bağlanmamıştır.

### 4.4. Sembol Kilidi

LockPanel, D3 kapısının yanına yerleştirildi.

Üç sembol yuvası için geçici görsel parçalar oluşturuldu.

**Planlanan çözüm:** Anahtar → Göz → Güve

Bu çözümün kontrolünü gerçekleştiren kod ve D3 kapısının açılma mekanizması henüz geliştirilmemiştir.

### 4.5. Referans Noktaları

- ArchivePlayerStart
- DoorD2
- DoorD3
- LetterInvitePoint
- LetterGretePoint
- LetterFelicePoint
- LockPanelPoint

### 4.6. ArchiveTest Kontrolleri

- [x] Oyuncu arşiv odasında hareket edebiliyor.
- [x] Zemin ve duvar collider'ları çalışıyor.
- [x] Okuma masasının çevresinden dolaşılabiliyor.
- [x] Rafların yanına yaklaşılabiliyor.
- [x] LetterInvite görünür durumda.
- [x] LetterGrete görünür durumda.
- [x] LetterFelice görünür durumda.
- [x] LockPanel'e yaklaşılabiliyor.
- [x] D2 ve D3 açıklıklarına erişilebiliyor.
- [x] Kamera ve dolaşım testlerinde sorun bildirilmedi.

### 4.7. Henüz Geliştirilmeyen Mekanikler

- Mektupların E tuşuyla açılması.
- Mektup okuma arayüzü.
- Mektup kayıt ve tekrar okuma sistemi.
- Grete mektubunun final kanıtına bağlanması.
- Üç sembollü kilit arayüzü.
- D3 kapısının doğru sembol sırasıyla açılması.

Bu mekanikler sonraki geliştirme görevlerine bırakılmıştır.

---

## 5. Genel Harita Durumu

| Alan | Oda kabuğu | Teknik durum | Sıradaki iş |
|---|---|---|---|
| Giriş | Tamamlandı | D0–D1 akışı bağımsız testte çalışıyor | Birleşik harita entegrasyonu |
| Bakım | Tamamlandı | Böcek navigasyonu ve D2 mekanizması çalışıyor | Son görsel düzenleme ve entegrasyon |
| Arşiv | Tamamlandı | Bağımsız yürüyüş testi başarılı | Mektup sistemi ve D3 kilidi |
| İksir | Henüz hazırlanmadı | Mekanikler bekliyor | Oda kabuğu ve iksir düzeni |
| Final | Henüz hazırlanmadı | Diyalog/LLM için ayrı test altyapıları mevcut | Oda kabuğu ve final entegrasyonu |

**Not:** D3, D4 ve D5 kapı mekanikleri henüz tamamlanmadı. Bu kapılar için yalnızca hikâye ve ilerleme koşulları belirlenmiş durumda.

---

## 6. Prefab ve Sahne Organizasyonu

### Hazırlanan Oda Prefabları

- MaintenanceRoom
- EntranceRoom
- ArchiveRoom

### Kullanılan Test Sahneleri

- CompanionTest — Böcek ve bakım mekanikleri
- EntranceTest — Giriş etkileşimi ve D0–D1 mekanikleri
- ArchiveTest — Arşiv yürüyüş ve erişilebilirlik testleri
- ArtTest — Oda geometrileri ve görsel hazırlık

### Prefab Kuralları

Oda prefablarının içerisinde global sistemlerin kopyaları bulunmaz.

- Player oda prefabının içinde bulunmaz.
- Main Camera oda prefabının içinde bulunmaz.
- GameProgress oda prefabının içinde bulunmaz.
- EventSystem oda prefabının içinde bulunmaz.

Sahneye özel referanslar gerekli olduğunda oda prefabının sahne örneğinde bağlanır.

Oda modelleri bağımsız şekilde hazırlanır; birleşik oyun sahnesine entegrasyon ayrı aşamada gerçekleştirilir.

---

## 7. Açık İşler

### Görsel ve Harita

- İksir odasının temel geometrisini oluşturmak.
- Final alanının temel geometrisini oluşturmak.
- Bakım odasının son materyal, aydınlatma ve dekor çalışmalarını yapmak.
- Giriş ve arşiv odalarının görsel ayrıntılarını geliştirmek.
- Hazırlanan odaları kapı açıklıklarına göre hizalamak.

### Oynanış

- Arşiv mektuplarına okuma ve kayıt mekaniklerini bağlamak.
- D3 için üç sembollü kilit sistemini geliştirmek.
- İksir ve dönüşüm mekaniklerini geliştirmek.
- D4 kapı koşullarını uygulamak.
- Final odası ve D5 kapı mekanizmasını geliştirmek.
- Şalterlerin etkin durumlarını gösteren görsel geri bildirim eklemek.

### Entegrasyon ve Test

- Odalar birleştirildiğinde NavMesh'i bütün haritaya göre yeniden hazırlamak.
- Açılan kapılardan companion geçişini ayrıca doğrulamak.
- Birleşik haritada kamera ve collider testlerini tekrarlamak.
- Farklı etkileşim panellerinin kontrol kilitlerini birlikte test etmek.
- Tam oyun rotası kurulduktan sonra uçtan uca oynanış testi yapmak.

---

## 8. Genel Sonuç

15 Ekim çalışmalarında bakım odasının oynanış mekanikleri ve oda organizasyonu düzenlendi.

16–17 Ekim çalışmalarında giriş ve arşiv odalarının oynanabilir kabukları, temel dekorları, geçiş açıklıkları ve referans noktaları hazırlandı.

Giriş odasında kurallar kartı ve D0–D1 ilerleme mekanikleri mevcut test sahnesine bağlanarak doğrulandı.

Arşiv odasında masa, raflar, üç mektup ve sembol kilidi için gerekli fiziksel yerleşim tamamlandı. Oyuncu hareketi, kamera ve erişilebilirlik testleri başarılı sonuçlandı.

**Güncel durum:** Giriş, bakım ve arşiv alanlarının temel oda yapıları hazırdır. Arşiv bulmacası, iksir bölümü, final bölümü ve birleşik harita entegrasyonu sonraki çalışmalara bırakılmıştır.

## 17 Ekim — İksir ve final oda kabukları

- PotionRoom ve FinalRoom prefabları oluşturuldu.
- Kapılar, mekanizma yerleri ve hedef noktaları yerleştirildi.
- Final karakter maketlerinin konumları düzenlendi.
- İnsanla duvar ve kapı çarpışmaları kontrol edildi.
- Final çıkış platformuna geçiş doğrulandı.
- İnsan, iksir bölmesine kapalı kapıdan veya küçük açıklıktan giremedi.
- Küçük böceğin dar açıklıktan gidiş ve dönüşü doğrulandı.

Henüz bağlı olmayanlar:
- İksir ve dönüşüm sistemi.
- LeverA'nın mandallı davranışı ve LeverB etkileşimi.
- İkinci kolun dışarıdan kullanılmasını engelleyen etkileşim kontrolü.
- Bölme kapısı ve D4'ün mekanizma bağlantıları.
- Final diyalogunun oda içindeki tetiklenmesi ve EndTrigger bitiş akışı.
- Açılan kapılara göre navigasyonun güncellenmesi.