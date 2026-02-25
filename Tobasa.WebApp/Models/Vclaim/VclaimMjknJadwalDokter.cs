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

namespace Tobasa.Models.Vclaim
{
    public class MjknJadwalDokterHfis
    {
        public string   KodeSubSpesialis { get; set; }
        public int      Hari { get; set; }
        public int      KapasitasPasien { get; set; }
        public int      Libur { get; set; }
        public string   NamaHari { get; set; }
        public string   Jadwal { get; set; }
        public string   NamaSubspesialis { get; set; }
        public string   NamaDokter { get; set; }
        public string   KodePoli { get; set; }
        public string   NamaPoli { get; set; }
        public int      KodeDokter { get; set; }
    }
}