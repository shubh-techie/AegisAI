using System.Diagnostics;
using AegisAI.Application.Authorization;
using AegisAI.Domain.Authorization;
using AegisAI.Infrastructure.Authorization;

namespace AegisAI.Experiments;

public sealed record Scenario(string Id, bool AssignedRole, string? Department, string Action, decimal? RiskIndicator);
public sealed record ExperimentPlan(string DataKind, string Version, int Seed, int WarmupCycles,
    int MeasurementCycles, int Repetitions, Scenario[] Scenarios)
{
    public void Validate()
    {
        if (DataKind != "SYNTHETIC" || string.IsNullOrWhiteSpace(Version)) throw new ArgumentException("A versioned SYNTHETIC plan is required.");
        if (WarmupCycles < 1 || MeasurementCycles < 1 || Repetitions < 1 ||
            WarmupCycles > 10000 || MeasurementCycles > 10000 || Repetitions > 100)
            throw new ArgumentException("Cycles must be 1..10000 and repetitions 1..100.");
        if (Scenarios is null || Scenarios.Length is < 1 or > 100 ||
            Scenarios.Any(s => s is null || string.IsNullOrWhiteSpace(s.Id) || string.IsNullOrWhiteSpace(s.Action) ||
                (s.Department is not null && string.IsNullOrWhiteSpace(s.Department)) || s.RiskIndicator is < 0m or > 1m) ||
            Scenarios.Select(s => s.Id).Distinct(StringComparer.Ordinal).Count() != Scenarios.Length)
            throw new ArgumentException("Scenarios require unique IDs and valid inputs.");
        if ((long)(WarmupCycles + MeasurementCycles) * Repetitions * Scenarios.Length * 3 > 2_000_000)
            throw new ArgumentException("Plan exceeds the two-million-call safety limit.");
    }
}

public sealed record Observation(int Repetition, string Model, string Phase, int Sequence, string Scenario,
    long ElapsedTicks, string? Decision, string? ErrorType);
public sealed record ModelMeasurement(string Model, int Attempts, int Completed, int Errors, double WindowSeconds,
    double DecisionsPerSecond, double? MeanMicroseconds, double? P50Microseconds, double? P95Microseconds,
    double? P99Microseconds, Dictionary<string, int> Decisions);
public sealed record RunMeasurements(List<Observation> Observations, List<ModelMeasurement> Models, List<string[]> ModelOrders);

// PLANNED extension points only; no implementation or inferred ground truth.
public sealed record LabeledDetection(bool IsMalicious, bool WasFlagged);
public sealed record ErrorRates(double? FalsePositiveRate, double? FalseNegativeRate);
public interface IFalsePositiveNegativeAnalysis { ErrorRates Analyze(IReadOnlyList<LabeledDetection> labeledDetections); }
public interface IBehavioralAnomalyEvaluation { IReadOnlyList<LabeledDetection> Evaluate(string independentlyLabeledDatasetPath); }

public static class SyntheticEngines
{
    public static IReadOnlyDictionary<string, Func<AuthorizationOutcome>> Create(Scenario scenario)
    {
        var request = new AuthorizationRequest(new("synthetic-actor", "https://synthetic.example.invalid"), new("synthetic-report"), new(scenario.Action));
        var policy = new RbacPolicy("synthetic-rbac-v1", [new("reader", [new(new("synthetic-report"), new("read"))])],
            scenario.AssignedRole ? [new(request.Subject, new[] { "reader" })] : []);
        var a = new RbacAuthorizationEngine(new InMemoryRbacPolicyProvider(policy));
        var attributes = scenario.Department is null ? AttributeSet.Empty : new AttributeSet([new("department", scenario.Department)]);
        var context = new InMemoryAbacContextProvider("synthetic-abac-v1",
            [new AbacRule("engineering-read", new("synthetic-report"), new("read"), [new(AttributeScope.Subject, "department", "engineering")])],
            [new(request.Subject, attributes)], [], AttributeSet.Empty);
        var b = new ModelBAuthorizationEngine(a, context, new AbacEvaluator());
        var risk = new RiskContext("synthetic-risk-v1", scenario.RiskIndicator is { } value
            ? Enum.GetValues<RiskSignal>().Select(signal => new KeyValuePair<RiskSignal, decimal>(signal, value)) : []);
        var c = new ModelCAuthorizationEngine(b, new InMemoryRiskContextProvider("synthetic-risk-v1", [new(request, risk)]), new ContextualRiskEngine());
        return new Dictionary<string, Func<AuthorizationOutcome>>
        {
            ["A"] = () => a.Authorize(request).Outcome,
            ["B"] = () => b.Authorize(request).Outcome,
            ["C"] = () => c.Authorize(request).Outcome
        };
    }
}

public static class ExperimentRunner
{
    public static RunMeasurements Run(ExperimentPlan plan)
    {
        plan.Validate();
        var engines = plan.Scenarios.Select(SyntheticEngines.Create).ToArray();
        var observations = new List<Observation>();
        var windows = new Dictionary<string, double> { ["A"] = 0, ["B"] = 0, ["C"] = 0 };
        var orders = new List<string[]>();
        var random = new Random(plan.Seed);
        for (var repetition = 0; repetition < plan.Repetitions; repetition++)
        {
            var workload = Enumerable.Range(0, plan.MeasurementCycles).SelectMany(_ => Enumerable.Range(0, engines.Length)).ToArray();
            random.Shuffle(workload);
            var models = new[] { "A", "B", "C" };
            random.Shuffle(models);
            orders.Add(models);
            foreach (var model in models)
            {
                Measure("warmup", Enumerable.Range(0, plan.WarmupCycles * engines.Length).Select(i => i % engines.Length).ToArray());
                windows[model] += Measure("measured", workload);

                double Measure(string phase, int[] sequence)
                {
                    // Preallocate to avoid list growth in the throughput window.
                    var buffer = new Observation[sequence.Length];
                    var windowStart = Stopwatch.GetTimestamp();
                    for (var i = 0; i < sequence.Length; i++)
                    {
                        var index = sequence[i];
                        var evaluate = engines[index][model];
                        string? error = null;
                        AuthorizationOutcome? decision = null;
                        var start = Stopwatch.GetTimestamp();
                        try { decision = evaluate(); }
                        catch (Exception exception) { error = exception.GetType().Name; }
                        var ticks = Stopwatch.GetTimestamp() - start;
                        buffer[i] = new(repetition, model, phase, i, plan.Scenarios[index].Id, ticks, decision?.ToString(), error);
                    }
                    var seconds = Stopwatch.GetElapsedTime(windowStart).TotalSeconds;
                    observations.AddRange(buffer);
                    return seconds;
                }
            }
        }
        return new(observations, windows.Select(pair => Summarize(pair.Key, observations, pair.Value)).ToList(), orders);
    }

    public static ModelMeasurement Summarize(string model, IEnumerable<Observation> observations, double seconds)
    {
        if (seconds <= 0 || !double.IsFinite(seconds)) throw new ArgumentOutOfRangeException(nameof(seconds));
        var measured = observations.Where(o => o.Model == model && o.Phase == "measured").ToArray();
        var complete = measured.Where(o => o.ErrorType is null && o.Decision is not null).ToArray();
        var latency = complete.Select(o => o.ElapsedTicks * 1_000_000d / Stopwatch.Frequency).Order().ToArray();
        double? Percentile(double p) => latency.Length == 0 ? null : latency[(int)Math.Ceiling(p * latency.Length) - 1];
        var distribution = Enum.GetNames<AuthorizationOutcome>().ToDictionary(outcome => outcome, outcome => complete.Count(o => o.Decision == outcome));
        return new(model, measured.Length, complete.Length, measured.Length - complete.Length, seconds,
            complete.Length / seconds, latency.Length == 0 ? null : latency.Average(), Percentile(.5), Percentile(.95), Percentile(.99), distribution);
    }
}
