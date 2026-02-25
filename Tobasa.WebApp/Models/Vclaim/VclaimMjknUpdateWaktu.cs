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
    public class RequestMjknUpdateWaktu
    {
        public string KodeBooking { get; set; }
        public int TaskId { get; set; }
        public ulong Waktu { get; set; }
    }

    public class RequestMjknUpdateStatusFKTP
    {
        public string TanggalPeriksa { get; set; }
        public string KodePoli { get; set; }
        public string NomorKartu { get; set; }
        public string Status { get; set; }     // Status 1 = Hadir; Status 2 = Tidak Hadir
        public ulong Waktu { get; set; }       // 1616559330000 ---> Waktu dalam bentuk timestamp milisecond
    }
}