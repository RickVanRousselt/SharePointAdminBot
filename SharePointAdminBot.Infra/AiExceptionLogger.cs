using System.Collections.Generic;
using System.Web.Http.ExceptionHandling;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;

namespace SharePointAdminBot.Infra
{
    public class AiExceptionLogger : ExceptionLogger
    {
        public override void Log(ExceptionLoggerContext context)
        {
            if (context != null && context.Exception != null)
            {
                var ai = new TelemetryClient();
                ai.TrackException(context.Exception);
            }
            base.Log(context);
        }
    }

    public static class TelemetryExtensions
    {
        public static TraceTelemetry CreateTraceTelemetry(this IDialogContext ctx, string message = null, IDictionary<string, string> properties = null)
        {
            var t = new TraceTelemetry(message);
            t.Properties.Add("ConversationData", JsonConvert.SerializeObject(ctx.ConversationData));
            t.Properties.Add("PrivateConversationData", JsonConvert.SerializeObject(ctx.PrivateConversationData));
            t.Properties.Add("UserData", JsonConvert.SerializeObject(ctx.UserData));

            var m = ctx.MakeMessage();
            t.Properties.Add("ConversationId", m.Conversation.Id);
            t.Properties.Add("UserId", m.Recipient.Id);

            if (properties != null)
            {
                foreach (var p in properties)
                {
                    t.Properties.Add(p);
                }
            }

            return t;
        }

        public static EventTelemetry CreateEventTelemetry(this IDialogContext ctx, string message = null, IDictionary<string, string> properties = null)
        {
            var t = new EventTelemetry(message);
            t.Properties.Add("ConversationData", JsonConvert.SerializeObject(ctx.ConversationData));
            t.Properties.Add("PrivateConversationData", JsonConvert.SerializeObject(ctx.PrivateConversationData));
            t.Properties.Add("UserData", JsonConvert.SerializeObject(ctx.UserData));

            var m = ctx.MakeMessage();
            t.Properties.Add("ConversationId", m.Conversation.Id);
            t.Properties.Add("UserId", m.Recipient.Id);

            if (properties != null)
            {
                foreach (var p in properties)
                {
                    t.Properties.Add(p);
                }
            }

            return t;
        }


        public static ExceptionTelemetry CreateExceptionTelemetry(this IDialogContext ctx, System.Exception ex, IDictionary<string, string> properties = null)
        {
            var t = new ExceptionTelemetry(ex);
            t.Properties.Add("ConversationData", JsonConvert.SerializeObject(ctx.ConversationData));
            t.Properties.Add("PrivateConversationData", JsonConvert.SerializeObject(ctx.PrivateConversationData));
            t.Properties.Add("UserData", JsonConvert.SerializeObject(ctx.UserData));

            var m = ctx.MakeMessage();
            t.Properties.Add("ConversationId", m.Conversation.Id);
            t.Properties.Add("UserId", m.Recipient.Id);

            if (properties != null)
            {
                foreach (var p in properties)
                {
                    t.Properties.Add(p);
                }
            }

            return t;
        }
    }
}