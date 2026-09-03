## Ne değişti?

<!-- Kısa açıklama. Bir issue'yu kapatıyorsa: Closes #123 -->

## Neden?

<!-- Bu değişikliğin çözdüğü problem -->

## Kontrol listesi

- [ ] `dotnet build -c Release` çalışıyor, uyarı yok
- [ ] `dotnet test -c Release` çalışıyor, tüm testler geçiyor
- [ ] Testi **önce** yazdım ve başarısız olduğunu gördüm
- [ ] Commit'lerde gerçek kimlik bilgisi yok

### Yeni bir uç eklediyseniz

- [ ] Uç, **resmi dokümantasyondan** doğrulandı (üçüncü taraf kaynak değil)
- [ ] `docs/vendor/<borsa>-capabilities.md` güncellendi (erişim tarihiyle)
- [ ] Fixture eklendi, tercihen canlı API'den alınmış gerçek yanıt
- [ ] Contract testi eklendi (`tests/.../Endpoints/`)
- [ ] Geçersiz girdi ağa çıkmadan reddediliyor
- [ ] README'deki uç tablosu güncellendi

### Dokümantasyondan farklı bir davranış bulduysanız

- [ ] `docs/spec/` ekine not düştüm

<!--
Not: Emir işlemleriyle ilgili değişiklikler gerçek para hareketi yaratabilir.
Otomatik yeniden deneme eklenmez (ADR-009).
-->
