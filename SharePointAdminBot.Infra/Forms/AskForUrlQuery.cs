using System;
using Microsoft.Bot.Builder.FormFlow;

namespace SharePointAdminBot.Infra.Forms
{
    [Serializable]
    public class AskForUrlQuery
    {
        [Prompt("Please enter the {&}")]
        public string Url { get; set; }
    }
}
