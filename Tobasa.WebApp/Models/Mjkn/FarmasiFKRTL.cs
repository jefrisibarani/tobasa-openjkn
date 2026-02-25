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
    public class RequestAmbilAntrianFarmasi
    {
        public string KodeBooking { get; set; }
    }

    public class RequestStatusAntrianFarmasi
    {
        public string KodeBooking { get; set; }
    }

    public class ResponseAmbilAntrianFarmasi
    {
        public string JenisResep { get; set; }
        public string NomorAntrean { get; set; }
        public string Keterangan { get; set; }
    }


    public class ResponseStatusAntrianFarmasi
    {
        public string JenisResep { get; set; } = "";
        public string TotalAntrean { get; set; } = "0";
        public string SisaAntrean { get; set; } = "0";
        public string AntreanPanggil { get; set; }
        public string Keterangan { get; set; } = "";
    }


    public class ResultGetAntrianFarmasi
    {
        public string Noreg { get; set; }
        public string TanggalResep { get; set; }
        public string JenisResep { get; set; }
        public string NomorAntrian { get; set; }
        public int    NomorAntri { get; set; } 
        public string StatusAntri { get; set; } 
        public string PrefixCode { get; set; }
        public string StartTime { get; set; }
    }
}