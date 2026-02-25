## Untuk Test murni dari Postman terhadap server WS lokal

### 1. AmbilAntrian,
   ini hanya menambah data di database lokal SIMRS,
   data antrian(kode booking) belum terkirim ke BPJS
   
   Note: Bila AmbilAntrian dijalankan oleh aplikasi MobileJKN
         maka data antrian telah tersimpan di server BPJS   

### 2. Check In Antrean
   Ini juga hanya mengupdate data lokal

### 3. Batal Antrean
   Ini juga hanya mengupdate data lokal

### 4. Agar data segera sinkron dengan WS BPJS (server BPJS)
   Tiga opsi ini harus di set TRUE:   
   - AutoUpdateBatalAntreanKeWsBPJS
   - AutoUpdateTambahAntreanKeWsBPJS
   - AutoUpdateWaktuCheckinAntreanKeWsBPJS
     
   Bila AutoUpdateWaktuCheckinAntreanKeWsBPJS diset true, 
   task id 1,2,3 akan dikirim ke bpjs untuk pasien lama, 
   untuk pasien baru hanya task id 1
