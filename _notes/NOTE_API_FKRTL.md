# Daftar API untuk FKRTL yang Diekspos ke BPJS

### Credential
   - User name      : mjknuser
   - Password       : UTNFJRYT7564JDNG
   - Base URL HTTP  : http://localhost:8084
   - Base URL HTTPS : https://localhost:8085

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

### 11. Status Antrean Farmasi
    POST https://{HOSTNAME}:{PORT}/api/fkrtl/status_antrian_farmasi