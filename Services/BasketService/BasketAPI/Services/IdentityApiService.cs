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
                // Gọi API check-exist (không yêu cầu Admin) để kiểm tra user
                var response = await _httpClient.GetAsync($"/api/User/check-exist/{userName}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false; 
            }
        }
    }
}
