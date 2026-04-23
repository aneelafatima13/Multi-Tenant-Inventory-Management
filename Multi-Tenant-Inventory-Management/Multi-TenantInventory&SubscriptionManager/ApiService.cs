using Common;
using System.Net.Http.Json;

namespace Multi_TenantInventory_SubscriptionManager
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            // Best practice: Move this string to appsettings.json
            _httpClient.BaseAddress = new Uri("https://localhost:7286/");
        }

        // 1. Generic GET Method
        public async Task<T?> GetAsync<T>(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<T>();
                }
                return default;
            }
            catch (Exception)
            {
                // Log exception here
                return default;
            }
        }

        // 2. Simplified POST Method (Returns response status for the Controller)
        public async Task<HttpResponseMessage> PostAsync<TRequest>(string endpoint, TRequest data)
        {
            return await _httpClient.PostAsJsonAsync(endpoint, data);
        }

        // 3. Generic POST Method (For when you need the object back, e.g., Login/Register)
        public async Task<TResponse?> PostWithResultAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {
            var response = await _httpClient.PostAsJsonAsync(endpoint, data);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TResponse>();
            }

            return default;
        }

        public async Task<PagedResponse<T>?> GetPagedAsync<T>(string endpoint, int skip, int take, string search)
        {
            // Constructs: api/TenantApi/paged?skip=0&take=10&search=abc
            var url = $"{endpoint}?skip={skip}&take={take}&search={Uri.EscapeDataString(search ?? "")}";

            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<PagedResponse<T>>();
            }
            return new PagedResponse<T>(); // Return empty instead of null to avoid JS errors
        }
    }
}