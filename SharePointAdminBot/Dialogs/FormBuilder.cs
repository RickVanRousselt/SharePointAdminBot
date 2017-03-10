using System;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using SharePointAdminBot.Infra.Forms;

namespace SharePointAdminBot.Dialogs
{
    [Serializable]
    public class FormBuilder
    {
        public IForm<CreateSiteCollectionQuery> BuildCreateSiteColForm()
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

        public IForm<AskForUrlQuery> AskForUrl()
        {
            OnCompletionAsyncDelegate<AskForUrlQuery> processAskForUrlQuery = async (context, state) =>
            {
                await context.PostAsync($"Processing....");
            };

            return new FormBuilder<AskForUrlQuery>()
                .Field(nameof(AskForUrlQuery.Url))
                 .OnCompletion(processAskForUrlQuery)
                .AddRemainingFields()
                .Build();
        }
    }
}