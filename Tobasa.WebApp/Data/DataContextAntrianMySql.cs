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
    public partial class DataContextAntrianMySql : DataContextAntrian
    {
        public DataContextAntrianMySql(IConfiguration configuration)
            : base(configuration)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connStr = Utils.GetConnectionString(_configuration);
                optionsBuilder.UseMySql(connStr, ServerVersion.AutoDetect(connStr));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("utf8mb4_general_ci")
                .HasCharSet("utf8mb4");

            modelBuilder.Entity<BaseRoles>(entity =>
            {
                entity.ToTable("base_roles");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Alias)
                    .HasMaxLength(100)
                    .HasColumnName("alias");

                entity.Property(e => e.Created)
                    .HasColumnType("datetime")
                    .HasColumnName("created")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.Enabled).HasColumnName("enabled");

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.Sysrole).HasColumnName("sysrole");

                entity.Property(e => e.Updated)
                    .HasColumnType("datetime")
                    .HasColumnName("updated")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            modelBuilder.Entity<BaseSites>(entity =>
            {
                entity.ToTable("base_sites");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Address)
                    .HasMaxLength(512)
                    .HasColumnName("address")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Code)
                    .HasMaxLength(7)
                    .HasColumnName("code")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .HasColumnName("name")
                    .HasDefaultValueSql("''");
            });

            modelBuilder.Entity<BaseUserRole>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                entity.ToTable("base_user_role");

                entity.HasIndex(e => e.RoleId, "FK_base_user_role_base_roles");

                entity.HasIndex(e => e.Id, "id")
                    .IsUnique();

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.RoleId).HasColumnName("role_id");

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("id");

                entity.Property(e => e.IsPrimary).HasColumnName("is_primary");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.BaseUserRole)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_base_user_role_base_roles");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.BaseUserRole)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_base_user_role_base_users");
            });

            modelBuilder.Entity<BaseUserSite>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.SiteId })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                entity.ToTable("base_user_site");

                entity.HasIndex(e => e.SiteId, "FK_base_user_site_base_sites");

                entity.HasIndex(e => e.Id, "id")
                    .IsUnique();

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.SiteId).HasColumnName("site_id");

                entity.Property(e => e.AllowLogin)
                    .HasColumnName("allow_login")
                    .HasDefaultValueSql("'1'");

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("id");

                entity.Property(e => e.IsAdmin).HasColumnName("is_admin");

                entity.HasOne(d => d.Site)
                    .WithMany(p => p.BaseUserSite)
                    .HasForeignKey(d => d.SiteId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_base_user_site_base_sites");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.BaseUserSite)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_base_user_site_base_users");
            });

            modelBuilder.Entity<BaseUsers>(entity =>
            {
                entity.ToTable("base_users");

                entity.HasIndex(e => e.UserName, "user_name")
                    .IsUnique();

                entity.HasIndex(e => e.Uuid, "uuid")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Address)
                    .HasMaxLength(512)
                    .HasColumnName("address")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.AllowLogin)
                    .HasColumnName("allow_login")
                    .HasDefaultValueSql("'1'");

                entity.Property(e => e.BirthDate).HasColumnName("birth_date");

                entity.Property(e => e.Created)
                    .HasColumnType("datetime")
                    .HasColumnName("created")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.Email)
                    .HasMaxLength(255)
                    .HasColumnName("email");

                entity.Property(e => e.Enabled)
                    .HasColumnName("enabled")
                    .HasDefaultValueSql("'1'");

                entity.Property(e => e.Expired)
                    .HasColumnType("datetime")
                    .HasColumnName("expired")
                    .HasDefaultValueSql("'2030-12-31 01:01:01'");

                entity.Property(e => e.FirstName)
                    .HasMaxLength(255)
                    .HasColumnName("first_name");

                entity.Property(e => e.Gender)
                    .HasMaxLength(1)
                    .HasColumnName("gender")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Image)
                    .HasMaxLength(255)
                    .HasColumnName("image")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.LastLogin)
                    .HasColumnType("datetime")
                    .HasColumnName("last_login");

                entity.Property(e => e.LastName)
                    .HasMaxLength(255)
                    .HasColumnName("last_name");

                entity.Property(e => e.Nik)
                    .HasMaxLength(16)
                    .HasColumnName("nik")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.PasswordHash)
                    .HasMaxLength(512)
                    .HasColumnName("password_hash");

                entity.Property(e => e.PasswordSalt)
                    .HasMaxLength(512)
                    .HasColumnName("password_salt");

                entity.Property(e => e.Phone)
                    .HasMaxLength(20)
                    .HasColumnName("phone")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.UniqueCode)
                    .HasMaxLength(48)
                    .HasColumnName("unique_code")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Updated)
                    .HasColumnType("datetime")
                    .HasColumnName("updated")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.UserName)
                    .HasMaxLength(100)
                    .HasColumnName("user_name");

                entity.Property(e => e.Uuid)
                    .HasMaxLength(64)
                    .HasColumnName("uuid");
            });

            modelBuilder.Entity<BlAntrian>(entity =>
            {
                entity.ToTable("bl_antrian");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Alamat)
                    .HasMaxLength(512)
                    .HasColumnName("alamat")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.CheckinAt).HasColumnType("datetime").HasColumnName("checkin_at");

                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(256)
                    .HasColumnName("created_by")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.EditedBy)
                    .HasMaxLength(256)
                    .HasColumnName("edited_by")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.EstimasiDilayani).HasColumnName("estimasi_dilayani");

                entity.Property(e => e.InsuranceId)
                    .HasMaxLength(10)
                    .HasColumnName("insurance_id")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.BookedAt).HasColumnType("datetime").HasColumnName("booked_at");
                entity.Property(e => e.ServedAt).HasColumnType("datetime").HasColumnName("served_at");
                entity.Property(e => e.CancelledAt).HasColumnType("datetime").HasColumnName("cancelled_at");

                entity.Property(e => e.JadwalId).HasColumnName("jadwal_id");

                entity.Property(e => e.JamMulai)
                    .HasMaxLength(8)
                    .HasColumnName("jam_mulai")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.JamPraktek)
                    .HasMaxLength(11)
                    .HasColumnName("jam_praktek")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.JenisKunjungan).HasColumnName("jenis_kunjungan");

                entity.Property(e => e.Keterangan)
                    .HasMaxLength(256)
                    .HasColumnName("keterangan")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Note)
                    .HasMaxLength(256)
                    .HasColumnName("note")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.KodeDokter)
                    .HasMaxLength(10)
                    .HasColumnName("kode_dokter")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.KodePoli)
                    .HasMaxLength(10)
                    .HasColumnName("kode_poli")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.KuotaJkn)
                    .HasColumnName("kuota_jkn")
                    .HasDefaultValueSql("'-1'");

                entity.Property(e => e.KuotaNonJkn)
                    .HasColumnName("kuota_non_jkn")
                    .HasDefaultValueSql("'-1'");

                entity.Property(e => e.NamaHari)
                    .HasMaxLength(20)
                    .HasColumnName("nama_hari")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.NamaPasien)
                    .HasMaxLength(256)
                    .HasColumnName("nama_pasien")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.NomorAntrian)
                    .HasMaxLength(20)
                    .HasColumnName("nomor_antrian")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.NomorAntrianInt).HasColumnName("nomor_antrian_int");

                entity.Property(e => e.NomorKartuJkn)
                    .HasMaxLength(13)
                    .HasColumnName("nomor_kartu_jkn")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.NomorRegistrasi)
                    .HasMaxLength(20)
                    .HasColumnName("nomor_registrasi")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.NomorRekamMedis)
                    .HasMaxLength(20)
                    .HasColumnName("nomor_rekam_medis")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.NomorRujukan)
                    .HasMaxLength(20)
                    .HasColumnName("nomor_rujukan")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.PasienBaru).HasColumnName("pasien_baru");

                entity.Property(e => e.Phone)
                    .HasMaxLength(16)
                    .HasColumnName("phone")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.SisaKuotaJkn)
                    .HasColumnName("sisa_kuota_jkn")
                    .HasDefaultValueSql("'-1'");

                entity.Property(e => e.SisaKuotaNonJkn)
                    .HasColumnName("sisa_kuota_non_jkn")
                    .HasDefaultValueSql("'-1'");

                entity.Property(e => e.StatusAntri).HasColumnName("status_antri");

                entity.Property(e => e.Tanggal).HasColumnName("tanggal");

                entity.Property(e => e.TanggalLahir).HasColumnName("tanggal_lahir");

                entity.Property(e => e.TokenAntrian)
                    .HasMaxLength(12)
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
                entity.HasKey(e => e.KodeDokter)
                    .HasName("PRIMARY");

                entity.ToTable("bl_dokter");

                entity.Property(e => e.KodeDokter)
                    .HasMaxLength(10)
                    .HasColumnName("kode_dokter");

                entity.Property(e => e.KodeDokterJkn).HasColumnName("kode_dokter_jkn");

                entity.Property(e => e.KapasitasJkn).HasColumnName("kapasitas_jkn");

                entity.Property(e => e.KapasitasNonJkn).HasColumnName("kapasitas_non_jkn");

                entity.Property(e => e.KodePoli)
                    .HasMaxLength(10)
                    .HasColumnName("kode_poli")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.NamaDokter)
                    .HasMaxLength(256)
                    .HasColumnName("nama_dokter");

                entity.Property(e => e.NomorUrutJadwal).HasColumnName("nomor_urut_jadwal");

                entity.Property(e => e.PasienTime).HasColumnName("pasien_time");

                entity.Property(e => e.ProfileImage)
                    .HasMaxLength(200)
                    .HasColumnName("profile_image")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.TotalKapasitas).HasColumnName("total_kapasitas");
            });

            modelBuilder.Entity<BlIdGen>(entity =>
            {
                entity.HasKey(e => e.KodeFaskes)
                    .HasName("PRIMARY");

                entity.ToTable("bl_id_gen");

                entity.Property(e => e.KodeFaskes)
                    .HasMaxLength(20)
                    .HasColumnName("kode_faskes");

                entity.Property(e => e.CurrentNumber).HasColumnName("current_number");

                entity.Property(e => e.GenRule)
                    .HasMaxLength(200)
                    .HasColumnName("gen_rule")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.LastNumber).HasColumnName("last_number");
            });

            modelBuilder.Entity<BlJadwal>(entity =>
            {
                entity.ToTable("bl_jadwal");

                entity.HasIndex(e => e.KodeJadwal, "kode_jadwal")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.JamMulai)
                    .HasMaxLength(5)
                    .HasColumnName("jam_mulai");

                entity.Property(e => e.JamSelesai)
                    .HasMaxLength(5)
                    .HasColumnName("jam_selesai");

                entity.Property(e => e.QuotaJkn).HasColumnName("quota_jkn");
                entity.Property(e => e.QuotaNonJkn).HasColumnName("quota_non_jkn");
                entity.Property(e => e.QuotaTotal).HasColumnName("quota_total");
                entity.Property(e => e.QuotaJknUsed).HasColumnName("quota_jkn_used");
                entity.Property(e => e.QuotaNonJknUsed).HasColumnName("quota_non_jkn_used");
                entity.Property(e => e.Locked)
                    .HasColumnName("locked")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Keterangan)
                    .HasMaxLength(256)
                    .HasColumnName("keterangan")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.KodeDokter)
                    .HasMaxLength(10)
                    .HasColumnName("kode_dokter");

                entity.Property(e => e.KodeJadwal)
                    .HasMaxLength(20)
                    .HasColumnName("kode_jadwal");

                entity.Property(e => e.KodePoli)
                    .HasMaxLength(10)
                    .HasColumnName("kode_poli");

                entity.Property(e => e.Libur)
                    .HasColumnName("libur")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.NamaHari)
                    .HasMaxLength(20)
                    .HasColumnName("nama_hari");

                entity.Property(e => e.Tanggal).HasColumnName("tanggal");
            });

            modelBuilder.Entity<BlJadwalBedah>(entity =>
            {
                entity.HasKey(e => e.NomorJadwal)
                    .HasName("PRIMARY");

                entity.ToTable("bl_jadwal_bedah");

                entity.Property(e => e.NomorJadwal)
                    .HasMaxLength(20)
                    .HasColumnName("nomor_jadwal");

                entity.Property(e => e.JenisTindakan)
                    .HasMaxLength(256)
                    .HasColumnName("jenis_tindakan")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.KodeDokterOperator)
                    .HasMaxLength(10)
                    .HasColumnName("kode_dokter_operator");

                entity.Property(e => e.NomorRegistrasi)
                    .HasMaxLength(20)
                    .HasColumnName("nomor_registrasi");

                entity.Property(e => e.NomorRekamMedis)
                    .HasMaxLength(20)
                    .HasColumnName("nomor_rekam_medis");

                entity.Property(e => e.TanggalOperasi).HasColumnName("tanggal_operasi");

                entity.Property(e => e.Terlaksana).HasColumnName("terlaksana");

                entity.Property(e => e.WaktuOperasi)
                    .HasMaxLength(5)
                    .HasColumnName("waktu_operasi");
            });

            modelBuilder.Entity<BlPasien>(entity =>
            {
                entity.HasKey(e => e.NomorRekamMedis)
                    .HasName("PRIMARY");

                entity.ToTable("bl_pasien");

                entity.HasIndex(e => e.Nama, "bl_pasien_namapas");

                entity.HasIndex(e => e.TanggalLahir, "bl_pasien_tgllahir");

                entity.Property(e => e.NomorRekamMedis)
                    .HasMaxLength(20)
                    .HasColumnName("nomor_rekam_medis");

                entity.Property(e => e.Alamat)
                    .HasMaxLength(512)
                    .HasColumnName("alamat")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.FingerPrint)
                    .HasColumnType("bit(1)")
                    .HasColumnName("finger_print")
                    .HasDefaultValueSql("b'0'");

                entity.Property(e => e.Gender)
                    .HasMaxLength(1)
                    .HasColumnName("gender");

                entity.Property(e => e.Honorific)
                    .HasMaxLength(20)
                    .HasColumnName("honorific")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.InsuranceId)
                    .HasMaxLength(10)
                    .HasColumnName("insurance_id")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.KodeDati2)
                    .HasMaxLength(10)
                    .HasColumnName("kode_dati2")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.KodeKecamatan)
                    .HasMaxLength(256)
                    .HasColumnName("kode_kecamatan")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.KodeKelurahan)
                    .HasMaxLength(20)
                    .HasColumnName("kode_kelurahan")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.KodePropinsi)
                    .HasMaxLength(10)
                    .HasColumnName("kode_propinsi")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.LastReservation)
                    .HasColumnName("last_reservation");

                entity.Property(e => e.ModifiedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("modified_at")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.Nama)
                    .HasMaxLength(256)
                    .HasColumnName("nama")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.NamaDati2)
                    .HasMaxLength(256)
                    .HasColumnName("nama_dati2")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.NamaKecamatan)
                    .HasMaxLength(256)
                    .HasColumnName("nama_kecamatan")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.NamaKelurahan)
                    .HasMaxLength(256)
                    .HasColumnName("nama_kelurahan")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.NamaPropinsi)
                    .HasMaxLength(256)
                    .HasColumnName("nama_propinsi")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.NomorIdentitas)
                    .HasMaxLength(16)
                    .HasColumnName("nomor_identitas")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.NomorKartuJkn)
                    .HasMaxLength(13)
                    .HasColumnName("nomor_kartu_jkn")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.NomorKartuKeluarga)
                    .HasMaxLength(16)
                    .HasColumnName("nomor_kartu_keluarga")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Phone)
                    .HasMaxLength(16)
                    .HasColumnName("phone")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Rt)
                    .HasMaxLength(3)
                    .HasColumnName("rt")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Rw)
                    .HasMaxLength(3)
                    .HasColumnName("rw")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.TanggalLahir).HasColumnName("tanggal_lahir");
            });

            modelBuilder.Entity<BlPoli>(entity =>
            {
                entity.HasKey(e => e.KodePoli)
                    .HasName("PRIMARY");

                entity.ToTable("bl_poli");

                entity.Property(e => e.KodePoli)
                    .HasMaxLength(10)
                    .HasColumnName("kode_poli");

                entity.Property(e => e.KodePoliJkn)
                    .HasMaxLength(10)
                    .HasColumnName("kode_poli_jkn")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.NamaPoli)
                    .HasMaxLength(256)
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
                    .HasMaxLength(11)
                    .HasColumnName("jam_praktek");

                entity.Property(e => e.JenisKunjungan).HasColumnName("jenis_kunjungan");

                entity.Property(e => e.KodeDokter).HasColumnName("kode_dokter");

                entity.Property(e => e.KodePoli)
                    .HasMaxLength(10)
                    .HasColumnName("kode_poli");

                entity.Property(e => e.Nik)
                    .HasMaxLength(16)
                    .HasColumnName("nik");

                entity.Property(e => e.NomorHp)
                    .HasMaxLength(16)
                    .HasColumnName("nomor_hp");

                entity.Property(e => e.NomorKartu)
                    .HasMaxLength(13)
                    .HasColumnName("nomor_kartu");

                entity.Property(e => e.NomorReferensi)
                    .HasMaxLength(19)
                    .HasColumnName("nomor_referensi");

                entity.Property(e => e.NomorRm)
                    .HasMaxLength(20)
                    .HasColumnName("nomor_rm");

                entity.Property(e => e.ReservationId).HasColumnName("reservation_id");

                entity.Property(e => e.TanggalPeriksa).HasColumnName("tanggal_periksa");
            });

            modelBuilder.Entity<MjknAntrianTransaction>(entity =>
            {
                entity.ToTable("mjkn_antrian_transaction");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.OAngkaAntrian)
                    .HasColumnName("o_angka_antrian")
                    .HasDefaultValueSql("'-1'");

                entity.Property(e => e.OEstimasiDilayani)
                    .HasColumnName("o_estimasi_dilayani")
                    .HasDefaultValueSql("'-1'");

                entity.Property(e => e.OKeterangan)
                    .HasMaxLength(256)
                    .HasColumnName("o_keterangan")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.OKodeBooking)
                    .HasMaxLength(12)
                    .HasColumnName("o_kode_booking")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.OKuotaJkn)
                    .HasColumnName("o_kuota_jkn")
                    .HasDefaultValueSql("'-1'");

                entity.Property(e => e.OKuotaNonJkn)
                    .HasColumnName("o_kuota_non_jkn")
                    .HasDefaultValueSql("'-1'");

                entity.Property(e => e.ONamaDokter)
                    .HasMaxLength(256)
                    .HasColumnName("o_nama_dokter")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.ONamaPoli)
                    .HasMaxLength(256)
                    .HasColumnName("o_nama_poli")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.ONomorAntrian)
                    .HasMaxLength(20)
                    .HasColumnName("o_nomor_antrian")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.OSisaKuotaJkn)
                    .HasColumnName("o_sisa_kuota_jkn")
                    .HasDefaultValueSql("'-1'");

                entity.Property(e => e.OSisaKuotaNonJkn)
                    .HasColumnName("o_sisa_kuota_non_jkn")
                    .HasDefaultValueSql("'-1'");

                entity.Property(e => e.RJamPraktek)
                    .HasMaxLength(11)
                    .HasColumnName("r_jam_praktek")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.RJenisKunjungan).HasColumnName("r_jenis_kunjungan");

                entity.Property(e => e.RKodeDokterJkn)
                    .HasColumnName("r_kode_dokter_jkn")
                    .HasDefaultValueSql("'-1'");

                entity.Property(e => e.RKodePoliJkn)
                    .HasMaxLength(3)
                    .HasColumnName("r_kode_poli_jkn")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.RNik)
                    .HasMaxLength(16)
                    .HasColumnName("r_nik")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.RNomorHp)
                    .HasMaxLength(16)
                    .HasColumnName("r_nomor_hp")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.RNomorKartu)
                    .HasMaxLength(13)
                    .HasColumnName("r_nomor_kartu")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.RNomorReferensi)
                    .HasMaxLength(19)
                    .HasColumnName("r_nomor_referensi")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.RNomorRm)
                    .HasMaxLength(20)
                    .HasColumnName("r_nomor_rm")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.RSource)
                    .HasMaxLength(20)
                    .HasColumnName("r_source")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.RTanggalPeriksa).HasColumnName("r_tanggal_periksa");

                entity.Property(e => e.SAlamatPasien)
                    .HasMaxLength(512)
                    .HasColumnName("s_alamat_pasien")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.SAntriId)
                    .HasMaxLength(20)
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
                    .HasColumnName("s_server_at");

                entity.Property(e => e.SJadwalId).HasColumnName("s_jadwal_id");

                entity.Property(e => e.SKeterangan)
                    .HasMaxLength(256)
                    .HasColumnName("s_keterangan")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.SKodepoliInternal)
                    .HasMaxLength(20)
                    .HasColumnName("s_kodepoli_internal");

                entity.Property(e => e.SKodokterInternal)
                    .HasMaxLength(20)
                    .HasColumnName("s_kodokter_internal");

                entity.Property(e => e.SNamaHari)
                    .HasMaxLength(20)
                    .HasColumnName("s_nama_hari");

                entity.Property(e => e.SNamaPasien)
                    .HasMaxLength(256)
                    .HasColumnName("s_nama_pasien")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.SPasienBaru).HasColumnName("s_pasien_baru");

                entity.Property(e => e.SPhone)
                    .HasMaxLength(100)
                    .HasColumnName("s_phone")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.SStatus).HasColumnName("s_status");

                entity.Property(e => e.STanggalLahir).HasColumnName("s_tanggal_lahir");

                entity.Property(e => e.SUseredit)
                    .HasMaxLength(100)
                    .HasColumnName("s_useredit")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.SUsername)
                    .HasMaxLength(100)
                    .HasColumnName("s_username")
                    .HasDefaultValueSql("''");
            });

            modelBuilder.Entity<MjknDokter>(entity =>
            {
                entity.ToTable("mjkn_dokter");

                entity.HasIndex(e => e.KodedokterJkn, "kodedokter_jkn")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.KodedokterInternal)
                    .HasMaxLength(10)
                    .HasColumnName("kodedokter_internal");

                entity.Property(e => e.KodedokterJkn).HasColumnName("kodedokter_jkn");

                entity.Property(e => e.NamaDokter)
                    .HasMaxLength(256)
                    .HasColumnName("nama_dokter")
                    .HasDefaultValueSql("''");
            });

            modelBuilder.Entity<MjknJadwalDokter>(entity =>
            {
                entity.ToTable("mjkn_jadwal_dokter");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Hari).HasColumnName("hari");

                entity.Property(e => e.Jadwal)
                    .HasMaxLength(20)
                    .HasColumnName("jadwal");

                entity.Property(e => e.JamMulai)
                    .HasMaxLength(5)
                    .HasColumnName("jam_mulai");

                entity.Property(e => e.JamTutup)
                    .HasMaxLength(5)
                    .HasColumnName("jam_tutup");

                entity.Property(e => e.KapasitasPasien).HasColumnName("kapasitas_pasien");

                entity.Property(e => e.KodePoli)
                    .HasMaxLength(10)
                    .HasColumnName("kode_poli");

                entity.Property(e => e.KodeSubspesialis)
                    .HasMaxLength(10)
                    .HasColumnName("kode_subspesialis");

                entity.Property(e => e.KodedokterInternal)
                    .HasMaxLength(10)
                    .HasColumnName("kodedokter_internal")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.KodedokterJkn).HasColumnName("kodedokter_jkn");

                entity.Property(e => e.LastUpdate)
                    .HasColumnType("datetime")
                    .HasColumnName("last_update");

                entity.Property(e => e.LastUpdateCode).HasColumnName("last_update_code");

                entity.Property(e => e.LastUpdateStatus)
                    .HasMaxLength(50)
                    .HasColumnName("last_update_status")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Libur).HasColumnName("libur");

                entity.Property(e => e.NamaDokter)
                    .HasMaxLength(256)
                    .HasColumnName("nama_dokter");

                entity.Property(e => e.NamaHari)
                    .HasMaxLength(20)
                    .HasColumnName("nama_hari");

                entity.Property(e => e.NamaPoli)
                    .HasMaxLength(256)
                    .HasColumnName("nama_poli");

                entity.Property(e => e.NamaSubspesialis)
                    .HasMaxLength(256)
                    .HasColumnName("nama_subspesialis");
            });

            modelBuilder.Entity<MjknPasien>(entity =>
            {
                entity.ToTable("mjkn_pasien");

                entity.HasIndex(e => e.NomorKartu, "nomor_kartu")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Alamat)
                    .HasMaxLength(512)
                    .HasColumnName("alamat")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.FingerPrint).HasColumnName("finger_print");

                entity.Property(e => e.JenisKelamin)
                    .HasMaxLength(1)
                    .HasColumnName("jenis_kelamin");

                entity.Property(e => e.KodeDati2)
                    .HasMaxLength(10)
                    .HasColumnName("kode_dati2")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.KodeKecamatan)
                    .HasMaxLength(256)
                    .HasColumnName("kode_kecamatan")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.KodeKelurahan)
                    .HasMaxLength(20)
                    .HasColumnName("kode_kelurahan")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.KodePropinsi)
                    .HasMaxLength(10)
                    .HasColumnName("kode_propinsi")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.LastReservation)
                    .HasColumnName("last_reservation");

                entity.Property(e => e.Nama)
                    .HasMaxLength(256)
                    .HasColumnName("nama");

                entity.Property(e => e.NamaDati2)
                    .HasMaxLength(256)
                    .HasColumnName("nama_dati2")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.NamaKecamatan)
                    .HasMaxLength(256)
                    .HasColumnName("nama_kecamatan")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.NamaKelurahan)
                    .HasMaxLength(256)
                    .HasColumnName("nama_kelurahan")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.NamaPropinsi)
                    .HasMaxLength(256)
                    .HasColumnName("nama_propinsi")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Nik)
                    .HasMaxLength(16)
                    .HasColumnName("nik");

                entity.Property(e => e.NomorHp)
                    .HasMaxLength(16)
                    .HasColumnName("nomor_hp")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.NomorKartu)
                    .HasMaxLength(13)
                    .HasColumnName("nomor_kartu");

                entity.Property(e => e.NomorKk)
                    .HasMaxLength(16)
                    .HasColumnName("nomor_kk");

                entity.Property(e => e.NomorRm)
                    .HasMaxLength(20)
                    .HasColumnName("nomor_rm");

                entity.Property(e => e.Rt)
                    .HasMaxLength(3)
                    .HasColumnName("rt")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Rw)
                    .HasMaxLength(3)
                    .HasColumnName("rw")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.TanggalLahir).HasColumnName("tanggal_lahir");
            });

            modelBuilder.Entity<MjknPoli>(entity =>
            {
                entity.ToTable("mjkn_poli");

                entity.HasIndex(e => e.Kode, "kode")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Kode)
                    .HasMaxLength(10)
                    .HasColumnName("kode");

                entity.Property(e => e.Nama)
                    .HasMaxLength(256)
                    .HasColumnName("nama");
            });

            modelBuilder.Entity<MjknPoliSub>(entity =>
            {
                entity.ToTable("mjkn_poli_sub");

                entity.HasIndex(e => e.Kode, "kode")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.FingerPrint).HasColumnName("finger_print");

                entity.Property(e => e.Kode)
                    .HasMaxLength(10)
                    .HasColumnName("kode");

                entity.Property(e => e.KodePoli)
                    .HasMaxLength(10)
                    .HasColumnName("kode_poli");

                entity.Property(e => e.Nama)
                    .HasMaxLength(256)
                    .HasColumnName("nama");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}