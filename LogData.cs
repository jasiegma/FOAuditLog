using System;
using Azure.Messaging;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;


namespace Jasiegma.Poc
{
    public class LogData
    {
        private readonly ILogger<LogData> _logger;
        

        public LogData(ILogger<LogData> logger)
        {
            _logger = logger;
        }

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