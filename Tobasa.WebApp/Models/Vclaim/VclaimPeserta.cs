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
    public class Peserta
    {
        public Cob Cob { get; set; }
        public HakKelas HakKelas { get; set; }
        public Informasi Informasi { get; set; }
        public JenisPeserta JenisPeserta { get; set; }
        public Mr Mr { get; set; }
        public string Nama { get; set; }
        public string Nik { get; set; }
        public string NoKartu { get; set; }
        public string Pisa { get; set; }
        public ProvUmum ProvUmum { get; set; }
        public string Sex { get; set; }
        public StatusPeserta StatusPeserta { get; set; }
        public string TglCetakKartu { get; set; }
        public string TglLahir { get; set; }
        public string TglTAT { get; set; }
        public string TglTMT { get; set; }
        public Umur Umur { get; set; }

    }

    public class Cob
    {
        public string NmAsuransi { get; set; }
        public string NoAsuransi { get; set; }
        public string TglTAT { get; set; }
        public string TglTMT { get; set; }
    }
    public class HakKelas 
    {
        public string Keterangan { get; set; }
        public string Kode { get; set; }
    }
    public class Informasi
    {
        public string Dinsos { get; set; }
        public string NoSKTM { get; set; }
        public string ProlanisPRB { get; set; }
    }
    public class JenisPeserta 
    {
        public string Keterangan { get; set; }
        public string Kode { get; set; }
    }
    public class Mr
    {
        public string NoMR { get; set; }
        public string NoTelepon { get; set; }
    }
    public class ProvUmum
    {
        public string KdProvider { get; set; }
        public string NmProvider { get; set; }
    }
    public class StatusPeserta
    {
        public string Keterangan { get; set; }
        public string Kode { get; set; }
    }
    public class Umur
    {
        public string UmurSaatPelayanan { get; set; }
        public string UmurSekarang { get; set; }
    }

}