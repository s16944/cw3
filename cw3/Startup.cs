using System.Text;
using cw3.DAL;
using cw3.DAL.Parsers;
using cw3.DTOs.Requests;
using cw3.Mappers;
using cw3.Middlewares;
using cw3.Models;
using cw3.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace cw3
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
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidIssuer = "Gakko",
                        ValidAudience = "Students",
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]))
                    };
                });

            services.AddSingleton<IRequestLogger>(provider => new FileRequestLogger("logs.txt"));

            services.AddSingleton<SqlRowParser<Student>, StudentSqlRowParser>();
            services.AddSingleton<SqlRowParser<Enrollment>, EnrollmentSqlRowParser>();
            services.AddSingleton<SqlRowParser<Studies>, StudiesSqlRowParser>();
            services.AddSingleton<SqlRowParser<Role>, RoleSqlRowParser>();

            services.AddSingleton<IMapper<EnrollStudentRequest, Student>, EnrollStudentToStudentMapper>();

            services.AddSingleton<IDbService, MsSqlDbService>();
            services.AddSingleton<ITransactionalDbService, MsSqlDbService>();

            services.AddSwaggerGen(config =>
                config.SwaggerDoc("v1", new OpenApiInfo {Title = "Students App API", Version = "v1"}));
            
            services.AddControllers().AddXmlSerializerFormatters();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(config => config.SwaggerEndpoint("/swagger/v1/swagger.json", "Students App API"));

            app.UseMiddleware<LoggingMiddleware>();

            app.UseHttpsRedirection();

            app.UseRouting();
            
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}