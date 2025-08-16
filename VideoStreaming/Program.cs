using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using VideoStreaming.Config;
using VideoStreaming.Data;
using VideoStreaming.Interfaces;
using VideoStreamingDemo.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();


builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source =app.db"));

builder.Services.Configure<FormOptions>(options => options.MultipartBodyLengthLimit = 1L * 1024 * 1024 * 1024);
builder.Services.AddScoped<TranscodingOptions>();
builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
builder.Services.AddHostedService<VideoProcessingWorker>();
builder.Services.AddScoped<IVideoProcessor, FfmpegVideoProcessor>();
var app = builder.Build();

void EnsureDir(string relPath)
{
    var abs = Path.Combine(app.Environment.WebRootPath ?? "wwwroot", relPath);
    Directory.CreateDirectory(abs);
}

EnsureDir("media/uploads");
EnsureDir("media/hls");

app.UseExceptionHandler("/Home/Error");
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".m3u8"] = "application/vnd.apple.mpegurl";
provider.Mappings[".ts"] = "video/MP2T";
provider.Mappings[".mpd"] = "application/dash+xml";
provider.Mappings[".m4s"] = "video/iso.segment";

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});


app.UseRouting();

app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Videos}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
