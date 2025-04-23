using SimplePresave.Libraries.Model;
using SimplePresave.Libraries.Repositories;
using SimplePresave.Libraries.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    // TODO: Es imperativo ajustar la configuración de CORS para un ambiente de producción.
    // La que se utiliza a continuación es totalmente permisiva y sólo con fines de desarrollo:
    options.AddPolicy("Dev Policy", builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.Configure<AzureSettings>(builder.Configuration.GetSection("Azure"));
builder.Services.Configure<SpotifySettings>(builder.Configuration.GetSection("Spotify"));
builder.Services.AddHttpClient();
builder.Services.AddSingleton<ServiceBusRepository>();
builder.Services.AddSingleton<SpotifyService>();
builder.Services.AddSingleton<TokenRepository>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors("Dev Policy");

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
