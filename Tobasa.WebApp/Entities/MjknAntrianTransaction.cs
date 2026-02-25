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
    public partial class MjknAntrianTransaction
    {
        public long Id { get; set; }
        public string RNomorKartu { get; set; } = null!;
        public string RNik { get; set; } = null!;
        public string RNomorHp { get; set; } = null!;
        public string RKodePoliJkn { get; set; } = null!;
        public string RNomorRm { get; set; } = null!;
        public DateOnly? RTanggalPeriksa { get; set; }
        public int RKodeDokterJkn { get; set; }
        public string RJamPraktek { get; set; } = null!;
        public int RJenisKunjungan { get; set; }
        public string RNomorReferensi { get; set; } = null!;
        public string RSource { get; set; } = null!;
        public string SUsername { get; set; } = null!;
        public string SUseredit { get; set; } = null!;
        public bool SPasienBaru { get; set; }
        public int SStatus { get; set; }
        public DateTime? SBookedAt { get; set; }
        public DateTime? SCheckinAt { get; set; }
        public DateTime? SServedAt { get; set; }
        public DateTime? SCancelledAt { get; set; }
        public long SAntriId { get; set; }
        public long SJadwalId { get; set; }
        public string SKodepoliInternal { get; set; } = null!;
        public string SKodokterInternal { get; set; } = null!;
        public string SNamaHari { get; set; } = null!;
        public string SNamaPasien { get; set; } = null!;
        public string SAlamatPasien { get; set; } = null!;
        public string SPhone { get; set; } = null!;
        public DateOnly? STanggalLahir { get; set; }
        public string ONomorAntrian { get; set; } = null!;
        public int OAngkaAntrian { get; set; }
        public string OKodeBooking { get; set; } = null!;
        public string ONamaPoli { get; set; } = null!;
        public string ONamaDokter { get; set; } = null!;
        public long OEstimasiDilayani { get; set; }
        public int OSisaKuotaJkn { get; set; }
        public int OKuotaJkn { get; set; }
        public int OSisaKuotaNonJkn { get; set; }
        public int OKuotaNonJkn { get; set; }
        public string OKeterangan { get; set; } = null!;
        public string SKeterangan { get; set; } = null!;
    }
}