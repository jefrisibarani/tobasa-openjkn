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


namespace Tobasa.Models.SimrsAntrian
{
    public class RequestAmbilAntrianSimrs
    {
        /// Kode Poli Rumah Sakit yang harus dimapping 
        /// dengan kode sup spesialis (sub poli) BPJS/BPJS
        public string KodePoli { get; set; } = null!;
        public string NoRm { get; set; } = null!;
        public string TanggalPeriksa { get; set; } = null!;
        public string KodeDokter { get; set; } = null!;
        public string JamPraktek { get; set; } = null!;
        public string JenisPasien { get; set; } = null!;
        public int    JenisKunjungan { get; set; }
        public string NomorReferensi { get; set; } = null!;
    }

    public class ResponseAmbilAntrianSimrs
    {
        public string NomorAntrian { get; set; } = null!;
        public int    AngkaAntrian { get; set; }
        public string KodeBooking { get; set; } = null!;
        public string NoRm { get; set; } = null!;
        public string NamaPoli { get; set; } = null!;
        public string NamaDokter { get; set; } = null!;
        public long   EstimasiDilayani { get; set; }
        public int    SisaKuotaJkn { get; set; }
        public int    KuotaJkn { get; set; }
        public int    SisaKuotaNonJkn { get; set; }
        public int    KuotaNonJkn { get; set; }
        public string Keterangan { get; set; } = null!;
    }
}