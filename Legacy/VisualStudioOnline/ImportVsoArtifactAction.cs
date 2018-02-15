﻿using System.ComponentModel;
using Inedo.Agents;
using Inedo.BuildMaster;
using Inedo.BuildMaster.Artifacts;
using Inedo.Documentation;
using Inedo.Serialization;

namespace Inedo.BuildMasterExtensions.TFS.VisualStudioOnline
{
    [DisplayName("Import Build Artifact from VS Online")]
    [Description("Downloads build output as a zip file from Visual Studio Online or TFS 2015 and imports it as a BuildMaster artifact.")]
    [RequiresInterface(typeof(IFileOperationsExecuter))]
    [Inedo.Web.CustomEditor(typeof(ImportVsoArtifactActionEditor))]
    [Tag(Tags.Builds)]
    [Tag("tfs")]
    [PersistFrom("Inedo.BuildMasterExtensions.TFS.VisualStudioOnline.ImportVsoArtifactAction,TFS")]
    public sealed class ImportVsoArtifactAction : TfsActionBase
    {
        /// <summary>
        /// Gets or sets the build number.
        /// </summary>
        [Persistent]
        public string BuildNumber { get; set; }

        /// <summary>
        /// Gets or sets the name of the artifact if not empty, otherwise use the build definition name.
        /// </summary>
        [Persistent]
        public string ArtifactName { get; set; }

        public override ExtendedRichDescription GetActionDescription()
        {
            var shortDesc = new RichDescription("Import VS Online Build Artifact from ", new Hilite(this.TeamProject));

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
            var configurer = this.GetExtensionConfigurer();

            VsoArtifactImporter.DownloadAndImportAsync(
                configurer, 
                this, 
                this.TeamProject, 
                this.BuildNumber,
                this.BuildDefinition,
                new ArtifactIdentifier(this.Context.ApplicationId, this.Context.ReleaseNumber, this.Context.BuildNumber, this.Context.DeployableId, this.ArtifactName)
            ).Result();
        }
    }
}
