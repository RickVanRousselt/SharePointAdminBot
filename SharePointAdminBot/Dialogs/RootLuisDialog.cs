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


        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"Sorry I did not understand: " + string.Join(", ", result.Intents.Select(i => i.Intent));
            System.Diagnostics.Trace.TraceInformation("No intent found by luis:{0}", result);
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("GetInfo")]
        public async Task GetSiteInfo(IDialogContext context, LuisResult result)
        {
            System.Diagnostics.Trace.TraceInformation($"GetInfo found by LUIS:{result}");
            context.UserData.TryGetValue(ContextConstants.AuthResultKey, out _authResult);
            context.UserData.TryGetValue("ResourceId", out _resourceId);
            List<string> returnedItems;
            EntityRecommendation entity;
            returnedItems = result.TryFindEntity("SiteCollection", out entity) ? SharePointInfo.GetSiteProperties(_authResult, _resourceId) : SharePointInfo.GetWebProperties(_authResult, _resourceId);
            foreach (var answer in returnedItems)
            {
                var message = answer;
                await context.PostAsync(message);
            }

            context.Wait(MessageReceived);

        }

        [LuisIntent("Create")]
        public async Task CreateSiteCollection(IDialogContext context, LuisResult result)
        {
            System.Diagnostics.Trace.TraceInformation($"Create found by LUIS:{result}");
            var createSiteColFormDialog = FormDialog.FromForm(this.BuildCreateSiteColForm, FormOptions.PromptInStart);
            context.Call(createSiteColFormDialog, AfterUrlProvided);
        }

        [LuisIntent("Logout")]
        public async Task Logout(IDialogContext context, LuisResult result)
        {
            System.Diagnostics.Trace.TraceInformation($"Logout found by LUIS:{result}");
            context.UserData.RemoveValue("ResourceId");
            _resourceId = ConfigurationManager.AppSettings["ActiveDirectory.ResourceId"];
            await context.Logout();
            context.Wait(MessageReceived);
        }

        private IForm<CreateSiteCollectionQuery> BuildCreateSiteColForm()
        {
            OnCompletionAsyncDelegate<CreateSiteCollectionQuery> processSiteCollectionQuery = async (context, state) =>
            {
                await context.PostAsync($"Sending request for site collection creation.. Just a moment please");
            };

            return new FormBuilder<CreateSiteCollectionQuery>()
                .Field(nameof(CreateSiteCollectionQuery.Title))
                .Message("Starting the creation of the site collection")
                .AddRemainingFields()
                 .OnCompletion(processSiteCollectionQuery)
                 .Build();
        }



        private async Task AfterUrlProvided(IDialogContext context, IAwaitable<CreateSiteCollectionQuery> result)
        {

            var formResults = await result;
            context.UserData.TryGetValue(ContextConstants.AuthResultKey, out _authResult);
            context.UserData.TryGetValue("ResourceId", out _resourceId);
            var success = Create.CreateSiteColleciton(_authResult, formResults, _resourceId);
            if (success)
            {
                string message = $"Site Collection creation request send";
                await context.PostAsync(message);
            }
            else
            {
                System.Diagnostics.Trace.TraceInformation("Site Collection creation error");
                string message = $"Sorry something went wrong. Please try again later.";
                await context.PostAsync(message);
            }
          
            context.Wait(MessageReceived);
        }
    }
}