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

namespace Tobasa.Models.Queue
{
    public class CreateQueueRequestData
    {
        public string PostCode { get; set; } = "";
        public string Station { get; set; } = ""; // Ini akan menjadi source, station yang membuat nomor ini
        public string BookingCode { get; set; } = "";

    }

    public class CreateQueueResultData
    {
        public int    QueueNumber { get; set; } = 0;
        public string PrefixCode { get; set; } = "";
        public string PostCode { get; set; } = "";
        public string StartTime { get; set; } = "";

        public string JenisResep { get; set; } = "";
        public string NomorAntrean()
        {
            return PrefixCode + QueueNumber.ToString();
        }

        public string NomorAntrianSimrs()
        {
            return PrefixCode + "-" + QueueNumber.ToString("0000");
        }
    }

    public class StatusAntrianRequest
    {
        public string KodeBooking { get; set; } = "";
        public string PostCode { get; set; } = "";
        public string Station { get; set; } = "";
    }

    public class StatusAntrianResult
    {
        public string PostCode { get; set; } = "";
        public int CalledLast { get; set; } = 0;
        public int CalledTotal { get; set; } = 0;  
        public int WaitingFirst { get; set; } = 0;
        public int WaitingLast { get; set; } = 0;
        public int WaitingTotal { get; set; } = 0;
        public string PostNumberPrefix { get; set; } = "";
        public string JenisResep { get; set; } = "";

        public int TotalQueue()
        {
            return CalledTotal + WaitingTotal;
        }
    }


    public class CreateQueueNumberRequest
    {
        public string PostCode { get; set; } = "";
        public string Station { get; set; } = ""; // Caller ID yang memanggil
    }

    public class PostQueueStatusRequest
    {
        public string PostCode { get; set; } = "";
        public string Station { get; set; } = ""; // Caller ID yang memanggil
    }

    public class CallNextQueueNumberRequest
    {
        public string PostCode { get; set; } = "";
        public string Station { get; set; } = ""; // Caller ID yang memanggil
    }

    public class CallNextQueueNumberResult
    {
        public int Id { get; set; } = 0;
        public int Number { get; set; } = 0;
        public int WaitingTotal { get; set; } = 0;
        public string PostNumberPrefix { get; set; } = "";
        public string PostCode { get; set; } = "";
        public string Station { get; set; } = "";
    }
}