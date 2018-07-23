// credit to - https://github.com/Azure/azure-documentdb-dotnet/blob/master/samples/code-samples/ChangeFeedProcessorV2/DocumentFeedObserver.cs

//--------------------------------------------------------------------------------- 
// <copyright file="DocumentFeedObserver.cs" company="Microsoft">
// Microsoft (R)  Azure SDK 
// Software Development Kit 
//  
// Copyright (c) Microsoft Corporation. All rights reserved.   
// 
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,  
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES  
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.  
// </copyright>
//---------------------------------------------------------------------------------

using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.ChangeFeedProcessor.FeedProcessing;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CosmosDbDiagAndFeed.CosmosDbUtils
{
    /// <summary>
    /// This class implements the IChangeFeedObserver interface and is used to observe 
    /// changes on change feed. ChangeFeedEventHost will create as many instances of 
    /// this class as needed. 
    /// </summary>
    public class DocumentFeedObserver : IChangeFeedObserver
    {
        private static int totalDocs = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentFeedObserver" /> class.
        /// Saves input DocumentClient and DocumentCollectionInfo parameters to class fields
        /// </summary>
        /// <param name="client"> Client connected to destination collection </param>
        /// <param name="destCollInfo"> Destination collection information </param>
        public DocumentFeedObserver()
        {

        }

        /// <summary>
        /// Called when change feed observer is opened; 
        /// this function prints out observer partition key id. 
        /// </summary>
        /// <param name="context">The context specifying partition for this observer, etc.</param>
        /// <returns>A Task to allow asynchronous execution</returns>
        public Task OpenAsync(IChangeFeedObserverContext context)
        {
            LifetimeEventsHostedService._logger.LogInformation("Observer opened for partition Key Range: {0}", context.PartitionKeyRangeId);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called when change feed observer is closed; 
        /// this function prints out observer partition key id and reason for shut down. 
        /// </summary>
        /// <param name="context">The context specifying partition for this observer, etc.</param>
        /// <param name="reason">Specifies the reason the observer is closed.</param>
        /// <returns>A Task to allow asynchronous execution</returns>
        public Task CloseAsync(IChangeFeedObserverContext context, ChangeFeedObserverCloseReason reason)
        {
            LifetimeEventsHostedService._logger.LogInformation("Observer closed, {0}", context.PartitionKeyRangeId);
            LifetimeEventsHostedService._logger.LogInformation("Reason for shutdown, {0}", reason);
            return Task.CompletedTask;
        }


        public Task ProcessChangesAsync(IChangeFeedObserverContext context, IReadOnlyList<Document> docs, CancellationToken cancellationToken)
        {
            LifetimeEventsHostedService._logger.LogInformation("Change feed: PartitionId {0} total {1} doc(s)", context.PartitionKeyRangeId, Interlocked.Add(ref totalDocs, docs.Count));
            foreach (Document doc in docs)
            {
                LifetimeEventsHostedService._logger.LogDebug(doc.Id.ToString());

                // write received data to a file whose name is the current epoch
                var fileName = "cosmosdb." + (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds.ToString();
                File.WriteAllText(Path.Combine(LifetimeEventsHostedService.localLogdirectory, fileName), doc.ToString());
                LifetimeEventsHostedService._logger.LogInformation("Change feed file written in " + LifetimeEventsHostedService.localLogdirectory + "/" + fileName + " at " + DateTime.Now);
            }
            return Task.CompletedTask;
        }
    }
}