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
    public class AntrianSummary
    {
        public AntrianSummary()
        {
            JadwalId                = 0;
            QuotaTotal              = 0;
            QuotaJkn                = 0;
            QuotaNonJkn             = 0;
            QuotaJknUsed            = 0;
            QuotaNonJknUsed         = 0;
            Totalantrian            = 0;
            SisaAntrianJkn          = 0;
            SisaAntrianNonJkn       = 0;
            SisaAntrianAll          = 0;
            LastServedAntrian       = string.Empty;
            LastServedAntrianNumber = 0;
            LastNumber              = 0;
        }

        public long   JadwalId { get; set; }
        public int    QuotaTotal { get; set; }
        public int    QuotaJkn { get; set; }
        public int    QuotaNonJkn { get; set; }
        public int    QuotaJknUsed { get; set; }
        public int    QuotaNonJknUsed { get; set; }
        public int    Totalantrian { get; set; }
        public int    SisaAntrianJkn { get; set; }
        public int    SisaAntrianNonJkn { get; set; }
        public int    SisaAntrianAll { get; set; }
        public string LastServedAntrian { get; set; }
        public int    LastServedAntrianNumber { get; set; } 
        public int    LastNumber { get; set; } // Nomor Urut Terakhir
    }
}