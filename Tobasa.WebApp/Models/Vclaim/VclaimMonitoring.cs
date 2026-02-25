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

namespace Tobasa.Models.Vclaim
{
    public class HistoryPelayanan
    {
        public string Diagnosa { get; set; }
        public string JnsPelayanan { get; set; }
        public string KelasRawat { get; set; }
        public string NamaPeserta { get; set; }
        public string NoKartu { get; set; }
        public string NoSep { get; set; }
        public string NoRujukan { get; set; }
        public string Poli { get; set; }
        public string PpkPelayanan { get; set; }
        public DateTime TglSep { get; set; }
#nullable enable
        public DateTime? TglPlgSep { get; set; }
        public string? Flag { get; set; }
        public string? Asuransi { get; set; }
#nullable disable
        public string PoliTujSep { get; set; }
    }

    public class ResponseGetDataHistoriPelayananPeserta
    {
        public List<HistoryPelayanan> Histori { get; set; }
    }
}