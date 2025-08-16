using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace VideoStreaming.Interfaces
{
    public interface IBackgroundTaskQueue
    {
        ValueTask QueueAsync(int videoId);
        IAsyncEnumerable<int> DequeueAllAsync(CancellationToken ct);
    }

    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private readonly Channel<int> _channel = Channel.CreateUnbounded<int>();
        public ValueTask QueueAsync(int videoId) => _channel.Writer.WriteAsync(videoId);
        public IAsyncEnumerable<int> DequeueAllAsync(CancellationToken ct) => _channel.Reader.ReadAllAsync(ct);
    }

    }