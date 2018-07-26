# Azure Cosmos DB - Monitor and Audit Activity, and Access the Change Feed

CosmosDbDiagAndFeed enables you to monitor and audit [Azure Cosmos DB](https://aka.ms/cosmosdb) activity and access a collection's change feed.  It's easy to download Cosmos DB diagnostics logs and a collection's change feed to a Windows, macOS, or Linux computer.  

Want to do more?  No problem!  Feel free to use our project and source code as a starter kit.   

## How It Works
Azure Cosmos DB enables you to:

* [Stream diagnostics logs](https://docs.microsoft.com/en-us/azure/monitoring-and-diagnostics/monitoring-stream-activity-logs-event-hubs) into an [Azure Event Hub](https://azure.microsoft.com/en-us/services/event-hubs/).  
* Access a [change feed](https://docs.microsoft.com/en-us/azure/cosmos-db/change-feed) in order to monitor and process changes to a collection 

Our CosmosDbDiagAndFeed project has two components

*  A script to setup the resources in Azure to support Cosmos DB diagnostic log streaming and access to the change feed
*  A .NET Core application to download diagnostic logs and the change feed content to a computer

***setupCosmosDbDiagAndFeed&#46;sh*** is a Bash shell script that automates the creation and configuration of the Azure resources required to support Cosmos DB diagnostic log streaming and access to the change feed

Our Azure CLI powered script:

* Creates an Azure Resource Group that will contain all the Azure-based resources required to support the integration
* Creates a Cosmos DB instance, database, and collection
* Creates a second Cosmos DB collection (known as a lease collection), which will be used to manage the synchronization state of the client application
* Creates an Event Hub namespace and Event Hub  
* Creates and configures an Azure Blob Storage account, and within that Storage account a Storage Container, which will be used to manage the Event Hub synchronization state of the client application
* Generates Shared Access Signatures for the Event Hub and Blob Storage account, eliminating the need to use master account keys in the client application
* Generates a configuration file (***cosmosDbDiagAndFeedSettings&#46;json***) storing all connection parameters required by the client application.

You'll need to only perform two manual steps in the Azure Portal: within your Cosmos DB instance, you'll enable and configure diagnostic logging and then set it up to use the Event Hub namespace and Event Hub indicated above.

NOTE: If you don't want to use our setup script to create and configure the required Azure resources, no problem!  We'll describe manual the steps required and the settings you need to place into ***cosmosDbDiagAndFeedSettings&#46;json***

***CosmosDbDiagAndFeed***, the client application, is built using [.NET Core](https://www.microsoft.com/net/) and can be run on Windows, macOS, and Linux.  It connects to Event Hub to download diagnostic logs, and Cosmos DB itself to access the change feed.  Contents of both are then placed onto the local filesystem of the computer CosmosDbDiagAndFeed is running on.  This application makes use of:

*  [Generic Host](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host) feaure introduced in .NET Core 2.1 
*  [Event Hub Processor Host](https://www.nuget.org/packages/Microsoft.Azure.ServiceBus.EventProcessorHost), part of the Event Hub .NET SDK, which integrates with Azure Storage to support checkpointing and parallel receives
*  [Cosmos DB Processor Host](https://www.nuget.org/packages/Microsoft.Azure.DocumentDB.ChangeFeedProcessor), part of the Cosmos DB .NET SDK, which integrates with a dedicated Azure Cosmos DB collection to support checkpointing and parallel receives
*  Connectivity values within ***cosmosDbDiagAndFeedSettings&#46;json*** configuration file as generated by the ***setupCosmosDbDiagAndFeed&#46;sh*** script

Let's get started! 

## Identify your working environment

Identify an Azure Environment and User Account
* You'll need an Azure account that has privileges to create and configure the Azure resources and services we've described 
* If you don’t already have an Azure account, [register for a free trial](https://azure.microsoft.com/en-us/free/) that includes $200 in credits.

Identify how you want to setup the Azure resources required to support Azure log streaming.
   * Recommended: Use our  ***setupCosmosDbDiagAndFeed&#46;sh*** script on a computer or environment that has Bash
   * Alternative: Manually setup the Azure resources using the directions we provide through the Azure Portal.  This is also a good option if you don't want to setup a Bash environment.

Identify where you want to want to build and run the ***CosmosDbDiagAndFeed*** client application
*  Your Own Environment
   
   This can be a VM you setup in Azure, your own computer, or any physical or virtual machine with connectivity to Azure and the Internet.  You'll need to:
   *  Clone this GitHub repository.    
      *  This will also include the ***setupCosmosDbDiagAndFeed&#46;sh*** setup script. 
   *  [Download](https://www.microsoft.com/net/download/core) and install .NET Core 2.1 SDK, which is necessary to build and run the ***CosmosDbDiagAndFeed*** application that downloads logs from Azure your computer

*  Easy Setup - Azure Virtual Machine: 

   Don't want to setup our use your own compute?  We can also create an Ubuntu virtual machine in Azure that automatically performs the steps described in Your Own Environment.   Click the Deploy to Azure button further down to get started
   * We've pre-selected a low-cost VM series (Standard_A1) available in all Azure regions. 
      * If you change it, make sure the VM Series you enter is available in the Azure region you target. 
      * Need help? The Azure VM Comparision website will show you the VMs available in a given region https://azureprice.net/
   * Azure supports data disks of up to 4TB (4095 GB). We've defaulted you to 512 GB.
   * DNS Label Prefix is the public-facing hostname of the machine.  It must be unique to the Azure region you are deploying to.
      * If you deploy to West US 2, for example, the fully-qualified hostname will be: your-hostname.westus2.cloudapp.azure.com
      * Creatively challenged?  
         * Just leave it blank.  We'll generate a unique one for you.  You can change it later.
      * Picking one yourself?
         * Unfortunately, we're unable to determine if the value you enter is already being used at this time.  
         * We suggest you append the month, day, year to achieve uniqueness.  For example: your-hostname-01012018
         * After the machine has been created, you can go back and change it through the Azure Portal
   * Before you deploy, you'll need to Agree to the terms and click Purchase to begin deployment.  As a reminder, you're not actually paying to use this free template. However, the resources that you deploy and make use of will be billed to your subscription.

     <a href="https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Ftamhinsf%2FCosmosDbDiagAndFeed%2Fmaster%2Fazuredeploy.json" target="_blank"> <img alt="Deploy to Azure" src="http://azuredeploy.net/deploybutton.png"/>
</a>&nbsp;&nbsp;<a href="http://armviz.io/#/?load=https%3A%2F%2Fraw.githubusercontent.com%2Ftamhinsf%2FCosmosDbDiagAndFeed%2Fmaster%2Fazuredeploy.json" target="_blank"> <img src="http://armviz.io/visualizebutton.png"/></a>

   * Once you've begun your deployment, you can remain in the Azure Portal or navigate away and come back. Either way, you'll receive a notification in the Azure Portal upon completion. Once this has occured:
      * Navigate to the Azure Resource Group you targeted
      * Look for a virtual machine called "cosmosdiagfeed".   Click it.
      * On the "Overview" Pane for "cosmosdiagfeed", you can:
         * Click DNS Name if you don't like the unique value we generated for the public-facing hostname
         * Click the Connect icon to see the username@hostname value you can supply to your SSH client.
      * Connect using your SSH credentials and preferred SSH client
   * After you login, look for a file called "done" in your home directory. This is an indication that the scripts used for configuration and deployment have completed.
   * Review the file called /tmp/azuredeploy.log.xxxx where xxxx is a random four digit number. Check for errors. The operations performed by our scripts may have failed due to unexpected network timeouts or other reasons.


## Setup Azure resources with ***setupCosmosDbDiagAndFeed&#46;sh*** (recommended)

Follow these steps if you want to use our ***setupCosmosDbDiagAndFeed&#46;sh*** script to setup the Azure resources required to enable log streaming.  

### Setup Bash Environment 

You will need a Bash environment and Azure CLI 2.0 to run the ***setupCosmosDbDiagAndFeed&#46;sh*** setup script

   *  Mac and Linux environments inclue Bash
      * [Download](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) and install Azure CLI 2.0.
   *  Windows Server and Windows 10
      * [Windows 10 Subsystem for Linux](https://docs.microsoft.com/en-us/windows/wsl/install-win10) and [Windows Server Subsystem for Linux](https://docs.microsoft.com/en-us/windows/wsl/install-on-server), known as "WSL", provides a Bash environment that can run in a Windows 10 and Windows Server environment
      * If you're using WSL, you'll need to install the Linux version of Azure CLI.
      *  [Download](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) and install Azure CLI 2.0.
   *  Easy Setup - Azure Virtual Machine
      * If you chose our Easy Setup option, the Ubuntu-based Azure Virtual machine we created for you has bash installed and setup as the default shell.
   *  Alternatively, ***setupCosmosDbDiagAndFeed&#46;sh*** does not have to run on the same computer as the client application.  You can run it elsewhere, as long as that environment has Bash and Azure CLI installed.  For instance:
      * [Azure Cloud Shell](https://shell.azure.com) provides you a browser-based Bash environment with Azure CLI pre-installed.   
      * You can also [deploy](https://docs.microsoft.com/en-us/azure/virtual-machines/linux/quick-create-portal) a Linux Virtual machine in Azure

### Review and Run setupCosmosDbDiagAndFeed&#46;sh

*  Go to the folder containing your local clone of this repository
   * If you used our Easy Setup - Azure Virtual Machine option, the path to the local clone will be /cosmosdiagfeed/CosmosDbDiagAndFeed
*  In a text editor, open ***setupCosmosDbDiagAndFeed&#46;sh***  
*  There are a number of variables you may wish to alter.  The values provided can probably be used as-is - but let's be sure!  
      * ***AZ_RESOURCE_GROUP*** is the name of the Azure Resource group that will contain the Azure resources created and used.  The name you use must be unique to your Azure subscription.
      *  ***AZ_REGION*** is the name of the Azure region in which the Resource Group and all of the supporting resources will be deployed.  By default, the script deploys to "westus2".  You can a list of region names through the Azure CLI as follows:
         * az login
         * az account list-locations --query [*].name
   *  Three variables must be unique across all of Azure.  To help you, we automatically append the current date and time, using a variable called ***DATE_TIME*** to the following values
      * ***AZ_STORAGE_ACCOUNT*** - the name of the Azure Storage account to create
      * ***AZ_EVENTHUB_NAMESPACE***  - the name of the Azure Event Hub namespace
      * ***AZ_COSMOSDB_ACCOUNT_NAME*** - the name of the Azure Cosmos DB instance
*  Optional: If you need to run ***setupCosmosDbDiagAndFeed&#46;sh*** elsewhere (i.e. your computer doesn't have Bash), copy it to the destination environment. You can also consider copying-and-pasting its content to a file of the same name.
*  Change the permissions on ***setupCosmosDbDiagAndFeed&#46;sh*** to make sure it can be run:
   * chmod +x setupCosmosDbDiagAndFeed&#46;sh
* Run ***setupCosmosDbDiagAndFeed&#46;sh*** as follows:
   * ./setupCosmosDbDiagAndFeed.sh
      *  Follow the directions to log in to Azure.  If you have logged in to Azure using Azure CLI before ("az login"), you have the option of using locally cached credentials.  
      *  You will be given the option of destroying any resource group that has the same name as the one you wish to use.  Deleting the resource group will also destroy all resources within it.  This is particularly useful if you plan to run the ***setupCosmosDbDiagAndFeed&#46;sh*** script multiple times for testing and demo purposes.
*  The script will take 5 to 10 minutes to complete
*  You'll be given the option of identifying the folder where downloaded activity log files are stored.  
   *  If you change your mind, alter the value of ***az_local_logs_dir*** in ***cosmosDbDiagAndFeedSettings&#46;json***.
   *  If you don't provide a value, we'll create a temporary folder each time you start the application
* A file named ***cosmosDbDiagAndFeedSettings&#46;json*** will be created containing all of the connection parameters needed by ***CosmosDbDiagAndFeed*** to download Azure activity logs. 
   *  If you are running ***setupCosmosDbDiagAndFeed&#46;sh*** in a different environment than where you will build and run CosmosDbDiagAndFeed, you must copy  ***cosmosDbDiagAndFeedSettings&#46;json*** back to it.  Alternatvely, you can copy-and-paste the content into a file of the same name.

## Setup Azure resources manually (optional)


If you've already followed the directions in the section called "Setup Azure Environment with ***setupAzureMonitor&#46;sh***" you can skip to the "Build and Run CosmosDbDiagAndFeed" section.   

The process below describes the steps required to manually configure the Azure resources needed to support Azure Monitor log streaming.  

* Navigate to the folder containing your local clone of this repository
   * Create a copy of the file name ***cosmosDbDiagAndFeedSettings&#46;sample&#46;json*** named ***cosmosDbDiagAndFeedSettings&#46;json*** 
   * ***cosmosDbDiagAndFeedSettings&#46;json***  will contain the connection parameters required by the client application to connect to Azure
   * If you used our Easy Setup - Azure Virtual Machine option, the path to the local clone will be /cosmosdiagfeed/CosmosDbDiagAndFeed

We'll assume that you have a working familiarity with how to access the Azure portal and setup resources.   The steps below are meant to provide you high-level guidance. 

* Login to the Azure portal
* Create or identify the Resource Group you want to place the supporting Azure components into, and make use of it as you create them.  
* Create an Event Hub namespace
  * ***cosmosDbDiagAndFeedSettings&#46;json***  - Assign the Event Hub namespace name to the value ***az_event_hub_name***
  * ***cosmosDbDiagAndFeedSettings&#46;json***  - Shared access policies -> RootManageSharedAccessKey -> Assign an Event Hub connection string to the value ***az_event_hub_connection_string***
  * Within the Event Hub namespace, create an Event Hub
     * We suggest a Partition Count of 4 and a Message Retention of 7.  Capture should be Off.
* Setup Azure Storage
  * Create an Azure Storage account
     * ***cosmosDbDiagAndFeedSettings&#46;json*** - Access Keys -> Assign the Storage Account name to the value ***az_storage_account***
     * ***cosmosDbDiagAndFeedSettings&#46;json*** - Access Keys -> Assign the Storage Account connecting string to ***az_storage_account_connection_string***     
  * Within the Azure Storage account, create a Blob Storage Container
     * ***cosmosDbDiagAndFeedSettings&#46;json***  - Assign the Blob Storage Container name to the value ***az_storage_account_blob_container***
* Create a Cosmos DB instance
     * ***cosmosDbDiagAndFeedSettings&#46;json***  - Keys -> Assign the URI to the value ***az_cosmos_uri***   
     * ***cosmosDbDiagAndFeedSettings&#46;json***  - Keys -> Primary Key to the value ***az_cosmos_key***   
     * Create a database within your Cosmos DB instance
       * ***cosmosDbDiagAndFeedSettings&#46;json***  - Assign the Cosmos DB database name to the value ***az_cosmos_db_name*** 
     * Create a collection within your just-created database
       * This will be the collection we monitor for changes
       * ***cosmosDbDiagAndFeedSettings&#46;json***  - Assign the collection name to the value ***az_cosmos_collection_name*** 
     * Create another collection within your just-created database
       * This will be the "lease collection" we use to manage the sychronization state of the CosmosDbDiagAndFeed client app
       * ***cosmosDbDiagAndFeedSettings&#46;json***  - Assign the collection name to the value ***az_cosmos_lease_collection_name*** 

* Identify Log File Location
     * ***cosmosDbDiagAndFeedSettings&#46;json***  - Assign the file system path where downloaded logs should be stored to  az_local_logs_dir

#### Manual Setup Advanced Security (Optional)

Azure Event Hubs and Storage Accounts support the use of Shared Access Signatures (SAS).  These enable you to reduce the level and duration of access a client application has to these two resources.  You can create a SAS for the Event Hub and Storage Account connection used by ***CosmosDbDiagAndFeed***.

* Event Hub - You can create a SAS at the Event Hub or Event Hub namespace level.  The only permission required by ***CosmosDbDiagAndFeed*** is Listen.  Apply the associated connection string to the value of ***az_event_hub_connection_string*** in ***cosmosDbDiagAndFeedSettings&#46;json***
* Storage Account - Generate a SAS at the Storage Account level. Grant access to: 
   * Blob service
   * Service, Container, Object resource types
   * Read, Write, Delete, List, Add, Create permissions
   * Set a Start time prior to the current date and time (to be safe)
   * Set an End expiry time sufficiently far into the future
   * Allowed IP addresses, Allowed protocols, and Signing key can be set to your needs
   * Click Generate SAS and connection string.  Copy the Connection string into the value of ***az_storage_account_connection_string*** in ***cosmosDbDiagAndFeedSettings&#46;json***
 
## Build and Run CosmosDbDiagAndFeed
*  Before you can build and run CosmosDbDiagAndFeed, you need to configure Cosmos DB to output diagnostic logs into your Event Hub.  Perform these steps - even if you used our setupCosmosDbDiagAndFeed&#46;sh setup script
   * Navigate to the Azure Portal at https://portal.azure.com
   * Go to your Resource Group (mycosmosrg is the default value in the setupCosmosDbDiagAndFeed&#46;sh setup script)
   * Open the Event Hub account
      * Take note of the Event Hub namespace name and Event Hub name
   * Open the Cosmos DB account
   * In the pane that contains Overview, scroll down to MONITORING, and select Diagnostic settings
   * Turn on diagnostics
   * On the Diagnostic settings page
      * Provide a name, check Stream to an event hub, then click Event hub -> Configure
         * Select the Event Hub namespace and Event Hub you noted from before
         * Event hub policy name should be RootManagedSharedAccessKey
         * Click OK
      * Under Log, check all three items 
      * Under Metric, check Requests
      * Click Save at the top
*  Navigate to the folder containing your local clone of this repository
   * If you used our Easy Setup - Azure Virtual Machine option, the path to the local clone will be /cosmosdiagfeed/CosmosDbDiagAndFeed
*  Make sure that ***CosmosDbDiagAndFeedSettings&#46;json***, as created by ***setupAzureMonitor&#46;sh*** is present.  If you manually setup the supporting Azure resources, make sure this file is present and contains the values listed the section "Setup Azure Environment manually". 
*  Run these commands to build and run the client application that will download Activity logs to your computer:
    *  dotnet clean
    *  dotnet build
    *  dotnet run

* Upon successful startup, you should see something like this:

```
info: CosmosDbDiagAndFeed.CosmosDbUtils.LifetimeEventsHostedService[0]
      OnStarted has been called.
info: CosmosDbDiagAndFeed.CosmosDbUtils.LifetimeEventsHostedService[0]
      Log directory is /path/you/identified
info: CosmosDbDiagAndFeed.EventHubUtils.LifetimeEventsHostedService[0]
      OnStarted has been called.
info: CosmosDbDiagAndFeed.EventHubUtils.LifetimeEventsHostedService[0]
      Log directory is /path/you/identified
Application started. Press Ctrl+C to shut down.
Hosting environment: Production
Content root path: /source/code/path/CosmosDbDiagAndFeed/bin/Debug/netcoreapp2.1/
info: CosmosDbDiagAndFeed.CosmosDbUtils.LifetimeEventsHostedService[0]
      Observer opened for partition Key Range: 0
info: CosmosDbDiagAndFeed.EventHubUtils.LifetimeEventsHostedService[0]
      SimpleEventProcessor initialized. Partition: '0'
info: CosmosDbDiagAndFeed.EventHubUtils.LifetimeEventsHostedService[0]
      SimpleEventProcessor initialized. Partition: '1'
info: CosmosDbDiagAndFeed.EventHubUtils.LifetimeEventsHostedService[0]
      SimpleEventProcessor initialized. Partition: '2'
info: CosmosDbDiagAndFeed.EventHubUtils.LifetimeEventsHostedService[0]
      SimpleEventProcessor initialized. Partition: '3'
```

Whenever ***CosmosDbDiagAndFeed*** downloads a new log file from Event Hub, you'll see a notice similiar to this on your screen

```
info: CosmosDbDiagAndFeed.EventHubUtils.LifetimeEventsHostedService[0]
      Event file written in /path/you/identified/eventhub.1532380852.54471 at 7/23/18 2:20:52 PM
info: CosmosDbDiagAndFeed.EventHubUtils.LifetimeEventsHostedService[0]
      Event file written in /path/you/identified/eventhub.1532380854.43281 at 7/23/18 2:20:54 PM
info: CosmosDbDiagAndFeed.EventHubUtils.LifetimeEventsHostedService[0]
      Event file written in /path/you/identified/eventhub.1532380855.08311 at 7/23/18 2:20:55 PM
```

Whenever ***CosmosDbDiagAndFeed*** notices a change in a Cosmos DB collection, you'll see a notice similiar to this on your screen

```
info: CosmosDbDiagAndFeed.CosmosDbUtils.LifetimeEventsHostedService[0]
      Change feed: PartitionId 0 total 1 doc(s)
info: CosmosDbDiagAndFeed.CosmosDbUtils.LifetimeEventsHostedService[0]
      Change feed file written in /path/you/identified/cosmosdb.1532381021.39533 at 7/23/18 2:23:41 PM
info: CosmosDbDiagAndFeed.CosmosDbUtils.LifetimeEventsHostedService[0]
      Change feed: PartitionId 0 total 2 doc(s)
info: CosmosDbDiagAndFeed.CosmosDbUtils.LifetimeEventsHostedService[0]
      Change feed file written in /path/you/identified/cosmosdb.1532381031.62923 at 7/23/18 2:23:51 PM
info: CosmosDbDiagAndFeed.CosmosDbUtils.LifetimeEventsHostedService[0]
      Change feed: PartitionId 0 total 3 doc(s)
info: CosmosDbDiagAndFeed.CosmosDbUtils.LifetimeEventsHostedService[0]
      Change feed file written in /path/you/identified/cosmosdb.1532381041.75643 at 7/23/18 2:24:01 PM
```

Pressing Control-C will shut down CosmosDbDiagAndFeed.   
```
info: CosmosDbDiagAndFeed.CosmosDbUtils.LifetimeEventsHostedService[0]
      Observer closed, 0
info: CosmosDbDiagAndFeed.CosmosDbUtils.LifetimeEventsHostedService[0]
      Reason for shutdown, Shutdown
info: CosmosDbDiagAndFeed.EventHubUtils.LifetimeEventsHostedService[0]
      OnStopping has been called.
info: CosmosDbDiagAndFeed.EventHubUtils.LifetimeEventsHostedService[0]
      Processor Shutting Down. Partition '3', Reason: 'Shutdown'.
info: CosmosDbDiagAndFeed.EventHubUtils.LifetimeEventsHostedService[0]
      Processor Shutting Down. Partition '1', Reason: 'Shutdown'.
info: CosmosDbDiagAndFeed.EventHubUtils.LifetimeEventsHostedService[0]
      Processor Shutting Down. Partition '2', Reason: 'Shutdown'.
info: CosmosDbDiagAndFeed.EventHubUtils.LifetimeEventsHostedService[0]
      Processor Shutting Down. Partition '0', Reason: 'Shutdown'.
info: CosmosDbDiagAndFeed.CosmosDbUtils.LifetimeEventsHostedService[0]
      OnStopped has been called.
info: CosmosDbDiagAndFeed.CosmosDbUtils.LifetimeEventsHostedService[0]
      Log directory is /path/you/identified
info: CosmosDbDiagAndFeed.EventHubUtils.LifetimeEventsHostedService[0]
      OnStopped has been called.
info: CosmosDbDiagAndFeed.EventHubUtils.LifetimeEventsHostedService[0]
      Log directory is /path/you/identified
```

## Publish and Install CosmosDbDiagAndFeed

If you're happy with the results, you can optionally publish CosmosDbDiagAndFeed into a self-contained application.  This will enable you to run CosmosDbDiagAndFeed from another folder or  another computer of the same operating system.
*  Navigate to the folder containing your local clone of this repository
   * If you used our Easy Setup - Azure Virtual Machine option, the path to the local clone will be /cosmosdiagfeed/CosmosDbDiagAndFeed
*  Determine the directory you want to publish and install CosmosDbDiagAndFeed into.
*  Find the "Runtime Identifier" (RID) that corresponds to your operating system environment on the following website https://docs.microsoft.com/en-us/dotnet/core/rid-catalog
   * Here are the RIDs for some popular operating systems
      * Windows 10 / Windows Server 2016 is win10-x64
      * Mac OS X is osx-x64
      * Ubuntu is ubuntu-x64
* Run the dotnet publish command, supply the RID for your environment, and your installation folder
   * dotnet publish -c Release --self-contained -r your-RID -o /your/destination/folder
   * Here's an example for Ubuntu
      * dotnet publish -c Release --self-contained -r ubuntu-x64 -o /your/destination/folder
   * The publish command will copy your configuration file ***cosmosDbDiagAndFeedSettings&#46;json*** to the destination folder as well.
   * Now, you can run CosmosDbDiagAndFeed simply like this:
      * /your/destination/folder/CosmosDbDiagAndFeed

## Customizing CosmosDbDiagAndFeed

### Within the EventHubUtils folder

***LifetimeEventsHostedService.cs*** hosts the service responsible for connecting to Azure Event Hub.  It contains event handlers where you can add additional activities that occur upon startup (OnStarted), during shut down (OnStopping), and after shut down (OnStop). 

***SimpleEventProcessor.cs*** manages the processing of the activity logs placed into Azure Event Hub.  It is registered within ***LifetimeEventsHostedService*** inside OnStarted and un-registerered through OnStopping.  Within ***SimpleEventProcessor*** ***ProcessEventsAsync*** is responsible for iterating over the Azure activity logs placed by Azure Monitor into Event Hub and writing them to a local file.   As such, ***ProcessEventsAsync*** is a great place to author your own custom action.   

### Within the CosmosDbUtils folder

***LifetimeEventsHostedService.cs*** hosts the service responsible for connecting to the Azure Cosmos DB change feed. It contains event handlers where you can add additional activities that occur upon startup (OnStarted), during shut down (OnStopping), and after shut down (OnStop). 

***DocumentFeedObserver.cs*** and ***DocumentFeedObserverFactory.cs*** contain the code used to access the change feed, which is invoked inside OnStarted within ***LifetimeEventsHostedService.cs***.  ***ProcessEventsAsync*** inside of ***DocumentFeedObserver.cs*** contains the actual code to read and write the change feed to a local file. As such, ***ProcessEventsAsync*** is a great place to author your own custom action.     

## Acknowledgements

CosmosDbDiagAndFeed is based upon the code contained in the following GitHub repositories.   Thank you the individuals involved in creating and providing Open Source solutions on Azure.

* [Receive events with the Event Processor Host in .NET Standard](https://github.com/Azure/azure-event-hubs/tree/master/samples/DotNet/Microsoft.Azure.EventHubs/SampleEphReceiver)
* [ASP.NET Generic Host Sample](https://github.com/aspnet/Docs/tree/master/aspnetcore/fundamentals/host/generic-host/samples/2.x/GenericHostSample)
* [Microsoft Azure Cosmos DB .NET SDK
](https://github.com/Azure/azure-documentdb-dotnet)

## Questions and comments
We'd love to get your feedback about this sample. You can send your questions and suggestions to us in the Issues section of this repository.

Questions about Azure Monitor development in general should be posted to Stack Overflow. Make sure that your questions or comments are tagged with [azure-monitoring](https://stackoverflow.com/questions/tagged/azure-monitoring).

## Resources

*  [Azure Event Hubs](https://azure.microsoft.com/en-us/services/event-hubs/)
*  [Azure Storage](https://azure.microsoft.com/en-us/services/storage/)
*  [Azure Cosmos DB](https://docs.microsoft.com/en-us/azure/cosmos-db/introduction)
*  [Azure CLI 2.0](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
*  [.NET Core](https://www.microsoft.com/net/)

## Copyright

Copyright (c) 2018 Tam Huynh. All rights reserved. 


### Disclaimer ###
**THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.**