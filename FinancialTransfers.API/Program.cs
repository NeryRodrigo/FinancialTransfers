using FinancialTransfers.API.Workers;
using FinancialTransfers.Application.Interfaces;
using FinancialTransfers.Application.Services;
using FinancialTransfers.Infraestructure.Messaging;
using FinancialTransfers.Infraestructure.Persistence;
using FinancialTransfers.Infraestructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Serilog; 

var builder = WebApplication.CreateBuilder(args);

// 1. CONFIGURACIÓN DE LOGGING (Senior Touch: Serilog)
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// 2. SERVICIOS DE CONTROLADORES Y SWAGGER
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Financial Transfers API", Version = "v1" });
});

// 3. BASE DE DATOS (Entity Framework Core)
/*builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));*/
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null)
    ));

// 4. INYECCIÓN DE DEPENDENCIAS (DI)
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IRiskService, RiskService>();
builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();

// MediatR (Para CQRS)
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(IRiskService).Assembly));

// 5. CONSUMIDOR DE RESPUESTAS (Background Service)
builder.Services.AddHostedService<PaymentResponseConsumerWorker>();

var app = builder.Build();

// 6. AUTO-MIGRACIÓN DE BASE DE DATOS (El toque maestro)
// Esto crea la DB y las tablas automáticamente al iniciar
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var logger = services.GetRequiredService<ILogger<Program>>();

    for (int i = 0; i < 10; i++) // Intentar 10 veces
    {
        try
        {
            logger.LogInformation("Verificando base de datos...");
            context.Database.Migrate(); // Esto crea la DB y las tablas
            logger.LogInformation("Base de datos lista y migrada.");
            break;
        }
        catch (Exception ex)
        {
            logger.LogWarning("SQL Server no está listo... reintentando en 5 segundos (Intento {Intento})", i + 1);
            System.Threading.Thread.Sleep(5000);
        }
    }
}

// 7. MIDDLEWARES
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();