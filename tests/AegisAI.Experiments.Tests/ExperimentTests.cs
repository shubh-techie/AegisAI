using System.Diagnostics;
using AegisAI.Experiments;
using AegisAI.Application.Authorization;
using Xunit;

namespace AegisAI.Experiments.Tests;

public sealed class ExperimentTests
{
    private static ExperimentPlan Plan => new("SYNTHETIC", "test-v1", 42, 1, 2, 2,
        [new("grant", true, "engineering", "read", 0), new("restrict", true, "engineering", "read", .5m)]);

    [Fact]
    public void Real_engines_supply_decisions_not_manifest_outcomes()
    {
        var engines = SyntheticEngines.Create(new("fixture", true, "engineering", "read", .5m));
        Assert.Equal(AuthorizationOutcome.ALLOW, engines["A"]());
        Assert.Equal(AuthorizationOutcome.ALLOW, engines["B"]());
        Assert.Equal(AuthorizationOutcome.LIMIT, engines["C"]());
        var mismatch = SyntheticEngines.Create(new("fixture", true, "finance", "read", 0));
        Assert.Equal(AuthorizationOutcome.ALLOW, mismatch["A"]());
        Assert.Equal(AuthorizationOutcome.DENY, mismatch["B"]());
        Assert.Equal(AuthorizationOutcome.DENY, mismatch["C"]());
        Assert.False(engines.ContainsKey("D"));
    }

    [Fact]
    public void Same_seed_pairs_workloads_and_excludes_warmup_from_counts()
    {
        var first = ExperimentRunner.Run(Plan);
        var second = ExperimentRunner.Run(Plan);
        Assert.Equal(first.ModelOrders.SelectMany(x => x), second.ModelOrders.SelectMany(x => x));
        Assert.Equal(first.Observations.Select(o => (o.Model, o.Phase, o.Scenario, o.Decision)),
            second.Observations.Select(o => (o.Model, o.Phase, o.Scenario, o.Decision)));
        foreach (var repetition in Enumerable.Range(0, Plan.Repetitions))
        {
            var sequences = new[] { "A", "B", "C" }.Select(model => first.Observations
                .Where(o => o.Repetition == repetition && o.Model == model && o.Phase == "measured").Select(o => o.Scenario).ToArray()).ToArray();
            Assert.Equal(sequences[0], sequences[1]); Assert.Equal(sequences[0], sequences[2]);
        }
        Assert.All(first.Models, model =>
        {
            Assert.Equal(8, model.Attempts);
            Assert.Equal(8, model.Completed);
            Assert.Equal(0, model.Errors);
            Assert.Equal(8, model.Decisions.Values.Sum());
            Assert.True(model.WindowSeconds > 0);
            Assert.Equal(model.Completed / model.WindowSeconds, model.DecisionsPerSecond);
            Assert.True(model.P50Microseconds >= 0);
        });
    }

    [Fact]
    public void Summaries_count_errors_and_use_nearest_rank_percentiles()
    {
        var ticks = Stopwatch.Frequency;
        Observation[] raw = [new(0,"A","warmup",0,"s",100*ticks,"ALLOW",null),
            new(0,"A","measured",0,"s",ticks,"ALLOW",null), new(0,"A","measured",1,"s",3*ticks,"DENY",null),
            new(0,"A","measured",2,"s",ticks,null,"TestError")];
        var summary = ExperimentRunner.Summarize("A", raw, 4);
        Assert.Equal(3, summary.Attempts); Assert.Equal(2, summary.Completed); Assert.Equal(1, summary.Errors);
        Assert.Equal(.5, summary.DecisionsPerSecond);
        Assert.Equal(2_000_000, summary.MeanMicroseconds);
        Assert.Equal(1_000_000, summary.P50Microseconds);
        Assert.Equal(3_000_000, summary.P95Microseconds);
        Assert.Null(ExperimentRunner.Summarize("B", raw, 1).MeanMicroseconds);
    }

    [Fact]
    public void Invalid_or_unlabeled_plans_are_rejected()
    {
        Assert.Throws<ArgumentException>(() => (Plan with { DataKind = "PRODUCTION" }).Validate());
        Assert.Throws<ArgumentException>(() => (Plan with { MeasurementCycles = 0 }).Validate());
        Assert.Throws<ArgumentException>(() => (Plan with { Repetitions = 0 }).Validate());
        Assert.Throws<ArgumentException>(() => (Plan with { Scenarios = [Plan.Scenarios[0], Plan.Scenarios[0]] }).Validate());
        Assert.Throws<ArgumentException>(() => (Plan with { Scenarios = [new("bad", true, "engineering", "read", 2)] }).Validate());
    }
}
