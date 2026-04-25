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
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5); // Kiểm tra mỗi 5 phút
        private readonly TimeSpan _idleThreshold = TimeSpan.FromMinutes(30); // Ngưỡng 30 phút im lặng

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
                var redisService = scope.ServiceProvider.GetRequiredService<ChatRedisService>();
                var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<ChatHub>>();

                // 1. Tìm các phiên bị "treo" (ACTIVE mà LastTime > 30p)
                var idleSessions = await mongoService.GetIdleSessionsAsync(_idleThreshold);

                if (idleSessions.Any())
                {
                    _logger.LogInformation($"Found {idleSessions.Count} idle sessions to close.");

                    foreach (var phien in idleSessions)
                    {
                        // 2. Chuyển trạng thái sang CLOSED
                        var isSuccess = await mongoService.CapNhatTrangThaiPhienAsync(phien.Id, "CLOSED", "ASSIGNED");
                        
                        if (isSuccess && !string.IsNullOrEmpty(phien.StaffID) && phien.StaffID != "BOT")
                        {
                            // 3. Thông báo cho Client (nếu còn kết nối) và Admin
                            await hubContext.Clients.Group(phien.Id.ToString()).SendAsync("SessionClosed", phien.Id.ToString());
                            await hubContext.Clients.Group("AdminGroup").SendAsync("SessionClosed", phien.Id.ToString());

                            _logger.LogInformation($"Auto-closed idle session: {phien.Id} for Staff: {phien.StaffID}");
                        }
                    }
                }
            }
        }
    }
}
