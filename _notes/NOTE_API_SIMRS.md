# INTERNAL SERVICES ACCESSED FROM LOCAL SIMRS APP VCLAIM

### Credential
   - User name      : mjknuser
   - Password       : UTNFJRYT7564JDNG
   - Base URL HTTP  : http://localhost:8084
   - Base URL HTTPS : https://localhost:8085

### 1. Get Peserta by NIK
    GET /api/simrs/vclaim/get_peserta_by_nik/{nik}/{tglSep}

### 2. Get Peserta by nomor kartu BPJS kesehatan
    GET /api/simrs/vclaim/get_peserta_by_nocard/{nomorBpjs}/{tglSep}

### 3. Get data Rujukan by nomor rujukan
    GET /api/simrs/vclaim/get_rujukan_by_norujuk/{nomorRujukan}

### 4. Get data Rujukan by nomor kartu BPJS kesehatan
    GET /api/simrs/vclaim/get_rujukan_by_nocard_single/{nomorBpjs}

### 5. Get data Rujukan by nomor kartu BPJS kesehatan (multiple result)
    GET /api/simrs/vclaim/get_rujukan_by_nocard_multi/{nomorBpjs}

### 6. Get data Surat Kontrol
    GET /api/simrs/vclaim/get_surat_kontrol/{noSuratKontrol}

### 7. Get Data Histori Pelayanan Peserta
    GET /api/simrs/vclaim/get_history_pelayanan/{nomorBPJS}/{tanggalAwal}/{tanggalAkhir}



# INTERNAL SERVICES ACCESSED FROM LOCAL SIMRS APP

### 1. Status Antrean
    GET /api/simrs/antrean/status_antrean/kodepoli/{kodepoliRs}/kodedokter/{kodedokterRs}/tanggalperiksa/{tanggalperiksa}/jampraktek/{jampraktek}

### 2. Tambah Antrean - Menambahkan antrean/reservasi yang telah dibuat ke server BPJS
    POST /api/simrs/antrean/bpjs_tambah_antrean/{bookingKode}

### 3. Get Antrian
    POST /api/simrs/antrean/ambil_antrean

### 4. Create Jadwal Praktek
    POST /api/simrs/antrean/create_jadwal_praktek

### 5. Create Slot Antrean
    POST /api/simrs/antrean/create_slot_antrean

### 6. Referensi Jadwal dokter - HFIS
    GET /api/simrs/antrean/jadwal_dokter_hfis/{kodePoliJkn}/{tanggal}

### 7. Referensi Jadwal dokter - HFIS
    GET /api/simrs/antrean/sinkron_jadwal_dokter_hfis/kodepolirs/{kodePoliRs}/tglperiksa/{tanggal}