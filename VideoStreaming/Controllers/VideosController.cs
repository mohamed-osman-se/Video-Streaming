using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VideoStreaming.Data;
using VideoStreaming.Interfaces;
using VideoStreaming.Models;

namespace VideoStreaming.Controllers
{

    public class VideosController : Controller
    {
        private readonly ILogger<VideosController> _logger;
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        private readonly IBackgroundTaskQueue _queue;

        public VideosController(ILogger<VideosController> logger, AppDbContext context, IWebHostEnvironment env, IBackgroundTaskQueue queue)
        {
            _logger = logger;
            _context = context;
            _env = env;
            _queue = queue;
        }

        public async Task<IActionResult> Index()
        {
            var videos = await _context.videos.OrderByDescending(v => v.CreatedAt).ToListAsync();
            return View(videos);
        }
        [HttpGet]
        public IActionResult Upload() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(string title, IFormFile file)
        {
            var webRoot = _env.WebRootPath ?? "wwwroot";
            var uploadsDir = Path.Combine(webRoot, "media", "uploads");
            Directory.CreateDirectory(uploadsDir);
            var safeBase = Guid.NewGuid().ToString("N");
            var ext = Path.GetExtension(file.FileName);
            var fileName = $"{safeBase}{ext}";
            var absPath = Path.Combine(uploadsDir, fileName);

            using (var stream = System.IO.File.Create(absPath))
                await file.CopyToAsync(stream);

            var relPath = $"/media/uploads/{fileName}";

            var video = new Video
            {
                Title = title.Trim(),
                UploadPath = relPath,
                Status = VideoStatus.Uploaded,
                OriginalFileName = fileName
            };

            _context.videos.Add(video);
            await _context.SaveChangesAsync();
            await _queue.QueueAsync(video.Id);
            TempData["msg"] = "Your video has been uploaded successfully. Processing has started.";

            return Redirect("/Videos/Index");
        }
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var video = await _context.videos.FindAsync(id);
            return View(video);
        }


    }
}