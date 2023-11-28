using ASP_Server.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AttendanceJournalLibrary;
using ASP_Server.CachedTables;
using ASP_Server;

var builder = WebApplication.CreateBuilder(args);

ConfigureServices();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


app.UseDeveloperExceptionPage();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
app.AddPostDB(new AttendanceJournalContext());

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(name: "teachers_id", pattern: "{controller=Teachers}/{action=Teacher}/{id?}");
app.MapControllerRoute(name: "teachers_search", pattern: "{controller=Teachers}/{action=Search}/");
app.MapControllerRoute(name: "teachers_add", pattern: "{controller=Teachers}/{action=Add}/");
app.MapControllerRoute(name: "teachers_edit", pattern: "{controller=Teachers}/{action=Edit}/");
app.MapControllerRoute(name: "teachers_delete", pattern: "{controller=Teachers}/{action=Delete}");

app.MapControllerRoute(name: "faculties_id", pattern: "{controller=Faculties}/{action=Faculty}/{id?}");
app.MapControllerRoute(name: "faculties_search", pattern: "{controller=Faculties}/{action=Search}/");
app.MapControllerRoute(name: "faculties_add", pattern: "{controller=Faculties}/{action=Add}/");
app.MapControllerRoute(name: "faculties_edit", pattern: "{controller=Faculties}/{action=Edit}/");
app.MapControllerRoute(name: "faculties_delete", pattern: "{controller=Faculties}/{action=Delete}/");

app.MapControllerRoute(name: "subjects_id", pattern: "{controller=Subjects}/{action=Subject}/{id?}");
app.MapControllerRoute(name: "subjects_search", pattern: "{controller=Subjects}/{action=Search}/");
app.MapControllerRoute(name: "subjects_add", pattern: "{controller=Subjects}/{action=Add}/");
app.MapControllerRoute(name: "subjects_edit", pattern: "{controller=Subjects}/{action=Edit}/");
app.MapControllerRoute(name: "subjects_delete", pattern: "{controller=Subjects}/{action=Delete}/");

app.MapControllerRoute(name: "classes_id", pattern: "{controller=Classes}/{action=Classe}/{id?}");
app.MapControllerRoute(name: "classes_search", pattern: "{controller=Classes}/{action=Search}/");
app.MapControllerRoute(name: "classes_add", pattern: "{controller=Classes}/{action=Add}/");
app.MapControllerRoute(name: "classes_edit", pattern: "{controller=Classes}/{action=Edit}/");
app.MapControllerRoute(name: "classes_delete", pattern: "{controller=Classes}/{action=Delete}/");

app.MapControllerRoute(name: "attendances_id", pattern: "{controller=Attendances}/{action=Attendance}/{id?}");
app.MapControllerRoute(name: "attendances_search",pattern: "{controller=Attendances}/{action=Search}/");
app.MapControllerRoute(name: "attendances_add", pattern: "{controller=Attendances}/{action=Add}/");
app.MapControllerRoute(name: "attendances_edit", pattern: "{controller=Attendances}/{action=Edit}/");
app.MapControllerRoute(name: "attendances_delete", pattern: "{controller=Attendances}/{action=Delete}/");

app.MapControllerRoute(name: "students_id", pattern: "{controller=Students}/{action=Student}/{id?}");
app.MapControllerRoute(name: "students_search", pattern: "{controller=Students}/{action=Search}/");
app.MapControllerRoute(name: "students_add", pattern: "{controller=Students}/{action=Add}/");
app.MapControllerRoute(name: "students_edit", pattern: "{controller=Students}/{action=Edit}/");
app.MapControllerRoute(name: "students_delete", pattern: "{controller=Students}/{action=Delete}/");

app.MapControllerRoute(name: "Account1", pattern: "{controller=Account}/{action=Main}/");
app.MapControllerRoute(name: "Account2", pattern: "{controller=Account}/{action=SignUp}/");
app.MapControllerRoute(name: "Account3", pattern: "{controller=Account}/{action=SignIn}/");
app.MapControllerRoute(name: "Account3", pattern: "{controller=Account}/{action=Edit}/");
app.MapControllerRoute(name: "Account3", pattern: "{controller=Account}/{action=EditPassword}/");

app.MapFallbackToController("ErrorPage", "Home");

app.Run();

void ConfigureServices()
{
    builder.Services.AddDbContext<AttendanceJournalContext>(options => options.UseSqlServer(new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build().GetConnectionString("DefaultConnection")));

	builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options => { options.LoginPath = new PathString("/Account/SignIn"); options.AccessDeniedPath = new PathString("/Account/SignIn"); });
	builder.Services.AddAuthorization(options => { options.AddPolicy("TeacherPolicy", policy => policy.RequireRole("Teacher")); options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin")); });
	
	builder.Services.AddTransient<ILoadingDB, LoadingDB>();
	
	builder.Services.AddTransient<TeachersCachService>();
	builder.Services.AddTransient<SubjectsCachService>();
	builder.Services.AddTransient<FacultiesCachService>();
	builder.Services.AddTransient<ClassesCachService>();
	builder.Services.AddTransient<AttendancesCachService>();
	builder.Services.AddMemoryCache();
	builder.Services.AddControllersWithViews(options =>
	{
		options.CacheProfiles.Add("Caching", new CacheProfile() { Duration = 2 * 6 + 240 });
	});
	builder.Services.AddDistributedMemoryCache();
	builder.Services.AddSession(options =>
	{
		options.Cookie.Name = ".AttendanceJournal.Session";
		options.Cookie.IsEssential = true;
	});

}


public static class InitializationDB
{
    public static IApplicationBuilder AddPostDB(this IApplicationBuilder builder, AttendanceJournalContext db)
    {
        return builder.UseMiddleware<AddPostsDB>(db);
    }
}

