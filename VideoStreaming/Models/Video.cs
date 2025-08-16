using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VideoStreaming.Models
{
    public enum VideoStatus { Uploaded = 0, Processing = 1, Ready = 2, Failed = 3 }

    public class Video
    {
        public int Id { get; set; }
        public string Title { get; set; } = default!;
        public string OriginalFileName { get; set; } = default!;
        public string UploadPath { get; set; } = default!;
        public string? HlsPath { get; set; }               
        public double? DurationSeconds { get; set; }    
        public VideoStatus Status { get; set; } = VideoStatus.Uploaded;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}