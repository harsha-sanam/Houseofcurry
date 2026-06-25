using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.FileProviders;
using MySqlConnector;
using Square;
using Square.Exceptions;
using System;
using System.IO;
using SquareEnvironment = Square.Environment;

var builder = WebApplication.CreateBuilder(args);

// Read critical settings from environment variables only
var mysqlHost = System.Environment.GetEnvironmentVariable("MYSQL_HOST") ?? "mysql";
var mysqlPort = System.Environment.GetEnvironmentVariable("MYSQL_PORT") ?? "3306";
var mysqlDb = System.Environment.GetEnvironmentVariable("MYSQL_DATABASE") ?? "paymentgateway";
var mysqlUser = System.Environment.GetEnvironmentVariable("MYSQL_USER") ?? "paymentuser";
var mysqlPassword = System.Environment.GetEnvironmentVariable("MYSQL_PASSWORD") ?? throw new InvalidOperationException("MYSQL_PASSWORD must be set in the environment");
var squareAccessToken = System.Environment.GetEnvironmentVariable("SQUARE_ACCESS_TOKEN") ?? throw new InvalidOperationException("SQUARE_ACCESS_TOKEN must be set in the environment");
var squareAppId = System.Environment.GetEnvironmentVariable("SQUARE_APPLICATION_ID") ?? "";
var squareLocationId = System.Environment.GetEnvironmentVariable("SQUARE_LOCATION_ID") ?? "";
var squareEnvRaw = System.Environment.GetEnvironmentVariable("SQUARE_ENVIRONMENT") ?? "production";

var mysqlConnectionString = new MySqlConnectionStringBuilder
{
    Server = mysqlHost,
    Port = uint.Parse(mysqlPort),
    Database = mysqlDb,
    UserID = mysqlUser,
    Password = mysqlPassword,
    SslMode = MySqlSslMode.None,
    AllowPublicKeyRetrieval = true,
}.ConnectionString;

// Configure services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton(new DatabaseConfig(mysqlConnectionString));

// Square client registration
SquareEnvironment squareEnv = squareEnvRaw.ToLower() switch
{
    "sandbox" => SquareEnvironment.Sandbox,
    _ => SquareEnvironment.Production
};
var squareClient = new SquareClient.Builder()
    .Environment(squareEnv)
    .AccessToken(squareAccessToken)
    .Build();

builder.Services.AddSingleton<ISquareClient>(squareClient);

// CORS - allow the nginx frontend origin (or all origins for initial deploy)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

using (var connection = new MySqlConnection(mysqlConnectionString))
{
    connection.Open();
    var createTableCommand = new MySqlCommand(
        @"CREATE TABLE IF NOT EXISTS Orders (
            Id BIGINT AUTO_INCREMENT PRIMARY KEY,
            SourceId VARCHAR(255) NOT NULL,
            AmountCents BIGINT NOT NULL,
            Currency VARCHAR(10) NOT NULL,
            CustomerName VARCHAR(255) NOT NULL,
            Phone VARCHAR(100) NOT NULL,
            SquarePaymentId VARCHAR(255),
            CreatedAt DATETIME NOT NULL,
            RawResponse TEXT
        );",
        connection);
    createTableCommand.ExecuteNonQuery();
}

// Use forwarded headers when behind a proxy (Nginx) so we can correctly detect HTTPS and client IPs
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var clientAppDist = Path.Combine(app.Environment.ContentRootPath, "ClientApp", "dist", "browser");
var clientAppProvider = new PhysicalFileProvider(clientAppDist);

app.UseCors("AllowAll");
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = clientAppProvider,
    RequestPath = string.Empty
});
app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.MapWhen(context =>
    !context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase) &&
    !Path.HasExtension(context.Request.Path.Value ?? string.Empty), spaApp =>
{
    spaApp.Run(async context =>
    {
        context.Response.ContentType = "text/html";
        await context.Response.SendFileAsync(Path.Combine(clientAppDist, "index.html"));
    });
});

app.Run();

public sealed record DatabaseConfig(string ConnectionString);
