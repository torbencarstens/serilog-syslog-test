using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Formatting.Json;
using Serilog.Sinks.Syslog;
using System;
using System.Security.Authentication;

namespace syslog_test
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var certPath = "{TODO:CERT_PATH}";
            var certProvider = new CertificateFileProvider(certPath);
            /*X509Store store = new X509Store(StoreName.My);
            store.Open(OpenFlags.ReadWrite);
            store.Add(new X509Certificate2(X509Certificate2.CreateFromCertFile(certPath)));*/

            var tcpConfig = new SyslogTcpConfig
            {
                Host = "{TODO:HOST_NAME}",
                Port = 6514,
                Formatter = new Rfc3164Formatter(host: "12345678-1234-1234-1234", applicationName: "doppler", templateFormatter: new JsonFormatter()),
                Framer = new MessageFramer(FramingType.OCTET_COUNTING),
                SecureProtocols = SslProtocols.Tls13 | SslProtocols.Tls12,
                CertProvider = certProvider,
            };

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.TcpSyslog(tcpConfig)
                .CreateLogger();

            Log.Error("Hello there");
            try
            {
                Log.Error("Starting up");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application start-up failed");
            }
            finally
            {
                // store.Close();
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
