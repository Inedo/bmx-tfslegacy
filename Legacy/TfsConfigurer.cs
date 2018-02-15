﻿using System;
using System.Linq;
using Inedo.Agents;
using Inedo.BuildMaster.Extensibility.Agents;
using Inedo.BuildMaster.Extensibility.Configurers.Extension;
using Inedo.Extensions.TFS;
using Inedo.Serialization;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Server;

[assembly: ExtensionConfigurer(typeof(Inedo.BuildMasterExtensions.TFS.TfsConfigurer))]

namespace Inedo.BuildMasterExtensions.TFS
{
    [Inedo.Web.CustomEditor(typeof(TfsConfigurerEditor))]
    [PersistFrom("Inedo.BuildMasterExtensions.TFS.TfsConfigurer,TFS")]
    public sealed class TfsConfigurer : ExtensionConfigurerBase, IVsoConnectionInfo
    {
        public static readonly string TypeQualifiedName = typeof(TfsConfigurer).FullName + "," + typeof(TfsConfigurer).Assembly.GetName().Name;

        /// <summary>
        /// Gets or sets the server identifier.
        /// </summary>
        [Persistent]
        public int? ServerId { get; set; }
        /// <summary>
        /// The base url of the TFS store, may include collection name, e.g. "http://server:port/tfs"
        /// </summary>
        [Persistent]
        public string BaseUrl { get; set; }
        /// <summary>
        /// The username used to connect to the server
        /// </summary>
        [Persistent]
        public string UserName { get; set; }
        /// <summary>
        /// The password used to connect to the server
        /// </summary>
        [Persistent(Encrypted = true)]
        public string Password { get; set; }
        /// <summary>
        /// The domain of the server
        /// </summary>
        [Persistent]
        public string Domain { get; set; }
        /// <summary>
        /// Returns true if BuildMaster should connect to TFS using its own account, false if the credentials are specified
        /// </summary>
        [Persistent]
        public bool UseSystemCredentials { get; set; }

        public Uri BaseUri => this.BaseUrl == null ? null : new Uri(this.BaseUrl);

        string IVsoConnectionInfo.TeamProjectCollectionUrl => this.BaseUrl;
        string IVsoConnectionInfo.PasswordOrToken => this.Password;

        internal TfsBuildInfo GetBuildInfo(string teamProject, string buildDefinition, string buildNumber, bool includeUnsuccessful)
        {
            var agent = this.ServerId != null ? BuildMasterAgent.Create((int)this.ServerId) : BuildMasterAgent.CreateLocalAgent();
            using (agent)
            {
                var methodExecuter = agent.GetService<IRemoteMethodExecuter>();
                return methodExecuter.InvokeFunc(this.GetBuildInfoInternal, teamProject, buildDefinition, buildNumber, includeUnsuccessful);
            }
        }
        internal string[] GetBuildDefinitions(string teamProject)
        {
            using (var agent = this.CreateAgent())
            {
                var methodExecuter = agent.GetService<IRemoteMethodExecuter>();
                return methodExecuter.InvokeFunc(this.GetBuildDefinitionsInternal, teamProject);
            }
        }
        internal string[] GetTeamProjects()
        {
            using (var agent = this.CreateAgent())
            {
                var methodExecuter = agent.GetService<IRemoteMethodExecuter>();
                return methodExecuter.InvokeFunc(this.GetTeamProjectsInternal);
            }
        }
        internal string TestConnection()
        {
            using (var agent = this.CreateAgent())
            {
                var methodExecuter = agent.GetService<IRemoteMethodExecuter>();
                return methodExecuter.InvokeFunc(this.TestConnectionInternal);
            }
        }

        private BuildMasterAgent CreateAgent()
        {
            if (this.ServerId == null)
                return BuildMasterAgent.CreateLocalAgent();
            else
                return BuildMasterAgent.Create((int)this.ServerId);
        }

        private string TestConnectionInternal()
        {
            try
            {
                using (var collection = TfsActionBase.GetTeamProjectCollection(this))
                {
                    collection.EnsureAuthenticated();
                }

                return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        private string[] GetTeamProjectsInternal()
        {
            using (var collection = TfsActionBase.GetTeamProjectCollection(this))
            {
                var structureService = collection.GetService<ICommonStructureService>();
                return structureService.ListProjects().Select(p => p.Name).ToArray();
            }
        }
        private string[] GetBuildDefinitionsInternal(string teamProject)
        {
            using (var collection = TfsActionBase.GetTeamProjectCollection(this))
            {
                var buildService = collection.GetService<IBuildServer>();
                return buildService.QueryBuildDefinitions(teamProject).Select(d => d.Name).ToArray();
            }
        }
        private TfsBuildInfo GetBuildInfoInternal(string teamProject, string buildDefinition, string buildNumber, bool includeUnsuccessful)
        {
            using (var collection = TfsActionBase.GetTeamProjectCollection(this))
            {
                var buildService = collection.GetService<IBuildServer>();

                var spec = buildService.CreateBuildDetailSpec(teamProject, AH.CoalesceString(buildDefinition, "*"));
                spec.BuildNumber = AH.CoalesceString(buildNumber, "*");
                spec.MaxBuildsPerDefinition = 1;
                spec.QueryOrder = BuildQueryOrder.FinishTimeDescending;
                spec.Status = includeUnsuccessful ? (BuildStatus.Failed | BuildStatus.Succeeded | BuildStatus.PartiallySucceeded) : BuildStatus.Succeeded;

                var result = buildService.QueryBuilds(spec).Builds.FirstOrDefault();
                if (result != null)
                    return new TfsBuildInfo(result);
                else
                    return null;
            }
        }
    }
}