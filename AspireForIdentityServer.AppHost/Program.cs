// Create a builder for the distributed application
var builder = DistributedApplication.CreateBuilder(args);

// Add services to the distributed application
var sqlServer = builder
    .AddSqlServer(name: "SqlServer", port: 62949)
    .WithContainerName("AfIdSrv-SqlServer")
    .WithLifetime(ContainerLifetime.Persistent);

var identityServerDb = sqlServer
    .AddDatabase(name: "IdentityServerDb", databaseName: "IdentityServer");

var redis = builder
    .AddRedis(name: "RedisCache", port: 6379)
    .WithContainerName("AfIdSrv-RedisCache")
    .WithRedisInsight()
    .WithLifetime(ContainerLifetime.Persistent);

// Add projects to the distributed application
var identityServer = builder.AddProject<Projects.IdentityServer>(name: "identityserver");
var weatherApi = builder.AddProject<Projects.WeatherApi>(name: "weatherapi");
var clientApplication = builder.AddProject<Projects.Client>(name: "clientapp");

// Configure the distributed application

identityServer
    .WithExternalHttpEndpoints()
    .WithReference(identityServerDb, connectionName: "SqlServer")
    .WithReference(redis, connectionName: "Redis")
    .WithHttpsHealthCheck(path: "/.well-known/openid-configuration")
    .WaitFor(identityServerDb);

weatherApi
    .WithExternalHttpEndpoints()
    .WithReference(redis, connectionName: "Redis")
    .WithEnvironment(name: "IdentityProvider__Authority", endpointReference: identityServer.GetEndpoint(name: "https"))
    .WithHttpsHealthCheck(path: "/_health")
    .WaitFor(identityServer);

clientApplication
    .WithExternalHttpEndpoints()
    .WithReference(redis, connectionName: "Redis")
    .WithEnvironment(name: "WeatherApi__BaseUrl", endpointReference: weatherApi.GetEndpoint(name: "https"))
    .WithEnvironment(name: "IdentityProvider__Authority", endpointReference: identityServer.GetEndpoint(name: "https"))
    .WithEnvironment(name: "IdentityProvider__ClientId", value: "mvc.par")
    .WithEnvironment(name: "IdentityProvider__ClientSecret", value: "secret")
    .WaitFor(identityServer);

// Build and run the distributed application
builder
    .Build()
    .Run();