# Custom Actions for Power BI REST API

A few custom actions to test integration between a bot made with Bot Framework Composer and Power BI REST API.

Hope it will be helpful to others too :-)

## Setup

In order to use it you need to have at least a Pro account in Power BI, because you need permission to create a workspace other than the personal My Workspace.

Firstly, follow the steps 1 to 4 in [this page](https://docs.microsoft.com/en-us/power-bi/developer/embedded/embed-service-principal), to register an app and add permissions to it in your PBI workspace.

Then, open the MySolutionBot folder with Bot Framework Composer and add the following custom property to the appsettings.json file, replacing the placeholders with the info you obtained in the steps above:
```
  "pbiOptions": {
    "authorityUri": "https://login.microsoftonline.com/organizations/",
    "tenantId": "<YOUR-TENANT-ID>",
    "clientId": "<YOUR-APP-REGISTRATION-ID>",
    "clientSecret": "<YOUR-APP-REGISTRATION-SECRET>",
    "scope": "https://analysis.windows.net/powerbi/api/.default",
    "workspaceId": "<YOUR-WORKSPACE-ID>",
    "renderUrl": "<YOUR-BOT-WWWROOT>/default.htm?d={0}&t={1}&q={2}",
    "browserWsEndpoint": "wss://chrome.browserless.io",
    "browserDelayMilis": "5000"
  }
```
For running locally with Composer, your bot wwwroot will be served at the root of localhost with the port number assigned for your bot (i.e. http://localhost:xxxx). 

Also for local bot, you may leave empty the browserWSEndpoint parameter, so Pupeteer will download and use a local browserless instance.

Finally, start the bot from within Bot Framework Composer. 


Enjoy.


## About the samples

The sample dialog has some simple examples of creating a dataset, allowing for the input of authors and books records, listing the workspace reports and asking free text queries about the dataset. 

The "ask PBI" intent is a embrionary PoC that rely on Power BI's Ask a Question feature, and thus dependends on a browser actually rendering the visual returned by that feature.

Although it still has some challenges for a real production feature, for the sake of curiosity I created a service worker that do it using PuppeteerSharp, either using a local headless browser instance (that unfortunately does not work with Azure web apps) or a external browser service like browserless.io


### Sample Teams 1

![ask pbi](/readme-contents/AskPBI_Teams.png "Ask PBI at Teams")


### Sample webchat 1

![ask pbi](/readme-contents/1AskPBI.png "Ask PBI")

![ask pbi](/readme-contents/1PBIResponse.png "PBI Response")


### Sample webchat 2

![ask pbi](/readme-contents/2AskPBI.png "Ask PBI")

![ask pbi](/readme-contents/2PBIResponse.png "PBI Response")


## Azure Devops Pipeline

If you want to play with Azure Devops pipelines, at the root of the repository you will find a folder named build, that contains a simplified version of the [sample pipeline](https://docs.microsoft.com/en-us/composer/how-to-cicd) available for bots built with Composer. It expects a few parameters (less than the original sample), that must be configured in your pipeline.

I removed a step in the original sample that writes settings to App Service's Configuration tab, for brevity, and created the following settings manually there:

runtimeSettings:telemetry:options:instrumentationKey
runtimeSettings:features:blobTranscript:connectionString
runtimeSettings:features:blobTranscript:containerName
luis:authoringKey
luis:endpointKey
MicrosoftAppId
MicrosoftAppPassword
pbiOptions:clientId
pbiOptions:clientSecret
pbiOptions:renderUrl
pbiOptions:tenantId
pbiOptions:workspaceId
pbiOptions:browserWsEndpoint
pbiOptions:browserDelayMilis
