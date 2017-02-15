using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using AuthBot;
using AuthBot.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;

namespace SharePointAdminBot.Dialogs
{
    [Serializable]
    [LuisModel("c75d7bef-7f85-4ac5-a22e-0b78de2c7328", "863224eec48243e6b163c4bcbdd1a4c8")]
    public class RootLuisDialog : LuisDialog<object>
    {
        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"Sorry I did not understand: " + string.Join(", ", result.Intents.Select(i => i.Intent));
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("GetInfo")]
        public async Task GetTenantInfo(IDialogContext context, LuisResult result)
        {
            var token = await context.GetAccessToken(AuthSettings.Scopes);

        }
    }
}