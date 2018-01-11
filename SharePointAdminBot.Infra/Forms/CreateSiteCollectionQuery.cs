using System;
using Microsoft.Bot.Builder.FormFlow;

namespace SharePointAdminBot.Infra.Forms
{
    [Serializable]
    public class CreateSiteCollectionQuery
    {

        //[Prompt("Please enter the {&} of the site collection you want to create")]
        //public string Url { get; set; }

        [Prompt("What is the {&}?")]
        public string Title { get; set; }

        //[Prompt("What's the email address of the {&}?")]
        //public string Owner { get; set; }

        [Prompt("What's the {&} amount in MB?")]
        public int Storage { get; set; }

        [Prompt("What's the {&} amount?")]
        public int Resource { get; set; }

        [Prompt("What {&} should the site be? {||}", ChoiceFormat = "{1}")]
        public SiteTemplate SiteTemplate { get; set; }


    }

    public enum SiteTemplate
    {
        Ignore,
        WikiSite,
        CommunitySite,
        TeamSite
    };
}