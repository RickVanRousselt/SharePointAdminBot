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

        public IForm<GlobalQuestion> GlobalQuestionForm()
        {
            OnCompletionAsyncDelegate<GlobalQuestion> processGlobalQuestionQuery = async (context, state) =>
            {
                await context.PostAsync($"Processing....");
            };

            return new FormBuilder<GlobalQuestion>()
                .Field(nameof(GlobalQuestion.Choice))
               // .OnCompletion(processGlobalQuestionQuery)
                .AddRemainingFields()
                .Build();
        }
        public IForm<CreateQuery> CreateQuestionForm()
        {
            OnCompletionAsyncDelegate<CreateQuery> processCreateQuestionQuery = async (context, state) =>
            {
                await context.PostAsync($"Processing....");
            };

            return new FormBuilder<CreateQuery>()
                .Field(nameof(CreateQuery.Create))
                //.OnCompletion(processCreateQuestionQuery)
                .AddRemainingFields()
                .Build();
        }

        public IForm<GetInfoQuery> CreateGetInfoForm()
        {
            OnCompletionAsyncDelegate<GetInfoQuery> processGetInfoQuestionQuery = async (context, state) =>
            {
                await context.PostAsync($"Processing....");
            };

            return new FormBuilder<GetInfoQuery>()
                .Field(nameof(GetInfoQuery.GetInfoAbout))
               // .OnCompletion(processGetInfoQuestionQuery)
                .AddRemainingFields()
                .Build();
        }


    }
}