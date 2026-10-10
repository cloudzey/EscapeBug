# Companion durum diyagramı

```mermaid
stateDiagram-v2
    direction TB

    [*] --> Wait
    Wait --> Follow: Başlangıç veya R
    Follow --> Wait: Q

    Follow --> MoveToTarget: F ve geçerli mekanizma
    Wait --> MoveToTarget: F ve geçerli mekanizma
    Failed --> MoveToTarget: F ve geçerli mekanizma

    MoveToTarget --> Interact: Varış, yönelme ve sensör başarısı
    MoveToTarget --> Failed: Yol, sensör veya süre hatası

    Interact --> Return: Mekanizma etkinleştirildi
    Interact --> Failed: Sensör, etkinleştirme veya süre hatası

    Return --> Follow: Oyuncuya ulaşıldı
    Return --> Failed: Yol veya süre hatası

    Follow --> Failed: Takip yolu hatası
    Failed --> Follow: R
    Failed --> Wait: Q
```

## Ek kurallar

- MoveToTarget, Interact ve Return sırasında ajan meşguldür.
- Meşgulken yeni F, Q ve R komutları görev akışını değiştirmez.
- Kullanılmış mekanizmaya verilen komut yeni görev başlatmaz.
- Geçersiz mekanizma ve kayıp zorunlu referanslar Failed oluşturabilir.
- Varış tek başına etkileşim başarısı değildir; sensör kontrolü gerekir.
- Görsel model, ajanın hareket ve algılama bileşenlerinden ayrıdır.