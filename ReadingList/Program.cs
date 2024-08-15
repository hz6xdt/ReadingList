using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ReadingList.Models;
using ReadingList.Ui.Services;
using System.Security.Claims;
using System.Text;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddHttpLogging(opts =>
{
    opts.LoggingFields = HttpLoggingFields.RequestMethod
    | HttpLoggingFields.RequestPath
    | HttpLoggingFields.RequestQuery
    | HttpLoggingFields.RequestHeaders
    | HttpLoggingFields.RequestBody
    | HttpLoggingFields.Response
    | HttpLoggingFields.Duration;
});

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("r1", new OpenApiInfo
    {
        Title = "ReadingList",
        Version = "r1",
        Description = "API for managing ReadingList items.",
        Contact = new OpenApiContact
        {
            Name = "dvc"
        }
    });
});


builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlServer(builder.Configuration["ConnectionStrings:ReadingListConnection"]);
    options.EnableSensitiveDataLogging(true);
});

builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<DataContext>();
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequiredLength = 12;
    options.User.RequireUniqueEmail = true;
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
}).AddCookie(opts =>
{
    opts.Events.DisableRedirectForPath(e => e.OnRedirectToLogin, "/api", StatusCodes.Status401Unauthorized);
    opts.Events.DisableRedirectForPath(e => e.OnRedirectToAccessDenied, "/api", StatusCodes.Status403Forbidden);
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["Data:JwtSecret"]!)),
        ValidateAudience = false,
        ValidateIssuer = false
    };
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async ctx =>
        {
            var usrmgr = ctx.HttpContext.RequestServices.GetRequiredService<UserManager<IdentityUser>>();
            var signinmgr = ctx.HttpContext.RequestServices.GetRequiredService<SignInManager<IdentityUser>>();
            string? username = ctx.Principal?.FindFirst(ClaimTypes.Name)?.Value;
            if (username != null)
            {
                IdentityUser? idUser = await usrmgr.FindByNameAsync(username);
                if (idUser != null)
                {
                    ctx.Principal = await signinmgr.CreateUserPrincipalAsync(idUser);
                }
            }
        }
    };
});


builder.Services.AddTransient<IBooksRepository, BooksRepository>();

builder.Services.AddControllers();
builder.Services.AddRazorPages();




var app = builder.Build();




if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseWebAssemblyDebugging();
}



app.UseHttpsRedirection();

app.UseHttpLogging();


app.UseMiddleware<ReadingList.TestMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/r1/swagger.json", "ReadingList");
});



app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();
app.UseBlazorFrameworkFiles();
app.MapFallbackToFile("/{*path:nonfile}", "/index.html");



var context = app.Services.CreateScope().ServiceProvider.GetRequiredService<DataContext>();
SeedData.SeedDatabase(context);
IdentitySeedData.CreateAdminAccount(app.Services, app.Configuration);


app.Run();
