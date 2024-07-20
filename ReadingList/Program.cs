using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ReadingList.Models;


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
app.MapControllers();
app.MapRazorPages();
app.UseBlazorFrameworkFiles();
app.MapFallbackToFile("/{*path:nonfile}", "/index.html");



var context = app.Services.CreateScope().ServiceProvider.GetRequiredService<DataContext>();
SeedData.SeedDatabase(context);


app.Run();
