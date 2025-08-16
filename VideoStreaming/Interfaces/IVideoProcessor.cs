using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VideoStreaming.Models;

namespace VideoStreaming.Interfaces
{
    public record ProcessResult(bool Success,string ? HLSRelPaht,double? DurationSec,string? Erro);
    public interface IVideoProcessor
    {
        Task<ProcessResult> TranscodeToHlsAsync(Video video, CancellationToken ct = default);
    }
}