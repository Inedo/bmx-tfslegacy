﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using Inedo.BuildMaster;
using Inedo.BuildMaster.Data;
using Inedo.BuildMaster.Extensibility.IssueTrackerConnections;
using Inedo.BuildMaster.Extensibility.Providers.IssueTracking;
using Inedo.BuildMaster.Web.Controls.Extensions;
using Inedo.BuildMaster.Web.Security;
using Inedo.Security;
using Inedo.Web.ClientResources;
using Inedo.Web.Controls;
using Inedo.Web.Controls.SimpleHtml;
using Inedo.Web.Handlers;
using Newtonsoft.Json;

namespace Inedo.BuildMasterExtensions.TFS.Providers
{
    internal sealed class TfsIssueTrackingApplicationFilterEditor : IssueTrackerApplicationConfigurationEditorBase
    {
        private SelectList ddlCollection;
        private SelectList ddlUseWiql;
        private HiddenField ctlProject;
        private HiddenField ctlAreaPath;
        private TextBox txtCustomWiql;
        private Lazy<TfsCollectionInfo[]> collections;

        public TfsIssueTrackingApplicationFilterEditor()
        {
            this.collections = new Lazy<TfsCollectionInfo[]>(this.GetCollections);
            this.ValidateBeforeCreate += this.TfsIssueTrackingApplicationFilterEditor_ValidateBeforeCreate;
        }

        private void TfsIssueTrackingApplicationFilterEditor_ValidateBeforeCreate(object sender, ValidationEventArgs<IssueTrackerApplicationConfigurationBase> e)
        {
            try
            {
                GC.KeepAlive(this.collections.Value);
            }
            catch (Exception ex)
            {
                e.ValidLevel = ValidationLevel.Error;
                e.Message = "Unable to contact TFS: " + ex.ToString();
            }
        }

        public override void BindToForm(IssueTrackerApplicationConfigurationBase extension)
        {
            var filter = (TfsIssueTrackingApplicationFilter)extension;

            if (filter.CollectionId != null)
            {
                this.ddlCollection.SelectedValue = filter.CollectionId.ToString();
            }
            else if(filter.CollectionName != null)
            {
                var item = this.ddlCollection.Items.Cast<ListItem>().FirstOrDefault(i => i.Text == filter.CollectionName);
                if (item != null)
                    item.Selected = true;
            }

            this.ctlProject.Value = filter.ProjectName;
            this.ctlAreaPath.Value = filter.AreaPath;
        }
        public override IssueTrackerApplicationConfigurationBase CreateFromForm()
        {
            return new TfsIssueTrackingApplicationFilter
            {
                CollectionId = Guid.Parse(this.ddlCollection.SelectedValue),
                CollectionName = this.ddlCollection.Items.FirstOrDefault(i => i.Selected)?.Text,
                ProjectName = this.ctlProject.Value,
                AreaPath = this.ctlAreaPath.Value
            };
        }

        protected override void OnPreRender(EventArgs e)
        {
            this.IncludeClientResourceInPage(
                new JavascriptResource("/extension-resources/TFSLegacy/Legacy/TfsIssueTrackingApplicationFilterEditor.js", InedoLibCR.select2)
            );

            base.OnPreRender(e);
        }
        protected override void CreateChildControls()
        {
            this.ddlCollection = new SelectList();
            this.ddlCollection.Items.AddRange(
                from c in this.collections.Value
                orderby c.Name
                select new SelectListItem(c.Name, c.Id.ToString())
            );

            if (this.HasProviderWiql())
            {
                this.Controls.Add(
                    new P("There is a custom WIQL query defined at the provider level. That query will override any project or area filtering here."),
                    new P("However, you may still specify a custom WIQL query here to override the provider's WIQL query.")
                );
            }

            this.ddlUseWiql = new SelectList
            {
                Items =
                {
                    new SelectListItem("Not using a custom query", "False"),
                    new SelectListItem("Custom WIQL query", "True")
                }
            };

            this.ctlProject = new HiddenField { ID = "ctlProject" };
            this.ctlAreaPath = new HiddenField { ID = "ctlAreaPath" };

            var ctlNoWiql = new Div(
                new SlimFormField("Project:", this.ctlProject),
                new SlimFormField("Area Path:", this.ctlAreaPath)
            );

            this.txtCustomWiql = new TextBox
            {
                TextMode = TextBoxMode.MultiLine,
                Rows = 5
            };

            var ctlWiql = new SlimFormField("Custom query:", this.txtCustomWiql);

            this.Controls.Add(
                new SlimFormField("Collection:", this.ddlCollection),
                new SlimFormField("Query mode:", this.ddlUseWiql),
                ctlNoWiql,
                ctlWiql,
                new RenderJQueryDocReadyDelegator(
                    w =>
                    {
                        w.Write("TfsIssueTrackingApplicationFilterEditor_Init(");
                        JsonSerializer.CreateDefault().Serialize(
                            w,
                            new
                            {
                                ddlCollection = ddlCollection.ClientID,
                                ddlUseWiql = ddlUseWiql.ClientID,
                                ctlWiql = ctlWiql.ClientID,
                                ctlNoWiql = ctlNoWiql.ClientID,
                                ctlProject = ctlProject.ClientID,
                                ctlArea = ctlAreaPath.ClientID,
                                getProjectsUrl = Ajax.GetUrl(new Func<int, string, object>(GetProjects)),
                                getAreasUrl = Ajax.GetUrl(new Func<int, string, string, object>(GetAreas)),
                                applicationId = this.EditorContext.ApplicationId
                            }
                        );
                        w.Write(");");
                    }
                )
            );
        }

        private TfsCollectionInfo[] GetCollections()
        {
            using (var provider = this.GetProvider())
            {
                return provider.GetCollections();
            }
        }
        private TfsIssueTrackingProvider GetProvider()
        {
            var application = DB.Applications_GetApplication(this.EditorContext.ApplicationId)
                .Applications_Extended
                .First();

            return (TfsIssueTrackingProvider)Util.Providers.CreateProviderFromId<IssueTrackingProviderBase>(application.IssueTracking_Provider_Id.Value);
        }
        private bool HasProviderWiql()
        {
            try
            {
                using (var provider = this.GetProvider())
                {
                    return !string.IsNullOrWhiteSpace(provider.CustomWiql);
                }
            }
            catch
            {
                return false;
            }
        }

        [AjaxMethod]
        private static object GetProjects(int applicationId, string collectionId)
        {
            WebUserContext.ValidatePrivileges(BuildMasterSecuredTask.Applications_Manage, applicationId: applicationId);

            var application = DB.Applications_GetApplication(applicationId)
                .Applications_Extended
                .First();

            using (var provider = (TfsIssueTrackingProvider)Util.Providers.CreateProviderFromId<IssueTrackingProviderBase>(application.IssueTracking_Provider_Id.Value))
            {
                return from p in provider.GetProjects(Guid.Parse(collectionId))
                       orderby p.Name
                       select new
                       {
                           id = p.Name,
                           text = p.Name
                       };
            }
        }

        [AjaxMethod]
        private static object GetAreas(int applicationId, string collectionId, string projectName)
        {
            WebUserContext.ValidatePrivileges(BuildMasterSecuredTask.Applications_Manage, applicationId: applicationId);

            var application = DB.Applications_GetApplication(applicationId)
                .Applications_Extended
                .First();

            using (var provider = (TfsIssueTrackingProvider)Util.Providers.CreateProviderFromId<IssueTrackingProviderBase>(application.IssueTracking_Provider_Id.Value))
            {
                return GetAreasInternal(null, provider.GetAreas(Guid.Parse(collectionId), projectName));
            }
        }

        private static IEnumerable<object> GetAreasInternal(string rootPath, TfsAreaInfo[] areas)
        {
            var root = !string.IsNullOrWhiteSpace(rootPath) ? (rootPath + "\\") : string.Empty;
            foreach (var area in areas)
            {
                yield return new
                {
                    id = root + area.Name,
                    text = area.Name,
                    children = GetAreasInternal(root + area.Name, area.Children)
                };
            }
        }
    }
}
