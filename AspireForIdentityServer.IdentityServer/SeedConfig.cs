using Duende.IdentityServer.Models;
using IdentityServer.Data.Models.Account;
using System.Text.Json;

namespace IdentityServer;

public class SeedConfig
{
    private readonly string BasePath;

    public SeedConfig(string seedingFileName)
    {
        BasePath = Path.Combine(AppContext.BaseDirectory, seedingFileName);

        if (!File.Exists(BasePath))
        {
            throw new FileNotFoundException("Seeding file not found", BasePath);
        }

        if (new FileInfo(BasePath).Length == 0)
        {
            throw new FileLoadException("Seeding file is empty", BasePath);
        }

        if (Path.GetExtension(BasePath) != ".json")
        {
            throw new FileLoadException("Seeding file is not a JSON file", BasePath);
        }

        if (JsonDocument.Parse(File.ReadAllText(BasePath)).RootElement.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException(message: "Seeding file is not a valid JSON object", path: BasePath, lineNumber: null, bytePositionInLine: null);
        }

        Users = LoadTestUsers();
        ApiScopes = LoadApiScopes();
        Clients = LoadClients();
    }

    private string JsonSectionData(string sectionName)
    {
        JsonDocument doc = JsonDocument.Parse(File.ReadAllText(BasePath));
        return doc.RootElement.GetProperty(sectionName).GetRawText();
    }

    private List<SeedUserModel> LoadTestUsers()
    {
        var json = JsonSectionData("Users");
        return JsonSerializer.Deserialize<List<SeedUserModel>>(json);
    }

    private List<ApiScope> LoadApiScopes()
    {
        var json = JsonSectionData("ApiScopes");
        return JsonSerializer.Deserialize<List<ApiScope>>(json);
    }

    private List<Client> LoadClients()
    {
        var json = JsonSectionData("Clients");
        var clients = JsonSerializer.Deserialize<List<Client>>(json);

        foreach (var client in clients)
        {
            foreach (var secret in client.ClientSecrets)
            {
                secret.Value = secret.Value.Sha256();
            }
        }

        return clients;
    }

    public IEnumerable<SeedUserModel> Users { get; init; } = [];
    public IEnumerable<ApiScope> ApiScopes { get; init; } = [];
    public IEnumerable<Client> Clients { get; init; } = [];
    public IEnumerable<IdentityResource> IdentityResources =>
        [
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
        ];
}
