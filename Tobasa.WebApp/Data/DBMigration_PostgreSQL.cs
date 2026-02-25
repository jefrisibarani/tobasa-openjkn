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
using Tobasa.Entities;

namespace Tobasa
{
    internal class DBMigration_PostgreSQL
    {
        public DBMigration_PostgreSQL() { }

        public static string t_base_users =
            @"
            CREATE TABLE public.base_users (
                id             serial PRIMARY KEY,
                uuid           varchar(64)    NOT NULL UNIQUE,
                user_name      varchar(100)   NOT NULL UNIQUE,
                first_name     varchar(255)   NOT NULL,
                last_name      varchar(255)   NOT NULL,
                email          varchar(255)   NOT NULL,
                image          varchar(255)   NOT NULL DEFAULT '',
                enabled        boolean        NOT NULL DEFAULT true,
                password_salt  bytea          NOT NULL,
                password_hash  bytea          NOT NULL,
                allow_login    boolean        NOT NULL DEFAULT true,
                created        timestamp      NOT NULL DEFAULT now(),
                updated        timestamp      NOT NULL DEFAULT now(),
                expired        timestamp      NOT NULL DEFAULT (now() + '1 year'::interval),
                last_login     timestamp,
                unique_code    varchar(48)    NOT NULL DEFAULT '',
                birth_date     date           NOT NULL,
                phone          varchar(20)    NOT NULL DEFAULT '',
                gender         varchar(1)     NOT NULL DEFAULT '',
                address        varchar(512)   NOT NULL DEFAULT '',
                nik            varchar(16)    NOT NULL DEFAULT ''
            ) WITH ( OIDS = FALSE ) TABLESPACE pg_default;
            ";

        public static string t_base_roles =
            @"
            CREATE TABLE public.base_roles (
                id       serial PRIMARY KEY,
                name     varchar(50)    NOT NULL,
                alias    varchar(100)   NOT NULL DEFAULT '',
                enabled  boolean        NOT NULL DEFAULT true,
                created  timestamp      NOT NULL DEFAULT now(),
                updated  timestamp      NOT NULL DEFAULT now(),
                sysrole  boolean        NOT NULL DEFAULT false
            ) WITH (OIDS = FALSE) TABLESPACE pg_default;
            ";

        public static string t_base_sites =
            @"
            CREATE TABLE public.base_sites (
                id       serial PRIMARY KEY,
                code     varchar(7)    NOT NULL DEFAULT '',
                name     varchar(50)   NOT NULL DEFAULT '',
                address  varchar(512)  NOT NULL DEFAULT ''
            ) WITH (OIDS = FALSE) TABLESPACE pg_default;
            ";

        public static string t_base_user_role =
            @"
            CREATE TABLE public.base_user_role (
                id          serial   NOT NULL,
                user_id     integer  NOT NULL,
                role_id     integer  NOT NULL,
                is_primary  boolean  NOT NULL DEFAULT false,
                CONSTRAINT base_user_role_pkey PRIMARY KEY (user_id, role_id),
                CONSTRAINT base_user_role_role_id_fkey FOREIGN KEY (role_id)
                    REFERENCES public.base_roles (id) MATCH SIMPLE ON UPDATE CASCADE ON DELETE CASCADE,
                CONSTRAINT base_user_role_user_id_fkey FOREIGN KEY (user_id)
                    REFERENCES public.base_users (id) MATCH SIMPLE ON UPDATE CASCADE ON DELETE CASCADE
            ) WITH (OIDS = FALSE) TABLESPACE pg_default;
            ";

        public static string t_base_user_site =
            @"
            CREATE TABLE public.base_user_site (
                id          serial   NOT NULL,
                user_id     integer  NOT NULL,
                site_id     integer  NOT NULL,
                allow_login boolean  NOT NULL DEFAULT true,
                is_admin    boolean  NOT NULL DEFAULT false,
                CONSTRAINT base_user_site_pkey PRIMARY KEY (user_id, site_id),
                CONSTRAINT base_user_site_site_id_fkey FOREIGN KEY (site_id)
                    REFERENCES public.base_sites (id) MATCH SIMPLE 
                    ON UPDATE NO ACTION ON DELETE NO ACTION NOT VALID,
                CONSTRAINT base_user_site_user_id_fkey FOREIGN KEY (user_id)
                    REFERENCES public.base_users (id) MATCH SIMPLE
                    ON UPDATE CASCADE ON DELETE CASCADE
            ) WITH (OIDS = FALSE) TABLESPACE pg_default;
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
                pasien_baru boolean NOT NULL DEFAULT false,
                status_antri integer NOT NULL DEFAULT 0, -- 0:BOOKED, 1:CHECKED_IN, 2:SERVED, -1:CANCELLED, -2:NOSHOW
                booked_at timestamp,    -- BOOKED
                checkin_at timestamp,   -- CHECKED_IN
                served_at timestamp,    -- SERVED
                cancelled_at timestamp, -- CANCELLED
                created_by varchar(256) NOT NULL DEFAULT '',
                edited_by varchar(256) NOT NULL DEFAULT ''
            );
            ";

        public static string t_bl_antrian_status_history =
            @"
            CREATE TABLE bl_antrian_status_history (
                id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
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
                kapasitas_non_jkn int NOT NULL DEFAULT 0,
                kapasitas_jkn int NOT NULL DEFAULT 0,
                total_kapasitas int NOT NULL DEFAULT 0,
                nomor_urut_jadwal bigint NOT NULL DEFAULT 0,
                pasien_time int NOT NULL DEFAULT 0,
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
                terlaksana boolean NOT NULL DEFAULT false,
                jenis_tindakan varchar(256) NOT NULL DEFAULT ''
            );
            ";

        public static string t_bl_jadwal =
            @"
            CREATE TABLE bl_jadwal (
                id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                kode_jadwal varchar(20) NOT NULL UNIQUE,
                kode_dokter varchar(10) NOT NULL,
                kode_poli varchar(10) NOT NULL,
                nama_hari varchar(20) NOT NULL,
                jam_mulai varchar(5) NOT NULL,
                jam_selesai varchar(5) NOT NULL,
                keterangan varchar(256) NOT NULL DEFAULT '',
                tanggal date NOT NULL,
                libur boolean NOT NULL DEFAULT false,
                quota_non_jkn int NOT NULL DEFAULT 0,
                quota_jkn int NOT NULL DEFAULT 0,
                quota_total int NOT NULL DEFAULT 0,
                quota_jkn_used integer NOT NULL DEFAULT 0,
                quota_non_jkn_used integer NOT NULL DEFAULT 0,
                locked boolean NOT NULL DEFAULT false
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
                last_reservation bigint NOT NULL DEFAULT 0,
                finger_print boolean NOT NULL DEFAULT false,
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
                created_at timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
                modified_at timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP
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
                id bigint PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
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
                created_at timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP
            );
            ";

        public static string t_mjkn_antrian_transaction =
            @"
            CREATE TABLE mjkn_antrian_transaction (
                id bigint PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
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
                s_pasien_baru boolean NOT NULL DEFAULT false,
                s_status integer NOT NULL DEFAULT 0,
                s_booked_at timestamp,
                s_checkin_at timestamp,
                s_served_at timestamp,
                s_cancelled_at timestamp,
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
                o_angka_antrian integer NOT NULL DEFAULT -1,
                o_kode_booking varchar(12) NOT NULL DEFAULT '',
                o_nama_poli varchar(256) NOT NULL DEFAULT '',
                o_nama_dokter varchar(256) NOT NULL DEFAULT '',
                o_estimasi_dilayani bigint NOT NULL DEFAULT -1,
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
                id integer PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
                kode_ppk varchar(20) NOT NULL,
                nama_ppk varchar(200) NOT NULL,
                kode_poli varchar(10) NOT NULL,
                nama_poli varchar(200) NOT NULL,
                tanggal timestamp NOT NULL,
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
                created_at timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP
            );
            ";

        public static string t_mjkn_dokter =
            @"
            CREATE TABLE mjkn_dokter (
                id integer PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
                kodedokter_internal varchar(10) NOT NULL,
                kodedokter_jkn integer NOT NULL UNIQUE,
                nama_dokter varchar(256) NOT NULL DEFAULT ''
            );
            ";

        public static string t_mjkn_jadwal_dokter =
            @"
            CREATE TABLE mjkn_jadwal_dokter (
                id integer PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
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
                libur boolean NOT NULL DEFAULT false,
                kapasitas_pasien integer NOT NULL,
                last_update timestamp NULL,
                last_update_status varchar(50) NOT NULL DEFAULT '',
                last_update_code integer NULL
            );
            ";

        public static string t_mjkn_pasien =
            @"
            CREATE TABLE mjkn_pasien (
                id bigint PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
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
                created_at timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
                finger_print boolean NOT NULL DEFAULT false
            );
            ";

        public static string t_mjkn_poli_sub =
            @"
            CREATE TABLE mjkn_poli_sub (
                id integer PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
                kode varchar(10) NOT NULL UNIQUE,
                nama varchar(256) NOT NULL,
                kode_poli varchar(10) NOT NULL,
                finger_print boolean NOT NULL DEFAULT false
            );
            ";

        public static string t_mjkn_poli =
            @"
            CREATE TABLE mjkn_poli (
                id integer PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
                kode varchar(10) NOT NULL UNIQUE,
                nama varchar(256) NOT NULL
            );
            ";

        public static string t_mjkn_tasks =
            @"
            CREATE TABLE mjkn_tasks (
                id integer PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
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
            string sql = $@"
            SELECT
              ( SELECT COUNT(*) FROM information_schema.tables 
                WHERE table_type = 'BASE TABLE' AND (table_name LIKE 'base_%' OR table_name LIKE 'bl_%' OR table_name LIKE 'mjkn_%')
                AND table_schema NOT IN ('pg_catalog', 'information_schema')) AS total_table

            , ( SELECT COUNT(*) FROM information_schema.tables 
                WHERE table_type = 'VIEW' AND table_name LIKE 'xx_yy_zz_%' 
                AND table_schema NOT IN ('pg_catalog', 'information_schema')) AS total_view

            , ( SELECT COUNT(*) FROM information_schema.triggers  
                WHERE event_object_schema NOT IN ('pg_catalog', 'information_schema')
                AND TRIGGER_NAME = 'xx_yy_zz_' ) AS total_trigger;
            ";
            return sql;
        }
    }
}