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
    public class RequestPasienBaruFKTP
    {
        public string NomorKartu { get; set; }
        public string Nik { get; set; }
        public string NomorKK { get; set; }
        public string Nama { get; set; }
        public string JenisKelamin { get; set; }
        public string TanggalLahir { get; set; }
        public string Alamat { get; set; }
        public string KodeProp { get; set; }
        public string NamaProp { get; set; }
        public string KodeDati2 { get; set; }
        public string NamaDati2 { get; set; }
        public string KodeKec { get; set; }
        public string NamaKec { get; set; }
        public string KodeKel { get; set; }
        public string NamaKel { get; set; }
        public string Rw { get; set; }
        public string Rt { get; set; }
    }
}