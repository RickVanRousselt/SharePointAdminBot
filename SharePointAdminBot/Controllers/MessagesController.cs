using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.ApplicationInsights;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using SharePointAdminBot.Dialogs;

namespace SharePointAdminBot.Controllers
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
   
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            var telemetry = new TelemetryClient();
            if (activity != null && activity.GetActivityType() == ActivityTypes.Message)
            {
                try
                {
                    telemetry.TrackTrace($"Entering POST {JsonConvert.SerializeObject(activity)}");
                    await Conversation.SendAsync(activity, () => new MasterDialog());
                }
                catch (Exception ex)
                {
                    telemetry.TrackException(ex);
                    ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                    Activity reply = activity.CreateReply("Sorry something went wrong. Please try again later or log an issue https://github.com/RickVanRousselt/SharePointAdminBot");
                    connector.Conversations.SendToConversation(reply);
                }
            }
            else
            {
                await HandleSystemMessage(activity);
            }
            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }

        private async Task<Activity> HandleSystemMessage(Activity message)
        {
            WebApiApplication.Telemetry.TrackEvent(@"SystemMessage", new Dictionary<string, string> { { @"Type", message.Type } });

            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                if (message.MembersAdded.Any())
                {
                    var newMembers = message.MembersAdded?.Where(t => t.Id != message.Recipient.Id);
                    if (newMembers != null)
                        foreach (var newMember in newMembers)
                        {
                            var telemetry = new TelemetryClient();
                            telemetry.TrackTrace($"New member added to chat: {newMember.Name}");
                            ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));
                            StateClient stateClient = message.GetStateClient();
                            BotData conversationData = await stateClient.BotState.GetConversationDataAsync(message.ChannelId, message.From.Id);
                            conversationData.SetProperty("Welcome", true);
                            Activity reply = message.CreateReply("Hi I'm the SharePoint Admin Bot");
                            await connector.Conversations.SendToConversationAsync(reply);
                        }
                }
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}