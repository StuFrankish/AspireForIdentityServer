[![.NET](https://github.com/StuFrankish/AspireForIdentityServer/actions/workflows/dotnet.yml/badge.svg)](https://github.com/StuFrankish/AspireForIdentityServer/actions/workflows/dotnet.yml)
[![CodeQL](https://github.com/StuFrankish/AspireForIdentityServer/actions/workflows/github-code-scanning/codeql/badge.svg)](https://github.com/StuFrankish/AspireForIdentityServer/actions/workflows/github-code-scanning/codeql)

# Aspire for IdentityServer, Client & API
This sample includes:
- A standard instance of IdentityServer from Duende using version 7.1.0-Preview.1 configured to use SQL Server storage.
- An MVC client application setup to use PAR (pushed authorisation requests) and configured to use Redis cache.
- A protected API resource, also configured to use Redis for output caching.
- Serilog integration in the above projects, output to console.
- Endpoint routing with MediatR.
- Sample unit tests using Moq & FluentValidation.

## Cloning and Building this project

### Prerequisites

Before you start, make sure you have the following installed on your machine:

- **.NET 9 SDK**: Download and install from the [official .NET website](https://dotnet.microsoft.com/download/dotnet/9.0).
- **IDE of Choice**: Preferably [Visual Studio](https://visualstudio.microsoft.com/) for its robust support for .NET development. Alternatively, you can use [Visual Studio Code](https://code.visualstudio.com/) or any other IDE that supports .NET.
- **Docker**: Preferablly Docker Desktop or Rancher for ease of use, but any installation of Docker should work fine.

### Customisations

- Custom Options handler and types (`ICustomOptions`).
- Customised `HostingExtensions.cs` (provides new `ConfigureServices()`, `InitializeDatabase()` and `ConfigurePipeline()` methods).
- Custom `WebApplicationBuilder` extensions to provide configuration of IdentityServer and Redis.
- Cleaned up `Config.cs` into `SeedConfig.cs`
- Added Redis and SQL Server resources to the Aspire AppHost project for use in the IdentityServer application.
- Endpoint routing with MediatR.

### Steps to Clone and Build the Project

> [!IMPORTANT]  
> With the update to Aspire v9, the solution now makes use of the `.WaitFor()` helpers, ensuring that the Identity Server, Client and API projects wait for the containers to finish loading and enter a healthy state before loading.
> If you do not already have the latest version of the Redis and MSSQL container images, it will take longer for them to provision while the images are downloaded.

1. **Clone the Repository**:

    Open a terminal or command prompt and run the following command to clone the repository:

    ```bash
    git clone https://github.com/StuFrankish/AspireForIdentityServer.git
    cd AspireForIdentityServer
    ```

2. **Restore the Dependencies**:

    Navigate to the project directory and restore the dependencies using the .NET CLI:

    ```bash
    dotnet restore
    ```

3. **Build the Project**:

    Build the project using the .NET CLI:

    ```bash
    dotnet build
    ```

4. **Run the Project**:

    To run the project, use the following command:

    ```bash
    dotnet run
    ```

    The application will start and open the Aspire dashboard in your default browser, where you should see the projects listed as well as the SQL & Redis containers begin to provision.

## Contributing

If you'd like to contribute to the project, feel free to fork the repository, make your changes, and create a pull request.

For any further questions or issues, please open an issue on the [GitHub repository](https://github.com/StuFrankish/AspireForIdentityServer/issues).
