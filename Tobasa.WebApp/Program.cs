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

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Tobasa
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                CreateHostBuilder(args).Build().Run();
                Log.CloseAndFlush();
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine("Application failed to start:");
                Console.Error.WriteLine(ex.Message);
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((ctx, config) => { config.ReadFrom.Configuration(ctx.Configuration); })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel((context, serverOptions) =>
                    {
                        var config = context.Configuration;
                        var certSection = config.GetSection("Kestrel:EndPoints:Https:Certificate");
                        var certPath = certSection.GetValue<string>("Path");
                        var certPassword = certSection.GetValue<string>("Password");

                        if (string.IsNullOrWhiteSpace(certPath) || !File.Exists(certPath))
                            throw new FileNotFoundException($"Certificate file not found: {certPath}");

                        var cert = new X509Certificate2(certPath, certPassword,
                            X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.EphemeralKeySet);

                        if (!cert.HasPrivateKey)
                            throw new Exception("Loaded certificate does not contain a private key.");

                        if (DateTime.UtcNow < cert.NotBefore || DateTime.UtcNow > cert.NotAfter)
                            throw new Exception("Certificate is not valid at the current time (expired or not yet valid).");

                        using (var chain = new X509Chain())
                        {
                            chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                            chain.ChainPolicy.RevocationFlag = X509RevocationFlag.ExcludeRoot;
                            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;
                            chain.ChainPolicy.VerificationTime = DateTime.UtcNow;

                            if (!chain.Build(cert))
                            {
                                var errs = string.Join("; ", chain.ChainStatus.Select(s => s.StatusInformation.Trim()));
                                throw new Exception($"Certificate chain validation failed: {errs}");
                            }
                        }
                    })
                    .UseStartup<Startup>();
                });
    }
}