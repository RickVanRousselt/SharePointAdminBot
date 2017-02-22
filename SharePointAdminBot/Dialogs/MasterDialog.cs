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

namespace SharePointAdminBot.Dialogs
{
    [Serializable]
    public class MasterDialog : IDialog<string>
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger("MasterDialog");
        private string _resourceId = ConfigurationManager.AppSettings["ActiveDirectory.ResourceId"];

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
                            if (Logger.IsErrorEnabled) Logger.Error("Error in masterdialog calling authentication", ex);
                        }
                    }
                    else
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(await context.GetAccessToken(_resourceId)))
                            {
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
                                if (Logger.IsDebugEnabled) Logger.DebugFormat("Calling RootLuisDialog");
                                await context.Forward(new RootLuisDialog(), null, message, CancellationToken.None);
                            }


                        }
                        catch (Exception e)
                        {
                            if (Logger.IsErrorEnabled) Logger.Error("Error in masterdialog forwarding to Luis", e);
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
            if (Logger.IsDebugEnabled) Logger.DebugFormat("Loggin Success");
            var message = await result;
            await context.PostAsync(message);
            await context.PostAsync("What would you like me to do?");
            context.Wait(MessageReceivedAsync);
        }
    }
}

