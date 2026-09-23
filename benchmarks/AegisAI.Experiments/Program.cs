using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using AegisAI.Experiments;

if (args.Length != 4 || args[0] != "--manifest" || args[2] != "--output")
{
    Console.Error.WriteLine("Usage: --manifest <synthetic-plan.json> --output <new-results-directory>");
    return 2;
}
var root = Directory.GetCurrentDirectory();
if (!File.Exists(Path.Combine(root, "AegisAI.sln"))) throw new InvalidOperationException("Run from the repository root.");
var output = Path.GetFullPath(args[3]);
var resultsRoot = Path.Combine(root, "research", "results") + Path.DirectorySeparatorChar;
if (!output.StartsWith(resultsRoot, StringComparison.Ordinal) || Directory.Exists(output))
    throw new ArgumentException("Output must be a NEW directory under research/results.");
var jsonOptions = new JsonSerializerOptions { WriteIndented = true, PropertyNameCaseInsensitive = true,
    UnmappedMemberHandling = System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow };
var manifestBytes = File.ReadAllBytes(args[1]);
var plan = JsonSerializer.Deserialize<ExperimentPlan>(manifestBytes, jsonOptions) ?? throw new ArgumentException("Missing plan.");
plan.Validate();
Directory.CreateDirectory(output);
File.WriteAllBytes(Path.Combine(output, "manifest.json"), manifestBytes);

string Command(string executable, params string[] arguments)
{
    var info = new ProcessStartInfo(executable) { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
    foreach (var argument in arguments) info.ArgumentList.Add(argument);
    using var process = Process.Start(info)!;
    var result = process.StandardOutput.ReadToEnd();
    var error = process.StandardError.ReadToEnd();
    process.WaitForExit();
    if (process.ExitCode != 0) throw new InvalidOperationException($"Metadata command failed: {executable} (exit {process.ExitCode})");
    return result.Trim();
}
string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
var sourceFiles = new[] { "src", "benchmarks", "tests", "deployment" }.SelectMany(folder => Directory.EnumerateFiles(Path.Combine(root, folder), "*", SearchOption.AllDirectories))
    .Where(path => !path.Split(Path.DirectorySeparatorChar).Any(part => part is "bin" or "obj") &&
        (path.EndsWith(".cs") || path.EndsWith(".csproj")))
    .Concat(new[] { "AegisAI.sln", "global.json" }.Select(file => Path.Combine(root, file))).Order().ToArray();
var hashes = sourceFiles.ToDictionary(file => Path.GetRelativePath(root, file), file => Hash(File.ReadAllBytes(file)));
using (var zip = ZipFile.Open(Path.Combine(output, "source.zip"), ZipArchiveMode.Create))
    foreach (var file in sourceFiles) zip.CreateEntryFromFile(file, Path.GetRelativePath(root, file));
var metadata = new
{
    SchemaVersion = 1, DataKind = "SYNTHETIC", ModelD = "PLANNED — not executed",
    Boundary = "In-process engine only; excludes HTTP, authentication, audit, telemetry, and enforcement",
    Concurrency = 1, WorkloadMode = "Sequential closed loop; no fixed arrival rate", StartedUtc = DateTimeOffset.UtcNow,
    GitCommit = Command("git", "rev-parse", "HEAD"), SourceWorkingTreeDirty = Command("git", "status", "--porcelain", "--", "src", "benchmarks", "tests", "deployment", "AegisAI.sln", "global.json", "research/experiments").Length > 0,
    Sdk = Command("dotnet", "--version"), Runtime = RuntimeInformation.FrameworkDescription,
    OS = RuntimeInformation.OSDescription, Architecture = RuntimeInformation.ProcessArchitecture.ToString(),
    LogicalProcessors = Environment.ProcessorCount, AvailableMemoryBytes = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes,
    StopwatchFrequency = Stopwatch.Frequency, Stopwatch.IsHighResolution,
#if DEBUG
    BuildConfiguration = "Debug",
#else
    BuildConfiguration = "Release",
#endif
    GcServer = System.Runtime.GCSettings.IsServerGC, GcLatencyMode = System.Runtime.GCSettings.LatencyMode.ToString(),
    RuntimeKnobs = new[] { "DOTNET_TieredCompilation", "DOTNET_TieredPGO", "DOTNET_ReadyToRun" }
        .ToDictionary(key => key, Environment.GetEnvironmentVariable),
    ManifestSha256 = Hash(manifestBytes), SourceHashes = hashes,
    BinaryHashes = Directory.EnumerateFiles(AppContext.BaseDirectory, "AegisAI.*.dll")
        .ToDictionary(file => Path.GetFileName(file)!, file => Hash(File.ReadAllBytes(file))),
    FutureMetrics = new { FalsePositiveRate = (double?)null, FalseNegativeRate = (double?)null,
        BehavioralAnomalyEvaluation = "PLANNED; requires independent labels and an implemented detector" }
};
File.WriteAllText(Path.Combine(output, "environment.json"), JsonSerializer.Serialize(metadata, jsonOptions));
var processBefore = Process.GetCurrentProcess().TotalProcessorTime;
var allocatedBefore = GC.GetTotalAllocatedBytes();
var run = ExperimentRunner.Run(plan);
var cpu = Process.GetCurrentProcess().TotalProcessorTime - processBefore;
var allocated = GC.GetTotalAllocatedBytes() - allocatedBefore;
using (var writer = new StreamWriter(Path.Combine(output, "observations.jsonl")))
    foreach (var observation in run.Observations) writer.WriteLine(JsonSerializer.Serialize(observation));
var summary = new { CompletedUtc = DateTimeOffset.UtcNow, DataKind = "SYNTHETIC", plan.Version,
    run.ModelOrders, run.Models, ProcessCpuSeconds = cpu.TotalSeconds, AllocatedBytes = allocated,
    Limitations = "Smoke/microbenchmark only. No production performance or detection claims. No durable audit boundary." };
File.WriteAllText(Path.Combine(output, "summary.json"), JsonSerializer.Serialize(summary, jsonOptions));
Console.WriteLine(JsonSerializer.Serialize(summary, jsonOptions));
return run.Observations.Any(o => o.ErrorType is not null) ? 1 : 0;
