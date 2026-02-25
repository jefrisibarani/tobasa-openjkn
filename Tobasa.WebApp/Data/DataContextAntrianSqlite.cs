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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Tobasa.Entities;

namespace Tobasa.Data
{
    public partial class DataContextAntrianSqlite : DataContextAntrian
    {
        public DataContextAntrianSqlite(IConfiguration configuration)
            : base(configuration)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connStr = Utils.GetConnectionString(_configuration);
                optionsBuilder.UseSqlite(connStr);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BaseRoles>(entity =>
            {
                entity.ToTable("base_roles");

                entity.HasIndex(e => e.Alias, "IX_base_roles_alias")
                    .IsUnique();

                entity.HasIndex(e => e.Name, "IX_base_roles_name")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Alias)
                    .HasColumnType("VARCHAR(100)")
                    .HasColumnName("alias");

                entity.Property(e => e.Created)
                    .HasColumnType("DATETIME")
                    .HasColumnName("created")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.Enabled)
                    .HasColumnType("TINYINT")
                    .HasColumnName("enabled");

                entity.Property(e => e.Name)
                    .HasColumnType("VARCHAR(50)")
                    .HasColumnName("name");

                entity.Property(e => e.Sysrole)
                    .HasColumnType("TINYINT")
                    .HasColumnName("sysrole");

                entity.Property(e => e.Updated)
                    .HasColumnType("DATETIME")
                    .HasColumnName("updated")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            modelBuilder.Entity<BaseSites>(entity =>
            {
                entity.ToTable("base_sites");

                entity.HasIndex(e => e.Code, "IX_base_sites_code")
                    .IsUnique();

                entity.HasIndex(e => e.Name, "IX_base_sites_name")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Address)
                    .HasColumnType("VARCHAR(512)")
                    .HasColumnName("address")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Code)
                    .HasColumnType("VARCHAR(7)")
                    .HasColumnName("code");

                entity.Property(e => e.Name)
                    .HasColumnType("VARCHAR(50)")
                    .HasColumnName("name");
            });

            modelBuilder.Entity<BaseUserRole>(entity =>
            {
                entity.ToTable("base_user_role");

                entity.HasIndex(e => new { e.UserId, e.RoleId }, "IX_base_user_role_user_id_role_id")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.IsPrimary)
                    .HasColumnType("TINYINT")
                    .HasColumnName("is_primary");

                entity.Property(e => e.RoleId).HasColumnName("role_id");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.BaseUserRole)
                    .HasForeignKey(d => d.RoleId);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.BaseUserRole)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<BaseUserSite>(entity =>
            {
                entity.ToTable("base_user_site");

                entity.HasIndex(e => new { e.UserId, e.SiteId }, "IX_base_user_site_user_id_site_id")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AllowLogin)
                    .HasColumnType("TINYINT")
                    .HasColumnName("allow_login")
                    .HasDefaultValueSql("1");

                entity.Property(e => e.IsAdmin)
                    .HasColumnType("TINYINT")
                    .HasColumnName("is_admin");

                entity.Property(e => e.SiteId).HasColumnName("site_id");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.Site)
                    .WithMany(p => p.BaseUserSite)
                    .HasForeignKey(d => d.SiteId);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.BaseUserSite)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<BaseUsers>(entity =>
            {
                entity.ToTable("base_users");

                entity.HasIndex(e => e.UserName, "IX_base_users_user_name")
                    .IsUnique();

                entity.HasIndex(e => e.Uuid, "IX_base_users_uuid")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Address)
                    .HasColumnType("VARCHAR(512)")
                    .HasColumnName("address")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.AllowLogin)
                    .HasColumnType("TINYINT")
                    .HasColumnName("allow_login")
                    .HasDefaultValueSql("1");

                entity.Property(e => e.BirthDate)
                    .HasColumnType("DATE")
                    .HasColumnName("birth_date");

                entity.Property(e => e.Created)
                    .HasColumnType("DATETIME")
                    .HasColumnName("created")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.Email)
                    .HasColumnType("VARCHAR(256)")
                    .HasColumnName("email");

                entity.Property(e => e.Enabled)
                    .HasColumnType("TINYINT")
                    .HasColumnName("enabled")
                    .HasDefaultValueSql("1");

                entity.Property(e => e.Expired)
                    .HasColumnType("DATETIME")
                    .HasColumnName("expired")
                    .HasDefaultValueSql("'2030-12-31 00:00:00'");

                entity.Property(e => e.FirstName)
                    .HasColumnType("VARCHAR(127)")
                    .HasColumnName("first_name");

                entity.Property(e => e.Gender)
                    .HasColumnType("VARCHAR(1)")
                    .HasColumnName("gender")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Image)
                    .HasColumnType("VARCHAR(256)")
                    .HasColumnName("image")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.LastLogin)
                    .HasColumnType("DATETIME")
                    .HasColumnName("last_login");

                entity.Property(e => e.LastName)
                    .HasColumnType("VARCHAR(127)")
                    .HasColumnName("last_name");

                entity.Property(e => e.Nik)
                    .HasColumnType("VARCHAR(16)")
                    .HasColumnName("nik")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.PasswordHash).HasColumnName("password_hash");

                entity.Property(e => e.PasswordSalt).HasColumnName("password_salt");

                entity.Property(e => e.Phone)
                    .HasColumnType("VARCHAR(20)")
                    .HasColumnName("phone")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.UniqueCode)
                    .HasColumnType("VARCHAR(48)")
                    .HasColumnName("unique_code")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Updated)
                    .HasColumnType("DATETIME")
                    .HasColumnName("updated")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.UserName)
                    .HasColumnType("VARCHAR(50)")
                    .HasColumnName("user_name");

                entity.Property(e => e.Uuid)
                    .HasColumnType("VARCHAR(64)")
                    .HasColumnName("uuid");
            });

            modelBuilder.Entity<BlAntrian>(entity =>
            {
                entity.ToTable("bl_antrian");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Alamat)
                    .HasColumnType("varchar(512)")
                    .HasColumnName("alamat")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.CheckinAt).HasColumnType("datetime").HasColumnName("checkin_At");

                entity.Property(e => e.CreatedBy)
                    .HasColumnType("varchar(256)")
                    .HasColumnName("created_by")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.EditedBy)
                    .HasColumnType("varchar(256)")
                    .HasColumnName("edited_by")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.EstimasiDilayani).HasColumnName("estimasi_dilayani");

                entity.Property(e => e.InsuranceId)
                    .HasColumnType("varchar(10)")
                    .HasColumnName("insurance_id")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.BookedAt).HasColumnType("datetime").HasColumnName("booked_at");
                entity.Property(e => e.ServedAt).HasColumnType("datetime").HasColumnName("served_at");
                entity.Property(e => e.CancelledAt).HasColumnType("datetime").HasColumnName("cancelled_at");

                entity.Property(e => e.JadwalId).HasColumnName("jadwal_id");

                entity.Property(e => e.JamMulai)
                    .HasColumnType("varchar(8)")
                    .HasColumnName("jam_mulai")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.JamPraktek)
                    .HasColumnType("varchar(11)")
                    .HasColumnName("jam_praktek")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.JenisKunjungan).HasColumnName("jenis_kunjungan");

                entity.Property(e => e.Keterangan)
                    .HasColumnType("varchar(256)")
                    .HasColumnName("keterangan")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Note)
                    .HasColumnType("varchar(256)")
                    .HasColumnName("note")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.KodeDokter)
                    .HasColumnType("varchar(10)")
                    .HasColumnName("kode_dokter")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.KodePoli)
                    .HasColumnType("varchar(10)")
                    .HasColumnName("kode_poli")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.KuotaJkn)
                    .HasColumnName("kuota_jkn")
                    .HasDefaultValueSql("-1");

                entity.Property(e => e.KuotaNonJkn)
                    .HasColumnName("kuota_non_jkn")
                    .HasDefaultValueSql("-1");

                entity.Property(e => e.NamaHari)
                    .HasColumnType("varchar(20)")
                    .HasColumnName("nama_hari")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.NamaPasien)
                    .HasColumnType("varchar(256)")
                    .HasColumnName("nama_pasien")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.NomorAntrian)
                    .HasColumnType("varchar(20)")
                    .HasColumnName("nomor_antrian")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.NomorAntrianInt).HasColumnName("nomor_antrian_int");

                entity.Property(e => e.NomorKartuJkn)
                    .HasColumnType("varchar(13)")
                    .HasColumnName("nomor_kartu_jkn")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.NomorRegistrasi)
                    .HasColumnType("varchar(20)")
                    .HasColumnName("nomor_registrasi")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.NomorRekamMedis)
                    .HasColumnType("varchar(10)")
                    .HasColumnName("nomor_rekam_medis")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.NomorRujukan)
                    .HasColumnType("varchar(20)")
                    .HasColumnName("nomor_rujukan")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.PasienBaru)
                    .HasColumnType("tinyint")
                    .HasColumnName("pasien_baru");

                entity.Property(e => e.Phone)
                    .HasColumnType("varchar(16)")
                    .HasColumnName("phone")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.SisaKuotaJkn)
                    .HasColumnName("sisa_kuota_jkn")
                    .HasDefaultValueSql("-1");

                entity.Property(e => e.SisaKuotaNonJkn)
                    .HasColumnName("sisa_kuota_non_jkn")
                    .HasDefaultValueSql("-1");

                entity.Property(e => e.StatusAntri).HasColumnName("status_antri");

                entity.Property(e => e.Tanggal)
                    .HasColumnType("date")
                    .HasColumnName("tanggal");

                entity.Property(e => e.TanggalLahir)
                    .HasColumnType("date")
                    .HasColumnName("tanggal_lahir");

                entity.Property(e => e.TokenAntrian)
                    .HasColumnType("varchar(12)")
                    .HasColumnName("token_antrian")
                    .HasDefaultValueSql("''");
            });

            modelBuilder.Entity<BlAntrianStatusHistory>(entity =>
            {
                entity.ToTable("bl_antrian_status_history");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AntrianId).HasColumnName("antrian_id");

                entity.Property(e => e.Status)
                    .HasMaxLength(20)
                    .HasColumnName("status")
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.ChangedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("changed_at");

                entity.Property(e => e.ChangedBy)
                    .HasMaxLength(50)
                    .HasColumnName("changed_by")
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.Note)
                    .HasMaxLength(256)
                    .HasColumnName("note")
                    .HasDefaultValueSql("('')");
            });

            modelBuilder.Entity<BlDokter>(entity =>
            {
                entity.HasKey(e => e.KodeDokter);

                entity.ToTable("bl_dokter");

                entity.Property(e => e.KodeDokter)
                    .HasColumnType("varchar(10)")
                    .HasColumnName("kode_dokter");

                entity.Property(e => e.KodeDokterJkn).HasColumnName("kode_dokter_jkn");

                entity.Property(e => e.KapasitasJkn).HasColumnName("kapasitas_jkn");

                entity.Property(e => e.KapasitasNonJkn).HasColumnName("kapasitas_non_jkn");

                entity.Property(e => e.KodePoli)
                    .HasColumnType("varchar(10)")
                    .HasColumnName("kode_poli")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.NamaDokter)
                    .HasColumnType("varchar(256)")
                    .HasColumnName("nama_dokter");

                entity.Property(e => e.NomorUrutJadwal).HasColumnName("nomor_urut_jadwal");

                entity.Property(e => e.PasienTime).HasColumnName("pasien_time");

                entity.Property(e => e.ProfileImage)
                    .HasColumnType("varchar(200)")
                    .HasColumnName("profile_image")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.TotalKapasitas).HasColumnName("total_kapasitas");
            });

            modelBuilder.Entity<BlIdGen>(entity =>
            {
                entity.HasKey(e => e.KodeFaskes);

                entity.ToTable("bl_id_gen");

                entity.Property(e => e.KodeFaskes)
                    .HasColumnType("varchar(20)")
                    .HasColumnName("kode_faskes");

                entity.Property(e => e.CurrentNumber)
                    .HasColumnType("bigint")
                    .HasColumnName("current_number");

                entity.Property(e => e.GenRule)
                    .HasColumnType("varchar(200)")
                    .HasColumnName("gen_rule")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.LastNumber)
                    .HasColumnType("bigint")
                    .HasColumnName("last_number");
            });

            modelBuilder.Entity<BlJadwal>(entity =>
            {
                entity.ToTable("bl_jadwal");

                entity.HasIndex(e => e.KodeJadwal, "IX_bl_jadwal_kode_jadwal")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.JamMulai)
                    .HasColumnType("varchar(5)")
                    .HasColumnName("jam_mulai");

                entity.Property(e => e.JamSelesai)
                    .HasColumnType("varchar(5)")
                    .HasColumnName("jam_selesai");

                entity.Property(e => e.QuotaJkn).HasColumnName("quota_jkn");
                entity.Property(e => e.QuotaNonJkn).HasColumnName("quota_non_jkn");
                entity.Property(e => e.QuotaTotal).HasColumnName("quota_total");
                entity.Property(e => e.QuotaJknUsed).HasColumnName("quota_jkn_used");
                entity.Property(e => e.QuotaNonJknUsed).HasColumnName("quota_non_jkn_used");
                entity.Property(e => e.Locked)
                    .HasColumnType("tinyint")
                    .HasColumnName("locked");

                entity.Property(e => e.Keterangan)
                    .HasColumnType("varchar(256)")
                    .HasColumnName("keterangan")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.KodeDokter)
                    .HasColumnType("varchar(10)")
                    .HasColumnName("kode_dokter");

                entity.Property(e => e.KodeJadwal)
                    .HasColumnType("varchar(20)")
                    .HasColumnName("kode_jadwal");

                entity.Property(e => e.KodePoli)
                    .HasColumnType("varchar(10)")
                    .HasColumnName("kode_poli");

                entity.Property(e => e.Libur)
                    .HasColumnType("tinyint")
                    .HasColumnName("libur");

                entity.Property(e => e.NamaHari)
                    .HasColumnType("varchar(20)")
                    .HasColumnName("nama_hari");

                entity.Property(e => e.Tanggal)
                    .HasColumnType("date")
                    .HasColumnName("tanggal");
            });

            modelBuilder.Entity<BlJadwalBedah>(entity =>
            {
                entity.HasKey(e => e.NomorJadwal);

                entity.ToTable("bl_jadwal_bedah");

                entity.Property(e => e.NomorJadwal)
                    .HasColumnType("varchar(20)")
                    .HasColumnName("nomor_jadwal");

                entity.Property(e => e.JenisTindakan)
                    .HasColumnType("varchar(256)")
                    .HasColumnName("jenis_tindakan")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.KodeDokterOperator)
                    .HasColumnType("varchar(10)")
                    .HasColumnName("kode_dokter_operator");

                entity.Property(e => e.NomorRegistrasi)
                    .HasColumnType("varchar(20)")
                    .HasColumnName("nomor_registrasi");

                entity.Property(e => e.NomorRekamMedis)
                    .HasColumnType("varchar(20)")
                    .HasColumnName("nomor_rekam_medis");

                entity.Property(e => e.TanggalOperasi)
                    .HasColumnType("date")
                    .HasColumnName("tanggal_operasi");

                entity.Property(e => e.Terlaksana)
                    .HasColumnType("tinyint")
                    .HasColumnName("terlaksana");

                entity.Property(e => e.WaktuOperasi)
                    .HasColumnType("varchar(5)")
                    .HasColumnName("waktu_operasi");
            });

            modelBuilder.Entity<BlPasien>(entity =>
            {
                entity.HasKey(e => e.NomorRekamMedis);

                entity.ToTable("bl_pasien");

                entity.HasIndex(e => e.Nama, "bl_pasien_namapas");

                entity.HasIndex(e => e.TanggalLahir, "bl_pasien_tgllahir");

                entity.Property(e => e.NomorRekamMedis)
                    .HasColumnType("varchar(20)")
                    .HasColumnName("nomor_rekam_medis");

                entity.Property(e => e.Alamat)
                    .HasColumnType("varchar(512)")
                    .HasColumnName("alamat")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.FingerPrint)
                    .HasColumnType("tinyint")
                    .HasColumnName("finger_print");

                entity.Property(e => e.Gender)
                    .HasColumnType("varchar(1)")
                    .HasColumnName("gender");

                entity.Property(e => e.Honorific)
                    .HasColumnType("varchar(20)")
                    .HasColumnName("honorific")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.InsuranceId)
                    .HasColumnType("varchar(10)")
                    .HasColumnName("insurance_id")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.KodeDati2)
                    .HasColumnType("varchar(10)")
                    .HasColumnName("kode_dati2")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.KodeKecamatan)
                    .HasColumnType("varchar(256)")
                    .HasColumnName("kode_kecamatan")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.KodeKelurahan)
                    .HasColumnType("varchar(20)")
                    .HasColumnName("kode_kelurahan")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.KodePropinsi)
                    .HasColumnType("varchar(10)")
                    .HasColumnName("kode_propinsi")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.LastReservation)
                    .HasColumnName("last_reservation");

                entity.Property(e => e.ModifiedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("modified_at")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.Nama)
                    .HasColumnType("varchar(256)")
                    .HasColumnName("nama")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.NamaDati2)
                    .HasColumnType("varchar(256)")
                    .HasColumnName("nama_dati2")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.NamaKecamatan)
                    .HasColumnType("varchar(256)")
                    .HasColumnName("nama_kecamatan")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.NamaKelurahan)
                    .HasColumnType("varchar(256)")
                    .HasColumnName("nama_kelurahan")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.NamaPropinsi)
                    .HasColumnType("varchar(256)")
                    .HasColumnName("nama_propinsi")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.NomorIdentitas)
                    .HasColumnType("varchar(16)")
                    .HasColumnName("nomor_identitas")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.NomorKartuJkn)
                    .HasColumnType("varchar(13)")
                    .HasColumnName("nomor_kartu_jkn")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.NomorKartuKeluarga)
                    .HasColumnType("varchar(16)")
                    .HasColumnName("nomor_kartu_keluarga")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Phone)
                    .HasColumnType("varchar(16)")
                    .HasColumnName("phone")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Rt)
                    .HasColumnType("varchar(3)")
                    .HasColumnName("rt")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Rw)
                    .HasColumnType("varchar(3)")
                    .HasColumnName("rw")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.TanggalLahir)
                    .HasColumnType("date")
                    .HasColumnName("tanggal_lahir");
            });

            modelBuilder.Entity<BlPoli>(entity =>
            {
                entity.HasKey(e => e.KodePoli);

                entity.ToTable("bl_poli");

                entity.Property(e => e.KodePoli)
                    .HasColumnType("varchar(10)")
                    .HasColumnName("kode_poli");

                entity.Property(e => e.KodePoliJkn)
                    .HasColumnType("varchar(10)")
                    .HasColumnName("kode_poli_jkn")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.NamaPoli)
                    .HasColumnType("varchar(256)")
                    .HasColumnName("nama_poli");
            });

            modelBuilder.Entity<MjknAmbilAntrian>(entity =>
            {
                entity.ToTable("mjkn_ambil_antrian");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.JamPraktek)
                    .HasColumnType("varchar(11)")
                    .HasColumnName("jam_praktek");

                entity.Property(e => e.JenisKunjungan).HasColumnName("jenis_kunjungan");

                entity.Property(e => e.KodeDokter).HasColumnName("kode_dokter");

                entity.Property(e => e.KodePoli)
                    .HasColumnType("varchar(10)")
                    .HasColumnName("kode_poli");

                entity.Property(e => e.Nik)
                    .HasColumnType("varchar(16)")
                    .HasColumnName("nik");

                entity.Property(e => e.NomorHp)
                    .HasColumnType("varchar(16)")
                    .HasColumnName("nomor_hp");

                entity.Property(e => e.NomorKartu)
                    .HasColumnType("varchar(13)")
                    .HasColumnName("nomor_kartu");

                entity.Property(e => e.NomorReferensi)
                    .HasColumnType("varchar(19)")
                    .HasColumnName("nomor_referensi");

                entity.Property(e => e.NomorRm)
                    .HasColumnType("varchar(20)")
                    .HasColumnName("nomor_rm");

                entity.Property(e => e.ReservationId).HasColumnName("reservation_id");

                entity.Property(e => e.TanggalPeriksa)
                    .HasColumnType("date")
                    .HasColumnName("tanggal_periksa");
            });

            modelBuilder.Entity<MjknAntrianTransaction>(entity =>
            {
                entity.ToTable("mjkn_antrian_transaction");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.OAngkaAntrian)
                    .HasColumnName("o_angka_antrian")
                    .HasDefaultValueSql("-1");

                entity.Property(e => e.OEstimasiDilayani)
                    .HasColumnName("o_estimasi_dilayani")
                    .HasDefaultValueSql("-1");

                entity.Property(e => e.OKeterangan)
                    .HasColumnType("varchar(256)")
                    .HasColumnName("o_keterangan")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.OKodeBooking)
                    .HasColumnType("varchar(12)")
                    .HasColumnName("o_kode_booking")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.OKuotaJkn)
                    .HasColumnName("o_kuota_jkn")
                    .HasDefaultValueSql("-1");

                entity.Property(e => e.OKuotaNonJkn)
                    .HasColumnName("o_kuota_non_jkn")
                    .HasDefaultValueSql("-1");

                entity.Property(e => e.ONamaDokter)
                    .HasColumnType("varchar(256)")
                    .HasColumnName("o_nama_dokter")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.ONamaPoli)
                    .HasColumnType("varchar(256)")
                    .HasColumnName("o_nama_poli")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.ONomorAntrian)
                    .HasColumnType("varchar(20)")
                    .HasColumnName("o_nomor_antrian")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.OSisaKuotaJkn)
                    .HasColumnName("o_sisa_kuota_jkn")
                    .HasDefaultValueSql("-1");

                entity.Property(e => e.OSisaKuotaNonJkn)
                    .HasColumnName("o_sisa_kuota_non_jkn")
                    .HasDefaultValueSql("-1");

                entity.Property(e => e.RJamPraktek)
                    .HasColumnType("varchar(11)")
                    .HasColumnName("r_jam_praktek")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.RJenisKunjungan).HasColumnName("r_jenis_kunjungan");

                entity.Property(e => e.RKodeDokterJkn)
                    .HasColumnName("r_kode_dokter_jkn")
                    .HasDefaultValueSql("-1");

                entity.Property(e => e.RKodePoliJkn)
                    .HasColumnType("varchar(3)")
                    .HasColumnName("r_kode_poli_jkn")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.RNik)
                    .HasColumnType("varchar(16)")
                    .HasColumnName("r_nik")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.RNomorHp)
                    .HasColumnType("varchar(16)")
                    .HasColumnName("r_nomor_hp")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.RNomorKartu)
                    .HasColumnType("varchar(13)")
                    .HasColumnName("r_nomor_kartu")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.RNomorReferensi)
                    .HasColumnType("varchar(19)")
                    .HasColumnName("r_nomor_referensi")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.RNomorRm)
                    .HasColumnType("varchar(20)")
                    .HasColumnName("r_nomor_rm")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.RSource)
                    .HasColumnType("varchar(20)")
                    .HasColumnName("r_source")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.RTanggalPeriksa)
                    .HasColumnType("date")
                    .HasColumnName("r_tanggal_periksa");

                entity.Property(e => e.SAlamatPasien)
                    .HasColumnType("varchar(512)")
                    .HasColumnName("s_alamat_pasien")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.SAntriId)
                    .HasColumnType("varchar(20)")
                    .HasColumnName("s_antri_id");

                entity.Property(e => e.SCancelledAt)
                    .HasColumnType("datetime")
                    .HasColumnName("s_cancelled_at");

                entity.Property(e => e.SCheckinAt)
                    .HasColumnType("datetime")
                    .HasColumnName("s_checkin_at");

                entity.Property(e => e.SBookedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("s_booked_at");

                entity.Property(e => e.SServedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("s_served_at");

                entity.Property(e => e.SJadwalId).HasColumnName("s_jadwal_id");

                entity.Property(e => e.SKeterangan)
                    .HasColumnType("varchar(256)")
                    .HasColumnName("s_keterangan")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.SKodepoliInternal)
                    .HasColumnType("varchar(20)")
                    .HasColumnName("s_kodepoli_internal");

                entity.Property(e => e.SKodokterInternal)
                    .HasColumnType("varchar(20)")
                    .HasColumnName("s_kodokter_internal");

                entity.Property(e => e.SNamaHari)
                    .HasColumnType("varchar(20)")
                    .HasColumnName("s_nama_hari");

                entity.Property(e => e.SNamaPasien)
                    .HasColumnType("varchar(256)")
                    .HasColumnName("s_nama_pasien")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.SPasienBaru)
                    .HasColumnType("tinyint")
                    .HasColumnName("s_pasien_baru");

                entity.Property(e => e.SPhone)
                    .HasColumnType("varchar(100)")
                    .HasColumnName("s_phone")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.SStatus).HasColumnName("s_status");

                entity.Property(e => e.STanggalLahir)
                    .HasColumnType("date")
                    .HasColumnName("s_tanggal_lahir");

                entity.Property(e => e.SUseredit)
                    .HasColumnType("varchar(100)")
                    .HasColumnName("s_useredit")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.SUsername)
                    .HasColumnType("varchar(100)")
                    .HasColumnName("s_username")
                    .HasDefaultValueSql("''");
            });

            modelBuilder.Entity<MjknDokter>(entity =>
            {
                entity.ToTable("mjkn_dokter");

                entity.HasIndex(e => e.KodedokterJkn, "IX_mjkn_dokter_kodedokter_jkn")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.KodedokterInternal)
                    .HasColumnType("varchar(10)")
                    .HasColumnName("kodedokter_internal");

                entity.Property(e => e.KodedokterJkn).HasColumnName("kodedokter_jkn");

                entity.Property(e => e.NamaDokter)
                    .HasColumnType("varchar(256)")
                    .HasColumnName("nama_dokter")
                    .HasDefaultValueSql("''");
            });

            modelBuilder.Entity<MjknJadwalDokter>(entity =>
            {
                entity.ToTable("mjkn_jadwal_dokter");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Hari).HasColumnName("hari");

                entity.Property(e => e.Jadwal)
                    .HasColumnType("varchar(20)")
                    .HasColumnName("jadwal");

                entity.Property(e => e.JamMulai)
                    .HasColumnType("varchar(5)")
                    .HasColumnName("jam_mulai");

                entity.Property(e => e.JamTutup)
                    .HasColumnType("varchar(5)")
                    .HasColumnName("jam_tutup");

                entity.Property(e => e.KapasitasPasien).HasColumnName("kapasitas_pasien");

                entity.Property(e => e.KodePoli)
                    .HasColumnType("varchar(10)")
                    .HasColumnName("kode_poli");

                entity.Property(e => e.KodeSubspesialis)
                    .HasColumnType("varchar(10)")
                    .HasColumnName("kode_subspesialis");

                entity.Property(e => e.KodedokterInternal)
                    .HasColumnType("varchar(10)")
                    .HasColumnName("kodedokter_internal")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.KodedokterJkn).HasColumnName("kodedokter_jkn");

                entity.Property(e => e.LastUpdate)
                    .HasColumnType("datetime")
                    .HasColumnName("last_update");

                entity.Property(e => e.LastUpdateCode).HasColumnName("last_update_code");

                entity.Property(e => e.LastUpdateStatus)
                    .HasColumnType("varchar(50)")
                    .HasColumnName("last_update_status")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Libur)
                    .HasColumnType("tinyint")
                    .HasColumnName("libur");

                entity.Property(e => e.NamaDokter)
                    .HasColumnType("varchar(256)")
                    .HasColumnName("nama_dokter");

                entity.Property(e => e.NamaHari)
                    .HasColumnType("varchar(20)")
                    .HasColumnName("nama_hari");

                entity.Property(e => e.NamaPoli)
                    .HasColumnType("varchar(256)")
                    .HasColumnName("nama_poli");

                entity.Property(e => e.NamaSubspesialis)
                    .HasColumnType("varchar(256)")
                    .HasColumnName("nama_subspesialis");
            });

            modelBuilder.Entity<MjknPasien>(entity =>
            {
                entity.ToTable("mjkn_pasien");

                entity.HasIndex(e => e.NomorKartu, "IX_mjkn_pasien_nomor_kartu")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Alamat)
                    .HasColumnType("varchar(512)")
                    .HasColumnName("alamat")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.FingerPrint)
                    .HasColumnType("tinyint")
                    .HasColumnName("finger_print");

                entity.Property(e => e.JenisKelamin)
                    .HasColumnType("varchar(1)")
                    .HasColumnName("jenis_kelamin");

                entity.Property(e => e.KodeDati2)
                    .HasColumnType("varchar(10)")
                    .HasColumnName("kode_dati2")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.KodeKecamatan)
                    .HasColumnType("varchar(256)")
                    .HasColumnName("kode_kecamatan")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.KodeKelurahan)
                    .HasColumnType("varchar(20)")
                    .HasColumnName("kode_kelurahan")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.KodePropinsi)
                    .HasColumnType("varchar(10)")
                    .HasColumnName("kode_propinsi")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.LastReservation)
                    .HasColumnName("last_reservation");

                entity.Property(e => e.Nama)
                    .HasColumnType("varchar(256)")
                    .HasColumnName("nama");

                entity.Property(e => e.NamaDati2)
                    .HasColumnType("varchar(256)")
                    .HasColumnName("nama_dati2")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.NamaKecamatan)
                    .HasColumnType("varchar(256)")
                    .HasColumnName("nama_kecamatan")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.NamaKelurahan)
                    .HasColumnType("varchar(256)")
                    .HasColumnName("nama_kelurahan")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.NamaPropinsi)
                    .HasColumnType("varchar(256)")
                    .HasColumnName("nama_propinsi")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Nik)
                    .HasColumnType("varchar(16)")
                    .HasColumnName("nik");

                entity.Property(e => e.NomorHp)
                    .HasColumnType("varchar(16)")
                    .HasColumnName("nomor_hp")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.NomorKartu)
                    .HasColumnType("varchar(13)")
                    .HasColumnName("nomor_kartu");

                entity.Property(e => e.NomorKk)
                    .HasColumnType("varchar(16)")
                    .HasColumnName("nomor_kk");

                entity.Property(e => e.NomorRm)
                    .HasColumnType("varchar(20)")
                    .HasColumnName("nomor_rm");

                entity.Property(e => e.Rt)
                    .HasColumnType("varchar(3)")
                    .HasColumnName("rt")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Rw)
                    .HasColumnType("varchar(3)")
                    .HasColumnName("rw")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.TanggalLahir)
                    .HasColumnType("date")
                    .HasColumnName("tanggal_lahir");
            });

            modelBuilder.Entity<MjknPoli>(entity =>
            {
                entity.ToTable("mjkn_poli");

                entity.HasIndex(e => e.Kode, "IX_mjkn_poli_kode")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Kode)
                    .HasColumnType("varchar(10)")
                    .HasColumnName("kode");

                entity.Property(e => e.Nama)
                    .HasColumnType("varchar(256)")
                    .HasColumnName("nama");
            });

            modelBuilder.Entity<MjknPoliSub>(entity =>
            {
                entity.ToTable("mjkn_poli_sub");

                entity.HasIndex(e => e.Kode, "IX_mjkn_poli_sub_kode")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.FingerPrint)
                    .HasColumnType("tinyint")
                    .HasColumnName("finger_print");

                entity.Property(e => e.Kode)
                    .HasColumnType("varchar(10)")
                    .HasColumnName("kode");

                entity.Property(e => e.KodePoli)
                    .HasColumnType("varchar(10)")
                    .HasColumnName("kode_poli");

                entity.Property(e => e.Nama)
                    .HasColumnType("varchar(256)")
                    .HasColumnName("nama");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}