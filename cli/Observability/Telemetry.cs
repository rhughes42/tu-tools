using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace CncCli.Observability;

internal readonly record struct TelemetryContext(string Environment, string Release, string Module);

internal readonly record struct TelemetryEvent(
    string Level,
    string Message,
    TelemetryContext Context,
    DateTimeOffset Timestamp,
    IReadOnlyDictionary<string, string> Tags,
    IReadOnlyDictionary<string, string> Data);

internal interface ITelemetrySink
{
    Task EmitAsync(TelemetryEvent telemetryEvent, CancellationToken ct = default);
}

internal sealed class ConsoleTelemetrySink : ITelemetrySink
{
    public Task EmitAsync(TelemetryEvent telemetryEvent, CancellationToken ct = default)
    {
        Console.WriteLine($"[{telemetryEvent.Timestamp:u}] {telemetryEvent.Level} {telemetryEvent.Context.Module}: {telemetryEvent.Message}");
        return Task.CompletedTask;
    }
}

internal sealed class SentryCompatibleTelemetrySink : ITelemetrySink
{
    private readonly HttpClient _httpClient = new();
    private readonly string _endpoint;

    public SentryCompatibleTelemetrySink(string endpoint)
    {
        _endpoint = endpoint;
    }

    public async Task EmitAsync(TelemetryEvent telemetryEvent, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_endpoint))
        {
            return;
        }

        var payload = new
        {
            message = telemetryEvent.Message,
            level = telemetryEvent.Level,
            environment = telemetryEvent.Context.Environment,
            release = telemetryEvent.Context.Release,
            module = telemetryEvent.Context.Module,
            timestamp = telemetryEvent.Timestamp,
            tags = telemetryEvent.Tags,
            extra = telemetryEvent.Data
        };

        try
        {
            using var response = await _httpClient.PostAsJsonAsync(_endpoint, payload, ct);
            _ = response.IsSuccessStatusCode;
        }
        catch
        {
            // Telemetry must not fail CNC execution.
        }
    }
}

internal sealed class TelemetryClient
{
    private readonly ITelemetrySink[] _sinks;
    private readonly double _sampleRate;
    private readonly Random _random = new();

    public TelemetryClient(double sampleRate, params ITelemetrySink[] sinks)
    {
        _sampleRate = Math.Clamp(sampleRate, 0.0, 1.0);
        _sinks = sinks ?? [];
    }

    public async Task EmitAsync(string level, string message, TelemetryContext context, IReadOnlyDictionary<string, string>? tags = null, IReadOnlyDictionary<string, string>? data = null, CancellationToken ct = default)
    {
        if (_sinks.Length == 0 || _random.NextDouble() > _sampleRate)
        {
            return;
        }

        var safeData = (data ?? new Dictionary<string, string>())
            .ToDictionary(k => k.Key, v => Redact(v.Value), StringComparer.OrdinalIgnoreCase);

        var evt = new TelemetryEvent(
            level,
            Redact(message),
            context,
            DateTimeOffset.UtcNow,
            tags ?? new Dictionary<string, string>(),
            safeData);

        foreach (var sink in _sinks)
        {
            await sink.EmitAsync(evt, ct);
        }
    }

    private static string Redact(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;

        string redacted = Regex.Replace(value, "[A-Za-z]:\\\\[^\\s]+", "[path-redacted]");
        redacted = Regex.Replace(redacted, "(token|apikey|secret)=([^\\s]+)", "$1=[redacted]", RegexOptions.IgnoreCase);
        return redacted;
    }
}
