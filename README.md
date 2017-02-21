#SharePoint Administration Bot
The SharePoint Admin Bot is an attempt to make the everyday routinous jobs that a SharePoint Online Administrator or Power User easier.
Currently the features are limited but the goal is to make them grow. If the feature you want is not in there yet please submit a feature request or even better contribute and do a pull request.

#Start to use the Bot
To use the SharePoint Admin Bot you do not have to clone or download the code. You can just connect to it using the links below.


##If you want your own version of the SharePoint Admin Bot then you can follow these steps.
* Clone or download the repository
* Register a new bot in the [Bot Framework](https://dev.botframework.com/) 
* Change the BotId, MicrosoftAppId and MicrosoftAppPassword in the web.config file
* Create a new web app in Azure and deploy the code
* Create a new app in your [Azure AD tenant](https://docs.microsoft.com/en-us/azure/app-service-mobile/app-service-mobile-how-to-configure-active-directory-authentication)
* Change the ClientId, ClientSecret and redirect url in the web.config file to your values. If you don't want it to be a multi-tenant app then change the Tenant value also.
* Now you can start talking to your own personal SharePoint Admin Bot from the Bot Framework test page and if you want you can even configure more channels.


The bot uses [LUIS.AI](https://www.luis.ai) to try and understand what you mean. Luis also has to learn so if your sentence is not recongized try to rephrase the question.





#Current features
#####Get Site Collection properties
Returns list of general properties from the Site Collection
#####Get Web properties
Returns list of general properties from the Rootweb of a Site Collection
#####Create Site Collection
Asks several question and then creates a Site Collection.



#Contribute
I would love if you would help contribute to this project. 


#More Information
Check out my [blog](https://www.rickvanrousselt.com/) for more information on the SharePoint Admin Bot

[![MIT license](https://img.shields.io/npm/l/express.svg)](https://github.com/RickVanRousselt/SharePointAdminBot/blob/master/LICENSE)