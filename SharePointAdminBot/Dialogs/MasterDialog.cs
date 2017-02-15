using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using AuthBot;
using AuthBot.Dialogs;
using AuthBot.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json.Linq;
using System.Configuration;

namespace SharePointAdminBot.Dialogs
{
    [Serializable]
    public class MasterDialog : IDialog<string>
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger("MessagesController");


        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            var message = await item;
            if (message.Text == "logout")
            {
                await context.Logout();
                context.Wait(this.MessageReceivedAsync);
            }
            else
            {
                if (string.IsNullOrEmpty(await context.GetAccessToken(ConfigurationManager.AppSettings["ActiveDirectory.ResourceId"])))
                {
                    try
                    {
                        await
                            context.Forward(
                                new AzureAuthDialog(ConfigurationManager.AppSettings["ActiveDirectory.ResourceId"]),
                                this.ResumeAfterAuth, message, CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        if (Logger.IsErrorEnabled) Logger.Error("Error in Post MessageController", ex);
                    }
                }
                else
                {
                    var token = await context.GetAccessToken(ConfigurationManager.AppSettings["ActiveDirectory.ResourceId"]);
                    var result = new AuthResult();
                    context.UserData.TryGetValue(ContextConstants.AuthResultKey, out result);
                    if (Logger.IsDebugEnabled) Logger.DebugFormat("Calling RootLuisDialog");
                    await context.Forward(new RootLuisDialog(), null, message, CancellationToken.None);
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

