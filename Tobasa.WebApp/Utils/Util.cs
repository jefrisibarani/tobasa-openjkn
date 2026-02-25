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

using Microsoft.Extensions.Configuration;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System;
using Tobasa.App;
using Microsoft.Extensions.Logging;

namespace Tobasa
{
    public class Utils
    {
        public static string TrimString(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "";
            else
                return input.Trim();
        }

        public static string ProcessPath
        {
            get
            {
                // Assembly.GetExecutingAssembly().CodeBase return the executing assembly, which mean
                // if this code compiled inside a .dll,it will return the dll name

                return System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            }
        }

        public static string ProcessDir
        {
            get
            {
                return Path.GetDirectoryName(Utils.ProcessPath);
            }
        }

        public static string ProcessName
        {
            get
            {
                string exepath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                return Path.GetFileNameWithoutExtension(exepath);
            }
        }

        static Random rd = new Random();
        public static string CreateRandomString(int stringLength, string allowedChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789")
        {
            char[] chars = new char[stringLength];

            for (int i = 0; i < stringLength; i++)
            {
                chars[i] = allowedChars[rd.Next(0, allowedChars.Length)];
            }

            return new string(chars);
        }

        // NOTE: https://stackoverflow.com/a/311179
        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
            {
                hex.AppendFormat("{0:x2}", b);
            }

            return hex.ToString();
        }

        // NOTE: https://stackoverflow.com/a/311179
        public static byte[] StringToByteArray(string hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }

            return bytes;
        }

        public static string EncryptPassword(string clearPassword, string salt)
        {
            using (SHA256 algorithm = SHA256.Create())
            {
                Byte[] saltSHA = algorithm.ComputeHash(Encoding.UTF8.GetBytes(salt)); // 32 bytes
                Byte[] IVtmp = new Byte[16];
                Array.Copy(saltSHA, IVtmp, 16);

                byte[] rijndaelKey = saltSHA; // 32 bytes Key
                byte[] rijndaelIV = IVtmp;    // 16 bytes IV

                // Encrypt the string to an array of bytes.
                byte[] encrypted = RijndaelEncryptStringToBytes(clearPassword, rijndaelKey, rijndaelIV);

                return ByteArrayToString(encrypted);
            }
        }

        public static string DecryptPassword(string encryptedPassword,string salt)
        {
            // Convert HEX encryptedPassword to a byte array.
            byte[] encrypted;
            encrypted = StringToByteArray(encryptedPassword);

            using (SHA256 algorithm = SHA256.Create())
            {
                Byte[] saltSHA = algorithm.ComputeHash(Encoding.UTF8.GetBytes(salt)); // 32 bytes
                Byte[] IVtmp = new Byte[16];
                Array.Copy(saltSHA, IVtmp, 16);

                byte[] rijndaelKey = saltSHA;  // 32 bytes Key
                byte[] rijndaelIV = IVtmp;     // 16 bytes IV

                // Decrypt the bytes to a string.
                string clearPassword = RijndaelDecryptStringFromBytes(encrypted, rijndaelKey, rijndaelIV);
                return clearPassword;
            }
        }

        // https://docs.microsoft.com/en-us/archive/blogs/shawnfa/the-differences-between-rijndael-and-aes

        public static byte[] RijndaelEncryptStringToBytes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            byte[] encrypted;

            // Create an Rijndael object
            // with the specified key and IV.
            using (Aes rijAlg = Aes.Create("AesManaged") )
            {
                rijAlg.Mode      = CipherMode.CBC;
                rijAlg.BlockSize = 128; // default
                rijAlg.KeySize   = 256;
                rijAlg.Key       = Key;
                rijAlg.IV        = IV;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

                try
                {
                    // Create the streams used for encryption.
                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {
                                // Write all data to the stream.
                                swEncrypt.Write(plainText);
                            }
                            encrypted = msEncrypt.ToArray();
                        }
                    }
                }
                catch (Exception ex)
                {
                    var x = ex.Message;
                    throw new AppException("Invalid input data");
                }
            }

            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

        // https://docs.microsoft.com/en-us/archive/blogs/shawnfa/the-differences-between-rijndael-and-aes

        public static string RijndaelDecryptStringFromBytes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an Rijndael object
            // with the specified key and IV.
            using (Aes rijAlg = Aes.Create("AesManaged") )
            {
                rijAlg.Mode      = CipherMode.CBC;
                rijAlg.BlockSize = 128; // default
                rijAlg.KeySize   = 256;
                rijAlg.Key       = Key;
                rijAlg.IV        = IV;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                try
                {
                    // Create the streams used for decryption.
                    using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                // Read the decrypted bytes from the decrypting stream
                                // and place them in a string.
                                plaintext = srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    var x = ex.Message;
                    throw new AppException("Invalid chiper data");
                }

            }

            return plaintext;
        }

        public static string HmacSHA256(string message, string keyIn)
        {
            if (string.IsNullOrWhiteSpace(message))
                return "";

            string keyReal;
            keyReal = string.IsNullOrWhiteSpace(keyIn) ? "pemudaharapanbangsa" : keyIn;
            Byte[] hmacKey = Encoding.UTF8.GetBytes(keyReal);

            using (HMACSHA256 hmac = new HMACSHA256(hmacKey))
            {
                // Compute the hash of the input message.
                byte[] hmacResult = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
                return BitConverter.ToString(hmacResult).Replace("-", "");
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////

        public static string ISODateToIndonesianDate(string isoDate)
        {
            DateTime dtVal = DateTime.ParseExact(isoDate, "yyyy-MM-dd", null);
            return dtVal.ToString("dd-MM-yyyy");
        }

        public static string ISODateTimeToIndonesianDate(string isoDateTime, bool useDayName = false)
        {
            DateTime dtVal = DateTime.ParseExact(isoDateTime, "yyyy-MM-ddTHH:mm:ss", null);
            return DateTimeToIndonesianDate(dtVal, useDayName);
        }

        public static string DateTimeToIndonesianDate(DateTime dtVal, bool useDayName=false)
        {
            string result;
            string sDay  = dtVal.ToString("dddd", new CultureInfo("en-US"));
            string sDate = dtVal.ToString("dd-MM-yyyy");
            string sTime = dtVal.ToString("HH:mm");

            string sHari;
            if(useDayName)
            {
                sHari = sDay switch
                {
                    "Sunday"    => "Minggu",
                    "Monday"    => "Senin",
                    "Tuesday"   => "Selasa",
                    "Wednesday" => "Rabu",
                    "Thursday"  => "Kamis",
                    "Friday"    => "Jumat",
                    "Saturday"  => "Sabtu",
                    _ => "",
                };

                result = $"{sHari}, {sDate} {sTime}";
            }
            else
                result = $"{sDate} {sTime}";


            return result;
        }

        public static bool IsValidISODate(string dateIn)
        {
            var dateFormat = "yyyy-MM-dd";
            bool validDate = DateTime.TryParseExact(dateIn, dateFormat, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out _);
            return validDate;
        }

        public static bool IsValidDate(string dateIn)
        {
            var dateFormat = "yyyy-MM-dd";
            bool validDate = DateTime.TryParseExact(dateIn, dateFormat, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out _);
            return validDate;
        }

        public static bool IsDateAllowed(string dateIn, int maximalDay = 1)
        {
            DateTime dtPeriksa = DateTime.ParseExact(dateIn, "yyyy-MM-dd", null).Date;

            if (dtPeriksa.Date >= DateTime.Today.Date && dtPeriksa.Date <= DateTime.Today.AddDays(maximalDay).Date)
                return true;
            else
                return false;
        }

        public static long GetUnixTimeStamp(string dateIn, string timeIn)
        {
            string[] timeInArray = timeIn.Split(':');
            DateTime dateOut;
            var dateFormat = "yyyy-MM-dd";

            bool validDate = DateTime.TryParseExact(dateIn, dateFormat, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out dateOut);
            if (validDate)
            {
                int _hour   = int.Parse(timeInArray[0]);
                int _minute = int.Parse(timeInArray[1]);
                TimeSpan ts = new TimeSpan(_hour, _minute, 0);
                dateOut     = dateOut.Date + ts;

                long unixtime = ((DateTimeOffset)dateOut).ToUnixTimeSeconds();
                return unixtime;
            }

            return 0;
        }

        public static DateTime UnixTimeStampToDateTime(ulong unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddMilliseconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }

        public static DateTime? DateFromString(string isoDate)
        {
            DateTime outDate;
            var dateFormat = "yyyy-MM-dd";
            bool validDate = DateTime.TryParseExact(isoDate, dateFormat, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out outDate);

            if (validDate)
                return outDate;
            else
                return null;
        }

        public static DateTime? DateFromStringTime(string timeString)
        {
            DateTime outDate;
            var dateFormat = "HH:mm";
            bool validDate = DateTime.TryParseExact(timeString, dateFormat, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out outDate);

            if (validDate)
                return outDate;
            else
                return null;
        }

        public static DateTime? DateFromStringTimeWithSeconds(string timeString)
        {
            DateTime outDate;
            var dateFormat = "HH:mm:ss";
            bool validDate = DateTime.TryParseExact(timeString, dateFormat, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out outDate);

            if (validDate)
                return outDate;
            else
                return null;
        }

        public static string ISODateTimeToIndonesianDay(DateTime datetime)
        {
            string sHari = datetime.DayOfWeek switch
            {
                DayOfWeek.Sunday    => "MINGGU",
                DayOfWeek.Monday    => "SENIN",
                DayOfWeek.Tuesday   => "SELASA",
                DayOfWeek.Wednesday => "RABU",
                DayOfWeek.Thursday  => "KAMIS",
                DayOfWeek.Friday    => "JUMAT",
                DayOfWeek.Saturday  => "SABTU",
                                  _ => "",
            };

            return sHari;
        }

        public static string ISODateTimeToIndonesianDay(string isoDateTime)
        {
            DateTime dtVal = DateTime.ParseExact(isoDateTime, "yyyy-MM-dd", null);
            return ISODateTimeToIndonesianDay(dtVal);
        }

        ////////////////////////////////////////////////////////////////////////////////////

        public static string GetConnectionString(IConfiguration config)
        {
            string connStringPartial;
            string passwordEncrypted;

            var databaseEngine = config["DatabaseEngine"];

            if (databaseEngine == "MSSQL")
            {
                connStringPartial = config.GetConnectionString("MsSqlProduction");
                passwordEncrypted = config.GetConnectionString("MsSqlProductionPassword");
            }
            else if (databaseEngine == "MYSQL")
            {
                connStringPartial = config.GetConnectionString("MySsqlProduction");
                passwordEncrypted = config.GetConnectionString("MySqlProductionPassword");
            }
            else if (databaseEngine == "PGSQL")
            {
                connStringPartial = config.GetConnectionString("PgSqlProduction");
                passwordEncrypted = config.GetConnectionString("PgSqlProductionPassword");

            }
            else if (databaseEngine == "SQLITE")
            {
                connStringPartial = config.GetConnectionString("SQLiteProduction");
                passwordEncrypted = config.GetConnectionString("SQLiteProductionPassword");

            }
            else
            {
                throw new AppException("Connection String failed on unsupported database type");
            }

            if (config["ConnectToLocalDatabase"] == true.ToString())
            {
                if (databaseEngine == "MSSQL")
                {
                    connStringPartial = config.GetConnectionString("MsSqlLocal");
                    passwordEncrypted = config.GetConnectionString("MsSqlLocalPassword");
                }
                else if (databaseEngine == "MYSQL")
                {
                    connStringPartial = config.GetConnectionString("MySqlLocal");
                    passwordEncrypted = config.GetConnectionString("MySqlLocalPassword");
                }
                else if (databaseEngine == "PGSQL")
                {
                    connStringPartial = config.GetConnectionString("PgSqlLocal");
                    passwordEncrypted = config.GetConnectionString("PgSqlLocalPassword");
                }
                else if (databaseEngine == "SQLITE")
                {
                    connStringPartial = config.GetConnectionString("SQLiteLocal");
                    passwordEncrypted = config.GetConnectionString("SQLiteLocalPassword");
                }
            }

            string passwordClear = "";
            if (databaseEngine != "SQLITE")
            {
                string securitySalt = config["AppSettings:SecuritySalt"];

                if (string.IsNullOrWhiteSpace(passwordEncrypted) || string.IsNullOrWhiteSpace(securitySalt))
                    throw new AppException("Invalid db configuration file");

                passwordClear = Utils.DecryptPassword(passwordEncrypted, securitySalt);
            }


            string connStringFull="";
            if (databaseEngine == "MSSQL") {
                connStringFull = connStringPartial + ";Pwd=" + passwordClear;
            }
            else if (databaseEngine == "MYSQL") { 
                connStringFull = connStringPartial + ";Password=" + passwordClear;
            }
            else if (databaseEngine == "PGSQL")
            {
                connStringFull = connStringPartial + ";Password=" + passwordClear;
            }
            else if (databaseEngine == "SQLITE")
            {
                connStringFull = connStringPartial;
            }

            return connStringFull;
        }

        /// Perbaiki format nomor rekam medis agar sesuai dengan format yang diharapkan.
        /// Misalnya nomor rekam medis dari Request 0023
        /// Di sistem format nomor rekam medis adalah D7(7 digit), artinya seharusnya 0000023
        public static string FixNomorRekamMedis(string nomorRm, IConfiguration config, ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(nomorRm))
            {
                logger.LogDebug($"  FixNomorRekamMedis: nomor rm null or white space");
                return "";
            }

            if ( config["AppSettings:TryFixNomorRekamMedis"] == true.ToString() )
            {
                try
                {
                    string jumlahKarakter = config["AppSettings:JumlahKarakterNomorRekamMedis"];
                    string formatStr = "D" + jumlahKarakter; // produce D7

                    var nomorRmInt = Convert.ToInt32(nomorRm);
                    var nomorRmNew = nomorRmInt.ToString(formatStr);

                    logger.LogDebug($"  FixNomorRekamMedis: nomor rm: {nomorRm} menjadi: {nomorRmNew}");

                    return nomorRmNew;
                }
                catch (Exception)
                {
                    throw new AppException("Gagal mengkonversi nomor rm");
                }
            }

            return nomorRm;
        }

        public static string CreateUniqueId()
        {
            return PushIDGen.GeneratePushID();
        }

        public static int NamaHariHfisToInteger(string namaHari)
        {
            int kodeHari;
            kodeHari = namaHari switch
            {
                "SENIN" => 1,
                "SELASA" => 2,
                "RABU" => 3,
                "KAMIS" => 4,
                "JUMAT" => 5,
                "SABTU" => 6,
                "MINGGU" => 7,
                "LIBUR_NAS" => 8,
                _ => 0,
            };

            return kodeHari;
        }

        public static string TransErrCodeSlotReservasi(string message, AppSettings settings)
        {
            string errMessage;

            if (     message == "ERROR_JKN_QUOTA_NOT_AVAILABLE")
                errMessage    = "Quota reservasi antrian pasien BPJS tidak tersedia";
            else if (message == "BPJS_QUOTA_USED_UP")
                errMessage    = "Quota reservasi antrian pasien BPJS telah penuh";
            else if (message == "ERROR_NON_JKN_QUOTA_NOT_AVAILABLE")
                errMessage    = "Quota reservasi antrian pasien non BPJS tidak tersedia";
            else if (message == "NON_BPJS_QUOTA_USED_UP")
                errMessage    = "Quota reservasi antrian pasien non BPJS telah penuh";
            else if (message == "ERROR_QUOTA_USED_UP")
                errMessage    = "Quota reservasi antrian sudah habis terpakai";
            else if (message == "ERROR_GETNEXT_RESERVATION")
                errMessage    = "Gagal mengambil reservasi berikutnya";
            else if (message == "ERROR_INVALID_DOCTOR")
                errMessage    = "Dokter tidak valid";
            else if (message == "ERROR_RCS_INVALID_JADWAL_ID")
                errMessage    = "Kode Jadwal tidak valid";
            else if (message == "ERROR_RCS_INVALID_DOCTOR_CODE")
                errMessage    = "Kode Dokter tidak valid";
            else if (message == "ERROR_RCS_INVALID_TDOKTER")
                errMessage    = "Slot reservasi tidak valid - dokter";
            else if (message == "ERROR_RCS_INVALID_JADWAL_ANTRIAN")
                errMessage    = "Slot reservasi Antrian tidak valid";
            else if (message == "ERROR_INVALID_SLOT")
                errMessage    = "Slot reservasi tidak valid";
            else if (message == "ERROR_OVER_QUOTA_REACH_LIMIT")
                errMessage    = "Antrian melebihi quota dan waktu layanan poli/dokter";
            else if (message == "ERROR_RESERVATION_OVER_JAM_PRAKTEK")
                errMessage    = "Reservasi melebihi waktu layanan poli/dokter";
            else
                errMessage = "Internal error code: 9000";

            return errMessage;
        }
    }
}