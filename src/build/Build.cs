using System.Text;
using System.Text.RegularExpressions;
using _build;
using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.GitHub;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Tools.VSWhere;
using Nuke.Common.Utilities.Collections;
using Octokit;
using Serilog;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;

class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode
    readonly string[] Projects =
    {
        "RhythmUI",
    };

    const string ArtifactsFolder = "output";
    public const string InstallerProject = "DeployInstaller";

    public const string BuildConfiguration = "Release";
    public const string InstallerConfiguration = "Installer";
    const string BinOutputFolder = "bin";

    static readonly Lazy<string> MsBuildPath = new(() =>
    {
        if (IsServerBuild) return null;
        var (_, output) = VSWhereTasks.VSWhere(settings => settings
            .EnableLatest()
            .AddRequires("Microsoft.Component.MSBuild")
            .SetProperty("installationPath")
        );

        if (output.Count > 0) return null;
        if (!File.Exists(CustomMsBuildPath)) throw new Exception($"Missing file: {CustomMsBuildPath}. Change the path to the build platform or install Visual Studio.");
        return CustomMsBuildPath;
    });

    //Specify the path to the MSBuild.exe file here if you are not using VisualStudio
    const string CustomMsBuildPath = @"C:\Program Files\JetBrains\JetBrains Rider\tools\MSBuild\Current\Bin\MSBuild.exe";
    readonly AbsolutePath ArtifactsDirectory = RootDirectory /InstallerProject / ArtifactsFolder;
    readonly AbsolutePath ChangeLogPath = RootDirectory / "CHANGELOG.md";
    [GitRepository] readonly GitRepository GitRepository;
    [Solution] readonly Solution Solution;
    
    [GitVersion(NoFetch = true)] readonly GitVersion GitVersion;
    [Parameter] string GitHubToken { get; set; }
    readonly Regex VersionRegex = new(@"(\d+\.)+\d+", RegexOptions.Compiled);
    public static int Main () => Execute<Build>(x => x.Clean);
    
    Target Compile => _ => _
        .TriggeredBy(Clean)
        .Executes(() =>
        {
            Console.WriteLine("Starting Some Process Boring ....^_^");
        });
    Target Clean => _ => _
        .Executes(() =>
        {
            // clear the output directory of msi file
            EnsureCleanDirectory(ArtifactsDirectory);
            if (IsServerBuild) return;
            // clear all old dll release
            foreach (var projectName in Projects)
            {
                var project = BuilderExtensions.GetProject(Solution, projectName);
                var binDirectory = (AbsolutePath)new DirectoryInfo(project.GetBinDirectory()).FullName;
                Console.WriteLine(binDirectory);
                binDirectory.GlobDirectories($"{BinOutputFolder}", $"{BuildConfiguration}*").ForEach(DeleteDirectory);
            }
        });

    Target RestoreAndBuild => _ => _
        .TriggeredBy(Compile)
        .Executes(() =>
        {
            var configurations = Solution.GetConfigurations(BuildConfiguration);
            configurations.ForEach(configuration =>
            {
                MSBuild(s => s
                    .SetTargets("Rebuild")
                    .SetProcessToolPath(MsBuildPath.Value)
                    .SetConfiguration(configuration)
                    .SetVerbosity(MSBuildVerbosity.Minimal)
                    .DisableNodeReuse()
                    .EnableRestore());
            });
        });
   
    Target CreateInstaller => _ => _
        .TriggeredBy(RestoreAndBuild)
        .OnlyWhenStatic(()=>IsLocalBuild || GitRepository.IsOnDevelopBranch())
        .Executes(() =>
        {
            //TODO
            // var configurations = Solution.GetConfigurations(InstallerConfiguration);
            // configurations.ForEach(configuration =>
            // {
            //     MSBuild(s => s
            //         .SetTargets("Rebuild")
            //         .SetProcessToolPath(MsBuildPath.Value)
            //         .SetConfiguration(configuration)
            //         .SetVerbosity(MSBuildVerbosity.Minimal)
            //         .DisableNodeReuse()
            //         .EnableRestore());
            // });
        });

    Target PublishGitHubRelease => _ => _
        .Requires(() => GitHubToken)
        .Requires(() => GitRepository)
        .Requires(() => GitVersion)
        .OnlyWhenStatic(() => GitRepository.IsOnMainOrMasterBranch())
        // .OnlyWhenStatic(() => IsServerBuild)
        .TriggeredBy(CreateInstaller)
        .Executes(() =>
        {
            GitHubTasks.GitHubClient = new GitHubClient(new ProductHeaderValue(Solution.Name))
            {
                Credentials = new Credentials(GitHubToken)
            };
            var gitHubName = GitRepository.GetGitHubName();
            var gitHubOwner = GitRepository.GetGitHubOwner();
            var artifacts = Directory.GetFiles(ArtifactsDirectory, "*");
            var version = GetProductVersion(artifacts);

            CheckTags(gitHubOwner, gitHubName, version);
            Log.Information("Detected Tag: {Version}", version);
            var newRelease = new NewRelease(version)
            {
                Name = version,
                //Body = CreateChangelog(version),
                Draft = true,
                TargetCommitish = GitVersion.Sha,
                GenerateReleaseNotes = true,

            };
            var draft = CreatedDraft(gitHubOwner, gitHubName, newRelease);
            UploadArtifacts(draft, artifacts);
            ReleaseDraft(gitHubOwner, gitHubName, draft);
        });
    string GetProductVersion(IEnumerable<string> artifacts)
    {
        var stringVersion = string.Empty;
        var doubleVersion = 0d;
        foreach (var file in artifacts)
        {
            var fileInfo = new FileInfo(file);
            var match = VersionRegex.Match(fileInfo.Name);
            if (!match.Success) continue;
            var version = match.Value;
            var parsedValue = double.Parse(version.Replace(".", ""));
            if (parsedValue > doubleVersion)
            {
                doubleVersion = parsedValue;
                stringVersion = version;
            }
        }

        if (stringVersion.Equals(string.Empty)) throw new ArgumentException("Could not determine product version from artifacts.");

        return stringVersion;
    }

    static void UploadArtifacts(Release createdRelease, IEnumerable<string> artifacts)
    {
        foreach (var file in artifacts)
        {
            var releaseAssetUpload = new ReleaseAssetUpload
            {
                ContentType = "application/x-binary",
                FileName = Path.GetFileName(file),
                RawData = File.OpenRead(file)
            };
            var _ = GitHubTasks.GitHubClient.Repository.Release.UploadAsset(createdRelease, releaseAssetUpload).Result;
            Log.Information("Added artifact: {Path}", file);
        }
    }

    static Release CreatedDraft(string gitHubOwner, string gitHubName, NewRelease newRelease) =>
        GitHubTasks.GitHubClient.Repository.Release
            .Create(gitHubOwner, gitHubName, newRelease)
            .Result;

    static void ReleaseDraft(string gitHubOwner, string gitHubName, Release draft)
    {
        var _ = GitHubTasks.GitHubClient.Repository.Release
            .Edit(gitHubOwner, gitHubName, draft.Id, new ReleaseUpdate { Draft = false })
            .Result;
    }

    string CreateChangelog(string version)
    {
        if (!File.Exists(ChangeLogPath))
        {
            Log.Warning("Can't find changelog file: {Log}", ChangeLogPath);
            return string.Empty;
        }
        Log.Information("Detected Changelog: {Path}", ChangeLogPath);

        var logBuilder = new StringBuilder();
        var changelogLineRegex = new Regex($@"^.*({version})\S*\s?");
        const string nextRecordSymbol = "- ";

        foreach (var line in File.ReadLines(ChangeLogPath))
        {
            if (logBuilder.Length > 0)
            {
                if (line.StartsWith(nextRecordSymbol)) break;
                logBuilder.AppendLine(line);
                continue;
            }
            if (!changelogLineRegex.Match(line).Success) continue;
            var truncatedLine = changelogLineRegex.Replace(line, string.Empty);
            logBuilder.AppendLine(truncatedLine);
        }

        if (logBuilder.Length == 0) Log.Warning("There is no version entry in the changelog: {Version}", version);
        return logBuilder.ToString();
    }

    static void CheckTags(string gitHubOwner, string gitHubName, string version)
    {
        var gitHubTags = GitHubTasks.GitHubClient.Repository
            .GetAllTags(gitHubOwner, gitHubName)
            .Result;

        if (gitHubTags.Select(tag => tag.Name).Contains(version)) throw new ArgumentException($"The repository already contains a Release with the tag: {version}");
    }

}
