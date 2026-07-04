using LinkUpPro.Application.Interfaces;
using LinkUpPro.Application.Mappings;
using LinkUpPro.Application.Services;
using LinkUpPro.Core.Entities;
using LinkUpPro.Infrastructure.Persistence;
using LinkUpPro.Infrastructure.Repositories;
using LinkUpPro.Shared.Services;
using LinkUpPro.Web.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddAutoMapper(typeof(PostProfile).Assembly);

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped(typeof(IGenericService<>), typeof(GenericService<>));

builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IUserSessionService, UserSessionService>();
builder.Services.AddScoped<ILinkBuilderService, LinkBuilderService>();

builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IPostReactionRepository, PostReactionRepository>();
builder.Services.AddScoped<IPostService, PostService>();

builder.Services.AddScoped<IFriendshipRepository, FriendshipRepository>();
builder.Services.AddScoped<IFriendRequestRepository, FriendRequestRepository>();
builder.Services.AddScoped<IFriendService, FriendService>();
builder.Services.AddScoped<IFriendRequestService, FriendRequestService>();

builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddScoped<IBattleshipRepository, BattleshipRepository>();
builder.Services.AddScoped<IBattleshipService, BattleshipService>();

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

    opt.User.RequireUniqueEmail = true;

    opt.Tokens.EmailConfirmationTokenProvider = "EmailConfirmationTokenProvider";

    opt.Tokens.PasswordResetTokenProvider = "PasswordResetTokenProvider";
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders()
.AddTokenProvider<EmailConfirmationTokenProvider<ApplicationUser>>("EmailConfirmationTokenProvider")
.AddTokenProvider<PasswordResetTokenProvider<ApplicationUser>>("PasswordResetTokenProvider");

builder.Services.Configure<EmailConfirmationTokenProviderOptions>(opt =>
    opt.TokenLifespan = TimeSpan.FromHours(24));

builder.Services.Configure<PasswordResetTokenProviderOptions>(opt =>
    opt.TokenLifespan = TimeSpan.FromHours(1));

builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.LoginPath = "/Account/Login";

    opt.LogoutPath = "/Account/Logout";

    opt.AccessDeniedPath = "/Home/Error";

    opt.ExpireTimeSpan = TimeSpan.FromMinutes(30);

    opt.SlidingExpiration = true;

    opt.Cookie.HttpOnly = true;

    opt.Cookie.SecurePolicy = CookieSecurePolicy.Always;

    opt.Cookie.SameSite = SameSiteMode.Lax;

    opt.Events.OnRedirectToLogin = context =>
    {
        // Solo se considera "inactividad" si el navegador todavía envía la cookie
        // de sesión (ticket expirado); un visitante anónimo va al login limpio.
        var hadSession = context.Request.Cookies.ContainsKey(".AspNetCore.Identity.Application");

        context.Response.Redirect(hadSession
            ? "/Account/Login?message=inactivity"
            : "/Account/Login");

        return Task.CompletedTask;
    };

});

var app = builder.Build();

app.UseExceptionHandler("/Home/Error");

if (!app.Environment.IsDevelopment())
{
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
