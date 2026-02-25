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

using System.Collections.Generic;

namespace Tobasa.Models.Vclaim
{
    public class KodeT
    {
        public string Kode { get; set; }
    }
    public class Field
    {
        public string Kode { get; set; }
        public string Nama { get; set; }
    }

    public class Sarana
    {
        public string KodeSarana { get; set; }
        public string NamaSarana { get; set; }
    }

    public class Spesialistik
    {
        public string KodeSpesialis { get; set; }
        public string NamaSpesialis { get; set; }
        public string Kapasitas { get; set; }
        public string JumlahRujukan { get; set; }
        public string Persentase { get; set; }
    }

    public class RujukanPeserta
    {
        public string Asuransi { get; set; }
        public string HakKelas { get; set; }
        public string JnsPeserta { get; set; }
        public string Kelamin { get; set; }
        public string Nama { get; set; }
        public string NoKartu { get; set; }
        public string NoMr { get; set; }
        public string TglLahir { get; set; }
    }

    public class Rujukan
    {
        public Field Diagnosa { get; set; }
        public string Keluhan { get; set; }
        public string NoKunjungan { get; set; }
        public Field Pelayanan  { get; set; }
        public Peserta Peserta  { get; set; }
        public Field PoliRujukan { get; set; }
        public Field ProvPerujuk { get; set; }
        public string TglKunjungan { get; set; }
    }

    public class RequestInsertRujukan
    {
        public string NoSep { get; set; }
        public string TglRujukan { get; set; }
        public string PpkDirujuk { get; set; }
        public string JnsPelayanan { get; set; }
        public string Catatan { get; set; }
        public string DiagRujukan { get; set; }
        public string TipeRujukan { get; set; }
        public string PoliRujukan { get; set; }
        public string User { get; set; }
    }

    public class ResultInsertRujukan
    {
        public Field AsalRujukan { get; set; }
        public Field Diagnosa { get; set; }
        public string noRujukan { get; set; }
        public RujukanPeserta Peserta { get; set; }
        public Field PoliTujuan { get; set; }
        public string TglRujukan { get; set; }
        public Field TujuanRujukan { get; set; }
    }

    public class RequestUpdateRujukan
    {
        public string NoRujukan { get; set; }
        public string PpkDirujuk { get; set; }
        public string Tipe { get; set; }
        public string JnsPelayanan { get; set; }
        public string Catatan { get; set; }
        public string DiagRujukan { get; set; }
        public string TipeRujukan { get; set; }
        public string PoliRujukan { get; set; }
        public string User { get; set; }
    }

    public class RequestDeleteRujukan
    {
        public string NoRujukan { get; set; }
        public string User { get; set; }
    }

    public class RequestInsertRujukanKhusus
    {
        public string NoRujukan { get; set; }
        public List<KodeT> Diagnosa { get; set; }
        public List<KodeT> Procedure { get; set; }
        public string User { get; set; }
    }

    public class ResultInsertRujukanKhusus
    {
        public string NoRujukan { get; set; }
        public string Nokapst { get; set; }
        public string Nmpst { get; set; }
        public string Diagppk { get; set; }
        public string Tglrujukan_awal { get; set; }
        public string Tglrujukan_berakhir { get; set; }
    }

    public class RequestDeleteRujukanKhusus
    {
        public string IdRujukan { get; set; }
        public string NnoRujukan { get; set; }
        public string User { get; set; }
    }

    public class ResultListRujukanKhusus
    {
        public string IdRujukan { get; set; }
        public string NoRujukan { get; set; }
        public string Nokapst { get; set; }
        public string Nmpst { get; set; }
        public string Diagppk { get; set; }
        public string Tglrujukan_awal { get; set; }
        public string Tglrujukan_berakhir { get; set; }
    }

    public class RequestInsertRujukan2
    {
        public string NoSep { get; set; }
        public string TglRujukan { get; set; }
        public string TglRencanaKunjungan { get; set; }
        public string PpkDirujuk { get; set; }
        public string JnsPelayanan { get; set; }
        public string Catatan { get; set; }
        public string DiagRujukan { get; set; }
        public string TipeRujukan { get; set; }
        public string PoliRujukan { get; set; }
        public string User { get; set; }
    }

    public class ResultInsertRujukan2
    {
        public Field AsalRujukan { get; set; }
        public Field Diagnosa { get; set; }
        public string NoRujukan { get; set; }
        public RujukanPeserta Peserta { get; set; }
        public Field PoliTujuan { get; set; }
        public string TglBerlakuKunjungan { get; set; }
        public string TglRencanaKunjungan { get; set; }
        public string TglRujukan { get; set; }
        public Field TujuanRujukan { get; set; }
    }

    public class RequestUpdateRujukan2
    {
        public string NoRujukan { get; set; }
        public string TglRujukan { get; set; }
        public string TglRencanaKunjungan { get; set; }
        public string PpkDirujuk { get; set; }
        public string JnsPelayanan { get; set; }
        public string Catatan { get; set; }
        public string DiagRujukan { get; set; }
        public string TipeRujukan { get; set; }
        public string PoliRujukan { get; set; }
        public string User { get; set; }
    }

    public class ResultListRujukanKeluarRS
    {
        public string NoRujukan { get; set; }
        public string TglRujukan { get; set; }
        public string JnsPelayanan { get; set; }
        public string NoSep { get; set; }
        public string NoKartu { get; set; }
        public string Nama { get; set; }
        public string PpkDirujuk { get; set; }
        public string NamaPpkDirujuk { get; set; }
    }

    public class ResultGetRujukanKeluarRS
    {
        public string NoRujukan { get; set; }
        public string NoSep { get; set; }
        public string NoKartu { get; set; }
        public string Nama { get; set; }
        public string KelasRawat { get; set; }
        public string Kelamin { get; set; }
        public string TglLahir { get; set; }
        public string TglSep { get; set; }
        public string TglRujukan { get; set; }
        public string TglRencanaKunjungan { get; set; }
        public string PpkDirujuk { get; set; }
        public string NamaPpkDirujuk { get; set; }
        public string JnsPelayanan { get; set; }
        public string Catatan { get; set; }
        public string DiagRujukan { get; set; }
        public string NamaDiagRujukan { get; set; }
        public string TipeRujukan { get; set; }
        public string NamaTipeRujukan { get; set; }
        public string PoliRujukan { get; set; }
        public string NamaPoliRujukan { get; set; }
    }
}