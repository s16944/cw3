using cw3.DAL;
using cw3.DAL.Parsers;
using cw3.DTOs.Requests;
using cw3.Mappers;
using cw3.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            services.AddSingleton<SqlRowParser<Student>, StudentSqlRowParser>();
            services.AddSingleton<SqlRowParser<Enrollment>, EnrollmentSqlRowParser>();
            services.AddSingleton<SqlRowParser<Studies>, StudiesSqlRowParser>();

            services.AddSingleton<IMapper<EnrollStudentRequest, Student>, EnrollStudentToStudentMapper>();

            services.AddSingleton<ITransactionalDbService, MsSqlDbService>();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddMvc()
                .AddJsonOptions(options => { options.SerializerSettings.DateFormatString = "dd.MM.yyyy"; });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}