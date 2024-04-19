using System;
using Azure.Messaging;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;


namespace Jasiegma.Poc
{
    /// <summary>
    /// Represents the LogData class. Used to receive the event data and store it in Cosmos DB.
    /// </summary>
    public class LogData
    {
        private readonly ILogger<LogData> _logger;
        
        /// <summary>
        /// Initializes a new instance of the LogData class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        public LogData(ILogger<LogData> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Represents the Run method that is triggered by an EventGrid event.
        /// </summary>
        /// <param name="cloudEvent">The CloudEvent object.</param>
        [Function(nameof(LogData))]
        public async Task Run([EventGridTrigger] CloudEvent cloudEvent)
        {
            try
            {
                if (cloudEvent != null && cloudEvent.Data != null)
                {
                    Jasiegma.Poc.EventData eventData = JsonSerializer.Deserialize<EventData>(cloudEvent.Data.ToString());
                    eventData.id = eventData.RequestId;
                    await StoreEventDataAsync(eventData.RequestId, eventData);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the event.");
                throw;
            }
        }

        /// <summary>
        /// Stores the event data in Cosmos DB.
        /// </summary>
        /// <param name="requestId">The request ID.</param>
        /// <param name="eventData">The event data.</param>
        private async Task StoreEventDataAsync(string requestId, EventData eventData)
        {
            // Initialize the Cosmos DB client
            var clientOptions = new CosmosClientOptions
            {
                Serializer = new CustomSystemTextJsonCosmosSerializer(logger: _logger)
            };
            var cosmosClient = new CosmosClient(Environment.GetEnvironmentVariable("CosmosConnectionString"), clientOptions);

            // Get a reference to the database and container
            var database = cosmosClient.GetDatabase(Environment.GetEnvironmentVariable("databaseName"));
            var container = database.GetContainer(Environment.GetEnvironmentVariable("containerName"));

            if (!string.IsNullOrEmpty(eventData.RequestId))
            {
                try
                {
                    // Create an item in the container
                    await container.CreateItemAsync<EventData>(item: eventData, partitionKey: new PartitionKey(eventData.RequestId));
                }
                catch (CosmosException ex)
                {
                    _logger.LogError(ex, "An error occurred while creating the item in Cosmos DB. RequestId: {RequestId}", eventData.RequestId);
                    throw;
                }
            }
        }
    }
}