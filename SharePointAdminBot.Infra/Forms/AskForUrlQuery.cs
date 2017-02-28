using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
