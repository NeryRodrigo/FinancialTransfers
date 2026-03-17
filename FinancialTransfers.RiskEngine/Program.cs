using FinancialTransfers.Application.Interfaces;
using FinancialTransfers.Application.Services;
using FinancialTransfers.Infraestructure.Messaging;
using FinancialTransfers.Infraestructure.Persistence;
using FinancialTransfers.Infraestructure.Repositories;
using FinancialTransfers.RiskEngine;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IRiskService, RiskService>();
builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();

builder.Services.AddHostedService<RiskEvaluationWorker>();

var host = builder.Build();
host.Run();