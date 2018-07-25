using Fiksu;
using Fiksu.Web;
using FiksuCore.Web.Http.Extensions;
using FiksuCore.Web.Internal;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using PackageRepository.Config;
using PackageRepository.Routing;
using PackageRepository.Services;
using System;

namespace PackageRepository {
    public class Startup {
        protected IConfiguration Configuration { get; }
        protected IHostingEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment env) {
            Configuration = configuration;
            Environment = env;
        }

        public void ConfigureServices(IServiceCollection services) {
            Enum.TryParse<ExecutionEnvironment>(Environment.EnvironmentName, true, out var env);
            services.AddSingleton(typeof(ExecutionEnvironment), env);

            services.AddDatabase($"Data Source={Configuration["database:source"]}");

            services.AddMvcCore().AddDataAnnotations().AddJsonFormatters(settings => {
                settings.ContractResolver = new DefaultContractResolver() { NamingStrategy = new CamelCaseNamingStrategy() };
                settings.Converters.Add(new StringEnumConverter());
            });

            services.Configure<RouteOptions>(opts => {
                opts.LowercaseUrls = true;
                opts.ConstraintMap.Add("semver", typeof(SemVerRouteConstraint));
                opts.ConstraintMap.Add("packagename", typeof(PackageNameRouteConstraint));
            });

            services.AddSingleton<IPackageService, PackageService>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.Use((context, next) => {
                context.Features.Set<IHttpContext>(new FiksuCoreHttpContext(context));
                return next.Invoke();
            });

            app.UseMvc();

            app.Use((context, next) => {
                var result = context.Response.NotFound(context.Request.Path);
                return result.ExecuteResultAsync(CreateActionContext(context));
            });
        }

        private static ActionContext CreateActionContext(HttpContext context) {
            return new ActionContext(context, new RouteData(), new ActionDescriptor());
        }
    }
}
