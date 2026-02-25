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

namespace Tobasa.Models.Mjkn
{
    public class CekSlotResult
    {
        public CekSlotResult()
        {
            Message          = "";
            TotalQuota       = -1;
            QuotaBpjsUsed    = -1;
            QuotaNonBpjsUsed = -1;
            QuotaBpjs        = -1;
            QuotaNonBpjs     = -1;
            Kodepos          = "";
        }

        public CekSlotResult(string message)
        {
            Message          = message;
            TotalQuota       = -1;
            QuotaBpjsUsed    = -1;
            QuotaNonBpjsUsed = -1;
            QuotaBpjs        = -1;
            QuotaNonBpjs     = -1;
            Kodepos          = "";
        }


        public string Message { get; set; }
        public int    TotalQuota { get; set; }
        public int    QuotaBpjsUsed { get; set; }
        public int    QuotaNonBpjsUsed { get; set; }
        public int    SlotsAvailable { get; set; }
        public int    QuotaBpjs { get; set; }
        public int    QuotaNonBpjs { get; set; }
        public string Kodepos { get; set; }
    }
}