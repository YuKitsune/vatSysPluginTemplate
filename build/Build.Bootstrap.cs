using System.Linq;
using System.Text.RegularExpressions;
using Fallout.Common;
using Fallout.Common.IO;
using Fallout.Common.Tools.Git;
using Fallout.Common.Tools.GitHub;
using Serilog;

partial class Build
{
    // The template values that the Bootstrap target replaces
    const string PluginNamePlaceholder = "My Plugin";
    const string ProjectNamePlaceholder = "MyPlugin";
    const string MailboxNamePlaceholder = "dev.example.my-plugin";
    const string RepositoryPlaceholder = "YuKitsune/vatSysPluginTemplate";

    // The Bootstrap target changes only the repository in this file, because the shield URLs contain it
    AbsolutePath ReadmePath => RootDirectory / "README.md";

    // The Bootstrap target runs only when this file exists. It deletes the file when it is complete.
    AbsolutePath TemplateMarkerPath => RootDirectory / ".template";

    // The Bootstrap target deletes this file when it is complete, because the bootstrapped project does not use it
    AbsolutePath BootstrapBuildFilePath => RootDirectory / "build" / "Build.Bootstrap.cs";

    static readonly string[] BootstrapFileExtensions = [".cs", ".csproj", ".slnx", ".xaml", ".bat"];

    [Parameter("The display name of the plugin (e.g. 'Hold Plugin')")]
    readonly string PluginName;

    [Parameter("The name of the project, the assembly, and the root namespace (e.g. 'HoldPlugin')")]
    readonly string ProjectName;

    [Parameter("The reverse DNS name of the plugin mailbox (e.g. 'dev.example.hold')")]
    readonly string MailboxName;

    Target Bootstrap => _ => _
        .Description("Replaces the template values with the names of your plugin. You can run this target only one time.")
        .Requires(() => PluginName)
        .Requires(() => ProjectName)
        .Requires(() => MailboxName)
        .Executes(() =>
        {
            Assert.FileExists(TemplateMarkerPath, "The project is already bootstrapped");
            Assert.True(
                Regex.IsMatch(PluginName, @"^[A-Za-z0-9][A-Za-z0-9 ._()-]*$"),
                "The plugin name can contain only letters, digits, spaces, and the characters . _ ( ) -");
            Assert.True(
                Regex.IsMatch(ProjectName, @"^[A-Za-z_][A-Za-z0-9_]*(\.[A-Za-z_][A-Za-z0-9_]*)*$"),
                "The project name must be a valid C# namespace (e.g. 'HoldPlugin')");
            Assert.True(
                Regex.IsMatch(MailboxName, @"^[a-z0-9-]+(\.[a-z0-9-]+)+$"),
                "The mailbox name must be a lowercase reverse DNS name (e.g. 'dev.example.hold')");
            Assert.True(
                GitRepository?.IsGitHubRepository() == true,
                "The repository must have a GitHub remote, so the README shields can link to it");

            var files = GitTasks.Git("ls-files", workingDirectory: RootDirectory, logOutput: false)
                .Select(o => RootDirectory / o.Text)
                .Where(f => BootstrapFileExtensions.Contains(f.Extension))
                .ToArray();

            foreach (var file in files)
            {
                var text = file.ReadAllText();

                // Replace the mailbox name first, because it is the longest value
                var newText = text
                    .Replace(MailboxNamePlaceholder, MailboxName)
                    .Replace(PluginNamePlaceholder, PluginName)
                    .Replace(ProjectNamePlaceholder, ProjectName);

                if (newText == text)
                    continue;

                file.WriteAllText(newText);
                Log.Information("Updated {File}", RootDirectory.GetRelativePathTo(file));
            }

            var repository = $"{GitRepository.GetGitHubOwner()}/{GitRepository.GetGitHubName()}";
            ReadmePath.WriteAllText(ReadmePath.ReadAllText().Replace(RepositoryPlaceholder, repository));
            Log.Information("Updated the repository in {File} to {Repository}", RootDirectory.GetRelativePathTo(ReadmePath), repository);

            var sourceDirectory = RootDirectory / "source";
            RenamePath(sourceDirectory / ProjectNamePlaceholder / $"{ProjectNamePlaceholder}.csproj", $"{ProjectName}.csproj");
            RenamePath(sourceDirectory / ProjectNamePlaceholder, ProjectName);
            RenamePath(sourceDirectory / $"{ProjectNamePlaceholder}.Core" / $"{ProjectNamePlaceholder}.Core.csproj", $"{ProjectName}.Core.csproj");
            RenamePath(sourceDirectory / $"{ProjectNamePlaceholder}.Core", $"{ProjectName}.Core");
            RenamePath(sourceDirectory / $"{ProjectNamePlaceholder}.Tests" / $"{ProjectNamePlaceholder}.Tests.csproj", $"{ProjectName}.Tests.csproj");
            RenamePath(sourceDirectory / $"{ProjectNamePlaceholder}.Tests", $"{ProjectName}.Tests");
            RenamePath(sourceDirectory / $"{ProjectNamePlaceholder}.slnx", $"{ProjectName}.slnx");

            DeleteBootstrapFile(TemplateMarkerPath);
            DeleteBootstrapFile(BootstrapBuildFilePath);

            Log.Information("Bootstrap complete");
        });

    void DeleteBootstrapFile(AbsolutePath path)
    {
        path.DeleteFile();
        Log.Information("Deleted {File}", RootDirectory.GetRelativePathTo(path));
    }

    void RenamePath(AbsolutePath path, string newName)
    {
        path.Rename(newName, ExistsPolicy.Fail);
        Log.Information("Renamed {Path} to {NewName}", RootDirectory.GetRelativePathTo(path), newName);
    }
}
