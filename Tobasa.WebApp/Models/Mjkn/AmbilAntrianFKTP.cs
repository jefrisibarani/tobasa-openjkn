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

namespace Tobasa.Models.Mjkn
{
    public class RequestAmbilAntrianFKTP
    {
        public string NomorKartu { get; set; }
        public string Nik { get; set; }
        public string KodePoli { get; set; } // kode subspesialis BPJS
        public string TanggalPeriksa { get; set; }
        public string Keluhan { get; set; }
        public int    KodeDokter { get; set; }
        public string JamPraktek { get; set; }
        public string NoRm { get; set; }
        public string NoHp { get; set; }
    }

    public class ResponseAmbilAntrianFKTP
    {
        public string NomorAntrean { get; set; }
        public int    AngkaAntrean { get; set; }
        public string NamaPoli { get; set; }
        public int    SisaAntrian { get; set; }
        public string AntreanPanggil { get; set; }
        public string Keterangan { get; set; }
    }
}