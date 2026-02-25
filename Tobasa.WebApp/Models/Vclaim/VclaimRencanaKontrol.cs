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
    public class ProvPerujukRK
    {
        public string KdProviderPerujuk { get; set; }
        public string NmProviderPerujuk { get; set; }
        public string AsalRujukan { get; set; }
        public string NoRujukan { get; set; }
        public string TglRujukan { get; set; }
    }
    public class PesertaRK
    {
        public string NoKartu { get; set; }
        public string Nama { get; set; }
        public string TglLahir { get; set; }
        public string Kelamin { get; set; }
        public string HakKelas { get; set; }
    }
    public class SepRK
    {
        public string NoSep { get; set; }
        public string TglSep { get; set; }
        public string JnsPelayanan { get; set; }
        public string Poli { get; set; }
        public string Diagnosa { get; set; }
        public PesertaRK Peserta { get; set; }
        public ProvUmum ProvUmum { get; set; }
        public ProvPerujukRK ProvPerujuk { get; set; }
    }

    public class SuratKontrol
    {
        public string NoSuratKontrol { get; set; }
        public string TglRencanaKontrol { get; set; }
        public string TglTerbit { get; set; }
        public string JnsKontrol { get; set; }
        public string PoliTujuan { get; set; }
        public string NamaPoliTujuan { get; set; }
        public string KodeDokter { get; set; }
        public string NamaDokter { get; set; }
        public string FlagKontrol { get; set; }
        public string KodeDokterPembuat { get; set; }
        public string NamaDokterPembuat { get; set; }
        public string NamaJnsKontrol { get; set; }
        public SepRK Sep { get; set; }
    }
}