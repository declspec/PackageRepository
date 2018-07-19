using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace PackageRepository {
    public static class Program {
        public static void Main(string[] args) {
            BuildHost<Startup>().Run();
        }

        public static void Configure(WebHostBuilderContext context, IConfigurationBuilder builder) {
            builder.AddEnvironmentVariables();
            builder.AddJsonFile("appsettings.json");
            builder.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName.ToLower()}.json", optional: true);
        }

        public static IWebHost BuildHost<TStartup>() where TStartup : class {
            return new WebHostBuilder()
                .UseKestrel()
#if DEBUG
                .UseIISIntegration()
#endif
                .ConfigureAppConfiguration(Configure)
                .UseStartup<TStartup>()
                .Build();
        }
    }
}
