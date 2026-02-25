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

namespace Tobasa
{
    internal class DBMigration_MySQL
    {
        public DBMigration_MySQL() { }

        public static string t_base_users =
            @"
            CREATE TABLE `base_users` (
               id              INT AUTO_INCREMENT PRIMARY KEY,
               `uuid`          VARCHAR(64)    NOT NULL UNIQUE,
               `user_name`     VARCHAR(100)   NOT NULL UNIQUE,
               `first_name`    VARCHAR(255)   NOT NULL,
               `last_name`     VARCHAR(255)   NOT NULL,
               `email`         VARCHAR(255)   NOT NULL,
               `image`         VARCHAR(255)   NOT NULL DEFAULT '',
               `enabled`       TINYINT        NOT NULL DEFAULT '1',
               `password_salt` VARBINARY(512) NOT NULL,
               `password_hash` VARBINARY(512) NOT NULL,
               `allow_login`   TINYINT        NOT NULL DEFAULT '1',
               `created`       DATETIME       NOT NULL DEFAULT CURRENT_TIMESTAMP,
               `updated`       DATETIME       NOT NULL DEFAULT CURRENT_TIMESTAMP,
               `expired`       DATETIME       NOT NULL DEFAULT '2030-12-31 01:01:01',
               `last_login`    DATETIME       NULL,
               `unique_code`   VARCHAR(48)    NOT NULL DEFAULT '',
               `birth_date`    DATE           NOT NULL,
               `phone`         VARCHAR(20)    NOT NULL DEFAULT '',
               `gender`        VARCHAR(1)     NOT NULL DEFAULT '',
               `address`       VARCHAR(512)   NOT NULL DEFAULT '',
               `nik`           VARCHAR(16)    NOT NULL DEFAULT ''
            ) COLLATE='utf8mb4_general_ci' ENGINE=InnoDB;
            ";

        public static string t_base_roles =
            @"
            CREATE TABLE `base_roles` (
               id        INT AUTO_INCREMENT PRIMARY KEY,
               `name`    VARCHAR(50)   NOT NULL,
               `alias`   VARCHAR(100)  NOT NULL,
               `enabled` TINYINT       NOT NULL DEFAULT '0',
               `created` DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
               `updated` DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
               `sysrole` TINYINT       NOT NULL DEFAULT '0'
            ) COLLATE='utf8mb4_general_ci' ENGINE=InnoDB;
            ";

        public static string t_base_sites =
            @"
            CREATE TABLE `base_sites` (
               id        INT AUTO_INCREMENT PRIMARY KEY,
               `code`    VARCHAR(7)    NOT NULL DEFAULT '',
               `name`    VARCHAR(50)    NOT NULL DEFAULT '',
               `address` VARCHAR(512)  NOT NULL DEFAULT ''
            ) COLLATE='utf8mb4_general_ci' ENGINE=InnoDB;
            ";

        public static string t_base_user_role =
            @"
            CREATE TABLE `base_user_role` (
               id           INT NOT NULL AUTO_INCREMENT,
               `user_id`    INT NOT NULL,
               `role_id`    INT NOT NULL,
               `is_primary` TINYINT NOT NULL,
               PRIMARY KEY (`user_id`, `role_id`),
               UNIQUE INDEX `id` (`id`),
               INDEX `FK_base_user_role_base_roles` (`role_id`),
               CONSTRAINT `FK_base_user_role_base_roles` FOREIGN KEY (`role_id`) REFERENCES `base_roles` (`id`) ON UPDATE NO ACTION ON DELETE NO ACTION,
               CONSTRAINT `FK_base_user_role_base_users` FOREIGN KEY (`user_id`) REFERENCES `base_users` (`id`) ON UPDATE NO ACTION ON DELETE NO ACTION
            ) COLLATE='utf8mb4_general_ci' ENGINE=InnoDB;
            ";

        public static string t_base_user_site =
            @"
            CREATE TABLE `base_user_site` (
               id             INT      NOT NULL AUTO_INCREMENT,
               `user_id`      INT      NOT NULL,
               `site_id`      INT      NOT NULL,
               `allow_login`  TINYINT NOT NULL DEFAULT '1',
               `is_admin`     TINYINT NOT NULL DEFAULT '0',
               PRIMARY KEY (`user_id`, `site_id`),
               UNIQUE INDEX `id` (`id`),
               INDEX `FK_base_user_site_base_sites` (`site_id`),
               CONSTRAINT `FK_base_user_site_base_sites` FOREIGN KEY (`site_id`) REFERENCES `base_sites` (`id`) ON UPDATE NO ACTION ON DELETE NO ACTION,
               CONSTRAINT `FK_base_user_site_base_users` FOREIGN KEY (`user_id`) REFERENCES `base_users` (`id`) ON UPDATE NO ACTION ON DELETE NO ACTION
            ) COLLATE='utf8mb4_general_ci' ENGINE=InnoDB;
            ";

        public static string t_bl_antrian =
            @"
            CREATE TABLE bl_antrian (
               id bigint AUTO_INCREMENT PRIMARY KEY,
               nomor_registrasi varchar(20) NOT NULL DEFAULT '',
               jadwal_id bigint NOT NULL,
               kode_poli varchar(10) NOT NULL DEFAULT '',
               tanggal date,
               nama_hari varchar(20) NOT NULL DEFAULT '',
               jam_mulai varchar(8) NOT NULL DEFAULT '',
               jam_praktek varchar(11) NOT NULL DEFAULT '',
               kode_dokter varchar(10) NOT NULL DEFAULT '',
               nama_pasien varchar(256) NOT NULL DEFAULT '',
               nomor_rekam_medis varchar(20) NOT NULL DEFAULT '',
               nomor_kartu_jkn varchar(13) NOT NULL DEFAULT '',
               tanggal_lahir date,
               alamat varchar(512) NOT NULL DEFAULT '',
               phone varchar(16) NOT NULL DEFAULT '',
               insurance_id varchar(10) NOT NULL DEFAULT '',
               nomor_rujukan varchar(20) NOT NULL DEFAULT '',
               nomor_antrian_int integer NOT NULL DEFAULT 0,
               nomor_antrian varchar(20) NOT NULL DEFAULT '0',
               token_antrian varchar(12) NOT NULL DEFAULT '',
               jenis_kunjungan integer NOT NULL DEFAULT 0,
               keterangan varchar(256) NOT NULL DEFAULT '',
               note varchar(256) NOT NULL DEFAULT '',
               estimasi_dilayani bigint NOT NULL DEFAULT 0,
               sisa_kuota_jkn integer NOT NULL DEFAULT -1,
               kuota_jkn integer NOT NULL DEFAULT -1,
               sisa_kuota_non_jkn integer NOT NULL DEFAULT -1,
               kuota_non_jkn integer NOT NULL DEFAULT -1,
               pasien_baru tinyint NOT NULL DEFAULT 0,
               status_antri integer NOT NULL DEFAULT 0, -- 0:BOOKED, 1:CHECKED_IN, 2:SERVED, -1:CANCELLED, -2:NOSHOW
               booked_at datetime,    -- BOOKED
               checkin_at datetime,   -- CHECKED_IN
               served_at datetime,    -- SERVED
               cancelled_at datetime, -- CANCELLED
               created_by varchar(256) NOT NULL DEFAULT '',
               edited_by varchar(256) NOT NULL DEFAULT ''
            ) COLLATE='utf8mb4_general_ci' ENGINE=InnoDB ;
            ";

        public static string t_bl_antrian_status_history =
            @"
            CREATE TABLE bl_antrian_status_history (
                id bigint AUTO_INCREMENT PRIMARY KEY,
                antrian_id bigint NOT NULL REFERENCES bl_antrian(id),
                status varchar(20) NOT NULL,  -- BOOKED, CHECKED_IN, SERVED, CANCELLED, NOSHOW
                changed_at datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
                changed_by varchar(50) NOT NULL DEFAULT '',
                note varchar(256) NOT NULL DEFAULT ''
            );
            ";

        public static string t_bl_dokter =
            @"
            CREATE TABLE bl_dokter (
               kode_dokter varchar(10) PRIMARY KEY,
               nama_dokter varchar(256) NOT NULL,
               kode_poli varchar(10) NOT NULL DEFAULT '',
               kode_dokter_jkn integer NOT NULL DEFAULT 0,
               kapasitas_non_jkn integer NOT NULL DEFAULT 0,
               kapasitas_jkn integer NOT NULL DEFAULT 0,
               total_kapasitas integer NOT NULL DEFAULT 0,
               nomor_urut_jadwal bigint NOT NULL DEFAULT 0,
               pasien_time integer NOT NULL DEFAULT 0,
               profile_image varchar(200) NOT NULL DEFAULT ''
            ) COLLATE='utf8mb4_general_ci' ENGINE=InnoDB ;
            ";

        public static string t_bl_jadwal_bedah =
            @"
            CREATE TABLE `bl_jadwal_bedah` (
	            `nomor_jadwal` varchar(20) PRIMARY KEY,
	            `tanggal_operasi` date NOT NULL,
	            `nomor_registrasi` varchar(20) NOT NULL,
	            `nomor_rekam_medis` varchar(20) NOT NULL,
	            `waktu_operasi` varchar(5) NOT NULL,
	            `kode_dokter_operator` varchar(10) NOT NULL,
	            `terlaksana` tinyint NOT NULL DEFAULT 0,
	            `jenis_tindakan` varchar(256) NOT NULL DEFAULT ''
            ) COLLATE='utf8mb4_general_ci' ENGINE=InnoDB ;
            ";

        public static string t_bl_jadwal =
            @"
            CREATE TABLE bl_jadwal (
               id bigint AUTO_INCREMENT PRIMARY KEY,
               kode_jadwal varchar(20) NOT NULL UNIQUE,
               kode_dokter varchar(10) NOT NULL,
               kode_poli varchar(10) NOT NULL,
               nama_hari varchar(20) NOT NULL,
               jam_mulai varchar(5) NOT NULL,
               jam_selesai varchar(5) NOT NULL,
               keterangan varchar(256) NOT NULL DEFAULT '',
               tanggal date NOT NULL,
               libur tinyint NOT NULL DEFAULT 0 CHECK (libur IN (0, 1)),
               quota_non_jkn integer NOT NULL DEFAULT 0,
               quota_jkn integer NOT NULL DEFAULT 0,
               quota_total integer NOT NULL DEFAULT 0,
               quota_jkn_used integer NOT NULL DEFAULT 0,
               quota_non_jkn_used integer NOT NULL DEFAULT 0,
               locked tinyint NOT NULL DEFAULT 0 CHECK (locked IN (0, 1))
            ) COLLATE='utf8mb4_general_ci' ENGINE=InnoDB ;
            ";

        public static string t_bl_id_gen =
            @"
            CREATE TABLE bl_id_gen (
               kode_faskes varchar(20) PRIMARY KEY,
               current_number bigint NOT NULL DEFAULT 0,
               last_number bigint NOT NULL DEFAULT 0,
               gen_rule varchar(200) NOT NULL DEFAULT ''
            ) COLLATE='utf8mb4_general_ci' ENGINE=InnoDB ;
            ";

        public static string t_bl_pasien =
            @"
            CREATE TABLE bl_pasien (
               nomor_rekam_medis varchar(20) PRIMARY KEY,
               nomor_identitas varchar(16) NOT NULL DEFAULT '',
               nomor_kartu_keluarga varchar(16) NOT NULL DEFAULT '',
               nomor_kartu_jkn varchar(13) NOT NULL DEFAULT '',
               nama varchar(256) NOT NULL DEFAULT '',
               honorific varchar(20) NOT NULL DEFAULT '',
               gender varchar(1) NOT NULL,
               tanggal_lahir date NOT NULL,
               insurance_id varchar(10) NOT NULL DEFAULT '',
               last_reservation bigint NOT NULL DEFAULT 0,
               finger_print tinyint NOT NULL DEFAULT 0 CHECK (finger_print IN (0, 1)), 
               alamat varchar(512) NOT NULL DEFAULT '',
               phone varchar(16) NOT NULL DEFAULT '',
               kode_propinsi varchar(10) NOT NULL DEFAULT '',
               nama_propinsi varchar(256) NOT NULL DEFAULT '',
               kode_dati2 varchar(10) NOT NULL DEFAULT '',
               nama_dati2 varchar(256) NOT NULL DEFAULT '',
               kode_kecamatan varchar(256) NOT NULL DEFAULT '',
               nama_kecamatan varchar(256) NOT NULL DEFAULT '',
               kode_kelurahan varchar(20) NOT NULL DEFAULT '',
               nama_kelurahan varchar(256) NOT NULL DEFAULT '',
               rw varchar(3) NOT NULL DEFAULT '',
               rt varchar(3) NOT NULL DEFAULT '',
               created_at datetime NOT NULL DEFAULT NOW(),
               modified_at datetime NOT NULL DEFAULT NOW(),
	            INDEX `bl_pasien_tgllahir` (`tanggal_lahir`),
	            INDEX `bl_pasien_namapas` (`nama`)
            ) COLLATE='utf8mb4_general_ci' ENGINE=InnoDB ;
            ";

        public static string t_bl_poli =
            @"
            CREATE TABLE `bl_poli` (
               `kode_poli` varchar(10) PRIMARY KEY,
               `kode_poli_jkn` varchar(10) NOT NULL DEFAULT '',
               `nama_poli` varchar(256) NOT NULL
            ) COLLATE='utf8mb4_general_ci' ENGINE=InnoDB ;
            ";

        public static string t_mjkn_ambil_antrian =
            @"
            CREATE TABLE mjkn_ambil_antrian (
	            id bigint AUTO_INCREMENT PRIMARY KEY,
	            nomor_kartu varchar(13) NOT NULL,
	            nik varchar(16) NOT NULL,
	            nomor_hp varchar(16) NOT NULL,
	            kode_poli varchar(10) NOT NULL,
	            nomor_rm varchar(20) NOT NULL,
	            tanggal_periksa date NOT NULL,
	            kode_dokter integer NOT NULL,
	            jam_praktek varchar(11) NOT NULL,
	            jenis_kunjungan integer NOT NULL,
	            nomor_referensi varchar(19) NOT NULL,
	            reservation_id bigint NOT NULL DEFAULT 0,
	            created_at datetime NOT NULL DEFAULT NOW()
            ) COLLATE='utf8mb4_general_ci' ENGINE=InnoDB ;
            ";

        public static string t_mjkn_antrian_transaction =
            @"
            CREATE TABLE mjkn_antrian_transaction (
               id bigint AUTO_INCREMENT PRIMARY KEY,
               r_nomor_kartu varchar(13) NOT NULL DEFAULT '',
               r_nik varchar(16) NOT NULL DEFAULT '',
               r_nomor_hp varchar(16) NOT NULL DEFAULT '',
               r_kode_poli_jkn varchar(3) NOT NULL DEFAULT '',
               r_nomor_rm varchar(20) NOT NULL DEFAULT '',
               r_tanggal_periksa date,
               r_kode_dokter_jkn integer NOT NULL DEFAULT '-1',
               r_jam_praktek varchar(11) NOT NULL DEFAULT '',
               r_jenis_kunjungan integer NOT NULL DEFAULT '0',
               r_nomor_referensi varchar(19) NOT NULL DEFAULT '',
               r_source varchar(20) NOT NULL DEFAULT '',
               s_username varchar(100) NOT NULL DEFAULT '',
               s_useredit varchar(100) NOT NULL DEFAULT '',
               s_pasien_baru tinyint NOT NULL DEFAULT 0,
               s_status integer not null DEFAULT 0,
               s_booked_at datetime,
               s_checkin_at datetime,
               s_served_at datetime,
               s_cancelled_at datetime,
               s_antri_id bigint NOT NULL,
               s_jadwal_id bigint NOT NULL,
               s_kodepoli_internal varchar(20) NOT NULL,
               s_kodokter_internal varchar(20) NOT NULL,
               s_nama_hari varchar(20) NOT NULL,
               s_nama_pasien varchar(256) NOT NULL DEFAULT '',
               s_alamat_pasien varchar(512) NOT NULL DEFAULT '',
               s_phone varchar(100) NOT NULL DEFAULT '',
               s_tanggal_lahir date,
               o_nomor_antrian varchar(20) NOT NULL DEFAULT '',
               o_angka_antrian integer NOT NULL DEFAULT '-1',
               o_kode_booking varchar(12) NOT NULL DEFAULT '',
               o_nama_poli varchar(256) NOT NULL DEFAULT '',
               o_nama_dokter varchar(256) NOT NULL DEFAULT '',
               o_estimasi_dilayani bigint NOT NULL DEFAULT '-1',
               o_sisa_kuota_jkn integer NOT NULL DEFAULT '-1',
               o_kuota_jkn integer NOT NULL DEFAULT '-1',
               o_sisa_kuota_non_jkn integer NOT NULL DEFAULT '-1',
               o_kuota_non_jkn integer NOT NULL DEFAULT '-1',
               o_keterangan varchar(256) NOT NULL DEFAULT '',
               s_keterangan varchar(256) NOT NULL DEFAULT ''
            ) COLLATE='utf8mb4_general_ci' ENGINE=InnoDB ;
            ";

        public static string t_mjkn_dashboard_antrian =
            @"
            CREATE TABLE mjkn_dashboard_antrian (
	            id integer AUTO_INCREMENT PRIMARY KEY,
	            kode_ppk varchar(20) NOT NULL,
	            nama_ppk varchar(200) NOT NULL,
	            kode_poli varchar(10) NOT NULL,
	            nama_poli varchar(200) NOT NULL,
	            tanggal datetime NOT NULL,
	            insert_date bigint NOT NULL,
	            jumlah_antrian integer NOT NULL,
	            waktu_task1 integer NOT NULL,
	            waktu_task2 integer NOT NULL,
	            waktu_task3 integer NOT NULL,
	            waktu_task4 integer NOT NULL,
	            waktu_task5 integer NOT NULL,
	            waktu_task6 integer NOT NULL,
	            avg_waktu_task1 integer NOT NULL,
	            avg_waktu_task2 integer NOT NULL,
	            avg_waktu_task3 integer NOT NULL,
	            avg_waktu_task4 integer NOT NULL,
	            avg_waktu_task5 integer NOT NULL,
	            avg_waktu_task6 integer NOT NULL,
	            created_at datetime NOT NULL DEFAULT NOW()
            ) COLLATE='utf8mb4_general_ci' ENGINE=InnoDB ;
            ";

        public static string t_mjkn_dokter =
            @"
            CREATE TABLE mjkn_dokter(
	            id integer AUTO_INCREMENT PRIMARY KEY,
	            kodedokter_internal varchar(10) NOT NULL,
	            kodedokter_jkn integer NOT NULL UNIQUE,
	            nama_dokter varchar(256) NOT NULL DEFAULT ''
            ) COLLATE='utf8mb4_general_ci' ENGINE=InnoDB ;
            ";

        public static string t_mjkn_jadwal_dokter =
            @"
            CREATE TABLE mjkn_jadwal_dokter (
	            id integer AUTO_INCREMENT PRIMARY KEY,
	            kodedokter_jkn integer NOT NULL,
	            kodedokter_internal varchar(10) NOT NULL DEFAULT '',
	            kode_poli varchar(10) NOT NULL,
	            kode_subspesialis varchar(10) NOT NULL,
	            nama_dokter varchar(256) NOT NULL,
	            nama_poli varchar(256) NOT NULL,
	            nama_subspesialis varchar(256) NOT NULL,
	            hari integer NOT NULL,
	            jadwal varchar(20) NOT NULL,
	            jam_mulai varchar(5) NOT NULL,
	            jam_tutup varchar(5) NOT NULL,
	            nama_hari varchar(20) NOT NULL,
	            libur tinyint NOT NULL DEFAULT 0,
	            kapasitas_pasien integer NOT NULL,
	            last_update datetime NULL,
	            last_update_status varchar(50) NOT NULL DEFAULT '',
	            last_update_code integer NULL
            ) COLLATE='utf8mb4_general_ci' ENGINE=InnoDB ;
            ";

        public static string t_mjkn_pasien =
            @"
            CREATE TABLE mjkn_pasien(
	            id bigint AUTO_INCREMENT PRIMARY KEY,
	            nomor_kartu varchar(13) NOT NULL UNIQUE,
	            nik varchar(16) NOT NULL,
	            nomor_kk varchar(16) NOT NULL,
	            nomor_rm varchar(20) NOT NULL,
	            nama varchar(256) NOT NULL,
	            jenis_kelamin varchar(1) NOT NULL,
	            tanggal_lahir date NOT NULL,
	            nomor_hp varchar(16) NOT NULL DEFAULT '',
	            alamat varchar(512) NOT NULL DEFAULT '',
	            kode_propinsi varchar(10) NOT NULL DEFAULT '',
	            nama_propinsi varchar(256) NOT NULL DEFAULT '',
	            kode_dati2 varchar(10) NOT NULL DEFAULT '',
	            nama_dati2 varchar(256) NOT NULL DEFAULT '',
	            kode_kecamatan varchar(256) NOT NULL DEFAULT '',
	            nama_kecamatan varchar(256) NOT NULL DEFAULT '',
	            kode_kelurahan varchar(20) NOT NULL DEFAULT '',
	            nama_kelurahan varchar(256) NOT NULL DEFAULT '',
	            rw varchar(3) NOT NULL DEFAULT '',
	            rt varchar(3) NOT NULL DEFAULT '',
	            last_reservation bigint NOT NULL DEFAULT 0,
	            created_at datetime NOT NULL DEFAULT NOW(),
               finger_print tinyint NOT NULL DEFAULT 0
            ) COLLATE='utf8mb4_general_ci' ENGINE=InnoDB ;
            ";

        public static string t_mjkn_poli_sub =
            @"
            CREATE TABLE mjkn_poli_sub (
	            id integer AUTO_INCREMENT PRIMARY KEY,
	            kode varchar(10) NOT NULL UNIQUE,
	            nama varchar(256) NOT NULL,
	            kode_poli varchar(10) NOT NULL,
               finger_print tinyint NOT NULL DEFAULT 0
            ) COLLATE='utf8mb4_general_ci' ENGINE=InnoDB ;
            ";

        public static string t_mjkn_poli =
            @"
            CREATE TABLE mjkn_poli (
	            id integer AUTO_INCREMENT PRIMARY KEY,
	            kode varchar(10) NOT NULL UNIQUE,
	            nama varchar(256) NOT NULL
            ) COLLATE='utf8mb4_general_ci' ENGINE=InnoDB ;
            ";

        public static string t_mjkn_tasks =
            @"
            CREATE TABLE mjkn_tasks (
	            id integer AUTO_INCREMENT PRIMARY KEY,
	            kode_booking varchar(12) NULL,
	            task_id integer NULL,
	            task_name varchar(200) NULL,
	            waktu_rs varchar(100) NULL,
	            waktu varchar(100) NULL
            ) COLLATE='utf8mb4_general_ci' ENGINE=InnoDB ;
            ";
        public static List<string> GetCommandList()
        {
            List<string> cmdList = new List<string>
            {
                t_base_roles
                , t_base_sites
                , t_base_users
                , t_base_user_role
                , t_base_user_site
                , t_bl_id_gen
                , t_bl_poli
                , t_bl_dokter
                , t_bl_pasien
                , t_bl_jadwal
                , t_bl_antrian
                , t_bl_antrian_status_history
                , t_bl_jadwal_bedah
                , t_mjkn_ambil_antrian
                , t_mjkn_antrian_transaction
                , t_mjkn_dashboard_antrian
                , t_mjkn_dokter
                , t_mjkn_jadwal_dokter
                , t_mjkn_pasien
                , t_mjkn_poli
                , t_mjkn_poli_sub
                , t_mjkn_tasks
            };

            return cmdList;
        }

        public static string GetObjectSummaryQuery(string databaseName)
        {
            string sql = $@"
            SELECT
              ( SELECT COUNT(*) FROM information_schema.tables 
                WHERE TABLE_TYPE = 'BASE TABLE'
                AND table_schema = '{databaseName}' AND (table_name LIKE 'base_%' OR table_name LIKE 'bl_%' OR table_name LIKE 'mjkn_%')   ) AS total_table
                
            , ( SELECT COUNT(*) FROM information_schema.tables 
                WHERE TABLE_TYPE = 'VIEW' 
                AND table_schema = '{databaseName}' AND table_name LIKE 'xx_yy_zz_%') AS total_view
                
            , ( SELECT COUNT(*) FROM information_schema.TRIGGERS 
                WHERE EVENT_OBJECT_TABLE = 'xx_yy_zz_' AND TRIGGER_SCHEMA = '{databaseName}' 
                AND TRIGGER_NAME = 'xx_yy_zz_' ) AS total_trigger;
            ";

            return sql;
        }
    }
}