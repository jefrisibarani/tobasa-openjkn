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

using LZStringCSharp;

namespace Tobasa.Models.Vclaim
{
    public class VclaimResultBase
    {
        protected bool _hasResponse = false;
        public virtual string Decrypt(string key)
        {
            return "";
        }

        public void HasResponse(bool value)
        {
            _hasResponse = value;
        }
        public bool HasResponse()
        {
            return _hasResponse;
        }
    }


    /////////////////////////////////////////////////////////////////////////////////////////////
    // Result come from Vclaim web service
    // https://dvlp.bpjs-kesehatan.go.id/VClaim-Katalog/

    public class VclaimResult : VclaimResultBase
    {
        public VclaimResult() : base() { }

        public string Response { get; set; }
        public ResultMetaData MetaData { get; set; }

        public override string Decrypt(string key)
        {
            var bpjsCrypt = new BpjsCrypt();
            string resultLZString = bpjsCrypt.Decrypt(key, Response);
            string resultDecompressed = LZString.DecompressFromEncodedURIComponent(resultLZString);
            return resultDecompressed;
        }
    }

    // Web Service VClaim Versi 1.1, menggunakan string untuk Code, bukan int
    public class ResultMetaData
    {
        public ResultMetaData() { }
        public string Message { get; set; }
        public string Code { get; set; }
    }


    /////////////////////////////////////////////////////////////////////////////////////////////
    // Result come from MJKN BPJS web service
    // https://dvlp.bpjs-kesehatan.go.id/VClaim-Katalog/

    public class VclaimResultJKN : VclaimResultBase
    {
        public VclaimResultJKN() : base() { }
        public string Response { get; set; }
        public ResultMetaDataJKN MetaData { get; set; }
        public override string Decrypt(string key)
        {
            var bpjsCrypt = new BpjsCrypt();
            string resultLZString = bpjsCrypt.Decrypt(key, Response);
            string resultDecompressed = LZString.DecompressFromEncodedURIComponent(resultLZString);
            return resultDecompressed;
        }
    }

    // Web Service MJKN BPJS menggunakan int untuk Code
    public class ResultMetaDataJKN
    {
        public ResultMetaDataJKN() { }
        public string Message { get; set; }
        public int Code { get; set; }
    }
}