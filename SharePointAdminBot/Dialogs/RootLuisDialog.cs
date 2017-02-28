using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using AuthBot;
using AuthBot.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Newtonsoft.Json;
using SharePointAdminBot.Infra;
using SharePointAdminBot.Infra.Forms;

namespace SharePointAdminBot.Dialogs
{
    [Serializable]
    [LuisModel("c75d7bef-7f85-4ac5-a22e-0b78de2c7328", "863224eec48243e6b163c4bcbdd1a4c8")]
    public class RootLuisDialog : LuisDialog<object>
    {
        private string _resourceId;
        private AuthResult _authResult;
        private readonly FormBuilder _formBuilder = new FormBuilder();


        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"Sorry I did not understand: " + string.Join(", ", result.Intents.Select(i => i.Intent));
            WebApiApplication.Telemetry.TrackTrace(context.CreateTraceTelemetry(nameof(None),new Dictionary<string, string> { { "No intent found by luis", JsonConvert.SerializeObject(result) } }));
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("GetInfo")]
        public async Task GetSiteInfo(IDialogContext context, LuisResult result)
        {
            WebApiApplication.Telemetry.TrackTrace(context.CreateTraceTelemetry(nameof(GetSiteInfo), new Dictionary<string, string> { { "GetInfo found by LUIS:", JsonConvert.SerializeObject(result) } }));
            var createUrlDialog = FormDialog.FromForm(_formBuilder.AskForUrl, FormOptions.PromptInStart);
            EntityRecommendation entity;
            if (result.TryFindEntity("SiteCollection", out entity))
            {
                context.Call(createUrlDialog, GetSiteCollectionInfo);
            }
            else { context.Call(createUrlDialog, GetWebInfo); }
        }

        [LuisIntent("Create")]
        public async Task CreateSiteCollection(IDialogContext context, LuisResult result)
        {
            WebApiApplication.Telemetry.TrackTrace(context.CreateTraceTelemetry(nameof(CreateSiteCollection), new Dictionary<string, string> { { "Create found by LUIS:", JsonConvert.SerializeObject(result) } }));
            var createSiteColFormDialog = FormDialog.FromForm(_formBuilder.BuildCreateSiteColForm, FormOptions.PromptInStart);
            context.Call(createSiteColFormDialog, AfterUrlProvided);
        }

        [LuisIntent("Logout")]
        public async Task Logout(IDialogContext context, LuisResult result)
        {
            WebApiApplication.Telemetry.TrackTrace(context.CreateTraceTelemetry(nameof(Logout), new Dictionary<string, string> { { "Logout found by LUIS:", JsonConvert.SerializeObject(result) } }));
            context.UserData.RemoveValue("ResourceId");
            _resourceId = ConfigurationManager.AppSettings["ActiveDirectory.ResourceId"];
            await context.Logout();
            context.Wait(MessageReceived);
        }

        [LuisIntent("ReIndex")]
        public async Task ReIndex(IDialogContext context, LuisResult result)
        {
            WebApiApplication.Telemetry.TrackTrace(context.CreateTraceTelemetry(nameof(ReIndex), new Dictionary<string, string> { { "ReIndex found by LUIS:", JsonConvert.SerializeObject(result) } }));
            var createUrlDialog = FormDialog.FromForm(_formBuilder.AskForUrl, FormOptions.PromptInStart);
            context.Call(createUrlDialog, ReindexSite);
        }

        private async Task ReindexSite(IDialogContext context, IAwaitable<AskForUrlQuery> result)
        {
            var formResults = await result;
            await context.GetAccessToken(formResults.Url);
            context.UserData.TryGetValue(ContextConstants.AuthResultKey, out _authResult);

            var success = SharePointInfo.ReIndexSiteCollection(_authResult, formResults.Url);
            if (success)
            {
                string message = $"Reindexing triggered";
                await context.PostAsync(message);
            }
            else
            {
                string message = $"Request for reindex went wrong";
                await context.PostAsync(message);
            }
            context.Wait(MessageReceived);
        }

        private async Task GetSiteCollectionInfo(IDialogContext context, IAwaitable<AskForUrlQuery> result)
        {
            var formResults = await result;
            await context.GetAccessToken(formResults.Url);
            context.UserData.TryGetValue(ContextConstants.AuthResultKey, out _authResult);

            var returnedItems = SharePointInfo.GetSiteProperties(_authResult, formResults.Url);

            foreach (var answer in returnedItems)
            {
                var message = answer;
                await context.PostAsync(message);
            }

            context.Wait(MessageReceived);
        }

        private async Task GetWebInfo(IDialogContext context, IAwaitable<AskForUrlQuery> result)
        {
            var formResults = await result;
            await context.GetAccessToken(formResults.Url);
            context.UserData.TryGetValue(ContextConstants.AuthResultKey, out _authResult);

            var returnedItems = SharePointInfo.GetWebProperties(_authResult, formResults.Url);

            foreach (var answer in returnedItems)
            {
                var message = answer;
                await context.PostAsync(message);
            }

            context.Wait(MessageReceived);
        }

        private async Task AfterUrlProvided(IDialogContext context, IAwaitable<CreateSiteCollectionQuery> result)
        {

            var formResults = await result;
            context.UserData.TryGetValue("ResourceId", out _resourceId);
            var tenantUrl = $"https://{_resourceId}-admin.sharepoint.com";
            await context.GetAccessToken(tenantUrl);
            context.UserData.TryGetValue(ContextConstants.AuthResultKey, out _authResult);
            var success = Create.CreateSiteColleciton(_authResult, formResults, _resourceId);
            if (success)
            {
                string message = $"Site Collection creation request send";
                await context.PostAsync(message);
            }
            else
            {
                WebApiApplication.Telemetry.TrackTrace(context.CreateTraceTelemetry(nameof(AfterUrlProvided), new Dictionary<string, string> { { "Site Collection creation error:", JsonConvert.SerializeObject(result) } }));
                string message = $"Sorry something went wrong. Please try again later.";
                await context.PostAsync(message);
            }

            context.Wait(MessageReceived);
        }
    }
}