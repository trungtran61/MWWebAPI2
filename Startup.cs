using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MWWebAPI2
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
      // Get JWT Token Settings from JwtSettings.json file
      AppSettings settings;
      settings = GetAppSettings();
      // Create singleton of JwtSettings
      services.AddSingleton<AppSettings>(settings);
     
      // Register Jwt as the Authentication service
      services.AddAuthentication(options =>
      {
        options.DefaultAuthenticateScheme = "JwtBearer";
        options.DefaultChallengeScheme = "JwtBearer";
      })
      .AddJwtBearer("JwtBearer", jwtBearerOptions =>
      {
        jwtBearerOptions.TokenValidationParameters =
            new TokenValidationParameters
            {
              ValidateIssuerSigningKey = true,
              IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(settings.JWTKey)),
              ValidateIssuer = true,
              ValidIssuer = settings.JWTIssuer,

              ValidateAudience = true,
              ValidAudience = settings.JWTAudience,

              ValidateLifetime = true,
              ClockSkew = TimeSpan.FromMinutes(
                       settings.JWTMinutesToExpiration)
            };
      });

      services.AddAuthorization(cfg =>
      {
        // NOTE: The claim type and value are case-sensitive
        cfg.AddPolicy("CanAccessProducts", p => p.RequireClaim("CanAccessProducts", "true"));
      });

      services.AddCors();
      services.AddMemoryCache();
      services.AddMvc()      
      .AddJsonOptions(options =>        
        options.SerializerSettings.ContractResolver =
        new CamelCasePropertyNamesContractResolver());      
      /*
      .AddMvcOptions(options =>
        {
            options.OutputFormatters.Add(new PascalCaseJsonProfileFormatter());
        });
      */
    }

public class PascalCaseJsonProfileFormatter : JsonOutputFormatter
{
    public PascalCaseJsonProfileFormatter() : base(new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() }, 
        ArrayPool<char>.Shared)
    {
        SupportedMediaTypes.Clear();
        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/json;profile=\"https://en.wikipedia.org/wiki/PascalCase\""));
    }
}
    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }

      app.UseCors(
        options => options.WithOrigins(
          "http://localhost:4200").AllowAnyMethod().AllowAnyHeader()
      );

      app.UseAuthentication();

      app.UseMvc();
    }

        public AppSettings GetAppSettings()
        {
            AppSettings settings = new AppSettings();

            settings.JWTKey = Configuration["JwtSettings:key"];
            settings.JWTAudience = Configuration["JwtSettings:audience"];
            settings.JWTIssuer = Configuration["JwtSettings:issuer"];
            settings.JWTMinutesToExpiration =
             Convert.ToInt32(Configuration["JwtSettings:minutesToExpiration"]);
            settings.MWConnectionString = Configuration["ConnectionStrings:MachineWorkCS"];
            settings.SecurityConnectionString = Configuration["ConnectionStrings:SecurityCS"];
            settings.ImageUrl = Configuration["AppSettings:ImageUrl"];
            return settings;
        }

    }
}
