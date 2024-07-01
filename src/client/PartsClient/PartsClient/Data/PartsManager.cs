using System.Net.Http.Json;
using System.Text.Json;

namespace PartsClient.Data;

public static class PartsManager
{
    private const string BaseAddress = "https://mslearnpartsserver70045167.azurewebsites.net";
    private const string Url = $"{BaseAddress}/api";
    private static string _authorizationKey;
    private static readonly HttpClient Client = new();

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static async Task<HttpClient> GetClient()
    {
        if (!string.IsNullOrEmpty(_authorizationKey))
            return Client;

        _authorizationKey = await Client.GetStringAsync($"{Url}/login");
        _authorizationKey = JsonSerializer.Deserialize<string>(_authorizationKey);

        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authorizationKey);
        Client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        return Client;
    }

    public static async Task<IEnumerable<Part>> GetAll()
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            return [];

        var client = await GetClient();
        var result = await client.GetStringAsync($"{Url}/parts");

        return JsonSerializer.Deserialize<IEnumerable<Part>>(result, JsonSerializerOptions) ?? [];
    }

    public static async Task Add(string partName, string supplier, string partType)
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet) return;

        var part = new Part
        {
            PartName = partName,
            Suppliers = [.. new[] { supplier }],
            PartID = string.Empty,
            PartType = partType,
            PartAvailableDate = DateTime.Now.Date
        };

        var client = await GetClient();

        var response = await client.PostAsJsonAsync($"{Url}/parts", part);
        response.EnsureSuccessStatusCode();

        await response.Content.ReadFromJsonAsync<Part>(JsonSerializerOptions);
    }

    public static async Task Update(Part part)
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            return;

        var client = await GetClient();

        var response = await client.PutAsJsonAsync($"{Url}/parts/{part.PartID}", part);
        response.EnsureSuccessStatusCode();
    }

    public static async Task Delete(string partId)
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            return;

        var client = await GetClient();

        var response = await client.DeleteAsync($"{Url}/parts/{partId}");
        response.EnsureSuccessStatusCode();
    }
}
