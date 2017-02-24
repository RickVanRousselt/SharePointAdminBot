using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuthBot.Models;
using Microsoft.ApplicationInsights;
using Microsoft.Online.SharePoint.TenantAdministration;
using Microsoft.SharePoint.Client;
using OfficeDevPnP.Core;
using SharePointAdminBot.Infra.Forms;

namespace SharePointAdminBot.Infra
{
    public static class Create
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger("Create");

        public static bool CreateSiteColleciton(AuthResult result, CreateSiteCollectionQuery formResult, string url)
        {
            var telemetry = new TelemetryClient();
            Logger.Debug($"Starting CreateSiteCollection");
            bool succes = false;
            try
            {
                AuthenticationManager authManager = new AuthenticationManager();
                var propertyList = new List<string>();
                using (ClientContext context = authManager.GetAzureADAccessTokenAuthenticatedContext(url, result.AccessToken))
                {
                    Tenant t = new Tenant(context);
                    SiteCreationProperties props = new SiteCreationProperties();
                    props.Url = formResult.Url;
                    props.Title = formResult.Title;
                    props.Owner = formResult.Owner;

                    props.StorageMaximumLevel = formResult.Storage;
                    props.UserCodeMaximumLevel = formResult.Resource;

                    props.Template = "STS#0";
                    switch (formResult.SiteTemplate)
                    {
                        case SiteTemplate.TeamSite:
                            props.Template = "STS#0";
                            break;
                        case SiteTemplate.CommunitySite:
                            props.Template = "COMMUNITY#0";
                            break;
                        case SiteTemplate.WikiSite:
                            props.Template = "WIKI#0";
                            break;
                    }
                    t.CreateSite(props);
                    context.ExecuteQuery();
                    succes = true;
                }
            }
            catch (Exception ex)
            {
                telemetry.TrackException(ex);
            }
            return succes;
        }
    }
}
