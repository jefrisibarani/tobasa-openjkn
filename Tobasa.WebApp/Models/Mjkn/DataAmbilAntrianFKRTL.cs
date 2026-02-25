/*
    Tobasa OpenJKN Bridge
    Copyright (C) 2020-2026 Jefri Sibarani
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using Tobasa.Models.SimrsAntrian;

namespace Tobasa.Models.Mjkn
{
    public class DataAmbilAntrianFKRTL
    {
        public DataAmbilAntrianFKRTL()
        {
            JenisPasien     = "";
            AppSource       = "";
            Nik             = "";
            KodePoliJKN     = "";
            NomorKartu      = "";
            TanggalPeriksa  = "";
            KodeDokterJKN   = -1;
            JamPraktek      = "";
            NomorRekamMedis = "";
            NomorHP         = "";
            KodePoliINT     = "";
            KodeDokterINT   = "";
            NamaDokterJKN   = "";
            NamaPoliJKN     = "";
            NomorReferensi  = "";  // khusus FKRTL
            JenisKunjungan  = -1;  // khusus FKRTL
            Keluhan         = "";  // khusus FKTP
}

        public DataAmbilAntrianFKRTL(RequestAmbilAntrianSimrs request)
        {
            JenisPasien     = request.JenisPasien;
            AppSource       = "SIMRS_WEBSVC";
            Nik             = "";
            KodePoliJKN     = "";
            NomorKartu      = "";
            TanggalPeriksa  = request.TanggalPeriksa;
            KodeDokterJKN   = -1;
            JamPraktek      = request.JamPraktek;
            NomorRekamMedis = request.NoRm;
            NomorHP         = "";
            KodePoliINT     = request.KodePoli;
            KodeDokterINT   = request.KodeDokter;
            NamaDokterJKN   = "";
            NamaPoliJKN     = "";
            NomorReferensi  = request.NomorReferensi;
            JenisKunjungan  = request.JenisKunjungan;
			//Keluhan         = ""; // khusus FKTP
        }

        public DataAmbilAntrianFKRTL(RequestAmbilAntrianFKRTL request)
        {
            JenisPasien     = Insurance.BPJS;
            AppSource       = "MJKN_WEBSVC";
            NomorKartu      = request.NomorKartu;
            Nik             = request.Nik;
            KodePoliJKN     = request.KodePoli;
            TanggalPeriksa  = request.TanggalPeriksa;
            KodeDokterJKN   = request.KodeDokter;
            JamPraktek      = request.JamPraktek;
            NomorRekamMedis = request.NoRm;
            NomorHP         = request.NoHp;

            KodePoliINT     = "";
            KodeDokterINT   = "";
            NamaDokterJKN   = "";
            NamaPoliJKN     = "";

            NomorReferensi  = "";
            JenisKunjungan  = JenisRujukan.NONE;
            //Keluhan         = ""; // khusus FKTP
        }

        public string JenisPasien { get; set; }
        public string AppSource { get; set; }
        public string NomorKartu { get; set; }
        public string Nik { get; set; }
        public string KodePoliJKN { get; set; }  // kode subspesialis BPJS
        public string KodePoliINT { get; set; }
        public string TanggalPeriksa { get; set; }
        public int    KodeDokterJKN { get; set; }
        public string KodeDokterINT { get; set; }
        public string JamPraktek { get; set; }
        public string NomorRekamMedis { get; set; }
        public string NomorHP { get; set; }
        public string NamaDokterJKN { get; set; }
        public string NamaPoliJKN { get; set; }
        
        public string Keluhan { get; set; } // khusus FKTP

        // Tambahan untuk FKTRL
        public string NomorReferensi { get; set; } // Nomor Rujukan dari FKTP   
        public int    JenisKunjungan { get; set; } //  Jenis kunjungan: 1=FKTP, 2=INTERNAL, 3=KONTROL, 4=ANTAR RS
    }

    public class ResultAmbilAntrianFKTRL
    {
        public ResultAmbilAntrianFKTRL()
        {
            RegistrationToken = "";
            IssuedAt          = "";
            AngkaAntrian      = -1;
            EstimatedTime     = "";
            EstimatedTimeUnix = -1;
            Message           = "";
            AntrianSummary    = null;
            StatusAntri       = 0;
        }
        public ResultAmbilAntrianFKTRL(string message)
        {
            RegistrationToken = "";
            IssuedAt          = "";
            AngkaAntrian      = -1;
            EstimatedTime     = "";
            EstimatedTimeUnix = -1;
            Message           = message;
            AntrianSummary    = null;
            StatusAntri       = 0;
        }
        public string RegistrationToken { get; set; }
        public string IssuedAt { get; set; }
        public string EstimatedTime { get; set; }
        public long   EstimatedTimeUnix { get; set; }
        public string Message { get; set; }
        public int    StatusAntri { get; set; }
        public string NomorAntrian { get; set; }
        public int    AngkaAntrian { get; set; }
        public string NamaPoli { get; set; }
        public string Keterangan { get; set; }
        public int    ReturnCode { get; set; }
        public AntrianSummary AntrianSummary { get; set; }
    }
}