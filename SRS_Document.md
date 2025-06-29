# Software Requirements Specification (SRS)
# BedAutomation - Hastane Yatak Otomasyon Sistemi

**Versiyon:** 1.0  
**Tarih:** 21 Aralık 2024  
**Proje:** BedAutomation Hospital Management System  
**Teknoloji:** ASP.NET Core 9 MVC + PostgreSQL  

---

## 📋 İçindekiler

1. [Giriş](#1-giriş)
2. [Genel Açıklama](#2-genel-açıklama)
3. [Sistem Özellikleri](#3-sistem-özellikleri)
4. [Fonksiyonel Gereksinimler](#4-fonksiyonel-gereksinimler)
5. [Non-Fonksiyonel Gereksinimler](#5-non-fonksiyonel-gereksinimler)
6. [Kullanıcı Arayüzü Gereksinimleri](#6-kullanıcı-arayüzü-gereksinimleri)
7. [Sistem Kısıtlamaları](#7-sistem-kısıtlamaları)
8. [Veri Gereksinimleri](#8-veri-gereksinimleri)
9. [Use Case Senaryoları](#9-use-case-senaryoları)
10. [Güvenlik Gereksinimleri](#10-güvenlik-gereksinimleri)

---

## 1. Giriş

### 1.1 Amaç
Bu belge, BedAutomation Hastane Yatak Otomasyon Sistemi'nin yazılım gereksinimlerini tanımlar. Sistem, hastane yatak yönetimi, hasta kayıt işlemleri, doktor atama ve tıbbi kayıt yönetimi süreçlerini otomatikleştirmek amacıyla geliştirilmiştir.

### 1.2 Kapsam
BedAutomation sistemi şu ana modülleri içerir:
- **Hasta Yönetimi**: Hasta kayıt, profil ve rezervasyon yönetimi
- **Yatak Yönetimi**: Oda ve yatak durumu takibi
- **Doktor Yönetimi**: Doktor profilleri ve atama işlemleri
- **Tıbbi Kayıt Sistemi**: Tıbbi kayıtlar, reçeteler, lab sonuçları
- **Rezervasyon Sistemi**: Yatak rezervasyon ve takip sistemi
- **Raporlama**: Analytics ve dashboard sistemi

### 1.3 Tanımlar ve Kısaltmalar
- **SRS**: Software Requirements Specification
- **MVC**: Model-View-Controller
- **EF Core**: Entity Framework Core
- **PostgreSQL**: Açık kaynak ilişkisel veritabanı
- **Identity**: ASP.NET Core kimlik doğrulama sistemi
- **CRUD**: Create, Read, Update, Delete işlemleri

### 1.4 Referanslar
- ASP.NET Core 9 Documentation
- PostgreSQL Documentation
- Bootstrap 5 Documentation
- Entity Framework Core Documentation

---

## 2. Genel Açıklama

### 2.1 Ürün Perspektifi
BedAutomation, modern hastaneler için tasarlanmış web tabanlı bir yönetim sistemidir. Sistem, hastane personeli ve hastaların yatak rezervasyon süreçlerini dijitalleştirerek operasyonel verimliliği artırmayı hedefler.

### 2.2 Ürün İşlevleri
#### Ana İşlevler:
1. **Kullanıcı Yönetimi**: Role-based authentication (Admin/Doctor/Patient)
2. **Hasta Kayıt Sistemi**: Hasta profil oluşturma ve düzenleme
3. **Yatak Rezervasyon**: Otomatik yatak tahsis ve rezervasyon
4. **Tıbbi Kayıt Yönetimi**: Hasta tıbbi geçmişi ve tedavi planları
5. **Reçete Yönetimi**: Elektronik reçete yazma ve takip
6. **Lab Sonuçları**: Laboratuvar test sonuçları yönetimi
7. **Vital Bulgular**: Hasta vital bulguları takibi
8. **Raporlama**: Gerçek zamanlı dashboard ve analytics

### 2.3 Kullanıcı Karakteristikleri
#### 2.3.1 Admin Kullanıcıları
- **Rol**: Sistem yöneticileri
- **Yetki**: Tam sistem erişimi
- **Teknik Seviye**: Orta-İleri düzey

#### 2.3.2 Doktor Kullanıcıları
- **Rol**: Tıp doktorları
- **Yetki**: Tıbbi kayıt ve hasta yönetimi
- **Teknik Seviye**: Temel-Orta düzey

#### 2.3.3 Hasta Kullanıcıları
- **Rol**: Hastalar
- **Yetki**: Kendi bilgileri ve rezervasyonları
- **Teknik Seviye**: Temel düzey

### 2.4 Kısıtlamalar
- Web tabanlı erişim gereksinimi
- PostgreSQL veritabanı kullanımı
- ASP.NET Core 9 framework zorunluluğu
- Türkçe dil desteği
- Bootstrap 5 UI framework kullanımı

---

## 3. Sistem Özellikleri

### 3.1 Teknoloji Stack
```
Frontend: HTML5, CSS3, JavaScript, Bootstrap 5
Backend: ASP.NET Core 9 MVC
Database: PostgreSQL
ORM: Entity Framework Core
Authentication: ASP.NET Core Identity
UI Icons: Bootstrap Icons
Styling: Modern CSS with CSS Variables
```

### 3.2 Sistem Mimarisi
- **Pattern**: MVC (Model-View-Controller)
- **Authentication**: Cookie-based authentication
- **Authorization**: Role-based access control
- **Database Access**: Repository pattern with EF Core
- **Middleware**: Custom role assignment middleware

---

## 4. Fonksiyonel Gereksinimler

### 4.1 Kullanıcı Yönetimi

#### 4.1.1 Kullanıcı Kaydı (REQ-001)
**Açıklama**: Yeni kullanıcılar sisteme kayıt olabilmelidir.
- **Input**: Email, şifre, onay şifresi
- **Process**: Otomatik "Patient" rolü atanması
- **Output**: Kullanıcı hesabı oluşturulması
- **Öncelik**: Yüksek

#### 4.1.2 Kullanıcı Girişi (REQ-002)
**Açıklama**: Kayıtlı kullanıcılar sisteme giriş yapabilmelidir.
- **Input**: Email ve şifre
- **Process**: Identity authentication
- **Output**: Role-based dashboard yönlendirme
- **Öncelik**: Yüksek

#### 4.1.3 Rol Bazlı Yetkilendirme (REQ-003)
**Açıklama**: Kullanıcılar rollerine göre farklı sayfalara erişim sağlamalıdır.
- **Roller**: Admin, Doctor, Patient
- **Process**: Middleware-based authorization
- **Output**: Role-appropriate UI ve işlevler
- **Öncelik**: Kritik

### 4.2 Hasta Yönetimi

#### 4.2.1 Hasta Profili Oluşturma (REQ-004)
**Açıklama**: Hastalar kendi profillerini oluşturabilmelidir.
- **Input**: Ad, soyad, TC kimlik, telefon, email, doğum tarihi, cinsiyet, adres
- **Validation**: TC kimlik unique kontrolü, email format kontrolü
- **Output**: Hasta profili kaydı
- **Öncelik**: Yüksek

#### 4.2.2 Hasta Profili Düzenleme (REQ-005)
**Açıklama**: Hastalar kendi bilgilerini güncelleyebilmelidir.
- **Input**: Güncellenmiş hasta bilgileri
- **Process**: Ownership validation
- **Output**: Güncellenmiş profil bilgileri
- **Öncelik**: Orta

#### 4.2.3 Admin Hasta Yönetimi (REQ-006)
**Açıklama**: Admin kullanıcıları tüm hastaları yönetebilmelidir.
- **Fonksiyonlar**: Listeleme, ekleme, düzenleme, silme
- **Process**: Admin role requirement
- **Output**: Hasta yönetim işlemleri
- **Öncelik**: Yüksek

### 4.3 Yatak ve Rezervasyon Yönetimi

#### 4.3.1 Yatak Rezervasyonu (REQ-007)
**Açıklama**: Hastalar yatak rezervasyonu yapabilmelidir.
- **Input**: Departman, oda tipi, oda, yatak seçimi, giriş tarihi
- **Validation**: Aktif rezervasyon kontrolü, yatak müsaitlik kontrolü
- **Process**: Cascading dropdown selection
- **Output**: Rezervasyon kaydı ve yatak durumu güncelleme
- **Öncelik**: Kritik

#### 4.3.2 Rezervasyon İptali (REQ-008)
**Açıklama**: Hastalar rezervasyonlarını iptal edebilmelidir.
- **Input**: İptal nedeni
- **Process**: Rezervasyon durumu güncelleme, yatak boşaltma
- **Output**: İptal edilmiş rezervasyon
- **Öncelik**: Yüksek

#### 4.3.3 Yatak Durumu Senkronizasyonu (REQ-009)
**Açıklama**: Yatak doluluk durumları otomatik senkronize edilmelidir.
- **Process**: Aktif rezervasyonlar ile yatak durumlarının eşleştirilmesi
- **Trigger**: Sistem başlangıcı ve dashboard erişimi
- **Output**: Güncel yatak durumları
- **Öncelik**: Yüksek

### 4.4 Doktor Yönetimi

#### 4.4.1 Doktor Profili Oluşturma (REQ-010)
**Açıklama**: Doktorlar kendi profillerini oluşturabilmelidir.
- **Input**: Ad, soyad, TC kimlik, lisans numarası, uzmanlık, deneyim yılı
- **Validation**: Lisans numarası unique kontrolü
- **Output**: Doktor profili kaydı
- **Öncelik**: Yüksek

#### 4.4.2 Doktor Atama (REQ-011)
**Açıklama**: Rezervasyonlara doktor atanabilmelidir.
- **Input**: Rezervasyon ID, Doktor ID
- **Process**: Optional doctor assignment
- **Output**: Doktor-hasta ilişkisi
- **Öncelik**: Orta

### 4.5 Tıbbi Kayıt Yönetimi

#### 4.5.1 Tıbbi Kayıt Oluşturma (REQ-012)
**Açıklama**: Doktorlar hasta tıbbi kayıtları oluşturabilmelidir.
- **Input**: Hasta bilgileri, şikayet, muayene bulguları, değerlendirme, plan
- **Authorization**: Doctor/Admin role requirement
- **Output**: Tıbbi kayıt kaydı
- **Öncelik**: Yüksek

#### 4.5.2 Reçete Yönetimi (REQ-013)
**Açıklama**: Doktorlar elektronik reçete yazabilmelidir.
- **Input**: Hasta, ilaçlar, dozaj, kullanım talimatları
- **Process**: İlaç veritabanı entegrasyonu
- **Output**: Elektronik reçete kaydı
- **Öncelik**: Yüksek

#### 4.5.3 Lab Sonuçları (REQ-014)
**Açıklama**: Lab sonuçları sisteme kaydedilebilmelidir.
- **Input**: Hasta, test türü, parametreler, sonuçlar
- **Process**: Referans değer karşılaştırması
- **Output**: Lab sonucu kaydı
- **Öncelik**: Orta

#### 4.5.4 Vital Bulgular (REQ-015)
**Açıklama**: Hasta vital bulguları takip edilebilmelidir.
- **Input**: Hasta, vital sign türü, değer, ölçüm tarihi
- **Process**: Trend analizi ve charting
- **Output**: Vital sign kayıtları ve grafikler
- **Öncelik**: Orta

### 4.6 Self-Service Portal

#### 4.6.1 Hasta Portal Erişimi (REQ-016)
**Açıklama**: Hastalar kendi bilgilerine erişebilmelidir.
- **Fonksiyonlar**: 
  - Tıbbi kayıtları görüntüleme
  - Lab sonuçları görüntüleme
  - Reçete geçmişi görüntüleme
  - Vital bulgular görüntüleme
  - Rezervasyon durumu kontrolü
- **Authorization**: Patient ownership validation
- **Output**: Kişisel sağlık dashboard'u
- **Öncelik**: Yüksek

### 4.7 Raporlama ve Analytics

#### 4.7.1 Admin Dashboard (REQ-017)
**Açıklama**: Admin kullanıcıları sistem istatistiklerini görüntüleyebilmelidir.
- **Metrics**: 
  - Toplam hasta sayısı
  - Toplam doktor sayısı
  - Yatak doluluk oranı
  - Aktif rezervasyon sayısı
  - Aylık rezervasyon trendleri
- **Real-time**: Gerçek zamanlı veri güncellemesi
- **Output**: Kapsamlı analytics dashboard
- **Öncelik**: Yüksek

---

## 5. Non-Fonksiyonel Gereksinimler

### 5.1 Performans Gereksinimleri

#### 5.1.1 Yanıt Süresi (NFR-001)
- **Web sayfası yükleme süresi**: < 2 saniye
- **Database sorgu süresi**: < 500ms
- **AJAX istekleri**: < 1 saniye
- **Dashboard yükleme**: < 3 saniye

#### 5.1.2 Throughput (NFR-002)
- **Eşzamanlı kullanıcı**: 100+ concurrent users
- **Database connection**: Connection pooling
- **Session management**: Efficient session handling

#### 5.1.3 Scalability (NFR-003)
- **Horizontal scaling**: Load balancer ready
- **Database scaling**: PostgreSQL clustering support
- **Caching**: Memory caching implementation

### 5.2 Güvenilirlik (NFR-004)
- **Uptime**: 99.9% availability
- **Error handling**: Graceful error management
- **Data backup**: Automated database backups
- **Recovery**: Disaster recovery procedures

### 5.3 Kullanılabilirlik (NFR-005)
- **User experience**: Modern, responsive design
- **Accessibility**: WCAG 2.1 guidelines
- **Cross-browser**: Chrome, Firefox, Safari, Edge support
- **Mobile-friendly**: Responsive design implementation

### 5.4 Güvenlik (NFR-006)
- **Authentication**: Secure login system
- **Authorization**: Role-based access control
- **Data encryption**: Password hashing (bcrypt)
- **Session security**: Secure session management
- **Input validation**: SQL injection prevention
- **HTTPS**: Encrypted communication

---

## 6. Kullanıcı Arayüzü Gereksinimleri

### 6.1 Genel UI Standartları
- **Framework**: Bootstrap 5
- **Typography**: Inter font family
- **Color Scheme**: CSS variables based color system
- **Icons**: Bootstrap Icons
- **Responsive**: Mobile-first design approach

### 6.2 Color Palette
```css
--primary: #6366f1 (Indigo)
--secondary: #64748b (Slate)
--success: #10b981 (Emerald)
--danger: #ef4444 (Red)
--warning: #f59e0b (Amber)
--info: #06b6d4 (Cyan)
--light: #f8fafc (Gray-50)
--dark: #0f172a (Gray-900)
```

### 6.3 Layout Gereksinimleri
- **Navigation**: Sidebar navigation with role-based menu items
- **Header**: User info and logout functionality
- **Content Area**: Main content with breadcrumbs
- **Footer**: System information and links
- **Cards**: Shadow-based card design for content blocks

### 6.4 Form Standartları
- **Validation**: Real-time client-side validation
- **Error Messages**: Clear, actionable error messages
- **Required Fields**: Visual indicators for required fields
- **Success Feedback**: Confirmation messages for successful operations

### 6.5 Dashboard Requirements
- **Statistics Cards**: Key metrics display
- **Charts**: Interactive charts for trends
- **Tables**: Sortable, searchable data tables
- **Filters**: Dynamic filtering capabilities

---

## 7. Sistem Kısıtlamaları

### 7.1 Teknoloji Kısıtlamaları
- **Platform**: ASP.NET Core 9 zorunluluğu
- **Database**: PostgreSQL kullanımı
- **Frontend**: Server-side rendering (MVC pattern)
- **Authentication**: ASP.NET Core Identity

### 7.2 İş Kısıtlamaları
- **Dil**: Türkçe arayüz zorunluluğu
- **Timezone**: UTC timezone kullanımı
- **Data Format**: ISO standartları uygulaması

### 7.3 Regulasyon Kısıtlamaları
- **KVKK**: Kişisel veri koruma uyumluluğu
- **Tıbbi Kayıt**: Tıbbi kayıt saklama düzenlemeleri
- **Güvenlik**: Sağlık veri güvenliği standartları

---

## 8. Veri Gereksinimleri

### 8.1 Ana Entities

#### 8.1.1 Patient (Hasta)
```csharp
- Id: Primary Key
- FirstName: string(100) - Required
- LastName: string(100) - Required
- IdentityNumber: string(11) - Required, Unique
- PhoneNumber: string - Required
- Email: string - Required, Email format
- DateOfBirth: DateTime - Required
- Gender: string(10) - Required
- Address: string(500)
- CreatedAt: DateTime - UTC
- UserId: Foreign Key to IdentityUser
```

#### 8.1.2 Reservation (Rezervasyon)
```csharp
- Id: Primary Key
- PatientId: Foreign Key - Required
- BedId: Foreign Key - Required
- CheckInDate: DateTime - Required
- CheckOutDate: DateTime - Nullable
- Status: string(50) - Required (Reserved/Active/Completed/Cancelled)
- Priority: string(100) - Required (Emergency/High/Normal/Low)
- AdmissionType: string(100) - Required
- MedicalNotes: string(1000)
- CreatedAt: DateTime - UTC
- DoctorId: Foreign Key - Optional
```

#### 8.1.3 Doctor (Doktor)
```csharp
- Id: Primary Key
- FirstName: string(100) - Required
- LastName: string(100) - Required
- IdentityNumber: string(11) - Required, Unique
- LicenseNumber: string(50) - Required, Unique
- Specialization: string(200) - Required
- YearsOfExperience: int - Required
- Email: string - Required
- PhoneNumber: string - Required
- IsActive: bool - Default true
- CreatedAt: DateTime - UTC
- UserId: Foreign Key to IdentityUser
```

#### 8.1.4 MedicalRecord (Tıbbi Kayıt)
```csharp
- Id: Primary Key
- PatientId: Foreign Key - Required
- DoctorId: Foreign Key - Optional
- RecordType: string(50) - Required
- ChiefComplaint: string(2000) - Required
- Assessment: string(1000)
- Plan: string(1000)
- RecordDate: DateTime - Required
- Status: string(50) - Required
- IsConfidential: bool - Default false
- CreatedAt: DateTime - UTC
```

### 8.2 Database Constraints
- **Unique Constraints**: IdentityNumber (Patient, Doctor), LicenseNumber (Doctor)
- **Foreign Key Constraints**: Cascade delete policies
- **Check Constraints**: Email format, phone format validation
- **Index Requirements**: Performance optimization indexes

### 8.3 Data Volume Estimates
- **Patients**: 10,000+ records
- **Reservations**: 50,000+ records annually
- **Medical Records**: 100,000+ records annually
- **Lab Results**: 200,000+ records annually

---

## 9. Use Case Senaryoları

### 9.1 Hasta Rezervasyon Senaryosu

**Primary Actor**: Patient User  
**Goal**: Yatak rezervasyonu yapma  
**Preconditions**: 
- Kullanıcı sisteme giriş yapmış
- Hasta profili oluşturulmuş
- Aktif rezervasyon bulunmamakta

**Main Success Scenario**:
1. Hasta rezervasyon oluştur sayfasına gider
2. Sistem departman listesini gösterir
3. Hasta departman seçer
4. Sistem oda tiplerini gösterir
5. Hasta oda tipi seçer
6. Sistem uygun odaları gösterir
7. Hasta oda seçer
8. Sistem boş yatakları gösterir
9. Hasta yatak ve giriş tarihi seçer
10. Hasta rezervasyon formunu gönderir
11. Sistem rezervasyonu kaydeder
12. Yatak durumu güncellenir
13. Başarı mesajı gösterilir

**Alternative Scenarios**:
- 2a. Aktif rezervasyon varsa hata mesajı
- 8a. Boş yatak yoksa alternatif önerisi
- 10a. Yatak başkası tarafından rezerve edilmişse hata

### 9.2 Doktor Tıbbi Kayıt Senaryosu

**Primary Actor**: Doctor User  
**Goal**: Hasta için tıbbi kayıt oluşturma  
**Preconditions**: 
- Doktor sisteme giriş yapmış
- Doktor profili oluşturulmuş

**Main Success Scenario**:
1. Doktor tıbbi kayıt oluştur sayfasına gider
2. Sistem hasta listesini gösterir
3. Doktor hasta seçer
4. Sistem tıbbi kayıt formunu gösterir
5. Doktor şikayet, muayene, değerlendirme bilgilerini girer
6. Doktor kayıt tipini ve durumunu seçer
7. Doktor formu gönderir
8. Sistem tıbbi kaydı oluşturur
9. Başarı mesajı gösterilir

### 9.3 Admin Dashboard Senaryosu

**Primary Actor**: Admin User  
**Goal**: Sistem durumunu görüntüleme  
**Preconditions**: Admin sisteme giriş yapmış

**Main Success Scenario**:
1. Admin ana sayfaya gider
2. Sistem istatistikleri hesaplar
3. Yatak doluluk durumu senkronize edilir
4. Dashboard metrikleri gösterilir
5. Aylık trend grafikleri yüklenir
6. Son rezervasyonlar listelenir
7. Durum dağılımı gösterilir

---

## 10. Güvenlik Gereksinimleri

### 10.1 Authentication Requirements
- **Password Policy**: Minimum 6 karakter, rakam zorunluluğu
- **Session Management**: Secure session cookies
- **Login Attempts**: Rate limiting implementation
- **Password Recovery**: Secure password reset mechanism

### 10.2 Authorization Requirements
- **Role-Based Access**: Admin/Doctor/Patient role hierarchy
- **Resource-Level Security**: User ownership validation
- **Method-Level Security**: Controller action authorization
- **Data Filtering**: User-specific data access

### 10.3 Data Protection
- **Password Hashing**: bcrypt/PBKDF2 implementation
- **Personal Data**: KVKK compliance
- **Medical Data**: Enhanced security for medical records
- **Audit Trail**: User action logging

### 10.4 Input Validation
- **SQL Injection**: Parameterized queries (EF Core)
- **XSS Prevention**: Input sanitization
- **CSRF Protection**: Anti-forgery tokens
- **File Upload**: File type and size validation

### 10.5 Communication Security
- **HTTPS**: SSL/TLS encryption requirement
- **API Security**: Secure API endpoints
- **Session Security**: Secure session configuration
- **CORS**: Cross-origin resource sharing policies

---

## 📊 Sonuç

Bu SRS belgesi, BedAutomation Hastane Yatak Otomasyon Sistemi'nin tüm fonksiyonel ve non-fonksiyonel gereksinimlerini kapsamaktadır. Sistem, modern web teknolojileri kullanarak güvenli, performanslı ve kullanıcı dostu bir hastane yönetim çözümü sunmaktadır.

### Ana Hedefler:
- ✅ **Operasyonel Verimlilik**: Yatak yönetimi süreçlerinin otomatikleştirilmesi
- ✅ **Hasta Memnuniyeti**: Self-service portal ile hasta deneyiminin iyileştirilmesi
- ✅ **Veri Güvenliği**: KVKK uyumlu güvenli veri yönetimi
- ✅ **Scalability**: Büyüyen hastane ihtiyaçlarına uyum sağlama
- ✅ **Modern UI/UX**: Bootstrap 5 ile modern kullanıcı arayüzü

Bu dokümantasyon, geliştirme ekibi için rehber niteliğinde olup, sistemin başarılı implementasyonu için gerekli tüm bilgileri içermektedir. 