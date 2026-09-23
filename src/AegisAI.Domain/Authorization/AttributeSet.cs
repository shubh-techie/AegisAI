using System.Collections.ObjectModel;

namespace AegisAI.Domain.Authorization;

public sealed class AttributeSet
{
    private readonly IReadOnlyDictionary<string, string> _values;
    public static AttributeSet Empty { get; } = new([]);

    public AttributeSet(IEnumerable<KeyValuePair<string, string>> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        var copy = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (key, value) in values)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            if (!copy.TryAdd(key, value)) throw new ArgumentException("Duplicate attribute key.", nameof(values));
        }
        _values = new ReadOnlyDictionary<string, string>(copy);
    }

    public string? Find(string key) => _values.TryGetValue(key, out var value) ? value : null;
}
