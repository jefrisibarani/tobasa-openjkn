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

namespace Tobasa.Models.SimrsAntrian
{
    public class RequestCreateJadwalPraktek
    {
        public string TanggalAwal { get; set; }
        public int    JumlahHari { get; set; }
        public string KodeDokter { get; set; }
        public string JamMulai { get; set; }    // 09:00
        public string JamAkhir { get; set; }    // 11:00
    }

    public class ResponseCreateJadwalPraktek
    {
    }

    public class RequestCreateSlotAntrian
    {
        public int KodeJadwal { get; set; }
        public string AppSource { get; set; }
    }
}