using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ParkingManagementSystem.WebApp.Services;

public class ApiService : IApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApiService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
    {
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient("API");

        var token = _httpContextAccessor.HttpContext?.Request.Cookies["jwt_token"];
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        return client;
    }

    public async Task<ApiResponse<TResult>> GetAsync<TResult>(string endpoint)
    {
        var client = CreateClient();
        var response = await client.GetAsync(endpoint);
        return await ProcessResponse<TResult>(response);
    }

    public async Task<ApiResponse<TResult>> PostAsync<TResult>(string endpoint, object data)
    {
        var client = CreateClient();
        var content = new StringContent(
            JsonSerializer.Serialize(data),
            Encoding.UTF8,
            "application/json");

        var response = await client.PostAsync(endpoint, content);
        return await ProcessResponse<TResult>(response);
    }

    private async Task<ApiResponse<TResult>> ProcessResponse<TResult>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            var data = JsonSerializer.Deserialize<TResult>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return new ApiResponse<TResult>(Success: true, Data: data, StatusCode: response.StatusCode,
                ErrorMessage: null);
        }

        return new ApiResponse<TResult>(Success: false, Data: default, ErrorMessage: content,
            StatusCode: response.StatusCode);
    }

    public async Task<ApiResponse<TResult>> PatchAsync<TResult>(string endpoint, object data)
    {
        var client = CreateClient();
        var content = new StringContent(
            JsonSerializer.Serialize(data),
            Encoding.UTF8,
            "application/json");

        var response = await client.PostAsync(endpoint, content);
        return await ProcessResponse<TResult>(response);
    }
}