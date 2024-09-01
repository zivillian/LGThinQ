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

app.MapGet("/route/certificate", (string? name, HttpContext context) =>
{
    if (String.IsNullOrEmpty(name))
    {
        return Results.Ok(new
        {
            result = new[]
            {
                "common-server",
                "aws-iot"
            },
            resultCode = "0000"
        });
    }
    var feature = context.Features.Get<ISslStreamFeature>();
    var ssl = feature.SslStream;
    var cert = ssl.LocalCertificate as X509Certificate2;
    var pem = name switch
    {
        "common-server" => """
                           -----BEGIN CERTIFICATE-----
                           MIIDoTCCAomgAwIBAgIGDqfplOKzMA0GCSqGSIb3DQEBCwUAMCgxEjAQBgNVBAMM
                           CW1pdG1wcm94eTESMBAGA1UECgwJbWl0bXByb3h5MB4XDTIxMDEyMTIwMzUxN1oX
                           DTI0MDEyMzIwMzUxN1owKDESMBAGA1UEAwwJbWl0bXByb3h5MRIwEAYDVQQKDAlt
                           aXRtcHJveHkwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQDnnPSlipgJ
                           Gf54JPiIJSnfnRyrGNSCbvPlp3Y0a6OcUAE931TBjW9oASZV41gnz496xw6yfIkD
                           ErD1b3WF/NFwsEOWgBvouipxOSstQEawHeN9+gqcbq5DXfH3AS9H7pdlfwQ/IAFd
                           LMT1nzmQheKc+g+KARfJAx4weUveNqfVGA/6OJmgFcx9oUOG6QYPDKWStwFTwKim
                           m417TmoexXNvvmARHW2GdnLTRyEPhfpa70xAhfdCJAUstlI9W67LQbYX34Bgu88C
                           n9PtnDh/i4Krq8ZUrKpV0XGAnmT4YSdNOve5IwL5ybaJR818W3txik3FejEepmAw
                           Knci15CIX2/HAgMBAAGjgdAwgc0wDwYDVR0TAQH/BAUwAwEB/zARBglghkgBhvhC
                           AQEEBAMCAgQweAYDVR0lBHEwbwYIKwYBBQUHAwEGCCsGAQUFBwMCBggrBgEFBQcD
                           BAYIKwYBBQUHAwgGCisGAQQBgjcCARUGCisGAQQBgjcCARYGCisGAQQBgjcKAwEG
                           CisGAQQBgjcKAwMGCisGAQQBgjcKAwQGCWCGSAGG+EIEATAOBgNVHQ8BAf8EBAMC
                           AQYwHQYDVR0OBBYEFGUNZyR2f1vn/D6hTHxXY8DeKk3LMA0GCSqGSIb3DQEBCwUA
                           A4IBAQC2ez2Cwk0dSCwbyahzZ0mfbLu9BtmoWYMnfU+oESTAOaCtQJYQjIwZ2iWZ
                           iYn8dcxRfcddkH4k6S3W4MbOz8JcFA5nzGH5XW2M7q6WmmGHI8wGxeVItBDXp0TF
                           wa+BymmP37wFZ9WrFHO2cPs7t9O4OWVuN2cepIaU8mlvRJsav24/zytpXK2uh7o+
                           gEEwnJWfmpuPuIWnSrLvDCtzXLsv7ME9ok1vhBnJzrRYdy153usgnMfpkr+6dAut
                           9B1Kp1JzAp9yWQONlRt4CfgIePL95lrA+8XrXdoQ765WNF6v4M6RMscllDY1ImTi
                           RTcDEhveCtegToUYX3y7uS6bcRFw
                           -----END CERTIFICATE-----
                           """,//todo insert your mitmproxy cert
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
