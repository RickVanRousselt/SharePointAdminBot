using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AuthBot;
using AuthBot.Dialogs;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Configuration;
using Microsoft.Bot.Builder.FormFlow;
using Newtonsoft.Json;
using SharePointAdminBot.Infra;
using SharePointAdminBot.Infra.Forms;

namespace SharePointAdminBot.Dialogs
{
    [Serializable]
    public class MasterDialog : IDialog<object>
    {
        private string _resourceId = ConfigurationManager.AppSettings["ActiveDirectory.ResourceId"];
        private readonly FormBuilder _formBuilder = new FormBuilder();

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            var message = await item;
            WebApiApplication.Telemetry.TrackTrace(context.CreateTraceTelemetry(
               nameof(MessageReceivedAsync),
               new Dictionary<string, string> { { "message", JsonConvert.SerializeObject(message) } }));

            if (message.Text == "logout" || message.Text == "reset")
            {
                context.UserData.RemoveValue("ResourceId");
                _resourceId = ConfigurationManager.AppSettings["ActiveDirectory.ResourceId"];
                await context.Logout();
                context.Wait(MessageReceivedAsync);
            }
            else
            {
                if (string.IsNullOrEmpty(await context.GetAccessToken(ConfigurationManager.AppSettings["ActiveDirectory.ResourceId"])))
                {
                    var measuredEvent = context.CreateEventTelemetry(@"Login with Graph URL");
                    var timer = new System.Diagnostics.Stopwatch();
                    timer.Start();
                    try
                    {
                        string reply = $"First we need to authenticate you";
                        await context.PostAsync(reply);
                        await
                            context.Forward(
                                new AzureAuthDialog(ConfigurationManager.AppSettings["ActiveDirectory.ResourceId"]),
                                ResumeAfterAuth, message, CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        measuredEvent.Properties.Add("exception", ex.ToString());
                        WebApiApplication.Telemetry.TrackException(context.CreateExceptionTelemetry(ex));
                    }
                    finally
                    {
                        timer.Stop();
                        measuredEvent.Metrics.Add(@"timeTakenMs", timer.ElapsedMilliseconds);
                        WebApiApplication.Telemetry.TrackEvent(measuredEvent);
                    }
                }
                else
                {
                    try
                    {
                        var spUrl = $"https://{_resourceId}.sharepoint.com";
                        WebApiApplication.Telemetry.TrackEvent(context.CreateEventTelemetry($"SPUrl: {spUrl}"));
                        if (string.IsNullOrEmpty(await context.GetAccessToken(spUrl)))
                        {
                            await
                            context.Forward(
                                new AzureAuthDialog(ConfigurationManager.AppSettings["ActiveDirectory.ResourceId"]),
                                ResumeAfterAuth, message, CancellationToken.None);
                        }
                        else
                        {
                            WebApiApplication.Telemetry.TrackEvent(context.CreateEventTelemetry(@"Calling RootLuisDialog"));
                            var createGlobalDialog = FormDialog.FromForm(_formBuilder.GlobalQuestionForm, FormOptions.PromptInStart);
                            context.Call(createGlobalDialog, AfterGlobalDialog);
                        }
                    }
                    catch (Exception ex)
                    {
                        WebApiApplication.Telemetry.TrackException(context.CreateExceptionTelemetry(ex));
                        WebApiApplication.Telemetry.TrackEvent(context.CreateEventTelemetry(@"Error in masterdialog forwarding to Luis"));
                        string reply = $"Sorry something went wrong";
                        await context.PostAsync(reply);
                        context.Wait(MessageReceivedAsync);
                    }
                }
            }
        }

        public async Task ResumeAfterAuth(IDialogContext context, IAwaitable<object> result)
        {
            var message = await result;
            var authResult = context.GetAuthResult();
            if (authResult != null)
            {
                var domain = authResult.Upn.Split('@')[1].Split('.')[0];
                context.UserData.SetValue("ResourceId", domain);
                _resourceId = domain;
            }
            await context.PostAsync((string)message);
            await context.PostAsync("What would you like me to do?");

            var createGlobalDialog = FormDialog.FromForm(_formBuilder.GlobalQuestionForm, FormOptions.PromptInStart);
            context.Call(createGlobalDialog, AfterGlobalDialog);

        }

        private async Task AfterGlobalDialog(IDialogContext context, IAwaitable<GlobalQuestion> result)
        {
            var formResults = await result;
            var originalText = context.Activity.AsMessageActivity().Text;

            if (formResults.Choice == GlobalChoice.Create && string.Equals(formResults.Choice.ToString(), originalText, StringComparison.CurrentCultureIgnoreCase))
            {
                var createGlobalDialog = FormDialog.FromForm(_formBuilder.CreateQuestionForm, FormOptions.PromptInStart);
                context.Call(createGlobalDialog, AfterCreateDialog);
            }
            else if (formResults.Choice == GlobalChoice.GetInfo && string.Equals(formResults.Choice.ToString(), originalText.Replace(" ",""), StringComparison.CurrentCultureIgnoreCase))
            {
                var createGetInfoDialog = FormDialog.FromForm(_formBuilder.CreateGetInfoForm, FormOptions.PromptInStart);
                context.Call(createGetInfoDialog, AfterGetInfoFormDialog);
            }
            else if (formResults.Choice == GlobalChoice.Reindex && string.Equals(formResults.Choice.ToString(), originalText, StringComparison.CurrentCultureIgnoreCase))
            {
                var message = context.MakeMessage();
                message.Text = "Reindex a site collection";
                await context.Forward(new RootLuisDialog(), ResumeAfterAuth, message, CancellationToken.None);
            }
            else
            {
                var message = context.MakeMessage();
                message.Text = context.Activity.AsMessageActivity().Text;
                await context.Forward(new RootLuisDialog(), ResumeAfterAuth, message, CancellationToken.None);
            }
        }

        private async Task AfterGetInfoFormDialog(IDialogContext context, IAwaitable<GetInfoQuery> result)
        {
            var formResults = await result;
            var message = context.MakeMessage();
            message.Text = "Get information about my " + formResults.GetInfoAbout;
            await context.Forward(new RootLuisDialog(), ResumeAfterAuth, message, CancellationToken.None);
        }

        private async Task AfterCreateDialog(IDialogContext context, IAwaitable<CreateQuery> result)
        {
            var formResults = await result;
            var message = context.MakeMessage();
            message.Text = "Create " + formResults.Create;
            await context.Forward(new RootLuisDialog(), ResumeAfterAuth, message, CancellationToken.None);
        }

    }
}

