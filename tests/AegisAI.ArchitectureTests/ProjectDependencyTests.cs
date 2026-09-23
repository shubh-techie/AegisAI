using System.Xml.Linq;
using Xunit;

namespace AegisAI.ArchitectureTests;

public sealed class ProjectDependencyTests
{
    [Theory]
    [InlineData("Domain", new string[] { })]
    [InlineData("Application", new[] { "Domain" })]
    [InlineData("Infrastructure", new[] { "Application", "Domain" })]
    [InlineData("Api", new[] { "Application", "Infrastructure" })]
    public void Production_projects_follow_allowed_dependency_direction(string project, string[] dependencies)
    {
        var root = FindRepositoryRoot();
        var projectPath = Path.Combine(root, "src", $"AegisAI.{project}", $"AegisAI.{project}.csproj");
        var document = XDocument.Load(projectPath);
        var actual = document.Descendants("ProjectReference")
            .Select(reference => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(projectPath)!,
                reference.Attribute("Include")!.Value)))
            .OrderBy(path => path).ToArray();
        var expected = dependencies.Select(dependency =>
            Path.Combine(root, "src", $"AegisAI.{dependency}", $"AegisAI.{dependency}.csproj"))
            .OrderBy(path => path).ToArray();

        Assert.Equal(expected, actual);
        Assert.All(actual, path => Assert.True(File.Exists(path), $"Missing project: {path}"));
        // Inner layers must remain independent of framework and package dependencies.
        if (project is "Domain" or "Application")
        {
            Assert.Empty(document.Descendants("PackageReference"));
            Assert.Empty(document.Descendants("FrameworkReference"));
            Assert.Empty(document.Descendants("Reference"));
        }
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AegisAI.sln")))
                return directory.FullName;
        }

        throw new DirectoryNotFoundException("Could not locate AegisAI.sln.");
    }
}
