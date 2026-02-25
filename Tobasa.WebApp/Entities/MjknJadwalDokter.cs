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

using System;
using System.Collections.Generic;

namespace Tobasa.Entities
{
    public partial class MjknJadwalDokter
    {
        public int Id { get; set; }
        public int KodedokterJkn { get; set; }
        public string KodedokterInternal { get; set; } = null!;
        public string KodePoli { get; set; } = null!;
        public string KodeSubspesialis { get; set; } = null!;
        public string NamaDokter { get; set; } = null!;
        public string NamaPoli { get; set; } = null!;
        public string NamaSubspesialis { get; set; } = null!;
        public int Hari { get; set; }
        public string Jadwal { get; set; } = null!;
        public string JamMulai { get; set; } = null!;
        public string JamTutup { get; set; } = null!;
        public string NamaHari { get; set; } = null!;
        public bool Libur { get; set; }
        public int KapasitasPasien { get; set; }
        public DateTime? LastUpdate { get; set; }
        public string LastUpdateStatus { get; set; } = null!;
        public int? LastUpdateCode { get; set; }
    }
}