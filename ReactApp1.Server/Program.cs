using Microsoft.EntityFrameworkCore;
using ReactApp1.Server.Data;
using ReactApp1.Server.Data.Seed;
using ReactApp1.Server.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc; // add

namespace ReactWithASP.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Load configuration from appsettings
            var config = builder.Configuration;
            var mysqlDb = config["MySQL:Db"];
            var mysqlUser = config["MySQL:User"];
            var mysqlPassword = config["MySQL:Password"];

            var mysqlConn = $"server=localhost;port=3306;user={mysqlUser};password={mysqlPassword};database={mysqlDb};TreatTinyAsBoolean=false";

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseMySql(mysqlConn, ServerVersion.AutoDetect(mysqlConn))
            );

            builder.Services
                .AddIdentityCore<IdentityUser>(options =>
                {
                    options.User.RequireUniqueEmail = true;
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequiredLength = 6;
                })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
                options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
            })
            .AddCookie(IdentityConstants.ApplicationScheme, options =>
            {
                options.Cookie.Name = ".ReactApp1.Auth";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // ensure consistent secure behavior
                options.SlidingExpiration = true;

                options.Events.OnRedirectToLogin = ctx => { ctx.Response.StatusCode = StatusCodes.Status401Unauthorized; return Task.CompletedTask; };
                options.Events.OnRedirectToAccessDenied = ctx => { ctx.Response.StatusCode = StatusCodes.Status403Forbidden; return Task.CompletedTask; };
            });

            builder.Services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-XSRF-TOKEN";
                options.Cookie.Name = "XSRF-TOKEN";
                options.Cookie.HttpOnly = false;
                options.Cookie.SameSite = SameSiteMode.Lax;
            });

            // Use AddControllersWithViews so MVC ViewFeatures (and the antiforgery filter) are registered
            builder.Services.AddControllersWithViews(options =>
            {
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // --- Register custom application services ---
            builder.Services.AddScoped<IGetPlantService, GetPlantService>();
            builder.Services.AddScoped<ISavePlantService, SavePlantService>();
            builder.Services.AddScoped<IDeletePlantService, DeletePlantService>();

            builder.Services.AddScoped<IGetPestService, GetPestService>();
            builder.Services.AddScoped<ISavePestService, SavePestService>();
            builder.Services.AddScoped<IDeletePestService, DeletePestService>();

            builder.Services.AddScoped<IGetCommentService, GetCommentService>();
            builder.Services.AddScoped<ISaveCommentService, SaveCommentService>();
            builder.Services.AddScoped<IDeleteCommentService, DeleteCommentService>();

            builder.Services.AddScoped<IGetSoilService, GetSoilService>();
            builder.Services.AddScoped<ISaveSoilService, SaveSoilService>();
            builder.Services.AddScoped<IDeleteSoilService, DeleteSoilService>();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                IdentitySeeder.SeedAsync(scope.ServiceProvider).GetAwaiter().GetResult();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}

































