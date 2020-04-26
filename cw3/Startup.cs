using cw3.DAL;
using cw3.DAL.Parsers;
using cw3.DTOs.Requests;
using cw3.Mappers;
using cw3.Middlewares;
using cw3.Models;
using cw3.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
            services.AddSingleton<IRequestLogger>(provider => new FileRequestLogger("logs.txt"));
            
            services.AddSingleton<SqlRowParser<Student>, StudentSqlRowParser>();
            services.AddSingleton<SqlRowParser<Enrollment>, EnrollmentSqlRowParser>();
            services.AddSingleton<SqlRowParser<Studies>, StudiesSqlRowParser>();

            services.AddSingleton<IMapper<EnrollStudentRequest, Student>, EnrollStudentToStudentMapper>();

            services.AddSingleton<IDbService, MsSqlDbService>();
            services.AddSingleton<ITransactionalDbService, MsSqlDbService>();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddMvc()
                .AddJsonOptions(options => { options.SerializerSettings.DateFormatString = "dd.MM.yyyy"; });

            services.AddSwaggerGen(config =>
                config.SwaggerDoc("v1", new OpenApiInfo {Title = "Students App API", Version = "v1"}));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IDbService service)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseSwagger();
            app.UseSwaggerUI(config => config.SwaggerEndpoint("/swagger/v1/swagger.json", "Students App API"));

            app.UseMiddleware<LoggingMiddleware>();
            
            app.Use(async (context, next) =>
            {
                if (!context.Request.Headers.ContainsKey("Index"))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Must pass index number");
                    return;
                }

                var index = context.Request.Headers["Index"].ToString();
                if (service.GetStudentByIndexNumber(index) == null)
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    await context.Response.WriteAsync("Student not found");
                    return;
                }

                await next();
            });
            
            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}