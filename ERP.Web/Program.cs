using ERP.Infrastructure.Database;
using ERP.Infrastructure.Helpers;
using ERP.Infrastructure.Repositories.Auth;
using ERP.Infrastructure.Services;
using ERP.Web.Conventions;
using ERP.Web.Services;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    options.Conventions.Add(new RequirePermissionConvention());
    options.Filters.Add(new AuthorizeFilter()); // Require login globally
    //options.Filters.Add<ModuleSelectionFilter>();
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/Home/AccessDenied";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);

        options.Cookie.IsEssential = true;
        options.Cookie.Name = "ERPWebAuth";
        options.Cookie.HttpOnly = true; // Prevent JS access
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // HTTPS only
        options.Cookie.SameSite = SameSiteMode.Strict;

        options.Cookie.MaxAge = null;
        options.Cookie.Expiration = null;
        options.Events.OnSigningIn = ctx =>
        {
            ctx.Properties.IsPersistent = false;
            return Task.CompletedTask;
        };
    });

builder.Services.AddDistributedMemoryCache();
builder.Services.AddMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = "ERPWeb.AntiForgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

builder.Services.AddScoped<DbConnection>();
builder.Services.AddScoped<IErrorLogger, ErrorLogger>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddSingleton<PermissionCache>();
builder.Services.AddScoped<PermissionHelper>();
//builder.Services.AddScoped<ModuleSelectionFilter>();
builder.Services.AddHttpContextAccessor();

// making the option mandatory - Permission attribute // This attribute triggering often, so used another method
//builder.Services.AddControllersWithViews(options =>
//{
//    options.Conventions.Add(new RequirePermissionConvention());
//});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();


app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    if (HttpMethods.IsPost(context.Request.Method) &&
        !context.Request.Path.StartsWithSegments("/api"))
    {
        var antiforgery = context.RequestServices.GetRequiredService<IAntiforgery>();
        try
        {
            await antiforgery.ValidateRequestAsync(context);
        }
        catch (AntiforgeryValidationException ex)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Antiforgery failed on {Path} for user {User}", context.Request.Path, context.User?.Identity?.Name);
            throw;
        }
    }
    await next();
});


app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            var logger = context.RequestServices.GetRequiredService<IErrorLogger>();
            var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();

            if (exceptionHandlerPathFeature != null)
            {
                logger.Log(exceptionHandlerPathFeature.Error, context, "Unhandled exception");
            }

            context.Response.Redirect("/Home/Error");
        });

    });
    app.UseHsts();
}
app.Run();
