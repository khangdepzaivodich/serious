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
            var context = _httpContextAccessor.HttpContext;
            if (context != null)
            {
                var authorization = context.Request.Headers.Authorization.ToString();
                if (!string.IsNullOrWhiteSpace(authorization) && authenticationHeaderValueCanParse(authorization))
                {
                    request.Headers.Authorization = AuthenticationHeaderValue.Parse(authorization);
                }
            }

            return await base.SendAsync(request, cancellationToken);
        }

        private static bool authenticationHeaderValueCanParse(string value)
        {
            return AuthenticationHeaderValue.TryParse(value, out _);
        }
    }
}