using System.Net.Sockets;

var builder = DistributedApplication.CreateBuilder(args);

// Add SQL Server instance shared across all modules
var sqlServer = builder.AddSqlServer("sqlserver")
    .WithDataVolume();

var usersDb = sqlServer.AddDatabase("usersdb");
var productsDb = sqlServer.AddDatabase("productsdb");
var customersDb = sqlServer.AddDatabase("customersdb");

var reportingDb = sqlServer.AddDatabase("reportingdb");

// Papercut SMTP container for email testing in development
var papercut = builder.AddContainer("papercut", "jijiechen/papercut", "latest")
    .WithEndpoint("smtp", e =>
    {
        e.TargetPort = 25;
        e.Port = 25;
        e.Protocol = ProtocolType.Tcp;
        e.UriScheme = "smtp";
    })
    .WithEndpoint("ui", e =>
    {
        e.TargetPort = 37408;
        e.Port = 37408;
        e.UriScheme = "http";
    });

// Add the Web API project with all database and email references
builder.AddProject<Projects.Nimble_Modulith_Web>("webapi")
    .WithReference(usersDb)
    .WithReference(productsDb)
    .WithReference(customersDb)
    .WithReference(reportingDb)
    .WaitFor(usersDb)
    .WaitFor(productsDb)
    .WaitFor(customersDb)
    .WaitFor(reportingDb)
    .WaitFor(papercut);

builder.Build().Run();
