using System.Net;

namespace ParkingManagementSystem.WebApp.Services;

public interface IApiService
{
    Task<ApiResponse<TResult>> GetAsync<TResult>(string endpoint);
    Task<ApiResponse<TResult>> PostAsync<TResult>(string endpoint, object data);
    Task<ApiResponse<TResult>> PatchAsync<TResult>(string endpoint, object data);
}

public record ApiResponse<T>(bool Success, T? Data, string? ErrorMessage, HttpStatusCode StatusCode);