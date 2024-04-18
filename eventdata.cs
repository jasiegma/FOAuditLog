using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jasiegma.Poc
{

    [JsonConverter(typeof(BaseValueJsonConverter))]
    public abstract class BaseValue
    {
        public BaseValue() { }
    
    }

    public class Value : BaseValue
    {
        public Value() { }

        public string? LogicalName { get; set; }
        public string? Id { get; set; }
        public List<Attribute> Attributes { get; set; } = new List<Attribute>();
    }

    public class ChangedFields : BaseValue
    {
        public ChangedFields() { }

        public string Key { get; set; } = string.Empty;
        public string[] Value { get; set; } = Array.Empty<string>();
    }

    public class InputParameter
    {
        public string Key { get; set; } = string.Empty;
        public BaseValue Value { get; set; } = new Value(); // Initialized with a default Value object to avoid null
    }

    public class EventData
    {
        public string id { get; set; } = string.Empty;
        public int Stage { get; set; }
        public int Mode { get; set; }
        public required string MessageName { get; set; }
        public required string PrimaryEntityName { get; set; }
        public string? SecondaryEntityName { get; set; }
        public required string RequestId { get; set; }
        public required string UserId { get; set; }
        public required DateTime OperationCreatedOn { get; set; }

        
        public List<InputParameter> InputParameters { get; set; } = new List<InputParameter>();
    }

    [JsonConverter(typeof(AttributeValueJsonConverter))]
    public class AttributeValue
    {
        public object Value { get; set; }
    }

    public class Attribute
    {
        public string Key { get; set; } = string.Empty;
        public AttributeValue Value { get; set; } = new AttributeValue();


    }


    public class AttributeValueJsonConverter : JsonConverter<AttributeValue>
    {
        public override AttributeValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number)
            {
                if (reader.TryGetDecimal(out decimal number))
                {
                    return new AttributeValue { Value = number };
                }
            }
            else if (reader.TokenType == JsonTokenType.String)
            {
                string stringValue = reader.GetString();
                if (DateTime.TryParse(stringValue, out var date))
                {
                    return new AttributeValue { Value = date };
                }
                else
                {
                    return new AttributeValue { Value = stringValue };
                }
            }
            else if (reader.TokenType == JsonTokenType.True || reader.TokenType == JsonTokenType.False)
            {
                return new AttributeValue { Value = reader.GetBoolean() };
            }
            else if (reader.TokenType == JsonTokenType.Null)
            {
                return new AttributeValue { Value = null };
            }
            else if (reader.TokenType == JsonTokenType.StartArray)
            {
                // Handle array type
                List<object> list = new List<object>();
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                    {
                        break;
                    }

                    var value = Read(ref reader, typeToConvert, options);
                    list.Add(value.Value);
                }
                return new AttributeValue { Value = list };
            }
            else if (reader.TokenType == JsonTokenType.StartObject)
            {
                // Handle object type
                var dictionary = new Dictionary<string, object>();
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                    {
                        break;
                    }

                    if (reader.TokenType == JsonTokenType.PropertyName)
                    {
                        string propertyName = reader.GetString();
                        reader.Read(); // Move to the value token
                        var value = Read(ref reader, typeToConvert, options);
                        dictionary.Add(propertyName, value.Value);
                    }
                }
                return new AttributeValue { Value = dictionary };
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, AttributeValue value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("Value");
            // Since AttributeValue has only one property 'Value', we need to check its type and serialize accordingly
            if (value.Value is null)
            {
                writer.WriteNullValue();
            }
            else if (value.Value is string stringValue)
            {
                writer.WriteStringValue(stringValue);
            }
            else if (value.Value is decimal decimalValue)
            {
                writer.WriteNumberValue(decimalValue);
            }
            else if (value.Value is bool boolValue)
            {
                writer.WriteBooleanValue(boolValue);
            }
            else if (value.Value is DateTime dateTimeValue)
            {
                writer.WriteStringValue(dateTimeValue);
            }
            else if (value.Value is IEnumerable<object> listValue)
            {
                writer.WriteStartArray();
                foreach (var item in listValue)
                {
                    JsonSerializer.Serialize(writer, item, options);
                }
                writer.WriteEndArray();
            }
            else if (value.Value is IDictionary<string, object> dictionaryValue)
            {
                writer.WriteStartObject();
                foreach (var kvp in dictionaryValue)
                {
                    writer.WritePropertyName(kvp.Key);
                    JsonSerializer.Serialize(writer, kvp.Value, options);
                }
                writer.WriteEndObject();
            }
            else
            {
                // Fallback for any other types not explicitly handled above
                JsonSerializer.Serialize(writer, value.Value, options);
            }

            writer.WriteEndObject();
        }


    }
    public class ValueJsonConverter : JsonConverter<Value>
    {
        public override Value Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                // If the JSON value is an object, deserialize it to the Value class
                return JsonSerializer.Deserialize<Value>(ref reader, options);
            }
            else if (reader.TokenType == JsonTokenType.StartArray)
            {
                // If the JSON value is an array, deserialize it to a list of strings
                List<string> stringList = JsonSerializer.Deserialize<List<string>>(ref reader, options);
                // Then convert the list of strings to a Value object
                return new Value { Attributes = stringList.Select(s => new Attribute { Value = new AttributeValue { Value = s } }).ToList() };
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, Value value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            if (value.LogicalName != null)
            {
                writer.WriteString("LogicalName", value.LogicalName);
            }

            if (value.Id != null)
            {
                writer.WriteString("Id", value.Id);
            }

            if (value.Attributes != null)
            {
                writer.WritePropertyName("Attributes");
                JsonSerializer.Serialize(writer, value.Attributes, options);
            }

            writer.WriteEndObject();
        }
    }

    public class InputParameterJsonConverter : JsonConverter<InputParameter>
    {
        public override InputParameter Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            InputParameter inputParameter = new InputParameter();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string propertyName = reader.GetString();

                    reader.Read(); // Advance to the value

                    if (propertyName == "Key")
                    {
                        inputParameter.Key = reader.GetString();
                    }
                    else if (propertyName == "Value")
                    {
                        if (reader.TokenType == JsonTokenType.StartObject)
                        {
                            inputParameter.Value = JsonSerializer.Deserialize<Value>(ref reader, options);
                        }
                        else if (reader.TokenType == JsonTokenType.StartArray)
                        {
                            inputParameter.Value = new ChangedFields { Key = inputParameter.Key, Value = JsonSerializer.Deserialize<string[]>(ref reader, options) };
                        }
                    }
                }
            }

            return inputParameter;
        }

        public override void Write(Utf8JsonWriter writer, InputParameter value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteString("Key", value.Key);

            writer.WritePropertyName("Value");
            if (value.Value is Value)
            {
                JsonSerializer.Serialize(writer, value.Value as Value, options);
            }
            else if (value.Value is ChangedFields)
            {
                JsonSerializer.Serialize(writer, value.Value as ChangedFields, options);
            }

            writer.WriteEndObject();
        }
    }
    public class BaseValueJsonConverter : JsonConverter<BaseValue>
    {
        public override BaseValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {

            if (reader.TokenType == JsonTokenType.StartArray)
            {
                // If the JSON value is an array, deserialize it to a list of strings
                List<string> stringList = JsonSerializer.Deserialize<List<string>>(ref reader, options);
                // Then convert the list of strings to a Value object
                return new Value { Attributes = stringList.Select(s => new Attribute { Value = new AttributeValue { Value = s } }).ToList() };
            }
            else
            {
                using (var jsonDocument = JsonDocument.ParseValue(ref reader))
                {
                    var jsonObject = jsonDocument.RootElement;

                    if (jsonObject.TryGetProperty("LogicalName", out _) || jsonObject.TryGetProperty("Id", out _) || jsonObject.TryGetProperty("Attributes", out _))
                    {
                        return JsonSerializer.Deserialize<Value>(jsonObject.GetRawText(), options);
                    }
                    else if (jsonObject.TryGetProperty("Key", out var keyProperty))
                    {
                        if (keyProperty.GetString() == "ChangedFields")
                        {
                            // Handle the case where Value is a string array
                            if (jsonObject.TryGetProperty("Value", out var valueProperty) && valueProperty.ValueKind == JsonValueKind.Array)
                            {
                                // Deserialize the array and do something with it
                                var array = JsonSerializer.Deserialize<string[]>(valueProperty.GetRawText(), options);
                                // Return a new ChangedFields object or something else based on the array
                                return new ChangedFields { Value = array };
                            }
                        }
                    }

                    throw new JsonException();
                }
            }
        }

        public override void Write(Utf8JsonWriter writer, BaseValue value, JsonSerializerOptions options)
        {
            if (value is Value)
            {
                JsonSerializer.Serialize(writer, value as Value, options);
            }
            else if (value is ChangedFields)
            {
                JsonSerializer.Serialize(writer, value as ChangedFields, options);
            }
        }
    }
}

