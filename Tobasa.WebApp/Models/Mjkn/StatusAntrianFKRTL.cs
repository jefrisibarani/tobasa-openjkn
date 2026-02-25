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
    public class RequestStatusAntrianFKRTL
    {
        public string KodePoli { get; set; }
        public int    KodeDokter { get; set; }
        public string TanggalPeriksa { get; set; }
        public string JamPraktek { get; set; }
        public string KodePoliINT { get; set; }
        public string KodeDokterINT { get; set; }
    }

    public class ResponseStatusAntrianFKRTL
    {
        public string NamaPoli { get; set; }
        public string NamaDokter { get; set; }
        public int    TotalAntrean { get; set; }
        public int    SisaAntrean { get; set; }
        public string AntreanPanggil { get; set; }
        public int    SisaKuotaJkn { get; set; }
        public int    KuotaJkn { get; set; }
        public int    SisaKuotaNonJkn { get; set; }
        public int    KuotaNonJkn { get; set; }
        public string Keterangan { get; set; }
    }
}