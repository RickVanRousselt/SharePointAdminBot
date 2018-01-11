using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.FormFlow;

namespace SharePointAdminBot.Infra.Forms
{
    [Serializable]
    public class GlobalQuestion
    {
        [Prompt("What kind of admin task can I do for you? {||}", ChoiceFormat = "{1}")]
        public GlobalChoice  Choice { get; set; }
    }

    public enum GlobalChoice
    {
        Ignore,
        Create,
        GetInfo,
        Reindex
    }
}
