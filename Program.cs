
using DemoWebAPI.Data;
using DemoWebAPI.Models;
using DemoWebAPI.Services;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Text;
using System.Text.Json;
/*using Newtonsoft.Json.Serialization;*/

namespace DemoWebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            //builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("Tasks"));
            builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
            }).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();
            builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["JWT:ValidAudience"],
                    ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]))
                };
            });
            builder.Services.AddControllers();
            builder.Services.AddScoped<SerilogLogger>();
            builder.Services.AddScoped<AesEncryption>();
            builder.Services.AddMvc().AddNewtonsoftJson(options =>
                options.SerializerSettings.ContractResolver = new DefaultContractResolver()
            );
            /*builder.Services.AddAntiforgery(options => options.HeaderName = "X-XFRS-TOKEN");
            builder.Services.AddDataProtection().SetApplicationName("DemoWebAPI").PersistKeysToFileSystem(new DirectoryInfo(@"/var/dpkeys/")).SetDefaultKeyLifetime(TimeSpan.FromDays(180));*/

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            /*builder.Services.AddScoped<AntiforgeryMiddleware>();*/
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();
            /*app.UseMiddleware<AntiforgeryMiddleware>();*/
            /*app.UseMiddleware<CustomAuthorizationMiddleware>();*/
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            
            app.UseAuthorization();
            var antiforgery = app.Services.GetRequiredService<IAntiforgery>();

            /*app.Use((context, next) =>
            {
                var requestPath = context.Request.Path.Value;
                var tokenSet = antiforgery.GetAndStoreTokens(context);
                context.Response.Cookies.Append("XSRF-TOKEN", tokenSet.RequestToken!,
                new CookieOptions { HttpOnly = false });
                *//*if (string.Equals(requestPath, "/", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(requestPath, "/index.html", StringComparison.OrdinalIgnoreCase))
                {
                    
                }*//*

                return next(context);
            });*/

            app.MapControllers();

            PrepDB.PrepPopulation(app);

            

            app.Run();

            
        }
    }
}
