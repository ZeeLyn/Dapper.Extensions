#addin nuget:?package=Cake.Git&version=1.1.0

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var output = Argument<string>("output", "./Output");
var version = Argument<string>("version", "5.1.3");
var target = Argument<string>("target", "Default");
var release = Argument<bool>("release", true);
var nugetApiKey = Argument<string>("nugetApiKey", null);
var currentBranch = Argument<string>("currentBranch", GitBranchCurrent("./").FriendlyName);
var configuration = release ? "Release" : "Debug";

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
   // Executed BEFORE the first task.
   Information("Running tasks...");
});

Teardown(ctx =>
{
   // Executed AFTER the last task.
   Information("Finished running tasks.");
});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("UpdateVersion").DoesForEach(GetFiles("**/Dapper.Extensions*.csproj"), (file) =>
{
   Information("Update Version:" + file);
   XmlPoke(file, "/Project/PropertyGroup/Version", version);
   XmlPoke(file, "/Project/PropertyGroup/GeneratePackageOnBuild", "false");
   XmlPoke(file, "/Project/PropertyGroup/Description", "A dapper extension library. Support MySQL,SQL Server,PostgreSQL,SQLite,Oracle and ODBC, Support cache.");
   XmlPoke(file, "/Project/PropertyGroup/PackageProjectUrl", "https://github.com/ZeeLyn/Dapper.Extensions");
   XmlPoke(file, "/Project/PropertyGroup/PackageTags", "Dapper,Dapper Extensions,DapperExtensions,Dapper.Extensions.NetCore,Extensions,DataBase,Sql Server,MSSQL,MySQL,PostgreSQL,SQLite,ODBC,Cahce,Caching,Redis,Memory,RedisCaching,MemoryCaching");
   XmlPoke(file, "/Project/PropertyGroup/icon", "https://raw.githubusercontent.com/DapperLib/Dapper/main/Dapper.png");
   XmlPoke(file, "/Project/PropertyGroup/Authors", "Owen");
   XmlPoke(file, "/Project/PropertyGroup/PackageLicenseExpression", "MIT");
});

Task("Restore").Does(() =>
{
   DotNetCoreRestore();
});

Task("Build").Does(() =>
{
   DotNetCoreBuild("Dapper.Extensions.sln", new DotNetCoreBuildSettings
   {
      Configuration = configuration
   });
});

Task("CleanPackage").Does(() =>
{
   if (DirectoryExists(output))
   {
      DeleteDirectory(output, new DeleteDirectorySettings
      {
         Recursive = true
      });
   }
});

Task("Pack")
.IsDependentOn("CleanPackage")
.IsDependentOn("UpdateVersion")
.DoesForEach(GetFiles("**/Dapper.Extensions*.csproj"), (file) =>
{
   DotNetCorePack(file.ToString(), new DotNetCorePackSettings
   {
      OutputDirectory = output,
      Configuration = configuration
   });
});

Task("Push")
.IsDependentOn("Pack")
.Does(() =>
{
   var nuGetPushSettings = new NuGetPushSettings
   {
      Source = "https://www.nuget.org/api/v2/package",
      ApiKey = nugetApiKey
   };
   if (currentBranch == "master")
   {
      foreach (var package in GetFiles("Output/*.nupkg"))
      {
         NuGetPush(package, nuGetPushSettings);
      }
   }
   else
   {
      Information("Non-master build. Not publishing to NuGet. Current branch: " + currentBranch);
   }
});

Task("Default")
.IsDependentOn("Restore")
.IsDependentOn("Build")
.Does(() =>
{

});



RunTarget(target);