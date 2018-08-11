﻿using Fiksu;
using Fiksu.Web;
using FiksuCore.Web.Http.Extensions;
using FiksuCore.Web.Internal;
using FiksuCore.Web.Routing;
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
using PackageRepository.Auth;
using PackageRepository.Config;
using PackageRepository.Services;
using System;
using System.Linq;
using System.Reflection;
using System.Security.Claims;

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
                settings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                settings.Converters.Add(new StringEnumConverter());
            });

            services.AddSingleton<IPackageService, PackageService>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.Use((context, next) => {
                context.Features.Set<IHttpContext>(new FiksuCoreHttpContext(context));

                // Temp fake auth
                var identity = new ClaimsIdentity("testauth");
                identity.AddClaim(new Claim(CustomClaimTypes.OrganisationId, "1"));
                identity.AddClaim(new Claim(CustomClaimTypes.Teams, "1"));
                identity.AddClaim(new Claim(identity.NameClaimType, "1"));

                context.User = new ClaimsPrincipal(identity);
            
                return next.Invoke();
            });

            app.UseMvc(opts => {
                var routes = RegexRouteFinder.FindAll(Assembly.GetExecutingAssembly()).ToList();
                opts.Routes.Add(new RegexRouter(opts.DefaultHandler, routes));
            });

            app.Use((context, next) => {
                var result = context.Response.Error(System.Net.HttpStatusCode.ServiceUnavailable, context.Request.Path);
                return result.ExecuteResultAsync(CreateActionContext(context));
            });
        }

        private static ActionContext CreateActionContext(HttpContext context) {
            return new ActionContext(context, new RouteData(), new ActionDescriptor());
        }
    }
}
