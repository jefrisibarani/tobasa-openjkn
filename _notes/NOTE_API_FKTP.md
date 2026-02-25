# Daftar API untuk FKTP yang Diekspos ke BPJS

### Credential
   - User name      : mjknuser
   - Password       : UTNFJRYT7564JDNG
   - Base URL HTTP  : http://localhost:8084
   - Base URL HTTPS : https://localhost:8085

### 1. Generate Token
    GET https://{HOSTNAME}:{PORT}/api/fktrl/token
    Default User name      : mjknuser
    Default Password       : UTNFJRYT7564JDNG

### 2. Get Status Antrean
    GET https://{HOSTNAME}:{PORT}/api/fktrl/status_antrian

### 3. Ambil Antrean
    POST https://{HOSTNAME}:{PORT}/api/fktrl/ambil_antrian

### 4. Get Sisa Antrean
    POST https://{HOSTNAME}:{PORT}/api/fktrl/sisa_antrian

### 5. Batal Antrean
    POST https://{HOSTNAME}:{PORT}/api/fktrl/batal_antrian

### 6. Check In
    POST https://{HOSTNAME}:{PORT}/api/fktrl/checkin

### 7. Info Pasien Baru
    POST https://{HOSTNAME}:{PORT}/api/fktrl/info_pasien_baru