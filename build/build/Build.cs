using System;
using System.Collections.Generic;
using System.Linq;
using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.IO;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Compile);

    [Solution]
    readonly Solution Solution;

    [GitRepository]
    readonly GitRepository GitRepository;

    [Parameter("configuration")]
    public string Configuration { get; set; }

    [Parameter("version-suffix")]
    public string VersionSuffix { get; set; }

    [Parameter("publish-framework")]
    public string PublishFramework { get; set; }

    [Parameter("publish-runtime")]
    public string PublishRuntime { get; set; }

    [Parameter("publish-project")]
    public string PublishProject { get; set; }

    [Parameter("publish-self-contained")]
    public bool PublishSelfContained { get; set; } = true;

    AbsolutePath SourceDirectory => RootDirectory / "src";

    AbsolutePath TestsDirectory => RootDirectory / "tests";

    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

    protected override void OnBuildInitialized()
    {
        Configuration = Configuration ?? "Release";
        VersionSuffix = VersionSuffix ?? "";
        InitializeBuild();
    }

    private void InitializeBuild()
    {
        if (OperatingSystem.IsMacOS())
        {
            var projectsToRemove = Solution.AllProjects.Where(x => x.Name == "WebViewSample");

            foreach (var project in projectsToRemove)
            {
                Console.WriteLine($"Removed project: {project.Name}");
                Solution.RemoveProject(project);
            }
            Solution.Save();
        }
    }

    private void DeleteDirectories(IReadOnlyCollection<AbsolutePath> directories)
    {
        foreach (var directory in directories)
        {
            directory.DeleteDirectory();
        }
    }

    Target Clean => _ => _
        .Executes(() =>
        {
            
            DeleteDirectories(SourceDirectory.GlobDirectories("**/bin", "**/obj"));
            DeleteDirectories(TestsDirectory.GlobDirectories("**/bin", "**/obj"));
            ArtifactsDirectory.CreateOrCleanDirectory();
        });

    Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetVersionSuffix(VersionSuffix)
                .EnableNoRestore());
        });

    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTest(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetLoggers("trx")
                .SetResultsDirectory(ArtifactsDirectory / "TestResults")
                .EnableNoBuild()
                .EnableNoRestore());
        });

    Target Pack => _ => _
        .DependsOn(Test)
        .Executes(() =>
        {
            DotNetPack(s => s
                .SetProject(Solution)
                .SetConfiguration(Configuration)
                .SetVersionSuffix(VersionSuffix)
                .SetOutputDirectory(ArtifactsDirectory / "NuGet")
                .EnableNoBuild()
                .EnableNoRestore());
        });

    Target Publish => _ => _
        .DependsOn(Test)
        .Requires(() => PublishRuntime)
        .Requires(() => PublishFramework)
        .Requires(() => PublishProject)
        .Executes(() =>
        {
            DotNetPublish(s => s
                .SetProject( Solution.AllProjects.FirstOrDefault(x => x.Name == PublishProject))
                .SetConfiguration(Configuration)
                .SetVersionSuffix(VersionSuffix)
                .SetFramework(PublishFramework)
                .SetRuntime(PublishRuntime)
                .SetSelfContained(PublishSelfContained)
                .SetOutput(ArtifactsDirectory / "Publish" / PublishProject + "-" + PublishFramework + "-" + PublishRuntime));
        });
}
