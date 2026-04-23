namespace BasketService.BasketAPI.Services
{
    public interface IIdentityService
    {
        Task<bool> ValidateUserAsync(string userName);
    }

    public class IdentityApiService : IIdentityService
    {
        private readonly HttpClient _httpClient;

        public IdentityApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> ValidateUserAsync(string userName)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/User/exists/{userName}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false; 
            }
        }
    }
}
