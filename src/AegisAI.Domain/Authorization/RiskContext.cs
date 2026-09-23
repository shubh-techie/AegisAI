namespace AegisAI.Domain.Authorization;

public enum RiskSignal { AuthenticationWeakness, ResourceSensitivity, OperationSensitivity, NetworkExposure, DeviceExposure }

/// <summary>Trusted normalized indicators; absent values are unknown, never zero.</summary>
public sealed class RiskContext
{
    private readonly Dictionary<RiskSignal, decimal> _signals;
    public string Version { get; }

    public RiskContext(string version, IEnumerable<KeyValuePair<RiskSignal, decimal>> signals)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(version);
        ArgumentNullException.ThrowIfNull(signals);
        Version = version;
        _signals = new();
        foreach (var (signal, value) in signals)
        {
            if (!Enum.IsDefined(signal) || value < 0m || value > 1m)
                throw new ArgumentOutOfRangeException(nameof(signals), "Indicators must be defined and between zero and one.");
            if (!_signals.TryAdd(signal, value)) throw new ArgumentException("Duplicate risk signal.", nameof(signals));
        }
    }

    public decimal? Find(RiskSignal signal) => _signals.TryGetValue(signal, out var value) ? value : null;
}
