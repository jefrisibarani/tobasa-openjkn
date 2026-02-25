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
    public partial class BlDokter
    {
        public string KodeDokter { get; set; } = null!;
        public string NamaDokter { get; set; } = null!;
        public string KodePoli { get; set; } = null!;
        public int    KodeDokterJkn { get; set; }
        public int    KapasitasNonJkn { get; set; }
        public int    KapasitasJkn { get; set; }
        public int    TotalKapasitas { get; set; }
        public long   NomorUrutJadwal { get; set; }
        public int    PasienTime { get; set; }
        public string ProfileImage { get; set; } = null!;
    }
}