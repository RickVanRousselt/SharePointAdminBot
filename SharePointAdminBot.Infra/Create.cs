using System;
using System.Collections.Generic;
using System.Web;
using AuthBot.Models;
using Microsoft.ApplicationInsights;
using Microsoft.Online.SharePoint.TenantAdministration;
using Microsoft.SharePoint.Client;
using Newtonsoft.Json;
using OfficeDevPnP.Core;
using SharePointAdminBot.Infra.Forms;

namespace SharePointAdminBot.Infra
{
    public static class Create
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger("Create");

        public static bool CreateSiteColleciton(AuthResult result, CreateSiteCollectionQuery formResult, string tenantUrl, string resourceId)
        {
            var telemetry = new TelemetryClient();
            bool succes = false;
            var telProps = new Dictionary<string,string>();
            try
            {
                AuthenticationManager authManager = new AuthenticationManager();
                using (ClientContext context = authManager.GetAzureADAccessTokenAuthenticatedContext(tenantUrl, result.AccessToken))
                {
                    telProps.Add("Create Site collection connection URL", tenantUrl);
                    Tenant t = new Tenant(context);
                    SiteCreationProperties props = new SiteCreationProperties
                    {
                        Url =
                            $"https://{resourceId}.sharepoint.com/sites/{HttpContext.Current.Server.UrlEncode(formResult.Title)}",
                        Title = formResult.Title,
                        Owner = result.Upn,
                        StorageMaximumLevel = formResult.Storage,
                        UserCodeMaximumLevel = formResult.Resource,
                        Template = "STS#0"
                    };


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
                    telProps.Add("Create site props", JsonConvert.SerializeObject(props));
                    telemetry.TrackEvent("Create site collection", telProps);
                    t.CreateSite(props);
                    context.ExecuteQuery();
                    succes = true;
                }
            }
            catch (Exception ex)
            {
                telemetry.TrackException(ex,telProps);
            }
            return succes;
        }
    }
}
