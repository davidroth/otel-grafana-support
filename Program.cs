using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddOpenTelemetry()
            .WithMetrics(builder =>
            {
                builder.AddAspNetCoreInstrumentation();
                builder.AddHttpClientInstrumentation();
                builder.AddRuntimeInstrumentation();
                builder.AddProcessInstrumentation();

                builder.AddPrometheusExporter();

            })
            .WithTracing(builder =>
            {
                builder.AddAspNetCoreInstrumentation();
                builder.AddHttpClientInstrumentation();
                builder.AddEntityFrameworkCoreInstrumentation();

                builder.AddOtlpExporter(x =>
                {
                    x.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                    var settings = new
                    {
                        Username = "",
                        Password = ""
                    };
                    x.Headers = "Authorization=Basic " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{settings.Username}:{settings.Password}"));
                    //x.Endpoint = new Uri("https://tempo-eu-west-0.grafana.net");
                    x.Endpoint = new Uri("https://tempo-prod-10-prod-eu-west-2.grafana.net");
                });

                builder.AddConsoleExporter();
            });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
