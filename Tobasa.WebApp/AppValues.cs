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

namespace Tobasa
{
    public class StatusAntri
    {
        public const int BOOKED     = 0;  // pasien sudah pesan
        public const int CHECKED_IN = 1;  // pasien datang dan registrasi
        public const int SERVED     = 2;  // pasien sudah dilayani
        public const int CANCELLED  = -1; // pasien membatalkan
        public const int NOSHOW     = -2; // pasien tidak hadir sampai jam layanan selesai

        public static string GetStatusMessage(int status)
        {
            return status switch
            {
                BOOKED     => "Pasien sudah melakukan pemesanan.",
                CHECKED_IN => "Pasien sudah melakukan registrasi.",
                SERVED     => "Pasien sudah dilayani.",
                CANCELLED  => "Pasien membatalkan antrian.",
                NOSHOW     => "Pasien tidak hadir.",
                _          => "Status tidak dikenal."
            };
        }

        public static string GetStatusStr(int status)
        {
            return status switch
            {
                BOOKED     => "BOOKED",
                CHECKED_IN => "CHECKED_IN",
                SERVED     => "SERVED",
                CANCELLED  => "CANCELLED",
                NOSHOW     => "NOSHOW",
                _          => "NONE"
            };
        }

    }

    public class JenisRujukan
    {
        public const int NONE     = 0;  // 
        public const int FKTP     = 1;  // Rujukan dari FKTP
        public const int INTERNAL = 2;  // Rujukan internal
        public const int KONTROL  = 3;  // Rujukan kontrol
        public const int ANTAR_RS = 4;  // Rujukan antar RS
    }

    public class Insurance
    {
        public const string BPJS    = "BPJS";
        public const string NON_JKN = "NON_JKN";
    }
}