# BedAutomation Projesi - Test Senaryoları ve Test Verileri

## Test Senaryosu 1: Kullanıcı Kayıt İşlevi

| # | Adım | Beklenen Sonuç | Test Verisi | +/- |
|---|------|----------------|-------------|-----|
| 1. | Kayıt sayfasına git | Kayıt sayfası doğru şekilde yüklenmeli | Buton Tıklandı | + |
| 2. | Kullanıcı adı alanını boş bırak ve diğer tüm gerekli alanları geçerli bilgilerle doldur | "Kullanıcı adı gereklidir" uyarısı görüntülenmeli | Kullanıcı adı: (Boş)<br/>E-posta: admin@test.com<br/>Şifre: Test123! | - |
| 3. | E-posta alanını boş bırak ve diğer gerekli alanları geçerli bilgilerle doldur | "E-posta adresi gereklidir" uyarısı görüntülenmeli | Kullanıcı adı: testuser<br/>E-posta: (Boş)<br/>Şifre: Test123! | - |
| 4. | Şifre alanını boş bırak ve diğer gerekli alanları doldur | "Şifre gereklidir" uyarısı görüntülenmeli | Kullanıcı adı: testuser<br/>E-posta: test@test.com<br/>Şifre: (Boş) | - |
| 5. | E-posta formatını kontrol et (geçersiz format) | "Geçerli bir e-posta adresi giriniz" uyarısı görüntülenmeli | Kullanıcı adı: testuser<br/>E-posta: invalidformat<br/>Şifre: Test123! | - |
| 6. | Zaten kullanılmış e-posta ile kayıt olmayı dene | "Bu e-posta adresi zaten kullanılmaktadır" uyarısı görüntülenmeli | Kullanıcı adı: newuser<br/>E-posta: admin@bedautomation.com<br/>Şifre: Test123! | - |
| 7. | Tüm alanları geçerli bilgilerle doldur ve formu gönder | Kayıt başarılı olmalı ve kullanıcı ana sayfaya yönlendirilmeli | Kullanıcı adı: newpatient<br/>E-posta: newpatient@test.com<br/>Şifre: Test123! | + |

## Test Senaryosu 2: Giriş İşlevi

| # | Adım | Beklenen Sonuç | Test Verisi | +/- |
|---|------|----------------|-------------|-----|
| 1. | Giriş sayfasına git | Giriş sayfası doğru şekilde yüklenmeli | Buton Tıklandı | + |
| 2. | E-posta/Kullanıcı adı alanını boş bırak ve şifre alanına geçerli şifre gir | "E-posta veya kullanıcı adı gereklidir" uyarısı görüntülenmeli | E-posta/Kullanıcı adı: (Boş)<br/>Şifre: Test123! | - |
| 3. | Şifre alanını boş bırak ve e-posta/kullanıcı adı alanına geçerli bilgi gir | "Şifre gereklidir" uyarısı görüntülenmeli | E-posta: admin@bedautomation.com<br/>Şifre: (Boş) | - |
| 4. | Geçersiz e-posta formatı gir | "Geçerli bir e-posta adresi veya kullanıcı adı giriniz" uyarısı görüntülenmeli | E-posta: invalidformat<br/>Şifre: Test123! | - |
| 5. | Geçerli olmayan kimlik bilgileri gir | "Giriş başarısız oldu" uyarısı görüntülenmeli | E-posta: wrong@test.com<br/>Şifre: WrongPass123! | - |
| 6. | Geçerli ve eşleşen bilgileri gir | Giriş başarılı olmalı ve kullanıcı ana sayfaya yönlendirilmeli | E-posta: admin@bedautomation.com<br/>Şifre: Admin123! | + |

## Test Senaryosu 3: Hasta Profili Oluşturma

| # | Adım | Beklenen Sonuç | Test Verisi | +/- |
|---|------|----------------|-------------|-----|
| 1. | Admin kullanıcısı olarak giriş yap | Giriş başarılı, admin paneli erişilebilir | E-posta: admin@bedautomation.com<br/>Şifre: Admin123! | + |
| 2. | Hastalar menüsüne git ve "Yeni Hasta Ekle" butonuna tıkla | Hasta ekleme formu açılmalı | Buton Tıklandı | + |
| 3. | Ad alanını boş bırak | "Ad alanı zorunludur" uyarısı görüntülenmeli | Ad: (Boş)<br/>Soyad: Yılmaz<br/>TC: 12345678901 | - |
| 4. | TC Kimlik No alanını geçersiz formatta gir | "Geçerli bir TC Kimlik No giriniz" uyarısı görüntülenmeli | Ad: Mehmet<br/>Soyad: Yılmaz<br/>TC: 123456 | - |
| 5. | Telefon numarasını geçersiz formatta gir | "Geçerli bir telefon numarası giriniz" uyarısı görüntülenmeli | Telefon: 123<br/>E-posta: mehmet@test.com | - |
| 6. | Tüm alanları geçerli bilgilerle doldur | Hasta kaydı başarılı olmalı | Ad: Mehmet<br/>Soyad: Yılmaz<br/>TC: 12345678901<br/>Telefon: 05551234567<br/>E-posta: mehmet@test.com | + |

## Test Senaryosu 4: Rezervasyon Oluşturma

| # | Adım | Beklenen Sonuç | Test Verisi | +/- |
|---|------|----------------|-------------|-----|
| 1. | Patient kullanıcısı olarak giriş yap | Giriş başarılı, hasta paneli erişilebilir | E-posta: jane.smith@example.com<br/>Şifre: Patient123! | + |
| 2. | Rezervasyonlar menüsüne git ve "Yeni Rezervasyon" butonuna tıkla | Rezervasyon formu açılmalı | Buton Tıklandı | + |
| 3. | Oda seçmeden rezervasyon oluşturmaya çalış | "Oda seçimi zorunludur" uyarısı görüntülenmeli | Oda: (Seçilmedi)<br/>Başlangıç: 2024-01-15<br/>Bitiş: 2024-01-20 | - |
| 4. | Geçmiş tarih seç | "Gelecek bir tarih seçiniz" uyarısı görüntülenmeli | Başlangıç: 2023-12-01<br/>Bitiş: 2023-12-05 | - |
| 5. | Bitiş tarihini başlangıç tarihinden önce seç | "Bitiş tarihi başlangıç tarihinden sonra olmalıdır" uyarısı görüntülenmeli | Başlangıç: 2024-01-20<br/>Bitiş: 2024-01-15 | - |
| 6. | Dolu olan odayı seç | "Seçilen tarihler için oda müsait değil" uyarısı görüntülenmeli | Oda: 101<br/>Başlangıç: 2024-01-15<br/>Bitiş: 2024-01-20 | - |
| 7. | Geçerli bilgilerle rezervasyon oluştur | Rezervasyon başarılı olmalı | Oda: 102<br/>Başlangıç: 2024-01-25<br/>Bitiş: 2024-01-30 | + |

## Test Senaryosu 5: Doktor Profili Yönetimi

| # | Adım | Beklenen Sonuç | Test Verisi | +/- |
|---|------|----------------|-------------|-----|
| 1. | Doctor kullanıcısı olarak giriş yap | Giriş başarılı, doktor paneli erişilebilir | E-posta: dr.smith@hospital.com<br/>Şifre: Doctor123! | + |
| 2. | Profil sayfasına git | Doktor profil bilgileri görüntülenmeli | Buton Tıklandı | + |
| 3. | Uzmanlık alanını boş bırakarak güncelle | "Uzmanlık alanı zorunludur" uyarısı görüntülenmeli | Uzmanlık: (Boş)<br/>Departman: Kardiyoloji | - |
| 4. | Lisans numarasını geçersiz formatta gir | "Geçerli bir lisans numarası giriniz" uyarısı görüntülenmeli | Lisans No: ABC<br/>Uzmanlık: Kardiyoloji | - |
| 5. | Geçerli bilgilerle profili güncelle | Profil güncelleme başarılı olmalı | Uzmanlık: Kardiyoloji<br/>Lisans No: 12345<br/>Departman: Kardiyoloji | + |

## Test Senaryosu 6: Oda ve Yatak Yönetimi

| # | Adım | Beklenen Sonuç | Test Verisi | +/- |
|---|------|----------------|-------------|-----|
| 1. | Admin kullanıcısı olarak giriş yap | Giriş başarılı, admin paneli erişilebilir | E-posta: admin@bedautomation.com<br/>Şifre: Admin123! | + |
| 2. | Odalar menüsüne git ve "Yeni Oda Ekle" butonuna tıkla | Oda ekleme formu açılmalı | Buton Tıklandı | + |
| 3. | Oda numarasını boş bırak | "Oda numarası zorunludur" uyarısı görüntülenmeli | Oda No: (Boş)<br/>Kat: 2<br/>Yatak Sayısı: 2 | - |
| 4. | Negatif yatak sayısı gir | "Yatak sayısı pozitif olmalıdır" uyarısı görüntülenmeli | Oda No: 201<br/>Yatak Sayısı: -1 | - |
| 5. | Mevcut oda numarasını tekrar kullan | "Bu oda numarası zaten mevcuttur" uyarısı görüntülenmeli | Oda No: 101<br/>Kat: 2<br/>Yatak Sayısı: 2 | - |
| 6. | Geçerli bilgilerle oda oluştur | Oda oluşturma başarılı olmalı | Oda No: 201<br/>Kat: 2<br/>Yatak Sayısı: 2<br/>Tip: Standart | + |

## Test Senaryosu 7: Tıbbi Kayıt Yönetimi

| # | Adım | Beklenen Sonuç | Test Verisi | +/- |
|---|------|----------------|-------------|-----|
| 1. | Doctor kullanıcısı olarak giriş yap | Giriş başarılı, doktor paneli erişilebilir | E-posta: dr.smith@hospital.com<br/>Şifre: Doctor123! | + |
| 2. | Tıbbi Kayıtlar menüsüne git ve "Yeni Kayıt Ekle" butonuna tıkla | Tıbbi kayıt formu açılmalı | Buton Tıklandı | + |
| 3. | Hasta seçmeden kayıt oluşturmaya çalış | "Hasta seçimi zorunludur" uyarısı görüntülenmeli | Hasta: (Seçilmedi)<br/>Tanı: Hipertansiyon | - |
| 4. | Tanı alanını boş bırak | "Tanı alanı zorunludur" uyarısı görüntülenmeli | Hasta: John Doe<br/>Tanı: (Boş) | - |
| 5. | Geçerli bilgilerle tıbbi kayıt oluştur | Tıbbi kayıt oluşturma başarılı olmalı | Hasta: John Doe<br/>Tanı: Hipertansiyon<br/>Notlar: Kan basıncı yüksek | + |

## Test Senaryosu 8: Laboratuvar Sonuçları

| # | Adım | Beklenen Sonuç | Test Verisi | +/- |
|---|------|----------------|-------------|-----|
| 1. | Doctor kullanıcısı olarak giriş yap | Giriş başarılı, doktor paneli erişilebilir | E-posta: dr.smith@hospital.com<br/>Şifre: Doctor123! | + |
| 2. | Lab Sonuçları menüsüne git ve "Yeni Sonuç Ekle" butonuna tıkla | Lab sonucu formu açılmalı | Buton Tıklandı | + |
| 3. | Test değerini negatif gir | "Test değeri pozitif olmalıdır" uyarısı görüntülenmeli | Test: Hemoglobin<br/>Değer: -5.2 | - |
| 4. | Geçersiz tarih gir | "Geçerli bir tarih giriniz" uyarısı görüntülenmeli | Tarih: 2025-13-01<br/>Test: Hemoglobin | - |
| 5. | Geçerli bilgilerle lab sonucu ekle | Lab sonucu ekleme başarılı olmalı | Hasta: John Doe<br/>Test: Hemoglobin<br/>Değer: 12.5<br/>Tarih: 2024-01-15 | + |

## Test Senaryosu 9: Reçete Yönetimi

| # | Adım | Beklenen Sonuç | Test Verisi | +/- |
|---|------|----------------|-------------|-----|
| 1. | Doctor kullanıcısı olarak giriş yap | Giriş başarılı, doktor paneli erişilebilir | E-posta: dr.smith@hospital.com<br/>Şifre: Doctor123! | + |
| 2. | Reçeteler menüsüne git ve "Yeni Reçete Yaz" butonuna tıkla | Reçete formu açılmalı | Buton Tıklandı | + |
| 3. | İlaç seçmeden reçete oluşturmaya çalış | "En az bir ilaç seçilmelidir" uyarısı görüntülenmeli | İlaç: (Seçilmedi)<br/>Hasta: John Doe | - |
| 4. | Negatif dozaj gir | "Dozaj pozitif olmalıdır" uyarısı görüntülenmeli | İlaç: Aspirin<br/>Dozaj: -100mg | - |
| 5. | Geçerli bilgilerle reçete oluştur | Reçete oluşturma başarılı olmalı | Hasta: John Doe<br/>İlaç: Aspirin<br/>Dozaj: 100mg<br/>Kullanım: Günde 2 defa | + |

## Test Senaryosu 10: Vital Signs (Yaşamsal Belirtiler)

| # | Adım | Beklenen Sonuç | Test Verisi | +/- |
|---|------|----------------|-------------|-----|
| 1. | Nurse kullanıcısı olarak giriş yap | Giriş başarılı, hemşire paneli erişilebilir | E-posta: nurse@hospital.com<br/>Şifre: Nurse123! | + |
| 2. | Vital Signs menüsüne git ve "Yeni Ölçüm Ekle" butonuna tıkla | Vital signs formu açılmalı | Buton Tıklandı | + |
| 3. | Negatif kan basıncı değeri gir | "Kan basıncı değeri pozitif olmalıdır" uyarısı görüntülenmeli | Sistolik: -120<br/>Diastolik: 80 | - |
| 4. | Aşırı yüksek ateş değeri gir | "Ateş değeri gerçekçi aralıkta olmalıdır" uyarısı görüntülenmeli | Ateş: 60°C<br/>Nabız: 80 | - |
| 5. | Geçerli vital signs ekle | Vital signs ekleme başarılı olmalı | Hasta: John Doe<br/>Sistolik: 120<br/>Diastolik: 80<br/>Ateş: 36.5°C<br/>Nabız: 75 | + |

---

## Test Verileri Özeti

### Kullanıcı Hesapları (Test için hazır)
- **Admin**: admin@bedautomation.com / Admin123!
- **Doktor 1**: dr.smith@hospital.com / Doctor123!
- **Doktor 2**: dr.johnson@hospital.com / Doctor123!
- **Hasta 1**: john.doe@example.com / Patient123!
- **Hasta 2**: jane.smith@example.com / Patient123!
- **Hasta 3**: bob.wilson@example.com / Patient123!
- **Hasta 4**: sarah.davis@example.com / Patient123!

### Test Ortamı Gereksinimleri
- PostgreSQL Database bağlantısı aktif
- Seed data yüklenmiş
- Tüm migration'lar uygulanmış
- Bootstrap 5 ve Bootstrap Icons yüklenmiş
- Modern browser (Chrome, Firefox, Edge)

### Kritik Test Alanları
1. **Authentication & Authorization** - En yüksek öncelik
2. **Patient Management** - Yüksek öncelik
3. **Reservation System** - Yüksek öncelik
4. **Medical Records** - Orta öncelik
5. **Prescription Management** - Orta öncelik
6. **Lab Results** - Orta öncelik
7. **Vital Signs** - Düşük öncelik 