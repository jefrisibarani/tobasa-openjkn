# Daftar API untuk FKTP yang Diekspos ke BPJS

### Credential
   - User name      : mjknuser
   - Password       : UTNFJRYT7564JDNG
   - Base URL HTTP  : http://localhost:8084
   - Base URL HTTPS : https://localhost:8085

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