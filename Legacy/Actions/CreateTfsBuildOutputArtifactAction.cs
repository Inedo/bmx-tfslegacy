﻿using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Inedo.Agents;
using Inedo.BuildMaster;
using Inedo.BuildMaster.Artifacts;
using Inedo.BuildMaster.Data;
using Inedo.BuildMaster.Extensibility.Agents;
using Inedo.BuildMaster.Files;
using Inedo.Documentation;
using Inedo.Serialization;
using Microsoft.TeamFoundation.Build.Client;

namespace Inedo.BuildMasterExtensions.TFS
{
    [DisplayName("Capture Artifact from TFS Build Output")]
    [Description("Creates a BuildMaster build artifact from a TFS build server drop location.")]
    [RequiresInterface(typeof(IFileOperationsExecuter))]
    [Inedo.Web.CustomEditor(typeof(CreateTfsBuildOutputArtifactActionEditor))]
    [Tag(Tags.Artifacts)]
    [Tag(Tags.Builds)]
    [Tag("tfs")]
    [PersistFrom("Inedo.BuildMasterExtensions.TFS.CreateTfsBuildOutputArtifactAction,TFS")]
    public sealed class CreateTfsBuildOutputArtifactAction : TfsActionBase
    {
        /// <summary>
        /// Gets or sets the build number if not empty, or includes all builds in the search.
        /// </summary>
        [Persistent]
        public string BuildNumber { get; set; }

        /// <summary>
        /// Gets or sets the name of the artifact if not empty, otherwise use the build definition name.
        /// </summary>
        [Persistent]
        public string ArtifactName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the build spec should include unsuccessful builds.
        /// </summary>
        [Persistent]
        public bool IncludeUnsuccessful { get; set; }

        public override ExtendedRichDescription GetActionDescription()
        {
            var shortDesc = new RichDescription("Capture TFS Build Artifact from ", new Hilite(this.TeamProject));

            var longDesc = new RichDescription("using ");
            if (string.IsNullOrEmpty(this.BuildNumber))
                longDesc.AppendContent("the last successful build");
            else
                longDesc.AppendContent("build ", new Hilite(this.BuildNumber));

            longDesc.AppendContent(" of ");

            if (string.IsNullOrEmpty(this.BuildDefinition))
                longDesc.AppendContent("any build definition");
            else
                longDesc.AppendContent("build definition ", new Hilite(this.BuildDefinition));

            longDesc.AppendContent(".");

            return new ExtendedRichDescription(shortDesc, longDesc);
        }

        protected override void Execute()
        {
            var collection = this.GetTeamProjectCollection();

            var buildService = collection.GetService<IBuildServer>();            

            var spec = buildService.CreateBuildDetailSpec(this.TeamProject, string.IsNullOrEmpty(this.BuildDefinition) ? "*" : this.BuildDefinition);
            if (!string.IsNullOrEmpty(this.BuildNumber))
                spec.BuildNumber = this.BuildNumber;
            spec.MaxBuildsPerDefinition = 1;
            spec.QueryOrder = BuildQueryOrder.FinishTimeDescending;
            spec.Status = BuildStatus.Succeeded;

            if (this.IncludeUnsuccessful)
                spec.Status |= (BuildStatus.Failed | BuildStatus.PartiallySucceeded);

            var result = buildService.QueryBuilds(spec);
            var build = result.Builds.FirstOrDefault();
            if (build == null)
                throw new InvalidOperationException($"Build {this.BuildNumber} for team project {this.TeamProject} definition {this.BuildDefinition} did not return any builds.");

            this.LogDebug($"Build number {build.BuildNumber} drop location: {build.DropLocation}");

            CreateArtifact(string.IsNullOrEmpty(this.ArtifactName) ? build.BuildDefinition.Name : this.ArtifactName, build.DropLocation);
        }

        private void CreateArtifact(string artifactName, string path)
        {
            if (string.IsNullOrEmpty(artifactName) || artifactName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                throw new InvalidOperationException("Artifact Name cannot contain invalid file name characters: " + new string(Path.GetInvalidFileNameChars()));

            if (DB.Releases_GetRelease(this.Context.ApplicationId, this.Context.ReleaseNumber)
                .ReleaseDeployables_Extended
                .Any(rd => rd.Deployable_Id == this.Context.DeployableId && rd.InclusionType_Code == Domains.DeployableInclusionTypes.Referenced))
            {
                this.LogError(
                    "An Artifact cannot be created for this Deployable because the Deployable is Referenced (as opposed to Included) by this Release. " +
                    "To prevent this error, either include this Deployable in the Release or use a Predicate to prevent this action group from being executed.");
                return;
            }

            var fileOps = this.Context.Agent.GetService<IFileOperationsExecuter>();
            var zipPath = fileOps.CombinePath(this.Context.TempDirectory, artifactName + ".zip");

            this.LogDebug("Preparing directories...");
            fileOps.DeleteFiles(new[] { zipPath });

            this.ThrowIfCanceledOrTimeoutExpired();

            var rootEntry = fileOps.GetDirectoryEntry(
                new GetDirectoryEntryCommand
                {
                    Path = path,
                    Recurse = false,
                    IncludeRootPath = false
                }
            ).Entry;

            if ((rootEntry.Files == null || rootEntry.Files.Length == 0) && (rootEntry.SubDirectories == null || rootEntry.SubDirectories.Length == 0))
                this.LogWarning("There are no files to capture in this artifact.");

            this.LogDebug("Zipping output...");
            fileOps.CreateZipFile(path, zipPath);

            this.ThrowIfCanceledOrTimeoutExpired();

            this.LogDebug("Transferring file to artifact library...");

            using (var stream = fileOps.OpenFile(zipPath, FileMode.Open, FileAccess.Read))
            {
                Artifact.CreateArtifact(
                    applicationId: this.Context.ApplicationId,
                    releaseNumber: this.Context.ReleaseNumber,
                    buildNumber: this.Context.BuildNumber,
                    deployableId: this.Context.DeployableId,
                    executionId: this.Context.ExecutionId,
                    artifactName: artifactName,
                    artifactData: stream,
                    overwrite: true
                );
            }

            this.LogDebug("Cleaning up...");
            fileOps.DeleteFiles(new[] { zipPath });

            this.LogInformation("Artfact captured from TFS.");
        }
    }
}
