using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Server.Kestrel.Core.Features;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.MapGet("/route", (HttpRequest request) =>
{
    if (request.Host.Host != "common.lgthinq.com") return Results.NotFound();

    return Results.Ok(new
    {
        result = new
        {
            apiServer = "http://localhost:5026",
            mqqtServer = "mqtt://localhost:1883"
        },
        resultCode = "0000"
    });
});

app.MapGet("/route/certificates", () => Results.Ok(new
{
    result = new[]
    {
        "common-server",
        "aws-iot"
    },
    resultCode = "0000"
}));

app.MapGet("/route/certificate", (string name, HttpContext context) =>
{
    var feature = context.Features.Get<ISslStreamFeature>();
    var ssl = feature.SslStream;
    var cert = ssl.LocalCertificate as X509Certificate2;
    var pem = name switch
    {
        "common-server" => cert.ExportCertificatePem(),
        _ => throw new BadHttpRequestException("invalid name")
    };
    return Results.Ok(new
    {
        result = new
        {
            certificatePem = pem
        },
        resultCode = "0000"
    });
});

app.Run();
