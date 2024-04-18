using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

public class CustomSystemTextJsonCosmosSerializer : CosmosSerializer
{
    private readonly JsonSerializerOptions _options;
    private readonly ILogger _logger; // Add a logger

    // Adjust the constructor to accept an ILogger
    public CustomSystemTextJsonCosmosSerializer(JsonSerializerOptions options = null, ILogger logger = null)
    {
        _options = options ?? new JsonSerializerOptions
        {
            
            // Add any other options or custom converters as needed
        };
        _logger = logger;
    }

    public override Stream ToStream<T>(T input)
    {
        MemoryStream stream = new MemoryStream();
        try
        {
            // Serialize the object to the stream
            JsonSerializer.SerializeAsync(stream, input, _options).GetAwaiter().GetResult();
            stream.Position = 0;

            
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Error during serialization: {ex.Message}");
            throw; // Rethrow the exception to ensure the error is not silently ignored
        }

        return stream;
    }

     public override T FromStream<T>(Stream stream)
    {
        if (stream == null || stream.CanRead == false)
        {
            return default(T);
        }

        using (stream)
        {
            return JsonSerializer.DeserializeAsync<T>(stream, _options).AsTask().GetAwaiter().GetResult();
        }
    }
}