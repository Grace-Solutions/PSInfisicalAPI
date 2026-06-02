using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PSInfisicalAPI.Errors;

namespace PSInfisicalAPI.Serialization
{
    public sealed class JsonInfisicalSerializer : IInfisicalSerializer
    {
        private readonly JsonSerializerSettings _settings;

        public JsonInfisicalSerializer(bool useCamelCase = true, bool indent = false)
        {
            _settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = indent ? Formatting.Indented : Formatting.None,
                DateParseHandling = DateParseHandling.DateTimeOffset
            };

            if (useCamelCase)
            {
                _settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            }
        }

        public string Serialize<T>(T value)
        {
            try
            {
                return JsonConvert.SerializeObject(value, _settings);
            }
            catch (JsonException jsonException)
            {
                throw new InfisicalSerializationException(string.Concat("JSON serialization failed: ", jsonException.Message), jsonException);
            }
        }

        public T Deserialize<T>(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return default(T);
            }

            try
            {
                return JsonConvert.DeserializeObject<T>(value, _settings);
            }
            catch (JsonReaderException readerException)
            {
                InfisicalSerializationException exception = new InfisicalSerializationException(string.Concat("JSON deserialization failed: ", readerException.Message), readerException);
                exception.LineNumber = readerException.LineNumber;
                exception.LinePosition = readerException.LinePosition;
                throw exception;
            }
            catch (JsonException jsonException)
            {
                throw new InfisicalSerializationException(string.Concat("JSON deserialization failed: ", jsonException.Message), jsonException);
            }
        }
    }
}
