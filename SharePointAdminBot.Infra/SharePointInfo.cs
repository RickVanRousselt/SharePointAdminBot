using System;
using System.Collections.Generic;
using System.Reflection;
using AuthBot.Models;
using Microsoft.ApplicationInsights;
using Microsoft.SharePoint.Client;
using OfficeDevPnP.Core;

namespace SharePointAdminBot.Infra
{
    public static class SharePointInfo
    {
        public static List<string> GetSiteProperties(AuthResult result, string url)
        {
            var telemetry = new TelemetryClient();
            try
            {
                AuthenticationManager authManager = new AuthenticationManager();
                Site site;
                var propertyList = new List<string>();
                using (ClientContext context = authManager.GetAzureADAccessTokenAuthenticatedContext(url, result.AccessToken))
                {
                    site = context.Site;
                    context.Load(site, x => x.AllowDesigner, x => x.CompatibilityLevel, x => x.Id, x => x.AllowCreateDeclarativeWorkflow, x => x.AllowMasterPageEditing, x => x.AllowRevertFromTemplate, x => x.AllowSaveDeclarativeWorkflowAsTemplate, x => x.AllowSavePublishDeclarativeWorkflow, x => x.AllowSelfServiceUpgrade, x => x.AllowSelfServiceUpgradeEvaluation,x => x.AuditLogTrimmingRetention, x => x.CanUpgrade, x => x.Classification, x => x.DisableAppViews, x => x.ExternalSharingTipsEnabled, x => x.Url);
                    context.ExecuteQuery();
                }
               
                var siteType = site.GetType();
                BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
                var properties = siteType.GetProperties(flags);
                foreach (var propertyInfo in properties)
                {
                    if (site.IsPropertyAvailable(propertyInfo.Name))
                    {
                        propertyList.Add($"{propertyInfo.Name}: {propertyInfo.GetValue(site, null)}");
                    }
                }
                return propertyList;
            }
            catch (Exception ex)
            {
                telemetry.TrackException(ex);
                return null;
            }
         
        }

        public static List<string> GetWebProperties(AuthResult result, string url)
        {
            var telemetry = new TelemetryClient();
            try
            {
                AuthenticationManager authManager = new AuthenticationManager();
                Web site;
                var propertyList = new List<string>();
                using (ClientContext context = authManager.GetAzureADAccessTokenAuthenticatedContext(url, result.AccessToken))
                {
                    site = context.Site.RootWeb;
                    context.Load(site, x => x.AlternateCssUrl, x => x.Title, x => x.Id, x => x.Description, x => x.Created, x => x.EnableMinimalDownload, x => x.CustomMasterUrl, x => x.IsMultilingual, x => x.Language, x => x.QuickLaunchEnabled, x => x.WebTemplate, x => x.UIVersion);
                    context.ExecuteQuery();
                }

                var siteType = site.GetType();
                BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
                var properties = siteType.GetProperties(flags);
                foreach (var propertyInfo in properties)
                {
                    if (site.IsPropertyAvailable(propertyInfo.Name))
                    {
                        propertyList.Add($"{propertyInfo.Name}: {propertyInfo.GetValue(site, null)}");
                    }
                }
                return propertyList;
            }
            catch (Exception ex)
            {
                telemetry.TrackException(ex);
                return null;
            }

        }

        public static bool ReIndexSiteCollection(AuthResult result, string url)
        {
            var telemetry = new TelemetryClient();
            try
            {
                AuthenticationManager authManager = new AuthenticationManager();
               using (
                    ClientContext context = authManager.GetAzureADAccessTokenAuthenticatedContext(url,
                        result.AccessToken))
                {
                    context.Web.ReIndexWeb();
                }
                return true;
            }
            catch (Exception ex)
            {
                telemetry.TrackException(ex);
                return false;
            }
           

        }

        public static string GetTenantId(string token)
        {
            return null;

        }
       
    }
}
