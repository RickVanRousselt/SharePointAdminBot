using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.SharePoint.Client;
using OfficeDevPnP.Core;

namespace SharePointAdminBot.Infra
{
    public static class Tenant
    {
        public static List<string> GetTenantProperties(string token)
        {
            //SharePoint Online - AccesToken from Azure AD
            string siteUrl = "https://rivaro.sharepoint.com";
            AuthenticationManager authManager = new AuthenticationManager();
            ClientContext context = authManager.GetAzureADAccessTokenAuthenticatedContext(siteUrl, token);
            return null;
        }

        public static string GetTenantId(string token)
        {
            return null;

        }
       
    }
}
