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
    public class RequestSisaAntrianFKRTL
    {
        public string KodeBooking { get; set; }
    }

    public class ResponseSisaAntrianFKRTL
    {
        public string NomorAntrean { get; set; }
        public string NamaPoli { get; set; }
        public string NamaDokter { get; set; }
        public int    SisaAntrean { get; set; }
        public string AntreanPanggil { get; set; }
        public long   WaktuTunggu { get; set; }
        public string Keterangan { get; set; }
    }
}