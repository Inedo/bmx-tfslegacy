﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Visual Studio via: 
//     Edit > Paste Special > Paste JSON as Classes
//     
//     JSON generated from: /DefaultCollection/TestProject1/_apis/build/definitions/1
//
// </auto-generated>
//------------------------------------------------------------------------------

using System;

namespace Inedo.Extensions.TFS.VisualStudioOnline.Model
{
    public class GetBuildDefinitionResponse
    {
        public Build[] build { get; set; }
        public Option[] options { get; set; }
        public Variables variables { get; set; }
        public Retentionrule[] retentionRules { get; set; }
        public _Links _links { get; set; }
        public DateTime createdDate { get; set; }
        public string comment { get; set; }
        public string jobAuthorizationScope { get; set; }
        public Repository repository { get; set; }
        public string quality { get; set; }
        public Authoredby authoredBy { get; set; }
        public Queue queue { get; set; }
        public string uri { get; set; }
        public string type { get; set; }
        public int revision { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public Project project { get; set; }
    }

    public class Variables
    {
        public Forceclean forceClean { get; set; }
        public Config config { get; set; }
        public Platform platform { get; set; }
    }

    public class Forceclean
    {
        public string value { get; set; }
        public bool allowOverride { get; set; }
    }

    public class Config
    {
        public string value { get; set; }
        public bool allowOverride { get; set; }
    }

    public class Platform
    {
        public string value { get; set; }
        public bool allowOverride { get; set; }
    }

    public class Authoredby
    {
        public string id { get; set; }
        public string displayName { get; set; }
        public string uniqueName { get; set; }
        public string url { get; set; }
        public string imageUrl { get; set; }
    }
    
    public class Build
    {
        public bool enabled { get; set; }
        public bool continueOnError { get; set; }
        public bool alwaysRun { get; set; }
        public string displayName { get; set; }
        public BuildTask task { get; set; }
        public Inputs inputs { get; set; }
    }

    public class BuildTask
    {
        public string id { get; set; }
        public string versionSpec { get; set; }
    }

    public class Inputs
    {
        public string solution { get; set; }
        public string msbuildArgs { get; set; }
        public string platform { get; set; }
        public string configuration { get; set; }
        public string clean { get; set; }
        public string restoreNugetPackages { get; set; }
        public string vsLocationMethod { get; set; }
        public string vsVersion { get; set; }
        public string vsLocation { get; set; }
        public string msbuildLocationMethod { get; set; }
        public string msbuildVersion { get; set; }
        public string msbuildArchitecture { get; set; }
        public string msbuildLocation { get; set; }
        public string logProjectEvents { get; set; }
        public string testAssembly { get; set; }
        public string testFiltercriteria { get; set; }
        public string runSettingsFile { get; set; }
        public string codeCoverageEnabled { get; set; }
        public string otherConsoleOptions { get; set; }
        public string vsTestVersion { get; set; }
        public string pathtoCustomTestAdapters { get; set; }
    }

    public class Option
    {
        public bool enabled { get; set; }
        public Definition definition { get; set; }
        public Inputs1 inputs { get; set; }
    }

    public class Inputs1
    {
        public string parallel { get; set; }
        public string multipliers { get; set; }
    }

    public class Retentionrule
    {
        public string[] branches { get; set; }
        public int daysToKeep { get; set; }
        public bool deleteBuildRecord { get; set; }
    }
}
