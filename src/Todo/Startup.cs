using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Diagnostics;
using Newtonsoft.Json;
using Todo.Formatters.HAL;
using Todo.Formatters.Siren;
using Todo.Infrastructure.ProblemJson;
using Todo.Models;

namespace Todo
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // our in-memory task store
            services.AddSingleton<TodoRepository>(servicesProvider => TodoRepositoryFactory.Build());

            services.AddMvc()
                .AddJsonOptions(jsonOptions => jsonOptions.SerializerSettings.NullValueHandling = NullValueHandling.Ignore);

            services.AddSirenFormatters();
            services.AddHalFormatters();
            services.AddProblemJson();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseMvc();
            app.UseCors(policyBuilder => policyBuilder
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowAnyOrigin());

        }
    }
}
