namespace AutoServiceApp.Tests;

public class TextLanguageTests
{
    [Fact]
    public void SourceFilesDoNotContainCyrillicText()
    {
        var root = FindRepositoryRoot();
        var files = Directory.GetFiles(root, "*.*", SearchOption.AllDirectories)
            .Where(x => !x.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
            .Where(x => !x.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
            .Where(x => !x.Contains($"{Path.DirectorySeparatorChar}TestResults{Path.DirectorySeparatorChar}"))
            .Where(x => !x.Contains($"{Path.DirectorySeparatorChar}.git{Path.DirectorySeparatorChar}"))
            .Where(x => x.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)
                || x.EndsWith(".axaml", StringComparison.OrdinalIgnoreCase)
                || x.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase)
                || x.EndsWith(".manifest", StringComparison.OrdinalIgnoreCase));

        var offenders = files
            .SelectMany(file => File.ReadLines(file).Select((line, index) => new { file, line, index }))
            .Where(x => x.line.Any(ch => ch >= '\u0400' && ch <= '\u04FF'))
            .Select(x => $"{Path.GetRelativePath(root, x.file)}:{x.index + 1}: {x.line}")
            .ToList();

        Assert.Empty(offenders);
    }

    private static string FindRepositoryRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "AutoServiceApp")))
            dir = dir.Parent;

        return dir?.FullName ?? AppContext.BaseDirectory;
    }
}
