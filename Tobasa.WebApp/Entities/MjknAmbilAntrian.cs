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
    public partial class MjknAmbilAntrian
    {
        public long Id { get; set; }
        public string NomorKartu { get; set; } = null!;
        public string Nik { get; set; } = null!;
        public string NomorHp { get; set; } = null!;
        public string KodePoli { get; set; } = null!;
        public string NomorRm { get; set; } = null!;
        public DateOnly TanggalPeriksa { get; set; }
        public int KodeDokter { get; set; }
        public string JamPraktek { get; set; } = null!;
        public int JenisKunjungan { get; set; }
        public string NomorReferensi { get; set; } = null!;
        public long ReservationId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
