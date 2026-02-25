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
    public partial class MjknPasien
    {
        public long Id { get; set; }
        public string NomorKartu { get; set; } = null!;
        public string Nik { get; set; } = null!;
        public string NomorKk { get; set; } = null!;
        public string NomorRm { get; set; } = null!;
        public string Nama { get; set; } = null!;
        public string JenisKelamin { get; set; } = null!;
        public DateOnly TanggalLahir { get; set; }
        public string NomorHp { get; set; } = null!;
        public string Alamat { get; set; } = null!;
        public string KodePropinsi { get; set; } = null!;
        public string NamaPropinsi { get; set; } = null!;
        public string KodeDati2 { get; set; } = null!;
        public string NamaDati2 { get; set; } = null!;
        public string KodeKecamatan { get; set; } = null!;
        public string NamaKecamatan { get; set; } = null!;
        public string KodeKelurahan { get; set; } = null!;
        public string NamaKelurahan { get; set; } = null!;
        public string Rw { get; set; } = null!;
        public string Rt { get; set; } = null!;
        public string LastReservation { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public bool FingerPrint { get; set; }
    }
}