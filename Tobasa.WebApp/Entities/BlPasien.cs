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
    public partial class BlPasien
    {
        public string NomorRekamMedis { get; set; } = null!;
        public string NomorIdentitas { get; set; } = null!;
        public string NomorKartuKeluarga { get; set; } = null!;
        public string NomorKartuJkn { get; set; } = null!;
        public string Nama { get; set; } = null!;
        public string Honorific { get; set; } = null!;
        public string Gender { get; set; } = null!;
        public DateOnly TanggalLahir { get; set; }
        public string InsuranceId { get; set; } = null!;
        public long LastReservation { get; set; }
        public bool FingerPrint { get; set; }
        public string Alamat { get; set; } = null!;
        public string Phone { get; set; } = null!;
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
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}