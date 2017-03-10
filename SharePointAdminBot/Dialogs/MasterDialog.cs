using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AuthBot;
using AuthBot.Dialogs;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Configuration;
using Newtonsoft.Json;
using SharePointAdminBot.Infra;

namespace SharePointAdminBot.Dialogs
{
    [Serializable]
    public class MasterDialog : IDialog<object>
    {
        private string _resourceId = ConfigurationManager.AppSettings["ActiveDirectory.ResourceId"];

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
                            await context.Forward(new RootLuisDialog(), null, message, CancellationToken.None);
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

        private async Task ResumeAfterAuth(IDialogContext context, IAwaitable<string> result)
        {
            var message = await result;
            var authResult = context.GetAuthResult();
            if (authResult != null)
            {
                var domain = authResult.Upn.Split('@')[1].Split('.')[0];
                context.UserData.SetValue("ResourceId", domain);
                _resourceId = domain;
            }
            await context.PostAsync(message);
            await context.PostAsync("What would you like me to do?");
            context.Wait(MessageReceivedAsync);
        }
    }
}

