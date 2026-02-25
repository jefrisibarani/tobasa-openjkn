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
    public partial class BlAntrian
    {
        public long Id { get; set; }
        public string NomorRegistrasi { get; set; } = null!;
        public long JadwalId { get; set; }
        public string KodePoli { get; set; } = null!;
        public DateOnly? Tanggal { get; set; }
        public string NamaHari { get; set; } = null!;
        public string JamMulai { get; set; } = null!;
        public string JamPraktek { get; set; } = null!;
        public string KodeDokter { get; set; } = null!;
        public string NamaPasien { get; set; } = null!;
        public string NomorRekamMedis { get; set; } = null!;
        public string NomorKartuJkn { get; set; } = null!;
        public DateOnly? TanggalLahir { get; set; }
        public string Alamat { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string InsuranceId { get; set; } = null!;
        public string NomorRujukan { get; set; } = null!;
        public int NomorAntrianInt { get; set; }
        public string NomorAntrian { get; set; } = null!;
        public string TokenAntrian { get; set; } = null!;
        public int JenisKunjungan { get; set; }
        public string Keterangan { get; set; } = null!;
        public string Note { get; set; } = null!;
        public long EstimasiDilayani { get; set; }
        public int SisaKuotaJkn { get; set; }
        public int KuotaJkn { get; set; }
        public int SisaKuotaNonJkn { get; set; }
        public int KuotaNonJkn { get; set; }
        public bool PasienBaru { get; set; }
        public int StatusAntri { get; set; }
        public DateTime BookedAt { get; set; }
        public DateTime? CheckinAt { get; set; }
        public DateTime? ServedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string CreatedBy { get; set; } = null!;
        public string EditedBy { get; set; } = null!;
    }
}