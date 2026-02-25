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
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Tobasa.App;
using Tobasa.Data;
using Tobasa.Models.Vclaim;
using Tobasa.Services.Vclaim;

namespace Tobasa.Services
{
    public interface IVClaimApiService
    {
        Task<Peserta> GetPesertaByNik(string nomorNik, string tanggalPelayanan);
        Task<Peserta> GetPesertaByNocard(string nomorBpjs, string tanggalPelayanan);
        Task<Rujukan> GetRujukanByNoRujukan(string nomorRujukan);
        Task<Rujukan> GetRujukanByNocardSingle(string nomorBpjs);
        Task<List<Rujukan>> GetRujukanByNocardMulti(string nomorBpjs);
        Task<SuratKontrol> GetSuratKontrol(string noSuratKontrol);
        Task<List<HistoryPelayanan>> GetHistoriPelayananPeserta(string nomorBPJS, string tanggalAwal, string tanggalAkhir);
    }
    public class VClaimApiService : IVClaimApiService
    {
        private DataContextAntrian   _sqlCtx;
        private readonly AppSettings _appSettings;
        private readonly ILogger    _logger;
        private readonly IConfiguration _configuration;

        public VClaimApiService(DataContextAntrian context,
            IOptions<AppSettings> appSettings,
            ILogger<MjknService> logger,
            IConfiguration configuration)
        {
            _sqlCtx      = context;
            _appSettings = appSettings.Value;
            _logger      = logger;
            _configuration = configuration;
        }

        public async Task<Peserta> GetPesertaByNik(string nomorNik, string tglSep)
        {
            var vclaimSvc = new VclaimPesertaService(_appSettings, _logger, _configuration);
            var peserta = await vclaimSvc.GetPesertaByNik(nomorNik, tglSep);
            if (peserta != null)
                return peserta;

            return null;
        }

        public async Task<Peserta> GetPesertaByNocard(string nomorBpjs, string tglSep)
        {
            var vclaimSvc = new VclaimPesertaService(_appSettings, _logger, _configuration);
            var peserta = await vclaimSvc.GetPesertaByNocard(nomorBpjs, tglSep);
            if (peserta != null)
                return peserta;

            return null;
        }

        public async Task<Rujukan> GetRujukanByNoRujukan(string nomorRujukan)
        {
            var vclaimSvc = new VclaimRujukanService(_appSettings, _logger, _configuration);
            var rujukan = await vclaimSvc.GetRujukanByNoRujukan(nomorRujukan);
            if (rujukan != null)
                return rujukan;

            return null;
        }

        public async Task<Rujukan> GetRujukanByNocardSingle(string nomorBpjs)
        {
            var vclaimSvc = new VclaimRujukanService(_appSettings, _logger, _configuration);
            var rujukan = await vclaimSvc.GetRujukanByNocardSingle(nomorBpjs);
            if (rujukan != null) { 
                return rujukan;
            }

            return null;
        }

        public async Task<List<Rujukan>> GetRujukanByNocardMulti(string nomorBpjs)
        {
            var vclaimSvc = new VclaimRujukanService(_appSettings, _logger, _configuration);
            var rujukan = await vclaimSvc.GetRujukanByNocardMulti(nomorBpjs);
            if (rujukan != null) {
                return rujukan;
            }

            return null;
        }

        public async Task<SuratKontrol> GetSuratKontrol(string noSuratKontrol)
        {
            var vclaimSvc = new VclaimRencanaKontrolService(_appSettings, _logger, _configuration);
            var suratKontrol = await vclaimSvc.GetSuratKontrol(noSuratKontrol);
            if (suratKontrol != null) {
                return suratKontrol;
            }

            return null;
        }

        public async Task<List<HistoryPelayanan>> GetHistoriPelayananPeserta(string nomorBPJS, string tanggalAwal, string tanggalAkhir)
        {
            var vclaimSvc = new VclaimMonitoringService(_appSettings, _logger, _configuration);
            var history = await vclaimSvc.GetDataHistoriPelayananPeserta(nomorBPJS, tanggalAwal, tanggalAkhir);
            if (history != null)
            {
                return history;
            }

            return null;
        }
    }
}