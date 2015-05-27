using System.Net.Http.Formatting;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace JSONAPI.Json
{
    /// <summary>
    /// Provides serialization and deserialization services for IPayload objects
    /// </summary>
    public interface IPayloadFormatter
    {
        /// <summary>
        /// Serializes an IPayload object into a json-api response 
        /// </summary>
        /// <param name="payload">The payload to serialize</param>
        /// <param name="writer">The json writer to write to</param>
        /// <param name="serializer">The json serializer</param>
        /// <returns>A task that should resolve when serialization is complete.</returns>
        Task SerializeAsync(IPayload payload, JsonWriter writer, JsonSerializer serializer);

        /// <summary>
        /// Deserializes an IPayload object from a json-api request
        /// </summary>
        /// <param name="reader">A json reader for the request content</param>
        /// <param name="formatterLogger">An error logger</param>
        /// <returns>A task that should resolve with the complete deserialized payload</returns>
        Task<IPayload> DeserializeAsync(JsonReader reader, IFormatterLogger formatterLogger);
    }
}
