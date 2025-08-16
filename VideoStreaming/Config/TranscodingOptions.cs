using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VideoStreaming.Config
{
    public class TranscodingOptions
    {
        public string FfmpegPath { get; set; } = "ffmpeg";
        public string FfprobePath { get; set; } = "ffprobe";
        public int SegmentDuration { get; set; } = 6;
    }
}