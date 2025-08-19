using System.Security.Claims;
using SpotifyController.Interfaces;
using SpotifyController.Model;
using SpotifyController.Services;
using SpotifyController.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication;

using FlaskAuthSDK;

namespace SpotifyController
{
    public class Startup
    {
        public const string FlaskAuthScheme = "FlaskAuth";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            Console.WriteLine($".NET Version: {Environment.Version}");
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllersWithViews();
            //// In production, the React files will be served from this directory
            //services.AddSpaStaticFiles(configuration =>
            //{
            //    configuration.RootPath = "ClientApp/build";
            //});
            services.AddHttpClient<SpotifyAPIService>();
            services.AddHttpClient<SpotifyTokenManager>();

            services.AddScoped<IDataRepository, SqlRepo>();
            services.AddScoped<FlaskAuthClient>(provider =>
            {
                // You can pull configuration, env vars, or other services here
                var authUri = "http://authentication:5000";
                var serviceName = "spotify";
                var spotifyServiceToken = Environment.GetEnvironmentVariable("SpotifyServiceLocalAuthToken");

                return new FlaskAuthClient(authUri, serviceName, spotifyServiceToken);
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = FlaskAuthScheme; // so [Authorize] uses it
                options.DefaultChallengeScheme    = FlaskAuthScheme;

            }).AddScheme<AuthenticationSchemeOptions, FlaskAuthHandler>(FlaskAuthScheme, options => { });

            services.AddAuthorization();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }


            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "API/{controller}/{action}/{id?}");
            });
        }
    }
}
