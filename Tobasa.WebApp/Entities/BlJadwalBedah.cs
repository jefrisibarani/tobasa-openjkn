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
    public partial class BlJadwalBedah
    {
        public string NomorJadwal { get; set; } = null!;
        public DateOnly TanggalOperasi { get; set; }
        public string NomorRegistrasi { get; set; } = null!;
        public string NomorRekamMedis { get; set; } = null!;
        public string WaktuOperasi { get; set; } = null!;
        public string KodeDokterOperator { get; set; } = null!;
        public bool Terlaksana { get; set; }
        public string JenisTindakan { get; set; } = null!;
    }
}