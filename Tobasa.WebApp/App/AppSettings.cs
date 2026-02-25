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

using System.Collections.Generic;

namespace Tobasa.App
{
    public class AppSettings
    {
        public string AuthJwtSecret { get; set; }
        public string AuthJwtIssuer { get; set; }

        // JWT Toke expiration in minutes. Default is 5 minutes
        public int    AuthJwtExpireTimeSpanMinutes { get; set; } = 5;

        public bool   BpjsVclaimUseProduction { get; set; } = true;
        public VclaimProperties BpjsVclaimProduction { get; set; }
        public VclaimProperties BpjsVclaimDevelopment { get; set; }
        public bool   BpjsVclaimValidateNomorPeserta { get; set; } = true;
        public bool   BpjsVclaimValidateRujukan { get; set; } = true;
        public bool   BpjsVclaimValidateRujukanCheckUsage { get; set; } = true;
        public bool   AutoImportJadwalPraktekDokterDariDataHfis { get; set; }
        public int    MaximalJumlahHariPengambilanAntrian { get; set; }

        // Untuk MJKN versi BPJS
        public bool   BpjsMjknUseProduction { get; set; } = true;
        public MjknProperties BpjsMjknProduction { get; set; }
        public MjknProperties BpjsMjknDevelopment { get; set; }

        public AplicareProperties BpjsApplicare { get; set; }
        public List<string> ListKodePoliNonSinkron { get; set; }
        public List<string> ListKodeDokterNonSinkron { get; set; }

        public bool   CheckInMjknHarusSesuaiTanggalServer { get; set; }
        public bool   AutoUpdateBatalAntrianKeWsBPJS { get; set; }
        public bool   AutoUpdateTambahAntrianKeWsBPJS { get; set; }
        public bool   AutoUpdateWaktuCheckinAntrianKeWsBPJS { get; set; }
    }

    public class VclaimProperties
    {
        // Type faskes rujukan, RS atau Pcare
        public string TypeFaskesRujukan { get; set; } = "RS";
        public string ConsID { get; set; } = "12345";
        public string ConsSecret { get; set; } = "xxxxxx";
        public string Url { get; set; } = "https://apijkn-dev.bpjs-kesehatan.go.id/";
        public string Service { get; set; } = "vclaim-rest-dev";
        public string UserKey { get; set; } = "";
    }

    public class MjknProperties
    {
        public string ConsID { get; set; } = "12345";
        public string ConsSecret { get; set; } = "xxxxxx";
        public string Url { get; set; } = "https://apijkn-dev.bpjs-kesehatan.go.id/";
        public string Service { get; set; } = "antreanrs_dev";
        public string UserKey { get; set; } = "";
    }

    public class AplicareProperties
    {
        public string ConsID { get; set; } = "12345";
        public string ConsSecret { get; set; } = "xxxxxx";
        public string Url { get; set; } = "https://new-api.bpjs-kesehatan.go.id/";
        public string Service { get; set; } = "aplicaresws";
        public string UserKey { get; set; } = "";
        public string KodeRS { get; set; } = "";
    }
}