using LinkUpPro.Application.Interfaces;
using LinkUpPro.Application.Services;
using LinkUpPro.Core.Entities;
using LinkUpPro.Infrastructure.Persistence;
using LinkUpPro.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IEmailService, EmailService>();



builder.Services.AddHttpContextAccessor();
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(builder.Configuration
    .GetConnectionString("DefaultConnection")));

builder.Services.Configure<EmailSettings>(builder.Configuration
    .GetSection("EmailSettings"));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(opt => 
{
    opt.Lockout.MaxFailedAccessAttempts = 5;

    opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

    opt.Password.RequireDigit = true;

    opt.Password.RequireUppercase = true;

    opt.Password.RequireLowercase = true;

    opt.Password.RequireNonAlphanumeric = true;

    opt.Password.RequiredLength = 8;

    opt.SignIn.RequireConfirmedEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.LoginPath = "/Account/Login";

    opt.LogoutPath = "/Account/Logout";

    opt.AccessDeniedPath = "/Home/Error";
    
    opt.ExpireTimeSpan = TimeSpan.FromMinutes(30);

    opt.SlidingExpiration = true;

});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
