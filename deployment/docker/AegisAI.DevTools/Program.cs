using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

const string issuer = "https://aegisai-local.example.invalid";
const string audience = "aegisai-local-development";
if (args.Length == 2 && args[0] == "init")
{
    Directory.CreateDirectory(args[1]);
    using var rsa = RSA.Create(2048);
    var config = JsonNode.Parse(File.ReadAllText("src/AegisAI.Api/appsettings.example.json"))!.AsObject();
    config.Remove("Authentication");
    config["LocalDevelopment"] = new JsonObject { ["PublicKeyPem"] = rsa.ExportSubjectPublicKeyInfoPem() };
    // Synthetic fixtures only; no production identities or posture evidence.
    var json = config.ToJsonString().Replace("replace-with-verified-subject", "synthetic-developer")
        .Replace("https://identity.example.invalid", issuer);
    var configPath = Path.Combine(args[1], "appsettings.Development.json");
    File.WriteAllText(configPath, json);
    if (!OperatingSystem.IsWindows()) File.SetUnixFileMode(configPath, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.GroupRead | UnixFileMode.OtherRead);
    string Encode(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    var now = DateTimeOffset.UtcNow;
    var header = Encode(JsonSerializer.SerializeToUtf8Bytes(new { alg = "RS256", typ = "JWT" }));
    var payload = Encode(JsonSerializer.SerializeToUtf8Bytes(new { iss = issuer, aud = audience,
        sub = "synthetic-developer", nbf = now.AddSeconds(-5).ToUnixTimeSeconds(), exp = now.AddHours(1).ToUnixTimeSeconds() }));
    var unsigned = header + "." + payload;
    var token = unsigned + "." + Encode(rsa.SignData(Encoding.ASCII.GetBytes(unsigned), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));
    var tokenPath = Path.Combine(args[1], "token");
    File.WriteAllText(tokenPath, token);
    if (!OperatingSystem.IsWindows()) File.SetUnixFileMode(tokenPath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
    Console.WriteLine("Generated synthetic local trust and a one-hour credential. Private key discarded; credential not printed.");
    return 0;
}
if (args.Length >= 2 && args[0] is "health" or "demo")
{
    using var client = new HttpClient { BaseAddress = new Uri(args[1]), Timeout = TimeSpan.FromSeconds(10) };
    using var health = await client.GetAsync("/health");
    health.EnsureSuccessStatusCode();
    Console.WriteLine(await health.Content.ReadAsStringAsync());
    if (args[0] == "health") return 0;
    if (args.Length != 3) throw new ArgumentException("demo requires a token file.");
    using var anonymous = await client.PostAsJsonAsync("/authorization/evaluate", new { resource = "reports", action = "read" });
    if (anonymous.StatusCode != HttpStatusCode.Unauthorized) throw new InvalidOperationException("Anonymous request was not rejected.");
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", File.ReadAllText(args[2]).Trim());
    foreach (var (route, expected) in new[] { ("", "ALLOW"), ("/model-b", "ALLOW"), ("/model-c", "STEP_UP") })
    {
        using var response = await client.PostAsJsonAsync("/authorization/evaluate" + route, new { resource = "reports", action = "read" });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(body);
        if (document.RootElement.GetProperty("outcome").GetString() != expected) throw new InvalidOperationException("Unexpected synthetic fixture decision.");
        Console.WriteLine($"Correlation: {response.Headers.GetValues("X-Correlation-ID").Single()}\n{body}");
    }
    using var denied = await client.PostAsJsonAsync("/authorization/evaluate", new { resource = "reports", action = "write" });
    denied.EnsureSuccessStatusCode();
    var denial = await denied.Content.ReadAsStringAsync();
    if (JsonNode.Parse(denial)!["outcome"]!.GetValue<string>() != "DENY") throw new InvalidOperationException("Forbidden action was not denied.");
    Console.WriteLine(denial);
    return 0;
}
Console.Error.WriteLine("Usage: init <directory> | health <base-url> | demo <base-url> <token-file>");
return 2;
