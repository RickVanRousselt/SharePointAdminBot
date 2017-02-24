using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.ExceptionHandling;
using Microsoft.ApplicationInsights;

namespace SharePointAdminBot
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
}