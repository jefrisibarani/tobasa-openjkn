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

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using Tobasa.App;
using Tobasa.Data;
using Tobasa.Entities;
using Tobasa.Services;

namespace Tobasa
{
    internal class DBMigration
    {
        private readonly IConfiguration _configuration;
        private IUserService _userService;
        private readonly ILogger _logger;

        public DBMigration(IConfiguration configuration, IUserService userService, ILogger<DBMigration> logger)
        {
            _configuration = configuration;
            _userService = userService;
            _logger = logger;
        }

        public bool InitializeDatabase(DataContextAntrian dataContext)
        {
            try
            {
                string dbProvider = _configuration["DatabaseEngine"];
                string dbName = "tobasa_mjkn";

                List<string> commandList = new List<string>();
                string objectSummarySql = "";
                string populateBasicDataSql = "";
                string populateSampleDataSql = "";

                if (dbProvider == "MYSQL")
                {
                    commandList = DBMigration_MySQL.GetCommandList();
                    objectSummarySql = DBMigration_MySQL.GetObjectSummaryQuery(dbName);
                }
                else if (dbProvider == "MSSQL")
                {
                    commandList = DBMigration_MSSQL.GetCommandList();
                    objectSummarySql = DBMigration_MSSQL.GetObjectSummaryQuery(dbName);
                }
                else if (dbProvider == "PGSQL")
                {
                    commandList = DBMigration_PostgreSQL.GetCommandList();
                    objectSummarySql = DBMigration_PostgreSQL.GetObjectSummaryQuery(dbName);
                }
                else if (dbProvider == "SQLITE" )
                {
                    commandList = DBMigration_SQLite.GetCommandList();
                    objectSummarySql = DBMigration_SQLite.GetObjectSummaryQuery(dbName);
                }
                else
                {
                    _logger.LogError("DBMigration failed due to unsupported database type");
                    return false;
                }

                populateBasicDataSql = DBMigration.cmd_insert_all_basic_data;
                populateSampleDataSql = DBMigration.cmd_insert_all_sample_Data;

                // Check for tables, views and triggers
                // ---------------------------------------------------------------------------------
                int totalTables   = 22;
                int totalViews    = 0;
                int totalTriggers = 0;

                int tablesFound   = 0;
                int triggerFound  = 0;
                int viewsFound    = 0;

                using ( DbCommand cmd = dataContext.Connection().CreateCommand() )
                {
                    cmd.CommandText = objectSummarySql;
                    _logger.LogTrace($"DBMigration Executing command \n{objectSummarySql}");
                    using DbDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        reader.Read();

                        tablesFound = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                        viewsFound = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                        triggerFound = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                    }
                }

                if (tablesFound == totalTables/* && viewsFound == totalViews && triggerFound == totalTriggers*/)
                {
                    _logger.LogDebug($"DBMigration found correct database objects");
                    return true;
                }
                else if (tablesFound != totalTables /* && viewsFound != totalViews && triggerFound != totalTriggers*/)
                {
                    _logger.LogDebug($"DBMigration creating database objects");

                    int affected0 = 0;
                    using (DbCommand cmd = dataContext.Connection().CreateCommand())
                    {
                        foreach (string sqlCmd in commandList)
                        {
                            _logger.LogTrace($"DBMigration Executing command \n{sqlCmd}");

                            cmd.CommandText = sqlCmd;
                            affected0 = cmd.ExecuteNonQuery();
                        }
                    }

                    _logger.LogTrace($"DBMigration populating basic data");
                    int affected1 = 0;
                    using (DbCommand cmd = dataContext.Connection().CreateCommand())
                    {
                        _logger.LogTrace($"DBMigration Executing command \n{populateBasicDataSql}");

                        cmd.CommandText = populateBasicDataSql;
                        affected1 = cmd.ExecuteNonQuery();
                    }

                    _logger.LogTrace($"DBMigration completed successfully");

                    bool demoMode = (_configuration["DemoMode"] == true.ToString());
                    if (demoMode)
                    {
                        _logger.LogTrace($"DBMigration populating sample data");
                        int affected2 = 0;
                        using (DbCommand cmd = dataContext.Connection().CreateCommand())
                        {
                            _logger.LogTrace($"DBMigration Executing command \n{populateSampleDataSql}");

                            cmd.CommandText = populateSampleDataSql;
                            affected2 = cmd.ExecuteNonQuery();
                        }
                    }

                    _logger.LogTrace($"DBMigration completed successfully");

                    return true;
                }
                else
                {
                    _logger.LogTrace($"DBMigration found {tablesFound} tables, {viewsFound} views, {triggerFound} trigger");
                    return false;
                }
            }
            catch (Exception e)
            {
                _logger.LogError("DBMigration failed due to exception: " + e.Message);
            }

            return false;
        }
    
        public bool CreateDefaultUser()
        {
            try
            {
                // Create default admin user and mjkn user  

                var userAdmin = _userService.GetByUsername("admin");
                if (userAdmin == null)
                {
                    _userService.Create(new BaseUsers
                    {
                        UserName = "admin",
                        FirstName = "Admin",
                        LastName = "Tobasa",
                        Email = "admin@mangapul.net"
                    }, "@JKKJK65658DKFJUR");
                }

                var userMjkn = _userService.GetByUsername("mjknuser");
                if (userMjkn == null)
                {
                    _userService.Create(new BaseUsers
                    {
                        UserName = "mjknuser",
                        FirstName = "MJKN",
                        LastName = "User",
                        Email = "mjknuser@mangapul.net"
                    }, "UTNFJRYT7564JDNG");
                }
            }
            catch (Exception e)
            {
                _logger.LogError("CreateDefaultUser failed due to exception: " + e.Message);
                return false;
            }   

            return true;
        }

        public static string cmd_insert_all_basic_data =
            @"
            INSERT INTO bl_id_gen (kode_faskes, current_number, last_number) VALUES ('PRIMARY', 0, 0);


            INSERT INTO mjkn_poli (kode, nama) VALUES ('INT', 'PENYAKIT DALAM');
            INSERT INTO mjkn_poli (kode, nama) VALUES ('BED', 'BEDAH');
            INSERT INTO mjkn_poli (kode, nama) VALUES ('OBG', 'OBSTETRI DAN GINEKOLOGI');
            INSERT INTO mjkn_poli (kode, nama) VALUES ('ANA', 'ANAK');
            INSERT INTO mjkn_poli (kode, nama) VALUES ('ANT', 'ANESTESI DAN TERAPI INTENSIF');
            INSERT INTO mjkn_poli (kode, nama) VALUES ('RDO', 'RADIOLOGI');
            INSERT INTO mjkn_poli (kode, nama) VALUES ('THT', 'TELINGA HIDUNG TENGGOROK- BEDAH KEPALA LEHER (THT-KL)');
            INSERT INTO mjkn_poli (kode, nama) VALUES ('SAR', 'SARAF');
            INSERT INTO mjkn_poli (kode, nama) VALUES ('BSY', 'BEDAH SARAF');
            INSERT INTO mjkn_poli (kode, nama) VALUES ('PAR', 'PARU');
            INSERT INTO mjkn_poli (kode, nama) VALUES ('BDP', 'BEDAH PLASTIK REKONSTRUKSI DAN ESTETIKA');
            INSERT INTO mjkn_poli (kode, nama) VALUES ('MAT', 'MATA');
            INSERT INTO mjkn_poli (kode, nama) VALUES ('KLT', 'KULIT KELAMIN');
            INSERT INTO mjkn_poli (kode, nama) VALUES ('ORT', 'ORTHOPEDI DAN TRAUMATOLOGY');
            INSERT INTO mjkn_poli (kode, nama) VALUES ('JIW', 'KEDOKTERAN JIWA');
            INSERT INTO mjkn_poli (kode, nama) VALUES ('RDT', 'RADIOTERAPI');
            INSERT INTO mjkn_poli (kode, nama) VALUES ('RDN', 'RADIOLOGI ONKOLOGI');
            INSERT INTO mjkn_poli (kode, nama) VALUES ('BKL', 'BEDAH KEPALA LEHER');
            INSERT INTO mjkn_poli (kode, nama) VALUES ('IRM', 'KEDOKTERAN FISIK DAN REHABILITASI');
            INSERT INTO mjkn_poli (kode, nama) VALUES ('PAK', 'PATOLOGI KLINIK');
            INSERT INTO mjkn_poli (kode, nama) VALUES ('JAN', 'JANTUNG DAN PEMBULUH DARAH');
            INSERT INTO mjkn_poli (kode, nama) VALUES ('BDA', 'BEDAH ANAK');
            INSERT INTO mjkn_poli (kode, nama) VALUES ('EMG', 'EMERGENSI');
            INSERT INTO mjkn_poli (kode, nama) VALUES ('AKP', 'AKUPUNKTUR');
            INSERT INTO mjkn_poli (kode, nama) VALUES ('AND', 'ANDROLOGI');
            INSERT INTO mjkn_poli (kode, nama) VALUES ('BDM', 'BEDAH MULUT');
            INSERT INTO mjkn_poli (kode, nama) VALUES ('BTK', 'BEDAH THORAX KARDIAK DAN VASKULER');
            INSERT INTO mjkn_poli (kode, nama) VALUES ('FMK', 'FARMAKOLOGI KLINIK');
            INSERT INTO mjkn_poli (kode, nama) VALUES ('FOR', 'KEDOKTERAN FORENSIC');
            INSERT INTO mjkn_poli (kode, nama) VALUES ('GIG', 'GIGI');
            INSERT INTO mjkn_poli (kode, nama) VALUES ('GIZ', 'GIZI KLINIK');
            INSERT INTO mjkn_poli (kode, nama) VALUES ('GND', 'KONSERVASI/ENDODON-SI');
            INSERT INTO mjkn_poli (kode, nama) VALUES ('GOR', 'ORTHODONTI');
            INSERT INTO mjkn_poli (kode, nama) VALUES ('GPR', 'PERIODONTI');
            INSERT INTO mjkn_poli (kode, nama) VALUES ('GRD', 'GIGI RADIOLOGI');
            INSERT INTO mjkn_poli (kode, nama) VALUES ('KDK', 'KEDOKTERAN KELAUTAN');
            INSERT INTO mjkn_poli (kode, nama) VALUES ('KDN', 'KEDOKTERAN NUKLIR');
            INSERT INTO mjkn_poli (kode, nama) VALUES ('KDO', 'KEDOKTERAN OKUPASI');
            INSERT INTO mjkn_poli (kode, nama) VALUES ('KDP', 'KEDOKTERAN PENERBANGAN');
            INSERT INTO mjkn_poli (kode, nama) VALUES ('KON', 'PEDODONTI');
            INSERT INTO mjkn_poli (kode, nama) VALUES ('KOR', 'KEDOKTERAAN OLAHRAGA');
            INSERT INTO mjkn_poli (kode, nama) VALUES ('MKB', 'MIKROBIOLOGI KLINIK');
            INSERT INTO mjkn_poli (kode, nama) VALUES ('PAA', 'PATOLOGI ANATOMI');
            INSERT INTO mjkn_poli (kode, nama) VALUES ('PNM', 'PENYAKIT MULUT');
            INSERT INTO mjkn_poli (kode, nama) VALUES ('PRM', 'PARASITOLOGI UMUM');
            INSERT INTO mjkn_poli (kode, nama) VALUES ('PTD', 'PROSTHODONTI');
            INSERT INTO mjkn_poli (kode, nama) VALUES ('URO', 'UROLOGI');
            INSERT INTO mjkn_poli (kode, nama) VALUES ('UMU', 'UMUM');


            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('004', 'ALERGI-IMMUNOLOGI KLINIK', 'INT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('005', 'GASTROENTEROLOGI-HEPATOLOGI', 'INT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('006', 'GERIATRI', 'INT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('007', 'GINJAL-HIPERTENSI', 'INT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('008', 'HEMATOLOGI - ONKOLOGI MEDIK', 'INT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('009', 'HEPATOLOGI', 'INT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('010', 'ENDOKRIN-METABOLIK-DIABETES', 'INT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('011', 'PSIKOSOMATIK', 'INT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('012', 'PULMONOLOGI', 'INT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('013', 'REUMATOLOGI', 'INT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('014', 'PENYAKIT TROPIK-INFEKSI', 'INT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('015', 'KARDIOVASKULAR', 'INT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('017', 'BEDAH ONKOLOGI', 'BED');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('018', 'BEDAH DIGESTIF', 'BED');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('020', 'FETOMATERNAL', 'OBG');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('021', 'ONKOLOGI GINEKOLOGI', 'OBG');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('022', 'UROGINEKOLOGI DAN REKONSTRUKSI', 'OBG');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('023', 'OBSTETRI GINEKOLOGI SOSIAL', 'OBG');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('024', 'ENDOKRINOLOGI', 'OBG');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('025', 'FERTILITAS DAN ENDOKRINOLOGI REPRODUKSI', 'OBG');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('027', 'ANAK ALERGI IMUNOLOGI', 'ANA');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('028', 'ANAK ENDOKRINOLOGI', 'ANA');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('029', 'ANAK GASTRO-HEPATOLOGI', 'ANA');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('030', 'ANAK HEMATOLOGI ONKOLOGI', 'ANA');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('031', 'ANAK INFEKSI & PEDIATRI TROPIS', 'ANA');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('032', 'ANAK KARDIOLOGI', 'ANA');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('033', 'ANAK NEFROLOGI', 'ANA');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('034', 'ANAK NEUROLOGI', 'ANA');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('035', 'ANAK NUTRISI & PENYAKIT METABOLIK', 'ANA');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('036', 'PEDIATRI GAWAT DARURAT', 'ANA');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('037', 'PENCITRAAN ANAK', 'ANA');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('038', 'PERINATOLOGI', 'ANA');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('039', 'RESPIROLOGI ANAK', 'ANA');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('040', 'TUMBUH KEMBANG PED. SOSIAL', 'ANA');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('041', 'KESEHATAN REMAJA', 'ANA');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('043', 'INTENSIVE CARE/ICU', 'ANT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('044', 'ANESTESI KARDIOVASKULER', 'ANT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('045', 'TERAPI NYERI', 'ANT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('046', 'REGIONAL ANESTESI', 'ANT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('047', 'NEUROANESTESI', 'ANT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('048', 'ANESTESI PEDIATRI', 'ANT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('049', 'ANESTESI OBSTETRI', 'ANT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('051', 'THORAX IMAGING', 'RDO');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('052', 'RADIOLOGI MUSKULOSKELETAL', 'RDO');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('053', 'RADIOLOGI TR URINARIUSGENITALIA', 'RDO');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('054', 'RADIOLOGI TR DIGESTIVUS', 'RDO');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('055', 'RADIOLOGI NEURO KEPALA LEHER', 'RDO');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('056', 'BREAST AND WOMEN IMAGING', 'RDO');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('057', 'RADIOLOGI INTERVENSIONAL KARDIOVASKULAR', 'RDO');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('058', 'PENCITRAAN KEPALA LEHER', 'RDO');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('059', 'RADIOLOGI PEDIATRIK', 'RDO');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('060', 'RADIOLOGI NUKLIR', 'RDO');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('067', 'OTOLOGI', 'THT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('068', 'NEUROTOLOGI', 'THT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('069', 'RINOLOGI', 'THT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('070', 'LARING FARING', 'THT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('071', 'ONKOLOGI BEDAH KEPALA LEHER', 'THT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('072', 'PLASTIK REKONSTRUKSI THT', 'THT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('073', 'ENDOSKOPI BRONKO-ESOFAGOLOGI', 'THT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('074', 'ALERGI IMUNOLOGI', 'THT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('075', 'THT KOMUNITAS', 'THT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('077', 'NEUROINTERVEN-SI', 'SAR');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('078', 'NEUROTRAUMA', 'BSY');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('079', 'NEUROINFEKSI', 'SAR');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('080', 'NEUROINFEKSI DAN IMUNOLOGI', 'SAR');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('081', 'EPILEPSI', 'SAR');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('082', 'NEUROFISIOLOGI KLINIS', 'SAR');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('083', 'NEUROMUSKULAR, SARAF PERIFER', 'SAR');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('084', 'NEUROBEHAVIOUR, MD, NEUROGERIATRI, DAN NEURORESTORASI', 'SAR');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('085', 'NEURO-OFTALMOLOGI DAN NEURO-OTOLOGI', 'SAR');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('086', 'NEURO-INTENSIF', 'SAR');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('087', 'NEUROPEDIATRI', 'BSY');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('095', 'INFEKSI PARU', 'PAR');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('096', 'ONKOLOGI TORAKS', 'PAR');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('097', 'ASMA PPOK', 'PAR');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('098', 'INTERVENSI DAN GAWAT NAFAS', 'PAR');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('099', 'FAAL PARU KLINIK', 'PAR');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('100', 'PARU KERJA DAN LINGKUNGAN', 'PAR');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('101', 'IMUNOLOGI PARU', 'PAR');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('104', 'BURN (LUKA BAKAR)', 'BDP');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('105', 'MICRO SURGERY', 'BDP');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('106', 'KRANIOFASIAL (KKF)', 'BDP');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('107', 'HAND (BEDAH TANGAN)', 'BDP');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('108', 'GENITALIA EKSTERNA', 'BDP');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('109', 'REKONTRUKSI DAN ESTETIK', 'BDP');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('132', 'BEDAH VASKULER', 'BED');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('133', 'KORNEA, LENSA DAN BEDAH REFRAKTIF', 'MAT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('134', 'INFEKSI DAN IMMUNOLOGI', 'MAT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('135', 'VITREO - RETINA', 'MAT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('136', 'PEDIATRI ONKOLOGI STRABISMUS', 'MAT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('137', 'NEURO OFTALMOLOGI', 'MAT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('138', 'GLAUKOMA', 'MAT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('139', 'PEDIATRIK OFTALMOLOGI', 'MAT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('140', 'REFRAKSI LENSA KONTAK', 'MAT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('141', 'REKONSTRUKSI OKULOPLASTI DAN ONKOLOGI', 'MAT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('142', 'ONKOLOGI MATA', 'MAT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('143', 'DERMATOLOGI INFEKSI TROPIK', 'KLT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('144', 'DERMATOLOGI PEDIATRIK', 'KLT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('145', 'DERMATOLOGI KOSMETIK', 'KLT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('146', 'INFEKSI MENULAR SEKSUAL', 'KLT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('147', 'DERMATO - ALERGO - IMUNOLOGI', 'KLT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('148', 'DERMATOLOGI GERIATRIK', 'KLT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('149', 'TUMOR DAN BEDAH KULIT', 'KLT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('150', 'DERMATOPATOLOGI', 'KLT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('151', 'TRAUMA DAN REKONSTRUKSI', 'ORT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('152', 'SPINE', 'ORT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('153', 'TUMOR MUSKULOSKELETAL', 'ORT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('154', 'PAEDIATRIC ORTHOPAEDI', 'ORT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('155', 'SPORT, SHOULDER AND ELBOW', 'ORT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('156', 'HAND AND MICROSURGERY', 'ORT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('157', 'HIP AND KNEE', 'ORT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('158', 'BIO ORTHOPEDIC', 'ORT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('160', 'NEUROPSIKIATRI DAN PSIKOMETRI', 'JIW');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('161', 'PSIKOTERAPI', 'JIW');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('162', 'KESEHATAN JIWA ANAK DAN REMAJA', 'JIW');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('163', 'PSIKIATRI GERIATRI', 'JIW');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('164', 'PSIKIATRI ADIKSI', 'JIW');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('165', 'CONSULTATION-LIAISON PSYCHIATRI', 'JIW');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('166', 'PSIKIATRI FORENSIK', 'JIW');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('167', 'PSIKIATRI KOMUNITAS', 'JIW');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('168', 'RADIOTERAPI', 'RDT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('169', 'RADIOLOGI ONKOLOGI', 'RDN');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('170', 'BEDAH KEPALA LEHER', 'BKL');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('172', 'EMERGENSI DAN RAWAT INTENSIF ANAK (ERIA)', 'ANA');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('173', 'NEONATOLOGI', 'ANA');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('174', 'IMAGING ABDOMEN', 'RDO');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('175', 'PEDIATRI', 'IRM');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('176', 'GERIATRI', 'IRM');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('177', 'MUSKULOSKELETAL', 'IRM');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('178', 'NEOMUSKULER', 'IRM');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('179', 'KARDIORESPIRASI', 'IRM');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('180', 'INFEKSI', 'PAK');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('181', 'HEMATOLOGI', 'PAK');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('182', 'IMUNOLOGI', 'PAK');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('183', 'KARDIOCERBROVASKULER', 'PAK');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('184', 'NEFROLOGI', 'PAK');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('185', 'HEPATOGASTROENTEROLOGI', 'PAK');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('186', 'ENDOKRIN DAN METABOLISME', 'PAK');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('187', 'ONKOLOGI', 'PAK');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('188', 'BANK DARAH DAN KEDOKTERAN LABORATORIUM', 'PAK');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('189', 'MANAJEMEN INTERVENSI NYERI', 'SAR');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('190', 'FUNGSI LUHUR', 'SAR');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('191', 'NEUROONKOLOGI', 'SAR');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('192', 'EUROSONOLOGI', 'SAR');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('193', 'PELAYANAN ARITMIA', 'JAN');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('194', 'PELAYANAN JANTUNG ANAK DAN PJB', 'JAN');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('195', 'PELAYANAN VASKULAR', 'JAN');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('196', 'PELAYANAN CARDIAC IMAGING', 'JAN');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('197', 'PELAYANAN INTENSIVE DAN KEGAWATAN KARDIOVAS-KULER', 'JAN');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('198', 'BEDAH DIGESTIF ANAK', 'BDA');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('199', 'UROGENITAL ANAK', 'BDA');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('200', 'NEUROONKOLOGI', 'BSY');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('201', 'NEUROSPINE', 'BSY');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('202', 'NEUROFUNGSI-ONAL', 'BSY');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('203', 'NEUROVASKULAR', 'BSY');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('204', 'OFTALMOLOGI KOMUNITAS', 'MAT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('205', 'FOOT AND ANKLE', 'ORT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('206', 'EMERGENSI', 'EMG');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('AKP', 'AKUPUNTUR MEDIK', 'AKP');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('ANA', 'ANAK', 'ANA');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('AND', 'ANDROLOGI', 'AND');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('ANT', 'ANASTESI', 'ANT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('BDA', 'BEDAH ANAK', 'BDA');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('BDM', 'GIGI BEDAH MULUT', 'BDM');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('BDP', 'BEDAH PLASTIK', 'BDP');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('BED', 'BEDAH', 'BED');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('BSY', 'BEDAH SARAF', 'BSY');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('BTK', 'BTKV', 'BTK');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('FMK', 'FARMAKOLOGI KLINIK', 'FMK');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('FOR', 'FORENSIK', 'FOR');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('GIG', 'GIGI', 'GIG');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('GIZ', 'GIZI KLINIK', 'GIZ');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('GND', 'GIGI ENDODONSI', 'GND');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('GOR', 'GIGI ORTHODONTI', 'GOR');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('GPR', 'GIGI PERIODONTI', 'GPR');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('GRD', 'GIGI RADIOLOGI', 'GRD');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('INT', 'PENYAKIT DALAM', 'INT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('IRM', 'REHABILITASI MEDIK', 'IRM');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('JAN', 'JANTUNG DAN PEMBULUH DARAH', 'JAN');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('JAW', 'JIWA ANAK', 'JIW');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('JIW', 'JIWA', 'JIW');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('KDK', 'KEDOKTERAN KELAUTAN', 'KDK');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('KDN', 'KEDOKTERAN NUKLIR', 'KDN');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('KDO', 'KEDOKTERAN OKUPASI', 'KDO');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('KDP', 'KEDOKTERAN PENERBANGAN', 'KDP');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('KLT', 'KULIT KELAMIN', 'KLT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('KON', 'GIGI PEDODONTIS', 'KON');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('KOR', 'KEDOKTERAAN OLAHRAGA', 'KOR');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('MAT', 'MATA', 'MAT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('MKB', 'MIKROBIOLOGI KLINIK', 'MKB');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('OBG', 'OBGYN', 'OBG');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('ORT', 'ORTHOPEDI', 'ORT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('PAA', 'PATOLOGI ANATOMI', 'PAA');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('PAK', 'PATOLOGI KLINIK', 'PAK');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('PAR', 'PARU', 'PAR');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('PNM', 'GIGI PENYAKIT MULUT', 'PNM');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('PRM', 'PARASITOLOGI UMUM', 'PRM');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('PTD', 'GIGI PRSOSTHODONTI', 'PTD');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('RDO', 'RADIOLOGI', 'RDO');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('SAR', 'SARAF', 'SAR');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('THT', 'THT-KL', 'THT');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('URO', 'UROLOGI', 'URO');
            INSERT INTO mjkn_poli_sub (kode, nama, kode_poli) VALUES ('UMU', 'UMUM', 'UMU');
            ";

        public static string cmd_insert_all_sample_Data =
            @"
            INSERT INTO bl_poli (kode_poli, kode_poli_jkn, nama_poli) VALUES ('P007', '007', 'GINJAL-HIPERTENSI'             );
            INSERT INTO bl_poli (kode_poli, kode_poli_jkn, nama_poli) VALUES ('PUMU', 'UMU', 'UMUM'                          );
            INSERT INTO bl_poli (kode_poli, kode_poli_jkn, nama_poli) VALUES ('P068', '068', 'NEUROTOLOGI'                   );
            INSERT INTO bl_poli (kode_poli, kode_poli_jkn, nama_poli) VALUES ('P069', '069', 'RINOLOGI'                      );
            INSERT INTO bl_poli (kode_poli, kode_poli_jkn, nama_poli) VALUES ('P008', '008', 'HEMATOLOGI - ONKOLOGI MEDIK'   );
            INSERT INTO bl_poli (kode_poli, kode_poli_jkn, nama_poli) VALUES ('P009', '009', 'HEPATOLOGI'                    );
            INSERT INTO bl_poli (kode_poli, kode_poli_jkn, nama_poli) VALUES ('P010', '010', 'ENDOKRIN-METABOLIK-DIABETES'   );


            INSERT INTO bl_dokter (kode_dokter, nama_dokter, kode_poli, kode_dokter_jkn, kapasitas_non_jkn, kapasitas_jkn, total_kapasitas, nomor_urut_jadwal, pasien_time ) 
                VALUES ('DR01', 'dr. Clark Blue',   'P068', 1,  3, 3, 6, 0, 5 );
            INSERT INTO bl_dokter (kode_dokter, nama_dokter, kode_poli, kode_dokter_jkn, kapasitas_non_jkn, kapasitas_jkn, total_kapasitas, nomor_urut_jadwal, pasien_time ) 
                VALUES ('DR02', 'dr. Strange',      'P007', 2,  3, 3, 6, 0, 5 );
            INSERT INTO bl_dokter (kode_dokter, nama_dokter, kode_poli, kode_dokter_jkn, kapasitas_non_jkn, kapasitas_jkn, total_kapasitas, nomor_urut_jadwal, pasien_time ) 
                VALUES ('DR03', 'dr. Brown Hat',    'PUMU', 3,  3, 3, 6, 0, 5 );
            INSERT INTO bl_dokter (kode_dokter, nama_dokter, kode_poli, kode_dokter_jkn, kapasitas_non_jkn, kapasitas_jkn, total_kapasitas, nomor_urut_jadwal, pasien_time ) 
                VALUES ('DR04', 'dr. Black Boots',  'PUMU', 4,  3, 3, 6, 0, 5 );
            INSERT INTO bl_dokter (kode_dokter, nama_dokter, kode_poli, kode_dokter_jkn, kapasitas_non_jkn, kapasitas_jkn, total_kapasitas, nomor_urut_jadwal, pasien_time ) 
                VALUES ('DR05', 'dr. Red Jacket',   'PUMU', 5,  3, 3, 6, 0, 5 );

            INSERT INTO mjkn_dokter (kodedokter_internal, kodedokter_jkn, nama_dokter) VALUES ('DR01', 1, 'dr. Clark Blue');
            INSERT INTO mjkn_dokter (kodedokter_internal, kodedokter_jkn, nama_dokter) VALUES ('DR02', 2, 'dr. Strange');
            INSERT INTO mjkn_dokter (kodedokter_internal, kodedokter_jkn, nama_dokter) VALUES ('DR03', 3, 'dr. Brown Hat');
            INSERT INTO mjkn_dokter (kodedokter_internal, kodedokter_jkn, nama_dokter) VALUES ('DR04', 4, 'dr. Black Boots');
            INSERT INTO mjkn_dokter (kodedokter_internal, kodedokter_jkn, nama_dokter) VALUES ('DR05', 5, 'dr. Red Jacket');

            INSERT INTO bl_pasien (nomor_rekam_medis, nomor_identitas, nomor_kartu_keluarga, nomor_kartu_jkn, nama, gender, tanggal_lahir, insurance_id, alamat, phone, kode_propinsi, nama_propinsi, kode_dati2, nama_dati2, kode_kecamatan, nama_kecamatan, kode_kelurahan, nama_kelurahan, rw, rt) VALUES
                    ('000001', '2000000000000001', '3000000000000001', '1000000000001', 'User001', 'L', '1990-03-01', 'BPJS', 'Jalan Kenari Besar 01', '620000000001', '11', 'Jawa Barat', '0120', 'Kab. Bandung', '1319', 'Soreang', 'D2105', 'CingCing', '013', '001'),
                    ('000002', '2000000000000002', '3000000000000002', '1000000000002', 'User002', 'L', '1990-03-02', 'BPJS', 'Jalan Kenari Besar 02', '620000000002', '11', 'Jawa Barat', '0120', 'Kab. Bandung', '1319', 'Soreang', 'D2105', 'CingCing', '013', '001'),
                    ('000003', '2000000000000003', '3000000000000003', '1000000000003', 'User003', 'L', '1990-03-03', 'BPJS', 'Jalan Kenari Besar 03', '620000000003', '11', 'Jawa Barat', '0120', 'Kab. Bandung', '1319', 'Soreang', 'D2105', 'CingCing', '013', '001'),
                    ('000004', '2000000000000004', '3000000000000004', '1000000000004', 'User004', 'L', '1990-03-04', 'BPJS', 'Jalan Kenari Besar 04', '620000000004', '11', 'Jawa Barat', '0120', 'Kab. Bandung', '1319', 'Soreang', 'D2105', 'CingCing', '013', '001'),
                    ('000005', '2000000000000005', '3000000000000005', '1000000000005', 'User005', 'L', '1990-03-05', 'BPJS', 'Jalan Kenari Besar 05', '620000000005', '11', 'Jawa Barat', '0120', 'Kab. Bandung', '1319', 'Soreang', 'D2105', 'CingCing', '013', '001'),
                    ('000006', '2000000000000006', '3000000000000006', '1000000000006', 'User006', 'L', '1990-03-06', 'BPJS', 'Jalan Kenari Besar 06', '620000000006', '11', 'Jawa Barat', '0120', 'Kab. Bandung', '1319', 'Soreang', 'D2105', 'CingCing', '013', '001'),
                    ('000007', '2000000000000007', '3000000000000007', '1000000000007', 'User007', 'L', '1990-03-07', 'BPJS', 'Jalan Kenari Besar 07', '620000000007', '11', 'Jawa Barat', '0120', 'Kab. Bandung', '1319', 'Soreang', 'D2105', 'CingCing', '013', '001'),
                    ('000008', '2000000000000008', '3000000000000008', '1000000000008', 'User008', 'L', '1990-03-08', 'BPJS', 'Jalan Kenari Besar 08', '620000000008', '11', 'Jawa Barat', '0120', 'Kab. Bandung', '1319', 'Soreang', 'D2105', 'CingCing', '013', '001'),
                    ('000009', '2000000000000009', '3000000000000009', '1000000000009', 'User009', 'L', '1990-03-09', 'BPJS', 'Jalan Kenari Besar 09', '620000000009', '11', 'Jawa Barat', '0120', 'Kab. Bandung', '1319', 'Soreang', 'D2105', 'CingCing', '013', '001'),
                    ('000010', '2000000000000010', '3000000000000010', '1000000000010', 'User010', 'L', '1990-03-10', 'XXXX', 'Jalan Kenari Besar 10', '620000000010', '11', 'Jawa Barat', '0120', 'Kab. Bandung', '1319', 'Soreang', 'D2105', 'CingCing', '013', '001');


            INSERT INTO mjkn_pasien (nomor_kartu, nik, nomor_kk, nomor_rm, nama, jenis_kelamin, tanggal_lahir, nomor_hp, alamat, kode_propinsi, nama_propinsi, kode_dati2, nama_dati2, kode_kecamatan, nama_kecamatan, kode_kelurahan, nama_kelurahan, rw, rt) VALUES
                    ('1000000000001', '2000000000000001', '3000000000000001', '000001', 'User001', 'L', '1990-03-01', '620000000001', 'Jalan Kenari Besar 01', '11', 'Jawa Barat', '0120', 'Kab. Bandung', '1319', 'Soreang', 'D2105', 'CingCing', '001', '013'),
                    ('1000000000002', '2000000000000002', '3000000000000002', '000002', 'User002', 'L', '1990-03-02', '620000000002', 'Jalan Kenari Besar 02', '11', 'Jawa Barat', '0120', 'Kab. Bandung', '1319', 'Soreang', 'D2105', 'CingCing', '001', '013'),
                    ('1000000000003', '2000000000000003', '3000000000000003', '000003', 'User003', 'L', '1990-03-03', '620000000003', 'Jalan Kenari Besar 03', '11', 'Jawa Barat', '0120', 'Kab. Bandung', '1319', 'Soreang', 'D2105', 'CingCing', '001', '013'),
                    ('1000000000004', '2000000000000004', '3000000000000004', '000004', 'User004', 'L', '1990-03-04', '620000000004', 'Jalan Kenari Besar 04', '11', 'Jawa Barat', '0120', 'Kab. Bandung', '1319', 'Soreang', 'D2105', 'CingCing', '001', '013'),
                    ('1000000000005', '2000000000000005', '3000000000000005', '000005', 'User005', 'L', '1990-03-05', '620000000005', 'Jalan Kenari Besar 05', '11', 'Jawa Barat', '0120', 'Kab. Bandung', '1319', 'Soreang', 'D2105', 'CingCing', '001', '013'),
                    ('1000000000006', '2000000000000006', '3000000000000006', '000006', 'User006', 'L', '1990-03-06', '620000000006', 'Jalan Kenari Besar 06', '11', 'Jawa Barat', '0120', 'Kab. Bandung', '1319', 'Soreang', 'D2105', 'CingCing', '001', '013'),
                    ('1000000000007', '2000000000000007', '3000000000000007', '000007', 'User007', 'L', '1990-03-07', '620000000007', 'Jalan Kenari Besar 07', '11', 'Jawa Barat', '0120', 'Kab. Bandung', '1319', 'Soreang', 'D2105', 'CingCing', '001', '013'),
                    ('1000000000008', '2000000000000008', '3000000000000008', '000008', 'User008', 'L', '1990-03-08', '620000000008', 'Jalan Kenari Besar 08', '11', 'Jawa Barat', '0120', 'Kab. Bandung', '1319', 'Soreang', 'D2105', 'CingCing', '001', '013'),
                    ('1000000000009', '2000000000000009', '3000000000000009', '000009', 'User009', 'L', '1990-03-09', '620000000009', 'Jalan Kenari Besar 09', '11', 'Jawa Barat', '0120', 'Kab. Bandung', '1319', 'Soreang', 'D2105', 'CingCing', '001', '013'),
                    ('1000000000010', '2000000000000010', '3000000000000010', '000010', 'User010', 'L', '1990-03-10', '620000000010', 'Jalan Kenari Besar 10', '11', 'Jawa Barat', '0120', 'Kab. Bandung', '1319', 'Soreang', 'D2105', 'CingCing', '001', '013');
            
            UPDATE bl_id_gen SET current_number=10, last_number=10 WHERE kode_faskes='PRIMARY';
            ";
    }
}