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

namespace Tobasa.Models.Mjkn
{

    public class RequestStatusAntrianFKTP
    {
        public string KodePoliInternal { get; set; }
        public string KodePoliJKN { get; set; } 
        public string TanggalPeriksa { get; set; }
    }


    public class StatusAntrianFKTP
    {
        public string NamaPoli { get; set; }
        public int    TotalAntrean { get; set; }
        public int    SisaAntrean { get; set; }
        public string AntreanPanggil { get; set; }
        public string Keterangan { get; set; }
        public int    KodeDokter { get; set; }
        public string NamaDokter { get; set; }
        public string JamPraktek { get; set; }
    }

    public class ResponseStatusAntrianFKTP
    {
        public List<StatusAntrianFKTP> StatusList { get; set; }
    }
}