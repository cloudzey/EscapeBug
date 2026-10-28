ESCAPE BUG — Görsel Yenileme Envanteri
Çalışma günü: 29 Ekim 2026
Sorumlu: Güngör
Kapsam: Giriş, Bakım, Arşiv, İksir ve Final odaları
Durum: Mevcut Unity sahnelerinin ekran görüntülerine dayalı ön envanter; 3D modeller henüz sahneye aktarılmadı.
Tasarım yönü
- Ortak dil: Kafkaesk, stilize low-poly, karanlık akademi; eski krem sıva, koyu ahşap, soğuk metal ve az miktarda pirinç detayı.
- Oynanabilirlik: Teknik collider, NavMesh, kapı, tetikleyici, tünel, şalter, mektup ve bulmaca bağlantıları korunacak.
- Uygulama: Yeni mesh'ler öncelikle ayrı Visual/dekor katmanlarına eklenecek. Graybox geometri ancak çalışma doğrulandıktan sonra ve kontrollü olarak gizlenecek/değiştirilecek.
- Paket: Daha önce üretilmiş dokulu FBX/OBJ asset paketinden yararlanılacak. Aşağıdaki model adları önceki planın hedef eşleştirmeleridir; ZIP/manifest dosya adları ve ölçüler kontrol edilmeden varlıkları veya Unity uyumlulukları kesin sayılmayacak.
Oda bazlı görsel eksikler
Oda	Ekran görüntüsünde görülen mevcut durum	Öncelikli yenileme	Korunacak teknik alan	Öncelik
Giriş	Gri banko ve gri duvarlar; kurallar/kapı çevresinde görsel kimlik zayıf	Koyu ahşap resepsiyon bankosu, kurallar panosu, sıcak duvar apliği	Kurallar UI ve kapı/etkileşim konumları	Yüksek
Bakım	Geniş boş zemin, belirgin tünel ve D2; teknik ekipman görünümü az	Elektrik dolabı, havalandırma ızgarası, üst kotta borular	TunnelEntry, PadTarget, CompanionSensors, D2, şalterler, NavMesh	Yüksek
Arşiv	Düz gri raf/dolap blokları ve masa; mektuplara göre zayıf atmosfer	Ahşap kitaplık, dosya dolabı, çalışma/okuma masası ve sıcak yerel ışık	Mektup etkileşimleri, sembol kilidi, erişim alanları	En yüksek
İksir	Üç etkileşimli şişe mevcut; masa ve çevre hâlâ basit, duvarlarda boşluk var	Simya tezgâhı, dekoratif şişe rafı, pirinç/taş ayrıntıları	A/B/C etkileşimli şişeler, LeverA/B, kablolar, Chamber ve geçişler	Orta
Final	Üç basit karakter silueti, boş ve gri salon	Koltuk, bekleme bankı, büyük saat, çıkış göstergesi	D5, EndTrigger, karakter mark'ları, kameralar ve böcek Kafka'nın çıkış rotası	Orta


Hedef asset eşleştirmesi
Öncelik	Oda	Pakette kontrol edilecek hedef model	Yerleşim ilkesi
P1	Giriş	Entrance_ReceptionDesk	Mevcut bankonun görselini yenile; etkileşim menzilini kapatma
P1	Giriş	Entrance_RulesBoard	Mevcut kurallar arayüzünü veya butonunu örtme
P1	Bakım	Maintenance_ElectricalCabinet	D2 ve şalterden uzakta, boş duvar yüzeyinde
P1	Bakım	Maintenance_VentGrille	Böcek tünelinin ölçülerini ve girişini değiştirmeden dekor olarak
P1	Arşiv	Archive_Bookshelf	Uzun, boş duvar boyunca; mektupların görünürlüğünü koru
P1	Arşiv	Archive_FilingCabinet	Dolaşım genişliğini kapatmadan duvar kenarında
P1	Arşiv	Archive_WritingTable	Mevcut masa etkileşimlerini taşımadan görsel değişim
P1	İksir	Potion_AlchemyWorkbench	Mevcut A/B/C şişeleri silmeden tezgâhı yenile
P1	İksir	Potion_BottleRack	Yalnız dekor; gerçek üç iksir şişesinin yerine geçmez
P1	Final	Final_Armchair	Kamera ve merkezdeki karakterleri kapatmadan kenarda
P1	Final	Final_ExitSign	D5'in üstünde, kapı animasyonunu engellemeden
P2	Bakım	Maintenance_PipeElbow, Maintenance_CargoCrate	Üst/yan boşluklara, NavMesh dışında veya çarpışma oluşturmadan
P2	Arşiv	Archive_BookStack, Archive_LetterBundle	Sadece dekor; Grete kanıtı olarak kullanılmaz
P2	İksir	Potion_StonePedestal, Potion_GlowLantern	İhtiyaç görülürse ekle; fazla parıltıdan kaçın
P2	Final	Final_WaitingBench, Final_GrandfatherClock	Merkezdeki final aksiyonundan uzakta


Kontrol ve riskler
1. Ölçek: Önizleme sahnesinde her modelin gerçek boyutunu 1 m referans küple karşılaştır.
2. Materyaller: OBJ/FBX malzemeleri ve dokuları Unity'de doğru bağlanıyor mu kontrol et; düz gri gelen modeli yerleştirme aşamasında çözüme kavuştur.
3. Etkileşim: Yeni görsel, mevcut collider/script referansını devralmış gibi varsayılmamalı.
4. Navigasyon: D2, bakım tüneli, oyuncu ve böcek rotaları yeniden test edilmeden teknik geometriyi değiştirme.
5. Final: RevealCameraAnchor görüşü, küçük böceğin görünürlüğü ve D5 çıkışı korunmalı.
6. Paket doğrulaması: Gerçek asset_manifest.csv ve varsa README_IMPORT.md üzerinden dosya adı, format, boyut, malzeme, lisans ve import yöntemini onayla.
29 Ekim ölçüm ve kanıt notları
- Ajan C/D/E ölçümleri: Gönderilen Unity Console ekran görüntülerinde C1–C3, D1–D3 ve E1–E3 için Success=True, Valid=True görülüyor; 9/9 başarılı.
- Ölçüm verisi: docs/tests/agent-results.csv dosyasında 9 denemenin sonuçları bulunuyor. Unity persistentDataPath içinde yazılan ham CSV satırları (özgün test_id'leri dahil) saklanmalı; gerektiğinde projedeki raporla karşılaştırılmalı.
- Negatif sensör deneyi: Engel yerleştirildiği hâlde Target Reachable=True ve Target Reason=Erişilebilir görüldü; böcek şalteri etkinleşti. Negatif sensör davranışı doğrulanmadı. Raycast hattı / sensör maskesi Bulut'la incelenecek.
- Ekran görüntüleri: Beş odaya ait Unity Scene görüntüleri paylaşıldı. Depoya eklenecekse docs/art/screenshots/ altında adlandırılıp izlenebilir hâle getirilmeli.
- 3D asset entegrasyonu: Bu çalışma gününde gerçekleştirilmiş olarak işaretlenmedi; görsel envanter ve yerleşim planı hazırlandı.
Sıradaki görsel çalışma
1. Dokulu paketin manifestini doğrula, ardından ayrı ArtPreview sahnesinde ilk beş ana modeli aç.
2. Önceliği arşiv → giriş → bakım → iksir → final olarak uygula.
3. Her odanın önce/sonra ekran görüntüsünü ve NavMesh / UI / etkileşim regresyon sonucunu kaydet.