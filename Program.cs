using BASE.Areas.Backend.Service;
using BASE.Extensions;
using BASE.Filters;
using BASE.Models.DB;
using BASE.Scheduler;
using BASE.Service;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Quartz.Impl;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.Unicode;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// DB
builder.Services.AddDbContext<DBContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// 防止 Jsonresult 輸出含循環參考
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation()
                .AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services.AddResponseCaching();

builder.Services.AddRazorPages();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

builder.Services.Configure<CookieTempDataProviderOptions>(options =>
{
    options.Cookie.IsEssential = true;
});



// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// HttpContext
builder.Services.AddHttpContextAccessor();

builder.Services.AddAntiforgery(option =>
{
    option.FormFieldName = "__X-XSRFVerificationToken";
    option.HeaderName = "X-XSRF-TOKEN";
});

/* 
* [LIFE_TIME]
* Transient：每次注入時都回傳新的物件（Controller > Service > View 皆為不同個物件）。
* Singleton：僅於第一次注入時建立新物件，後面注入時會拿到第一次建立的物件(只要執行緒還活著)。
* Scoped：在同一個Request中會回傳同一個物件（Controller > Service > View 皆為同一個物件）。
*/

// Dependency Injection
builder.Services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.All));
builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
builder.Services.AddSingleton<ITempDataProvider, CookieTempDataProvider>();

builder.Services.AddScoped<FileService>();
builder.Services.AddScoped<MailService>();
builder.Services.AddScoped<AllCommonService>();
builder.Services.AddScoped<CommonService>();
builder.Services.AddScoped<ExportService>();
builder.Services.AddScoped<ImportService>();

//Backend Service
builder.Services.AddScoped<AuthorityService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<MemberService>();
builder.Services.AddScoped<EventService>();
builder.Services.AddScoped<AdvertiseService>();
builder.Services.AddScoped<RelationLinkService>();
builder.Services.AddScoped<NewsService>();
builder.Services.AddScoped<YouTubeVideoService>();
builder.Services.AddScoped<AlbumService>();
builder.Services.AddScoped<ConsultService>();
builder.Services.AddScoped<HrArticleService>();
builder.Services.AddScoped<HrPackageService>();
builder.Services.AddScoped<B_ContactUsService>();
builder.Services.AddScoped<B_ProjectService>();
builder.Services.AddScoped<B_ProjectModifyService>();
builder.Services.AddScoped<PromoteService>();
builder.Services.AddScoped<QuizService>();

//Frontend Servie
builder.Services.AddScoped<BASE.Areas.Frontend.Service.NewsService>();
builder.Services.AddScoped<BASE.Areas.Frontend.Service.YouTubeService>();
builder.Services.AddScoped<BASE.Areas.Frontend.Service.ActivityService>();
builder.Services.AddScoped<BASE.Areas.Frontend.Service.AlbumService>();
builder.Services.AddScoped<BASE.Areas.Frontend.Service.OnePageService>();
builder.Services.AddScoped<BASE.Areas.Frontend.Service.ProjectService>();
builder.Services.AddScoped<BASE.Areas.Frontend.Service.HRService>();
builder.Services.AddScoped<BASE.Areas.Frontend.Service.ConsultService>();
builder.Services.AddScoped<BASE.Areas.Frontend.Service.AdService>();
builder.Services.AddScoped<BASE.Areas.Frontend.Service.RelationLinkService>();
builder.Services.AddScoped<BASE.Areas.Frontend.Service.QuizService>();


/* Quartz */
builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionScopedJobFactory();

    //// 註冊 Job 
    //q.AddJob<NewsMailJob>().AddTrigger(opts => opts
    //    .ForJob(nameof(NewsMailJob)) // 傳入與 Job class 相同名稱
    //    .WithIdentity(nameof(NewsMailJob) + "-trigger") // Trigger 名稱
    //    .StartNow()
    //    // 時間排程設定
    //    .WithDailyTimeIntervalSchedule(x => x
    //        .OnEveryDay() // 每日執行
    //        .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(15, 35))
    //        //.StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(DateTime.Now.Hour, DateTime.Now.Minute + 1)) // 開站後一分鐘後馬上執行(測試用)
    //        .WithIntervalInMinutes(5) // 每 5分鐘 執行一次
    //        .EndingDailyAfterCount(4) // 每日最大執行次數，結束後停止
    //        .WithMisfireHandlingInstructionIgnoreMisfires()));
});


// Add the Quartz.NET hosted service
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseResponseCaching();

app.UseAuthorization();

/* 必須在 UseRouting 之後，在 MapRazorPages、MapControllerRoute 之前 */
app.UseSession();

app.UseEndpoints(endpoints =>
{
    //預設路由
    endpoints.MapAreaControllerRoute(
        name: "default",
        areaName: "Frontend",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    //匹配所有Area路由
    endpoints.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

    //匹配所有Area路由
    //endpoints.MapControllerRoute(
    //    name: "areas",
    //    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
});



app.MapRazorPages();

app.Run();
