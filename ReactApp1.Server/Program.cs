using Microsoft.EntityFrameworkCore;
using ReactApp1.Server.Data;
using ReactApp1.Server.Services;


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

            // Connection string without Unicode or CharSet
            var mysqlConn = $"server=localhost;port=3306;user={mysqlUser};password={mysqlPassword};database={mysqlDb};TreatTinyAsBoolean=false";

            // Configure Entity Framework Core with MySQL
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseMySql(mysqlConn, ServerVersion.AutoDetect(mysqlConn))
            );

            // Add framework services
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // --- Register custom application services ---
            // Plants
            builder.Services.AddScoped<IGetPlantService, GetPlantService>();
            builder.Services.AddScoped<ISavePlantService, SavePlantService>();
            builder.Services.AddScoped<IDeletePlantService, DeletePlantService>();

            // Pests
            builder.Services.AddScoped<IGetPestService, GetPestService>();
            builder.Services.AddScoped<ISavePestService, SavePestService>();
            builder.Services.AddScoped<IDeletePestService, DeletePestService>();

            // Comments
            builder.Services.AddScoped<IGetCommentService, GetCommentService>();
            builder.Services.AddScoped<ISaveCommentService, SaveCommentService>();
            builder.Services.AddScoped<IDeleteCommentService, DeleteCommentService>();

            // Soils
            builder.Services.AddScoped<IGetSoilService, GetSoilService>();
            builder.Services.AddScoped<ISaveSoilService, SaveSoilService>();
            builder.Services.AddScoped<IDeleteSoilService, DeleteSoilService>();

            var app = builder.Build();

            // Middleware setup
            app.UseDefaultFiles();
            app.UseStaticFiles();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();

            // Map controllers
            app.MapControllers();

            // Start the web host
            app.Run();
        }
    }
}

















