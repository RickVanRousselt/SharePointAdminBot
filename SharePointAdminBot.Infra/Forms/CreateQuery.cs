using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.FormFlow;

namespace SharePointAdminBot.Infra.Forms
{
    [Serializable]
    public class CreateQuery
    {
        [Prompt("What do you want to create? {||}", ChoiceFormat = "{1}")]
        public CreateChoice Create { get; set; }
    }

    public enum CreateChoice
    {
        Ignore,
        SiteCollection,
        Group
    }
}
