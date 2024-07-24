
// Create a builder for the distributed application
var builder = DistributedApplication.CreateBuilder(args);

// Add services to the distributed application
var sqlServer = builder.AddSqlServer(name: "SqlServer", port: 62949);
var identityServerDb = sqlServer.AddDatabase(name: "IdentityServerDb", databaseName: "IdentityServer");
var redis = builder.AddRedis(name: "RedisCache", port: 6379);

// Build and run the distributed application
var identityServer = builder.AddProject<Projects.IdentityServer>("identityserver")
    .WithExternalHttpEndpoints()
    .WithReference(identityServerDb, connectionName: "SqlServer")
    .WithReference(redis, connectionName: "Redis");



// Build and run the distributed application
builder
    .Build()
    .Run();