using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VideoStreaming.Data;
using VideoStreaming.Interfaces;
using VideoStreaming.Models;

namespace VideoStreamingDemo.Services
{
    public class VideoProcessingWorker : BackgroundService
    {
        private readonly ILogger<VideoProcessingWorker> _logger;
        private readonly IBackgroundTaskQueue _queue;
        private readonly IServiceProvider _sp;

        public VideoProcessingWorker(ILogger<VideoProcessingWorker> logger, IBackgroundTaskQueue queue, IServiceProvider sp)
        {
            _logger = logger;
            _queue = queue;
            _sp = sp;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var videoId in _queue.DequeueAllAsync(stoppingToken))
            {
                try
                {
                    using var scope = _sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var proc = scope.ServiceProvider.GetRequiredService<IVideoProcessor>();

                    var video = await db.videos.FirstOrDefaultAsync(v => v.Id == videoId, stoppingToken);
                    if (video is null) continue;

                    _logger.LogInformation("Processing video {Id} ...", video.Id);

                    video.Status = VideoStatus.Processing;
                    await db.SaveChangesAsync(stoppingToken);

                    var result = await proc.TranscodeToHlsAsync(video, stoppingToken);

                    if (!result.Success)
                        _logger.LogError("Video {Id} failed: {Err}", video.Id, result.Erro);
                    else
                        _logger.LogInformation("Video {Id} ready at {Path}", video.Id, result.HLSRelPaht);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled error while processing video");
                }
            }
        }
    }
}