using System.Collections.Concurrent;
using System.Threading.Channels;

namespace TheRabbitHole.Core;

// A simple in-memory queue to decouple the content-saving notification from the background processing of podcast episodes.
// Tracks in-flight episode keys so the save-notification handler can ignore the saves we make while processing
// (otherwise every save of the results would re-enqueue the episode and create a feedback loop).
public class PodcastEpisodeQueue
{
    private readonly Channel<Guid> _channel = Channel.CreateUnbounded<Guid>();
    private readonly ConcurrentDictionary<Guid, byte> _inFlight = new();

    public ChannelWriter<Guid> Writer => _channel.Writer;
    public ChannelReader<Guid> Reader => _channel.Reader;

    public bool IsInFlight(Guid key) => _inFlight.ContainsKey(key);
    public bool BeginProcessing(Guid key) => _inFlight.TryAdd(key, 0);
    public void EndProcessing(Guid key) => _inFlight.TryRemove(key, out _);
}