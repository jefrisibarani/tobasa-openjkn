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
using System.Threading.Tasks;
using Tobasa.Data;
using Tobasa.Services;

namespace Tobasa.Models.Vclaim
{
    public class MjknRefDokterFKTP
    {
        public string NamaDokter { get; set; }
        public int    KodeDokter { get; set; }
        public string JamPraktek { get; set; }
        public int    Kapasitas { get; set; }
        public string KodePoli  { get; set; }

        public static async Task<List<MjknJadwalDokterHfis>> ToMjknJadwalDokterHfis(List<MjknRefDokterFKTP> fk, string kodePoli, string tanggal, IMjknServiceFKTP jknService)
        {
            if (!Utils.IsValidDate(tanggal))
            {
                throw new AppException("Format tanggal periksa salah, seharusnya yyyy-MM-dd");
            }

            DateTime dtTanggal = DateTime.ParseExact(tanggal, "yyyy-MM-dd", null);
            string namaHari = Utils.ISODateTimeToIndonesianDay(dtTanggal).ToUpper();


            var list = new List<MjknJadwalDokterHfis>();
            foreach (var item in fk)
            {
                var poli = await jknService.GetSubPoli(item.KodeDokter);


                var mjkn = new MjknJadwalDokterHfis
                {
                    KodeSubSpesialis = poli.KodeSubPoliJKN,  
                    Hari             = Utils.NamaHariHfisToInteger(namaHari),
                    Libur            = 0,    
                    NamaHari         = namaHari,   
                    NamaSubspesialis = poli.NamSubPoliJKN,
                    KodePoli         = kodePoli,   
                    NamaPoli         = poli.NamaPoliJKN,   
                    NamaDokter       = item.NamaDokter,
                    KodeDokter       = item.KodeDokter,
                    Jadwal           = item.JamPraktek,
                    KapasitasPasien  = item.Kapasitas
                };
                list.Add(mjkn);
            }
            return list;
        }

    }
}