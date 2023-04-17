using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using SendGrid;
using SendGrid.Extensions.DependencyInjection;
using TodoList.Domain;
using TodoList.Domain.BLL.Interfaces;
using TodoList.Domain.BLL.Services;
using TodoList.Domain.DAL.Entities;
using TodoList.Domain.DAL.Interfaces;
using TodoList.Domain.DAL.Repositories;
using TodoList.MVC.Mappers;
using TodoList.MVC.Services;

namespace TodoList.MVC
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.AddAntiforgery();
            services.AddAutoMapper(typeof(MappingProfile));

            services.AddDbContext<ApplicationDbContext>(opts =>
            {
                opts.UseSqlServer(
                    Configuration["ConnectionStrings:ApplicationDbContextConnection"]);
            });

            services.AddDefaultIdentity<ApplicationUser>(options
                => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddScoped<IRepository<ToDoList>, ToDoListRepository>();
            services.AddScoped<IRepository<ToDoListTask>, ToDoListTaskRepository>();
            services.AddScoped<IToDoListService, ToDoListService>();
            services.AddScoped<IToDoListTaskService, ToDoListTaskService>();

            services.Configure<EmailSenderOptions>(Configuration.GetSection("EmailSender"));
            services.AddSendGrid(options =>
            {
                options.ApiKey = Configuration.GetSection("EmailSender:SendGridKey").Value;
            });

            services.AddTransient<IEmailSender, EmailSender>();
        }

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

            app.UseStatusCodePages();
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapRazorPages();
            });
        }
    }
}
