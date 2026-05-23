using Microsoft.EntityFrameworkCore;
using ReactApp1.Server.Data;
using ReactApp1.Server.Data.Seed;
using ReactApp1.Server.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc; // add
using ReactApp1.Server.Hubs;
using ReactApp1.Server.Services.Email;

namespace ReactWithASP.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var environmentValues = LoadEnvironmentFile();

            var builder = WebApplication.CreateBuilder(args);

            // Explicitly configure Kestrel to ensure HTTP and HTTPS endpoints are bound.
            // Bind HTTP to any IP so the app is reachable on the LAN, and bind HTTPS to the machine LAN IP.
            builder.WebHost.ConfigureKestrel(options =>
            {
                // HTTP endpoint on 0.0.0.0:5166
                options.ListenAnyIP(5166);

                // HTTPS endpoint on the LAN IP at 7047. Replace this IP if your machine's LAN address changes.
                // This will use the default certificate store. Browsers will warn unless the certificate is valid for the IP.
                options.Listen(System.Net.IPAddress.Parse("192.168.68.104"), 7047, listenOptions =>
                {
                    listenOptions.UseHttps();
                });
            });

            // Load configuration from appsettings
            var config = builder.Configuration;
            var mysqlHost = GetSetting(environmentValues, "MySQL_Host", "MySQL:Host") ?? config["MySQL:Host"] ?? "localhost";
            var mysqlDb = GetSetting(environmentValues, "MySQL_Db", "MySQL:Db") ?? config["MySQL:Db"];
            var mysqlUser = GetSetting(environmentValues, "MySQL_User", "MySQL:User") ?? config["MySQL:User"];
            var mysqlPassword = GetSetting(environmentValues, "MySQL_Password", "MySQL:Password") ?? config["MySQL:Password"];

            var mysqlConn = $"server={mysqlHost};port=3306;user={mysqlUser};password={mysqlPassword};database={mysqlDb};TreatTinyAsBoolean=false";

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseMySql(mysqlConn, new MySqlServerVersion(new Version(8, 0, 0)))
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
                options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
                    ? CookieSecurePolicy.SameAsRequest
                    : CookieSecurePolicy.Always;
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
            builder.Services.AddHttpClient();
            builder.Services.AddMemoryCache();
            builder.Services.AddSignalR();

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
            builder.Services.AddScoped<IPlantDescriptionAiService, PlantDescriptionAiService>();
            builder.Services.Configure<SmtpEmailOptions>(config.GetSection("Smtp"));

            var smtpHost = config["Smtp:Host"];
            if (!string.IsNullOrWhiteSpace(smtpHost))
            {
                builder.Services.AddScoped<IEmailSenderService, SmtpEmailSender>();
            }
            else
            {
                builder.Services.AddScoped<IEmailSenderService, FileEmailSender>();
            }

            var app = builder.Build();

            try
            {
                using var scope = app.Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate();
                IdentitySeeder.SeedAsync(scope.ServiceProvider).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                app.Logger.LogWarning(ex, "Database startup migration or seeding skipped because MySQL is unavailable. The app will still start, but database-backed endpoints may fail until the database is reachable.");
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapHub<CommentsHub>("/hubs/comments");
            app.MapFallbackToFile("index.html");

            app.Run();
        }

        private static string? GetSetting(IReadOnlyDictionary<string, string> environmentValues, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (environmentValues.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }

                var aliasKey = key.Replace(':', '_');
                if (environmentValues.TryGetValue(aliasKey, out value) && !string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return null;
        }

        private static Dictionary<string, string> LoadEnvironmentFile()
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var candidatePaths = new[]
            {
                Path.Combine(Directory.GetCurrentDirectory(), "ReactApp1.Server", ".env"),
                Path.Combine(Directory.GetCurrentDirectory(), ".env"),
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".env"),
            };

            foreach (var path in candidatePaths)
            {
                var fullPath = Path.GetFullPath(path);
                if (!File.Exists(fullPath))
                {
                    continue;
                }

                foreach (var rawLine in File.ReadAllLines(fullPath))
                {
                    var line = rawLine.Trim();
                    if (line.Length == 0 || line.StartsWith('#'))
                    {
                        continue;
                    }

                    var equalsIndex = line.IndexOf('=');
                    if (equalsIndex <= 0)
                    {
                        continue;
                    }

                    var key = line[..equalsIndex].Trim();
                    var value = line[(equalsIndex + 1)..].Trim().Trim('"');
                    values[key] = value;
                    Environment.SetEnvironmentVariable(key, value);

                    var aliasKey = key.Replace(':', '_');
                    if (!string.Equals(aliasKey, key, StringComparison.Ordinal))
                    {
                        values[aliasKey] = value;
                        Environment.SetEnvironmentVariable(aliasKey, value);
                    }
                }

                break;
            }

            return values;
        }
    }
}

































