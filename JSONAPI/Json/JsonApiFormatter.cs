using System.Collections.ObjectModel;
using JSONAPI.Attributes;
using JSONAPI.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using JSONAPI.Extensions;

namespace JSONAPI.Json
{
    public class JsonApiFormatter : JsonMediaTypeFormatter
    {
        public JsonApiFormatter(IPluralizationService pluralizationService) :
            this(new ModelManager(pluralizationService))
        {
            
        }

        public JsonApiFormatter(IModelManager modelManager)
            : this(new PayloadFormatter(modelManager))
        {
            
        }

        public JsonApiFormatter(IPayloadFormatter payloadFormatter) :
            this(payloadFormatter, new ErrorSerializer())
        {
        }

        internal JsonApiFormatter(IPayloadFormatter payloadFormatter, IErrorSerializer errorSerializer)
        {
            _payloadFormatter = payloadFormatter;
            _errorSerializer = errorSerializer;
            SupportedMediaTypes.Insert(0, new MediaTypeHeaderValue("application/vnd.api+json"));
            ValidateRawJsonStrings = true;
        }

        public bool ValidateRawJsonStrings { get; set; }

        private readonly IPayloadFormatter _payloadFormatter;
        private readonly IErrorSerializer _errorSerializer;

        private readonly IModelManager _modelManager;
        public IModelManager ModelManager
        {
            get
            {
                return _modelManager;
            }
        }

        public override bool CanReadType(Type type)
        {
            return type == typeof (IPayload);
        }

        public override bool CanWriteType(Type type)
        {
            return type == typeof (IPayload) ||
                type == typeof (IJsonApiResponse) ||
                _errorSerializer.CanSerialize(type);
        }

        public override async Task WriteToStreamAsync(System.Type type, object value, Stream writeStream,
            System.Net.Http.HttpContent content, System.Net.TransportContext transportContext)
        {
            var contentHeaders = content == null ? null : content.Headers;
            var effectiveEncoding = SelectCharacterEncoding(contentHeaders);
            JsonWriter writer = this.CreateJsonWriter(typeof (object), writeStream, effectiveEncoding);
            JsonSerializer serializer = this.CreateJsonSerializer();
            if (_errorSerializer.CanSerialize(type))
            {
                // `value` is an error
                _errorSerializer.SerializeError(value, writeStream, writer, serializer);
            }
            else if (type == typeof (IPayload))
            {
                var payload = (IPayload) value;
                await _payloadFormatter.SerializeAsync(payload, writer, serializer);
            }
            else if (type == typeof (IJsonApiResponse))
            {
                var payload = await ((IJsonApiResponse) value).Resolve();
                await _payloadFormatter.SerializeAsync(payload, writer, serializer);
            }
        }
    }
}
