# Tobasa OpenJKN Bridge

Tobasa OpenJKN Bridge adalah aplikasi middleware open-source dengan lisensi
GNU GPL yang dirancang untuk menjembatani sistem informasi fasilitas kesehatan 
dengan layanan Mobile JKN yang disediakan oleh BPJS Kesehatan.

Aplikasi ini dibangun menggunakan .NET 6 dan dapat berjalan pada sistem
operasi Windows maupun Linux, sehingga fleksibel untuk berbagai
kebutuhan infrastruktur fasilitas kesehatan.


## Tujuan Project
-   Menyederhanakan proses integrasi Mobile JKN
-   Menyediakan middleware yang stabil, aman, dan scalable
-   Mendukung berbagai sistem basis data
-   Dapat digunakan oleh fasilitas kesehatan tingkat pertama dan
    lanjutan


## Dukungan Fasilitas Kesehatan

### FKTP (Fasilitas Kesehatan Tingkat Pertama)
-   Klinik
-   Puskesmas
### FKRTL (Fasilitas Kesehatan Rujukan Tingkat Lanjutan)
-   Rumah Sakit 



## Fitur Utama
-   REST API berbasis .NET 6
-   Cross-platform (Windows & Linux)
-   Middleware integrasi Mobile JKN
-   Logging dan monitoring
-   Konfigurasi fleksibel
-   Siap dijalankan secara on-premise


## Dukungan Database
Tobasa OpenJKN Bridge mendukung berbagai backend database:

-   Microsoft SQL Server
-   PostgreSQL
-   MySQL
-   SQLite

Hal ini memungkinkan integrasi dengan sistem lama maupun sistem baru
tanpa keterbatasan platform database.


## Gambaran Arsitektur

        +-----------------------+
        |  SIMRS / SIM Klinik   |
        +-----------------------+
                    |
                    v
    +-------------------------------+
    | Tobasa OpenJKN Bridge - REST  |
    | API - Business Logic -        |
    | Database Layer - Security &   |
    | Logging                       |
    +-------------------------------+
                    |
                    v
    +-------------------------------+
    |      Layanan Mobile JKN       |
    +-------------------------------+

Aplikasi ini bertindak sebagai lapisan middleware yang menghubungkan
sistem internal fasilitas kesehatan dengan layanan Mobile JKN.


## Teknologi yang Digunakan
-   .NET 6
-   ASP.NET Core Web API
-   Mendukung Windows & Linux
-   Database fleksibel (SQL Server, PostgreSQL, MySQL, SQLite)


## Cara Instalasi & Menjalankan

### 1. Clone Repository
```
git clone https://github.com/jefrisibarani/tobasa-openjkn.git 
cd tobasa-openjkn
```

### 2. Atur File Konfigurasi appsettings.json

Edit file appsettings.json:

Pengaturan Parameter Bridging Vclaim / MJKN
    BpjsVclaimProduction, BpjsVclaimDevelopment, BpjsMjknProduction, BpjsMjknDevelopment,
    BpjsMjknUseProduction, BpjsVclaimUseProduction

Pengaturan Database pada: 
   DatabaseEngine, ConnectToLocalDatabase dan ConnectionStrings

Password pada ConnectionStrings adalah password terenkripsi

Pada konfigurasi default clear passwordnya adalah: MRinjani3726

Bila ingin menggunakan database selain SQLite dengan file konfigurasi default,
cukup gunakan user: mjknwebsvc dan password: MRinjani3726, pada Database SQL.

Nilai DatabaseEngine dapat diisi dengan:

- MSSQL   ( MS Sql Server)
- PGSQL   ( PostgreSQL )
- MYSQL   ( MySQL)
- SQLITE  ( SQLite )


### 3. Build dan Jalankan
```
dotnet restore 
dotnet build 
dotnet run dotnet run --project Tobasa.WebApp
```
Secara default aplikasi berjalan di:
- http://localhost:8084
- https://localhost:8085


### 4. Publish ke local folder ( .\_publish )
    dotnet publish -c Release /p:PublishProfile=FolderProfile

------------------------------------------------------------------------

## Daftar API untuk FKRTL yang Diekspos ke BPJS

### 1. Generate Token
    GET https://{HOSTNAME}:{PORT}/api/fkrtl/token
    Default User name      : mjknuser
    Default Password       : UTNFJRYT7564JDNG

### 2. Get Status Antrean
    POST https://{HOSTNAME}:{PORT}/api/fkrtl/status_antrian

### 3. Ambil Antrean
    POST https://{HOSTNAME}:{PORT}/api/fkrtl/ambil_antrian

### 4. Get Sisa Antrean
    POST https://{HOSTNAME}:{PORT}/api/fkrtl/sisa_antrian

### 5. Batal Antrean
    POST https://{HOSTNAME}:{PORT}/api/fkrtl/batal_antrian

### 6. Check In
    POST https://{HOSTNAME}:{PORT}/api/fkrtl/checkin

### 7. Info Pasien Baru
    POST https://{HOSTNAME}:{PORT}/api/fkrtl/info_pasien_baru

### 8. Jadwal Operasi RS
    POST https://{HOSTNAME}:{PORT}/api/fkrtl/jadwal_operasi_rs

### 9. Jadwal Operasi Pasien
    POST https://{HOSTNAME}:{PORT}/api/fkrtl/jadwal_operasi_pasien

### 10. Ambil Antrean Farmasi
    POST https://{HOSTNAME}:{PORT}/api/fkrtl/ambil_antrian_farmasi

## 11. Status Antrean Farmasi
    POST https://{HOSTNAME}:{PORT}/api/fkrtl/status_antrian_farmasi

------------------------------------------------------------------------

## Daftar API untuk FKTP yang Diekspos ke BPJS

### 1. Generate Token
    GET https://{HOSTNAME}:{PORT}/api/fktp/token
    Default User name      : mjknuser
    Default Password       : UTNFJRYT7564JDNG

### 2. Get Status Antrean
    GET https://{HOSTNAME}:{PORT}/api/fktp/status_antrian

### 3. Ambil Antrean
    POST https://{HOSTNAME}:{PORT}/api/fktp/ambil_antrian

### 4. Get Sisa Antrean
    POST https://{HOSTNAME}:{PORT}/api/fktp/sisa_antrian

### 5. Batal Antrean
    POST https://{HOSTNAME}:{PORT}/api/fktp/batal_antrian

### 6. Check In
    POST https://{HOSTNAME}:{PORT}/api/fktp/checkin

### 7. Info Pasien Baru
    POST https://{HOSTNAME}:{PORT}/api/fktp/info_pasien_baru

------------------------------------------------------------------------

## Keamanan

Untuk penggunaan produksi disarankan:

-   Mengaktifkan HTTPS
-   Mengamankan kredensial database
-   Mengaktifkan firewall dan pembatasan akses jaringan


## Lisensi
Proyek ini menggunakan lisensi:

GNU General Public License (GPL)

Anda diperbolehkan untuk menggunakan, memodifikasi, dan mendistribusikan
ulang perangkat lunak ini sesuai dengan ketentuan lisensi GNU GPL.


## Disclaimer
Proyek ini tidak berafiliasi, tidak didukung, dan tidak terhubung secara
resmi dengan BPJS Kesehatan.
Mobile JKN adalah merek dagang milik BPJS Kesehatan.
Tobasa OpenJKN Bridge merupakan proyek open-source independen yang
dibuat untuk membantu integrasi sistem fasilitas kesehatan.


## Kontribusi
Kontribusi sangat terbuka untuk pengembangan lebih lanjut.
1.  Fork repository
2.  Buat branch fitur
3.  Commit perubahan
4.  Ajukan Pull Request


## Pengembang
Dikembangkan oleh Jefri Sibarani - jefrisibarani@gmail.com