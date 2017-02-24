using System;
using System.Threading;
using System.Threading.Tasks;
using AuthBot;
using AuthBot.Dialogs;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Configuration;
using System.Text.RegularExpressions;
using AuthBot.Helpers;
using AuthBot.Models;
using log4net.Repository.Hierarchy;
using Microsoft.ApplicationInsights;

namespace SharePointAdminBot.Dialogs
{
    [Serializable]
    public class MasterDialog : IDialog<string>
    {
        private string _resourceId = ConfigurationManager.AppSettings["ActiveDirectory.ResourceId"];
        [NonSerialized()]
        private TelemetryClient telemetry = new TelemetryClient();

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> item)
        {

            var message = await item;
            if (message.Text == "logout" || message.Text == "reset")
            {
                context.UserData.RemoveValue("ResourceId");
                _resourceId = ConfigurationManager.AppSettings["ActiveDirectory.ResourceId"];
                await context.Logout();
                context.Wait(MessageReceivedAsync);
            }
            else
            {
                var welcome = false;
                Uri resourceUrl = null;
                if (Uri.TryCreate(message.Text, UriKind.Absolute, out resourceUrl))
                {
                    //uri.tryparse
                    context.UserData.SetValue("ResourceId", message.Text);
                    _resourceId = message.Text;
                }
                if (context.UserData.TryGetValue("ResourceId", out _resourceId))
                {
                    if (string.IsNullOrEmpty(await context.GetAccessToken(ConfigurationManager.AppSettings["ActiveDirectory.ResourceId"])))
                    {
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
                            telemetry.TrackTrace("Error in masterdialog calling authentication");
                            telemetry.TrackException(ex);
                        }
                    }
                    else
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(await context.GetAccessToken(_resourceId)))
                            {
                                telemetry.TrackTrace("Re-configuring AuthResult");
                                AuthResult _authResult;
                                context.UserData.TryGetValue(ContextConstants.AuthResultKey, out _authResult);
                                InMemoryTokenCacheADAL tokenCache = new InMemoryTokenCacheADAL(_authResult.TokenCache);
                                var result = await AzureActiveDirectoryHelper.GetToken(_authResult.UserUniqueId, tokenCache, _resourceId);
                                _authResult.AccessToken = result.AccessToken;
                                _authResult.ExpiresOnUtcTicks = result.ExpiresOnUtcTicks;
                                _authResult.TokenCache = tokenCache.Serialize();
                                context.StoreAuthResult(_authResult);
                                context.Wait(MessageReceivedAsync);
                            }
                            else
                            {
                                telemetry.TrackTrace("Calling RootLuisDialog");
                                await context.Forward(new RootLuisDialog(), null, message, CancellationToken.None);
                            }


                        }
                        catch (Exception ex)
                        {
                            telemetry.TrackException(ex);
                            telemetry.TrackTrace("Error in masterdialog forwarding to Luis");
                            string reply = $"Sorry something went wrong";
                            await context.PostAsync(reply);
                            context.Wait(MessageReceivedAsync);
                        }
                    }
                }
                else if (context.ConversationData.TryGetValue("Welcome", out welcome))
                {
                    string reply = $"Sorry but {message.Text} is not a valid URL";
                    await context.PostAsync(reply);
                    context.Wait(MessageReceivedAsync);
                }

            }

        }

        private async Task ResumeAfterAuth(IDialogContext context, IAwaitable<string> result)
        {
            var message = await result;
            await context.PostAsync(message);
            await context.PostAsync("What would you like me to do?");
            context.Wait(MessageReceivedAsync);
        }
    }
}

