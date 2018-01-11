using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.FormFlow;

namespace SharePointAdminBot.Infra.Forms
{
    [Serializable]
    public class GetInfoQuery
    {
        [Prompt("About what would you like some information? {||}", ChoiceFormat = "{1}")]
        public GetInfoChoices GetInfoAbout { get; set; }
    }

    public enum GetInfoChoices
    {
        Ignore,
        SiteCollection,
        Web,
        Groups,
        Plans
    }
}
