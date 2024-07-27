
// Create a builder for the distributed application
var builder = DistributedApplication.CreateBuilder(args);

// Add services to the distributed application
var sqlServer = builder.AddSqlServer(name: "SqlServer", port: 62949);
var identityServerDb = sqlServer.AddDatabase(name: "IdentityServerDb", databaseName: "IdentityServer");
var redis = builder.AddRedis(name: "RedisCache", port: 6379);


// Add projects to the distributed application
var identityServer = builder.AddProject<Projects.IdentityServer>("identityserver")
    .WithExternalHttpEndpoints()
    .WithReference(identityServerDb, connectionName: "SqlServer")
    .WithReference(redis, connectionName: "Redis");

_ = builder.AddProject<Projects.Client>("clientapp")
    .WithExternalHttpEndpoints()
    .WithEnvironment("IdentityProvider__Authority", identityServer.GetEndpoint("https"))
    .WithEnvironment("IdentityProvider__ClientId", "mvc.par")
    .WithEnvironment("IdentityProvider__ClientSecret", "secret");


// Build and run the distributed application
builder
    .Build()
    .Run();