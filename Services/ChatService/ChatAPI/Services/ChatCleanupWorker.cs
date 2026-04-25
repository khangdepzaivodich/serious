using Microsoft.AspNetCore.SignalR;
using ChatService.ChatAPI.Services;
using ChatService.ChatAPI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ChatService.ChatAPI.Services
{
    public class ChatCleanupWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ChatCleanupWorker> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(2); // Kiểm tra thường xuyên hơn
        private readonly TimeSpan _idleThreshold = TimeSpan.FromMinutes(30); // Ngưỡng cho Staff
        private readonly TimeSpan _guestThreshold = TimeSpan.FromMinutes(15); // Ngưỡng 15 phút cho Khách

        public ChatCleanupWorker(IServiceProvider serviceProvider, ILogger<ChatCleanupWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Chat Cleanup Worker is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await DoWorkAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in Chat Cleanup Worker.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("Chat Cleanup Worker is stopping.");
        }

        private async Task DoWorkAsync(CancellationToken stoppingToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var mongoService = scope.ServiceProvider.GetRequiredService<ChatMongoService>();
                var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<ChatHub>>();

                // 1. XỬ LÝ PHIÊN KHÁCH (GUEST) - XÓA NẾU QUÁ 15P
                var idleGuests = await mongoService.GetIdleGuestSessionsAsync(_guestThreshold);
                if (idleGuests.Any())
                {
                    _logger.LogInformation($"Found {idleGuests.Count} idle GUEST sessions to delete.");
                    foreach (var guest in idleGuests)
                    {
                        await mongoService.XoaPhienChatAsync(guest.Id);
                        // Thông báo đóng cho client (nếu còn)
                        await hubContext.Clients.Group(guest.Id.ToString()).SendAsync("SessionClosed", "GUEST_TIMEOUT");
                        await hubContext.Clients.Group("AdminGroup").SendAsync("SessionClosed", guest.Id.ToString());
                    }
                }

                // 2. XỬ LÝ PHIÊN STAFF (ASSIGNED) - ĐÓNG NẾU QUÁ 30P
                var idleSessions = await mongoService.GetIdleSessionsAsync(_idleThreshold);

                if (idleSessions.Any())
                {
                    _logger.LogInformation($"Found {idleSessions.Count} idle assigned sessions to close.");

                    foreach (var phien in idleSessions)
                    {
                        var isSuccess = await mongoService.CapNhatTrangThaiPhienAsync(phien.Id, "CLOSED", "ASSIGNED");
                        
                        if (isSuccess)
                        {
                            await hubContext.Clients.Group(phien.Id.ToString()).SendAsync("SessionClosed", "AUTO_IDLE");
                            await hubContext.Clients.Group("AdminGroup").SendAsync("SessionClosed", phien.Id.ToString());

                            _logger.LogInformation($"Auto-closed idle assigned session: {phien.Id}");
                        }
                    }
                }
            }
        }
    }
}
