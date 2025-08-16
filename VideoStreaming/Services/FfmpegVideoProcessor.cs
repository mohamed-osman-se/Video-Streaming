using System.Diagnostics;
using Microsoft.Extensions.Options;
using VideoStreaming.Config;
using VideoStreaming.Data;
using VideoStreaming.Interfaces;
using VideoStreaming.Models;

namespace VideoStreamingDemo.Services
{
    public class FfmpegVideoProcessor : IVideoProcessor
    {
        private readonly IWebHostEnvironment _env;
        private readonly TranscodingOptions _opt;
        private readonly AppDbContext _db;

        public FfmpegVideoProcessor(IWebHostEnvironment env, IOptions<TranscodingOptions> opt, AppDbContext db)
        {
            _env = env;
            _opt = opt.Value;
            _db = db;
        }

        public async Task<ProcessResult> TranscodeToHlsAsync(Video video, CancellationToken ct = default)
        {
            try
            {
                // input video path ==> wwwroot/media/upload/123v1.mp4
                var webRoot = _env.WebRootPath ?? "wwwroot";
                var inputAbs = Path.Combine(webRoot, video.UploadPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

                // output folder => wwwroot/media/hls/123
                var outDirRel = $"/media/hls/{video.Id}";
                var outDirAbs = Path.Combine(webRoot, "media", "hls", video.Id.ToString());
                Directory.CreateDirectory(outDirAbs);

                double? duration = await ProbeDurationAsync(inputAbs, ct);

                // get output file paths
                var indexFile = Path.Combine(outDirAbs, "index.m3u8");
                var segPattern = Path.Combine(outDirAbs, "480p_%03d.ts");

                var vf = "scale=w=854:h=480:force_original_aspect_ratio=decrease,pad=ceil(iw/2)*2:ceil(ih/2)*2";

                // ffmpeg args
                var args = new[]
                {
                    "-y",
                    "-i", Quote(inputAbs),
                    "-vf", vf,
                    "-c:v", "h264", "-profile:v", "main", "-level", "4.0",
                    "-preset", "veryfast", "-crf", "21",
                    "-c:a", "aac", "-b:a", "128k", "-ac", "2", "-ar", "48000",
                    "-f", "hls",
                    "-hls_time", _opt.SegmentDuration.ToString(),
                    "-hls_playlist_type", "vod",
                    "-hls_flags", "independent_segments",
                    "-hls_segment_filename", Quote(segPattern),
                    Quote(indexFile)
                };

                var ok = await RunProcessAsync(_opt.FfmpegPath, string.Join(' ', args), ct);
                if (!ok)
                    return new ProcessResult(false, null, null, "FFmpeg failed.");

                var relIndex = $"{outDirRel}/index.m3u8";
                video.HlsPath = relIndex.Replace("\\", "/");
                video.DurationSeconds = duration;
                video.Status = VideoStatus.Ready;

                await _db.SaveChangesAsync(ct);
                return new ProcessResult(true, video.HlsPath, video.DurationSeconds, null);
            }
            catch (Exception ex)
            {
                video.Status = VideoStatus.Failed;
                await _db.SaveChangesAsync(ct);
                return new ProcessResult(false, null, null, ex.Message);
            }
        }

        private static string Quote(string s) => $"\"{s}\"";

        private static async Task<bool> RunProcessAsync(string fileName, string args, CancellationToken ct)
        {
            var psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var p = new Process { StartInfo = psi, EnableRaisingEvents = true };
            p.Start();

            _ = Task.Run(() => p.StandardError.ReadToEndAsync(), ct);
            _ = Task.Run(() => p.StandardOutput.ReadToEndAsync(), ct);

            await p.WaitForExitAsync(ct);
            return p.ExitCode == 0;
        }

        private async Task<double?> ProbeDurationAsync(string inputAbs, CancellationToken ct)
        {
            var psi = new ProcessStartInfo
            {
                FileName = _opt.FfprobePath,
                Arguments = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 {Quote(inputAbs)}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var p = Process.Start(psi)!;
            var output = await p.StandardOutput.ReadToEndAsync();
            await p.WaitForExitAsync(ct);

            if (double.TryParse(output.Trim(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var d))
                return d;

            return null;
        }
    }
}
