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
    internal class DBMigration_SQLite
    {
        public DBMigration_SQLite() { }

        public static string t_base_users =
            @"
            CREATE TABLE base_users (
               id             INTEGER PRIMARY KEY AUTOINCREMENT,
               uuid           VARCHAR(64)    NOT NULL UNIQUE,
               user_name      VARCHAR(50)    NOT NULL UNIQUE,
               first_name     VARCHAR(127)   NOT NULL,
               last_name      VARCHAR(127)   NOT NULL,
               email          VARCHAR(256)   NOT NULL,
               image          VARCHAR(256)   NOT NULL DEFAULT '',
               enabled        TINYINT        NOT NULL DEFAULT 1 CHECK (enabled IN (0, 1)),
               password_salt  BLOB           NOT NULL,
               password_hash  BLOB           NOT NULL,
               allow_login    TINYINT        NOT NULL DEFAULT 1 CHECK (allow_login IN (0, 1)),
               created        DATETIME       NOT NULL DEFAULT CURRENT_TIMESTAMP,
               updated        DATETIME       NOT NULL DEFAULT CURRENT_TIMESTAMP,
               expired        DATETIME       NOT NULL DEFAULT '2030-12-31 00:00:00',
               last_login     DATETIME       NULL,
               unique_code    VARCHAR(48)    NULL DEFAULT '',
               birth_date     DATE           NOT NULL,
               phone          VARCHAR(20)    NULL DEFAULT '',
               gender         VARCHAR(1)     NULL DEFAULT '',
               address        VARCHAR(512)   NOT NULL DEFAULT '',
               nik            VARCHAR(16)    NOT NULL DEFAULT ''
            );
            ";

        public static string t_base_roles =
            @"
            CREATE TABLE base_roles (
               id       INTEGER PRIMARY KEY AUTOINCREMENT,
               name     VARCHAR(50)    NOT NULL UNIQUE,
               alias    VARCHAR(100)   NOT NULL UNIQUE,
               enabled  TINYINT        NOT NULL DEFAULT 0 CHECK (enabled IN (0, 1)),
               created  DATETIME       NOT NULL DEFAULT CURRENT_TIMESTAMP,
               updated  DATETIME       NOT NULL DEFAULT CURRENT_TIMESTAMP,
               sysrole  TINYINT        NOT NULL DEFAULT 0 CHECK (enabled IN (0, 1))
            );
            ";

        public static string t_base_sites =
            @"
            CREATE TABLE base_sites (
               id       INTEGER PRIMARY KEY AUTOINCREMENT,
               code     VARCHAR(7)     NOT NULL UNIQUE,
               name     VARCHAR(50)    NOT NULL UNIQUE,
               address  VARCHAR(512)   NOT NULL DEFAULT ''
            );
            ";

        public static string t_base_user_role =
            @"
            CREATE TABLE base_user_role (
               id          INTEGER PRIMARY KEY AUTOINCREMENT,
               user_id     INTEGER NOT NULL,
               role_id     INTEGER NOT NULL,
               is_primary  TINYINT NOT NULL DEFAULT 0 CHECK (is_primary IN (0, 1)),
               FOREIGN KEY (role_id) REFERENCES base_roles(id) ON UPDATE CASCADE ON DELETE CASCADE,
               FOREIGN KEY (user_id) REFERENCES base_users(id) ON UPDATE CASCADE ON DELETE CASCADE,
               UNIQUE(user_id, role_id)
            );
            ";

        public static string t_base_user_site =
            @"
            CREATE TABLE base_user_site (
               id          INTEGER PRIMARY KEY AUTOINCREMENT,
               user_id     INTEGER NOT NULL,
               site_id     INTEGER NOT NULL,
               allow_login TINYINT NOT NULL DEFAULT 1 CHECK (allow_login IN (0, 1)),
               is_admin    TINYINT NOT NULL DEFAULT 0 CHECK (is_admin IN (0, 1)),
               FOREIGN KEY (site_id) REFERENCES base_sites(id) ON UPDATE CASCADE ON DELETE CASCADE,
               FOREIGN KEY (user_id) REFERENCES base_users(id) ON UPDATE CASCADE ON DELETE CASCADE,
               UNIQUE (user_id, site_id)
            );
            ";

        public static string t_bl_antrian =
            @"
            CREATE TABLE bl_antrian (
               id integer PRIMARY KEY AUTOINCREMENT,
               nomor_registrasi varchar(20) NOT NULL DEFAULT '',
               jadwal_id integer NOT NULL,
               kode_poli varchar(10) NOT NULL DEFAULT '',
               tanggal date,
               nama_hari varchar(20) NOT NULL DEFAULT '',
               jam_mulai varchar(8) NOT NULL DEFAULT '',
               jam_praktek varchar(11) NOT NULL DEFAULT '',
               kode_dokter varchar(10) NOT NULL DEFAULT '',
               nama_pasien varchar(256) NOT NULL DEFAULT '',
               nomor_rekam_medis varchar(10) NOT NULL DEFAULT '',
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
               estimasi_dilayani integer NOT NULL DEFAULT 0,
               sisa_kuota_jkn integer NOT NULL DEFAULT -1,
               kuota_jkn integer NOT NULL DEFAULT -1,
               sisa_kuota_non_jkn integer NOT NULL DEFAULT -1,
               kuota_non_jkn integer NOT NULL DEFAULT -1,
               pasien_baru tinyint NOT NULL DEFAULT 0 CHECK (pasien_baru IN (0, 1)),
               status_antri integer NOT NULL DEFAULT 0, -- 0:BOOKED, 1:CHECKED_IN, 2:SERVED, -1:CANCELLED, -2:NOSHOW
               booked_at datetime,    -- BOOKED
               checkin_at datetime,   -- CHECKED_IN
               served_at datetime,    -- SERVED
               cancelled_at datetime, -- CANCELLED
               created_by varchar(256) NOT NULL DEFAULT '',
               edited_by varchar(256) NOT NULL DEFAULT ''
            );
            ";

        public static string t_bl_antrian_status_history =
            @"
            CREATE TABLE bl_antrian_status_history (
                id integer PRIMARY KEY AUTOINCREMENT,
                antrian_id integer NOT NULL REFERENCES bl_antrian(id),
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
               nomor_urut_jadwal integer NOT NULL DEFAULT 0,
               pasien_time integer NOT NULL DEFAULT 0,
               profile_image varchar(200) NOT NULL DEFAULT ''
            );
            ";

        public static string t_bl_jadwal_bedah =
            @"
            CREATE TABLE bl_jadwal_bedah (
               nomor_jadwal varchar(20) PRIMARY KEY,
               tanggal_operasi date NOT NULL,
               nomor_registrasi varchar(20) NOT NULL,
               nomor_rekam_medis varchar(20) NOT NULL,
               waktu_operasi varchar(5) NOT NULL,
               kode_dokter_operator varchar(10) NOT NULL,
               terlaksana tinyint NOT NULL DEFAULT 0 CHECK (terlaksana IN (0, 1)),
               jenis_tindakan varchar(256) NOT NULL DEFAULT ''
            );
            ";

        public static string t_bl_jadwal =
            @"
            CREATE TABLE bl_jadwal (
               id integer PRIMARY KEY AUTOINCREMENT,
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
            );
            ";

        public static string t_bl_id_gen =
            @"
            CREATE TABLE bl_id_gen (
               kode_faskes varchar(20) PRIMARY KEY,
               current_number bigint NOT NULL DEFAULT 0,
               last_number bigint NOT NULL DEFAULT 0,
               gen_rule varchar(200) NOT NULL DEFAULT ''
            );
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
               last_reservation integer NOT NULL DEFAULT 0,
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
               created_at datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
               modified_at datetime NOT NULL DEFAULT CURRENT_TIMESTAMP
            );
            CREATE INDEX bl_pasien_tgllahir ON bl_pasien (tanggal_lahir);
            CREATE INDEX bl_pasien_namapas ON bl_pasien (nama);
            ";

        public static string t_bl_poli =
            @"
            CREATE TABLE bl_poli (
	            kode_poli varchar(10) PRIMARY KEY,
	            kode_poli_jkn varchar(10) NOT NULL DEFAULT '',
	            nama_poli varchar(256) NOT NULL
            );
            ";

        public static string t_mjkn_ambil_antrian =
            @"
            CREATE TABLE mjkn_ambil_antrian (
	            id integer PRIMARY KEY AUTOINCREMENT,
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
	            reservation_id integer NOT NULL DEFAULT 0,
	            created_at datetime NOT NULL DEFAULT CURRENT_TIMESTAMP
            );
            ";

        public static string t_mjkn_antrian_transaction =
            @"
            CREATE TABLE mjkn_antrian_transaction (
               id integer PRIMARY KEY AUTOINCREMENT,
               r_nomor_kartu varchar(13) NOT NULL DEFAULT '',
               r_nik varchar(16) NOT NULL DEFAULT '',
               r_nomor_hp varchar(16) NOT NULL DEFAULT '',
               r_kode_poli_jkn varchar(3) NOT NULL DEFAULT '',
               r_nomor_rm varchar(20) NOT NULL DEFAULT '',
               r_tanggal_periksa date,
               r_kode_dokter_jkn integer NOT NULL DEFAULT -1,
               r_jam_praktek varchar(11) NOT NULL DEFAULT '',
               r_jenis_kunjungan integer NOT NULL DEFAULT 0,
               r_nomor_referensi varchar(19) NOT NULL DEFAULT '',
               r_source varchar(20) NOT NULL DEFAULT '',
               s_username varchar(100) NOT NULL DEFAULT '',
               s_useredit varchar(100) NOT NULL DEFAULT '',
               s_pasien_baru tinyint NOT NULL DEFAULT 0 CHECK (s_pasien_baru IN (0, 1)),
               s_status integer NOT NULL DEFAULT 0,
               s_booked_at datetime,
               s_checkin_at datetime,
               s_served_at datetime,
               s_cancelled_at datetime,
               s_antri_id integer NOT NULL,
               s_jadwal_id integer NOT NULL,
               s_kodepoli_internal varchar(20) NOT NULL,
               s_kodokter_internal varchar(20) NOT NULL,
               s_nama_hari varchar(20) NOT NULL,
               s_nama_pasien varchar(256) NOT NULL DEFAULT '',
               s_alamat_pasien varchar(512) NOT NULL DEFAULT '',
               s_phone varchar(100) NOT NULL DEFAULT '',
               s_tanggal_lahir date,
               o_nomor_antrian varchar(20) NOT NULL DEFAULT '',
               o_angka_antrian integer NOT NULL DEFAULT -1,
               o_kode_booking varchar(12) NOT NULL DEFAULT '',
               o_nama_poli varchar(256) NOT NULL DEFAULT '',
               o_nama_dokter varchar(256) NOT NULL DEFAULT '',
               o_estimasi_dilayani integer NOT NULL DEFAULT -1,
               o_sisa_kuota_jkn integer NOT NULL DEFAULT -1,
               o_kuota_jkn integer NOT NULL DEFAULT -1,
               o_sisa_kuota_non_jkn integer NOT NULL DEFAULT -1,
               o_kuota_non_jkn integer NOT NULL DEFAULT -1,
               o_keterangan varchar(256) NOT NULL DEFAULT '',
               s_keterangan varchar(256) NOT NULL DEFAULT ''
            );
            ";

        public static string t_mjkn_dashboard_antrian =
            @"
            CREATE TABLE mjkn_dashboard_antrian (
	            id integer PRIMARY KEY AUTOINCREMENT,
	            kode_ppk varchar(20) NOT NULL,
	            nama_ppk varchar(200) NOT NULL,
	            kode_poli varchar(10) NOT NULL,
	            nama_poli varchar(200) NOT NULL,
	            tanggal datetime NOT NULL,
	            insert_date integer NOT NULL,
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
	            created_at datetime NOT NULL DEFAULT CURRENT_TIMESTAMP
            );
            ";

        public static string t_mjkn_dokter =
            @"
            CREATE TABLE mjkn_dokter(
	            id integer PRIMARY KEY AUTOINCREMENT,
	            kodedokter_internal varchar(10) NOT NULL,
	            kodedokter_jkn integer NOT NULL UNIQUE,
	            nama_dokter varchar(256) NOT NULL DEFAULT ''
            );
            ";

        public static string t_mjkn_jadwal_dokter =
            @"
            CREATE TABLE mjkn_jadwal_dokter (
	            id integer PRIMARY KEY AUTOINCREMENT,
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
	            libur tinyint NOT NULL DEFAULT 0 CHECK (libur IN (0, 1)),
	            kapasitas_pasien integer NOT NULL,
	            last_update datetime NULL,
	            last_update_status varchar(50) NOT NULL DEFAULT '',
	            last_update_code integer NULL
            );
            ";

        public static string t_mjkn_pasien =
            @"
            CREATE TABLE mjkn_pasien (
               id integer PRIMARY KEY AUTOINCREMENT,
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
               last_reservation integer NOT NULL DEFAULT 0,
               created_at datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
               finger_print tinyint NOT NULL DEFAULT 0 CHECK (finger_print IN (0, 1))
            );
            ";

        public static string t_mjkn_poli_sub =
            @"
            CREATE TABLE mjkn_poli_sub (
               id integer PRIMARY KEY AUTOINCREMENT,
               kode varchar(10) NOT NULL UNIQUE,
               nama varchar(256) NOT NULL,
               kode_poli varchar(10) NOT NULL,
               finger_print tinyint NOT NULL DEFAULT 0 CHECK (finger_print IN (0, 1))
            );
            ";

        public static string t_mjkn_poli =
            @"
            CREATE TABLE mjkn_poli (
	            id integer PRIMARY KEY AUTOINCREMENT,
	            kode varchar(10) NOT NULL UNIQUE,
	            nama varchar(256) NOT NULL
            );
            ";

        public static string t_mjkn_tasks =
            @"
            CREATE TABLE mjkn_tasks (
	            id integer PRIMARY KEY AUTOINCREMENT,
	            kode_booking varchar(12) NULL,
	            task_id integer NULL,
	            task_name varchar(200) NULL,
	            waktu_rs varchar(100) NULL,
               waktu varchar(100) NULL
            );
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
            string sql = @"
            SELECT 
              ( SELECT count(name) FROM sqlite_master 
                WHERE type='table' AND (name LIKE 'base_%' OR name LIKE 'bl_%' OR name LIKE 'mjkn_%') ) AS total_table

            , ( SELECT COUNT(name) FROM sqlite_master 
                WHERE type='view' AND name LIKE 'xx_yy_zz_%') AS total_view

            , ( SELECT COUNT(name) FROM sqlite_master 
                WHERE type = 'trigger' AND tbl_name = 'xx_yy_zz_' 
                AND name = 'xx_yy_zz_' ) AS total_trigger;
            ";

            return sql;
        }
    }
}