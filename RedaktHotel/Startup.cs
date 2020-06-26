using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Redakt.BackOffice.Onboarding;
using RedaktHotel.BackOfficeExtensions;

namespace RedaktHotel
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            // Startup filters are executed in reverse order of adding; add HotelSiteSeeder first so it gets executed after RedaktStartupFilter.
            services.AddTransient<IStartupFilter, HotelSiteSeeder>();

            services.AddRedakt(Configuration, true)
                .AddLiteDbDataStore()
                .AddLiteDbFileStore()
                .AddIdentityServer()
                .AddContentManagement()
                .AddBackOffice();

            // Startup filters are executed in reverse order of adding; add HotelSiteSeeder first so it gets executed after RedaktStartupFilter.
            services.AddTransient<IBackOfficeOnboardingStep, CustomOnboardingStep>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //var supportedCultures = new[] { "en", "nl" };
            //app.UseRequestLocalization(options => options
            //    .AddSupportedCultures(supportedCultures)
            //    .AddSupportedUICultures(supportedCultures)
            //    .SetDefaultCulture(supportedCultures[0]));

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            //app.UseStaticFiles(new StaticFileOptions
            //{
            //    OnPrepareResponse = ctx =>
            //    {
            //        // Cache static files for 30 days
            //        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=2592000");
            //        ctx.Context.Response.Headers.Append("Expires", DateTime.UtcNow.AddDays(30).ToString("R", CultureInfo.InvariantCulture));
            //    }
            //});

            app.UseRouting();

            app.UseAuthorization();

            // Redakt middleware registration
            app.UseRedaktIdentityServer();
            app.UseRedaktBackOffice();
            app.UseRedaktContentManagement();

            // Default MVC endpoint registration
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
