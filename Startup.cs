using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderItApp.Data;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace OrderItApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // add MVC
            services.AddControllersWithViews();

            // configure in‑memory EF context
            services.AddDbContext<OrderContext>(options =>
                options.UseInMemoryDatabase("OrderDb"));

            // register MongoDB
            services.AddSingleton<IMongoClient>(new MongoClient(Configuration["MongoDb:ConnectionString"]));
            services.AddScoped<IUserService, UserService>();

            // password hasher provided by Identity library
            services.AddScoped<Microsoft.AspNetCore.Identity.IPasswordHasher<OrderItApp.Models.User>,
                                Microsoft.AspNetCore.Identity.PasswordHasher<OrderItApp.Models.User>>();

            // cookie authentication with production‑grade settings
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Account/Login";
                    options.Cookie.HttpOnly = true;
                    options.SlidingExpiration = true;
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                });
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
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // authentication must run before authorization
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Orders}/{action=Index}/{id?}");
            });
        }
    }
}