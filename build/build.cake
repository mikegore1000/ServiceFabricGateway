#tool "nuget:?package=NUnit.Runners&version=2.6.4"

// ARGUMENTS
var target = Argument<string>("target");
var configuration = Argument("configuration", "Release");
var packageVersion = Argument<string>("packageVersion");

// PREPARATION
// Solution paths
var solutionFile = "../ServiceFabricGateway/ServiceFabricGateway.sln";
var serviceFabricDir = "../ServiceFabricGateway/ServiceFabricGateway/";
var serviceFabricProject = serviceFabricDir + "ServiceFabricGateway.sfproj";
var serviceFabricPackageOutput = serviceFabricDir + "pkg/" + configuration + "/";

// Package options
var packageOutput = "./nuget";
var packageId = "ServiceFabricGateway.Application";
var packageAuthors = new [] { "Mike Gore" };
var packageDescription = "Service Fabric Gateway deployment package";

// XML transform config
var xmlSettings = new XmlPokeSettings 
{
    Namespaces = new Dictionary<string, string> 
    {
        { "sf", "http://schemas.microsoft.com/2011/01/fabric" }
    }
};

// TASKS
Task("Clean")
    .Does(() => CleanDirectory(packageOutput));

Task("Restore-NuGet-Packages")
    .Does(() => NuGetRestore(solutionFile));

Task("Build-Solution")
    .Does(() => MSBuild(
        solutionFile,
        settings => settings.SetConfiguration(configuration)));

Task("Run-Unit-Tests")
    .Does(() => NUnit("../**/bin/" + configuration + "/*.Tests.dll"));

Task("Create-Service-Fabric-Package")
    .Does(() => MSBuild(
        serviceFabricProject, 
        settings => settings.SetConfiguration(configuration).WithTarget("Package")));

Task("Set-Service-Fabric-Package-Version")
    .Does(() =>
{
    foreach(var m in GetFiles(serviceFabricPackageOutput + "**/ApplicationManifest.xml"))
    {
        XmlPoke(m, "//sf:ApplicationManifest/@ApplicationTypeVersion", packageVersion, xmlSettings);
        XmlPoke(m, "//sf:ServiceManifestImport", packageVersion, xmlSettings);
    }    

    foreach(var m in GetFiles(serviceFabricPackageOutput + "**/ServiceManifest.xml"))
    {
        XmlPoke(m, "//sf:ServiceManifest/@Version", packageVersion, xmlSettings);
        XmlPoke(m, "//sf:CodePackage/@Version", packageVersion, xmlSettings);
        XmlPoke(m, "//sf:ConfigPackage/@Version", packageVersion, xmlSettings);
    }
});

// This approach requires a .nuspec file alongside the SF package
/*
Task("Create-OD-Package")
    .Does(() =>
{
    var nuspecFile = new FilePath(serviceFabricProject).ChangeExtension(".nuspec");
    var settings =  new NuGetPackSettings 
    { 
        Version = packageVersion,
        Properties = new Dictionary<string, string> 
        {
            { "configuration", configuration }
        }
    };

    NuGetPack(nuspecFile, settings);
});
*/

// This approach creates the nuspec file and package in one go
Task("Create-OD-Package")
    .Does(() =>
{
    CreateDirectory(packageOutput);

    var settings =  new NuGetPackSettings 
    { 
        Id = packageId,
        Authors = packageAuthors,
        Description = packageDescription,
        Version = packageVersion,
        Files = new [] {
            new NuSpecContent { Source = @"pkg\" + configuration + @"\**", Target = "pkg" },
            new NuSpecContent { Source = @"ApplicationParameters\Cloud.xml", Target = "ApplicationParameters" },
            new NuSpecContent { Source = @"PublishProfiles\ODProfile.xml", Target = "PublishProfiles" },
            new NuSpecContent { Source = @"scripts\Deploy-FabricApplication.ps1", Target = "scripts" }
        },
        BasePath = serviceFabricDir,
        OutputDirectory = packageOutput
    };

    NuGetPack(settings);
});

Task("Publish-OD-Package")
    .Does(() =>
{
    var nugetSource = Argument<string>("nugetSource");
    var nugetApiKey = Argument<string>("nugetApiKey");
    var packages = GetFiles("*.nupkg");

    NuGetPush(packages, new NuGetPushSettings {
        Source = nugetSource,
        ApiKey = nugetApiKey
    });
});

// TASK TARGETS
Task("Default")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore-NuGet-Packages")
    .IsDependentOn("Build-Solution")
    .IsDependentOn("Run-Unit-Tests")
    .IsDependentOn("Set-Service-Fabric-Package-Version")
    .IsDependentOn("Create-OD-Package");

Task("Publish")
    .IsDependentOn("Default")
    .IsDependentOn("Publish-OD-Package");

// EXECUTION
RunTarget(target);