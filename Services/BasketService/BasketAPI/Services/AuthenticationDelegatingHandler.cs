using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;

namespace BasketService.BasketAPI.Services
{
    public class AuthenticationDelegatingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthenticationDelegatingHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Lấy token từ request hiện tại của user đang gọi BasketService
            var context = _httpContextAccessor.HttpContext;
            if (context != null)
            {
                var token = await context.GetTokenAsync("access_token");
                if (!string.IsNullOrEmpty(token))
                {
                    // Gắn token này vào request chuẩn bị gởi đi (sang Identity/Catalog)
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}