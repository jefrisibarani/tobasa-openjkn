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

namespace Tobasa.Models.Vclaim
{
    public class RequestMjknTambahAntrian
    {
        public string KodeBooking { get; set; }
        public string JenisPasien { get; set; }
        public string NomorKartu { get; set; }
        public string Nik { get; set; }
        public string NoHp { get; set; }
        public string KodePoli { get; set; }
        public string NamaPoli { get; set; }
        public int    PasienBaru { get; set; }
        public string NoRm { get; set; }
        public string TanggalPeriksa { get; set; }
        public int    KodeDokter { get; set; }
        public string NamaDokter { get; set; }
        public string JamPraktek { get; set; }
        public int    JenisKunjungan { get; set; }
        public string NomorReferensi { get; set; }
        public string NomorAntrean { get; set; }
        public int    AngkaAntrean { get; set; }
        public long   EstimasiDilayani { get; set; }
        public int    SisaKuotaJkn { get; set; }
        public int    KuotaJkn { get; set; }
        public int    SisaKuotaNonJkn { get; set; }
        public int    KuotaNonJkn { get; set; }
        public string Keterangan { get; set; }
    }

    public class RequestMjknTambahAntrianFKTP
    {
        public string NomorKartu { get; set; }
        public string Nik { get; set; }
        public string NoHp { get; set; }
        public string KodePoli { get; set; }
        public string NamaPoli { get; set; }
        public string NoRm { get; set; }
        public string TanggalPeriksa { get; set; }
        public int KodeDokter { get; set; }
        public string NamaDokter { get; set; }
        public string JamPraktek { get; set; }
        public string NomorAntrean { get; set; }
        public int AngkaAntrean { get; set; }
        public string Keterangan { get; set; }
    }
}