using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Threading.Tasks;
using JSONAPI.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JSONAPI.Json
{
    /// <summary>
    /// Serializes and deserializes payloads according to the json-api spec
    /// </summary>
    public class PayloadFormatter : IPayloadFormatter
    {
        protected const string PrimaryDataKeyName = "data";
        protected const string RelatedDataKeyName = "related";
        protected const string LinksKey = "links";
        protected const string MetadataKey = "meta";
        protected const string ResourceObjectIdKey = "id";
        protected const string ResourceObjectTypeNameKey = "type";
        protected const string LinksObjectIdKey = "id";
        protected const string LinksObjectTypeKey = "type";
        protected const string LinksObjectDataKey = "data";

        private readonly IModelManager _modelManager;

        /// <summary>
        /// Creates a new PayloadFormatter
        /// </summary>
        /// <param name="modelManager">The model manager service</param>
        public PayloadFormatter(IModelManager modelManager)
        {
            _modelManager = modelManager;
        }

        #region Serialization

        /// <inheritdoc />
        public Task SerializeAsync(IPayload payload, JsonWriter writer, JsonSerializer serializer)
        {
            writer.Formatting = Formatting.Indented;

            writer.WriteStartObject();

            SerializePrimaryData(payload, writer, serializer);
            SerializeRelatedResources(payload, writer, serializer);
            SerializeMetadata(payload, writer, serializer);

            writer.WriteEndObject();

            return Task.FromResult(0);
        }

        private void SerializePrimaryData(IPayload payload, JsonWriter writer, JsonSerializer serializer)
        {
            writer.WritePropertyName(PrimaryDataKeyName);

            if (payload.IsCollection)
            {
                SerializeResourceObjectCollection(payload.PrimaryData, writer, serializer);
            }
            else
            {
                SerializeSingleResourceObject(payload.PrimaryData.FirstOrDefault(), writer, serializer);
            }
        }

        private void SerializeRelatedResources(IPayload payload, JsonWriter writer, JsonSerializer serializer)
        {
            if (payload.RelatedData == null) return;

            writer.WritePropertyName(RelatedDataKeyName);

            SerializeResourceObjectCollection(payload.RelatedData, writer, serializer);
            ///* This is a bit messy, because we may add items of a given type to the
            // * set we are currently processing. Not only is this an issue because you
            // * can't modify a set while you're enumerating it (hence why we make a
            // * copy first), but we need to catch the newly added objects and process
            // * them as well. So, we have to keep making passes until we detect that
            // * we haven't added any new objects to any of the appendices.
            // */
            //Dictionary<Type, ISet<object>>
            //    processed = new Dictionary<Type, ISet<object>>(),
            //    toBeProcessed = new Dictionary<Type, ISet<object>>(); // is this actually necessary?
            ///* On top of that, we need a new JsonWriter for each appendix--because we
            // * may write objects of type A, then while processing type B find that
            // * we need to write more objects of type A! So we can't keep appending
            // * to the same writer.
            // */
            ///* Oh, and we have to keep a reference to the TextWriter of the JsonWriter
            // * because there's no member to get it back out again. ?!?
            // * */
            //Dictionary<Type, KeyValuePair<JsonWriter, StringWriter>> writers = new Dictionary<Type, KeyValuePair<JsonWriter, StringWriter>>();

            //int numAdditions;
            //do
            //{
            //    numAdditions = 0;
            //    Dictionary<Type, ISet<object>> appxs = new Dictionary<Type, ISet<object>>(aggregator.Appendices); // shallow clone, in case we add a new type during enumeration!
            //    foreach (KeyValuePair<Type, ISet<object>> apair in appxs)
            //    {
            //        Type type = apair.Key;
            //        ISet<object> appendix = apair.Value;
            //        JsonWriter jw;
            //        if (writers.ContainsKey(type))
            //        {
            //            jw = writers[type].Key;
            //        }
            //        else
            //        {
            //            // Setup and start the writer for this type...
            //            StringWriter sw = new StringWriter();
            //            jw = new JsonTextWriter(sw);
            //            writers[type] = new KeyValuePair<JsonWriter, StringWriter>(jw, sw);
            //            jw.WriteStartArray();
            //        }

            //        HashSet<object> tbp;
            //        if (processed.ContainsKey(type))
            //        {
            //            toBeProcessed[type] = tbp = new HashSet<object>(appendix.Except(processed[type]));
            //        }
            //        else
            //        {
            //            toBeProcessed[type] = tbp = new HashSet<object>(appendix);
            //            processed[type] = new HashSet<object>();
            //        }

            //        if (tbp.Count > 0)
            //        {
            //            numAdditions += tbp.Count;
            //            foreach (object obj in tbp)
            //            {
            //                Serialize(obj, writeStream, jw, serializer, aggregator); // Note, not writer, but jw--we write each type to its own JsonWriter and combine them later.
            //            }
            //            processed[type].UnionWith(tbp);
            //        }

            //        //TODO: Add traversal depth limit??
            //    }
            //} while (numAdditions > 0);

            //if (aggregator.Appendices.Count > 0)
            //{
            //    writer.WritePropertyName("linked");
            //    writer.WriteStartObject();

            //    // Okay, we should have captured everything now. Now combine the type writers into the main writer...
            //    foreach (KeyValuePair<Type, KeyValuePair<JsonWriter, StringWriter>> apair in writers)
            //    {
            //        apair.Value.Key.WriteEnd(); // close off the array
            //        writer.WritePropertyName(_modelManager.GetResourceTypeNameForType(apair.Key));
            //        writer.WriteRawValue(apair.Value.Value.ToString()); // write the contents of the type JsonWriter's StringWriter to the main JsonWriter
            //    }

            //    writer.WriteEndObject();
            //}
        }

        private void SerializeMetadata(IPayload payload, JsonWriter writer, JsonSerializer serializer)
        {
            if (payload.Metadata == null) return;
            writer.WritePropertyName(MetadataKey);
            payload.Metadata.WriteTo(writer);
        }

        private void SerializeResourceObjectCollection(IEnumerable<IResourceObject> collection, JsonWriter writer, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            foreach (var resourceObject in collection)
            {
                SerializeSingleResourceObject(resourceObject, writer, serializer);
            }
            writer.WriteEndArray();

        }

        private void SerializeSingleResourceObject(IResourceObject resourceObject, JsonWriter writer, JsonSerializer serializer)
        {
            throw new NotImplementedException();

            //    writer.WriteStartObject();

            //    var resourceType = value.GetType();

            //    // Write the type
            //    writer.WritePropertyName("type");
            //    var jsonTypeKey = _modelManager.GetResourceTypeNameForType(resourceType);
            //    writer.WriteValue(jsonTypeKey);

            //    // Do the Id now...
            //    writer.WritePropertyName("id");
            //    var idProp = _modelManager.GetIdProperty(resourceType);
            //    writer.WriteValue(GetValueForIdProperty(idProp, value));

            //    // Leverage the cached map to avoid another costly call to System.Type.GetProperties()
            //    PropertyInfo[] props = _modelManager.GetProperties(value.GetType());

            //    // Do non-model properties first, everything else goes in "links"
            //    //TODO: Unless embedded???
            //    IList<PropertyInfo> modelProps = new List<PropertyInfo>();

            //    foreach (PropertyInfo prop in props)
            //    {
            //        string propKey = _modelManager.GetJsonKeyForProperty(prop);
            //        if (propKey == "id") continue; // Don't write the "id" property twice, see above!

            //        if (prop.PropertyType.CanWriteAsJsonApiAttribute())
            //        {
            //            if (prop.GetCustomAttributes().Any(attr => attr is JsonIgnoreAttribute))
            //                continue;

            //            // numbers, strings, dates...
            //            writer.WritePropertyName(propKey);

            //            var propertyValue = prop.GetValue(value, null);

            //            if (prop.PropertyType == typeof(Decimal) || prop.PropertyType == typeof(Decimal?))
            //            {
            //                if (propertyValue == null)
            //                    writer.WriteNull();
            //                else
            //                    writer.WriteValue(propertyValue.ToString());
            //            }
            //            else if (prop.PropertyType == typeof(string) &&
            //                prop.GetCustomAttributes().Any(attr => attr is SerializeStringAsRawJsonAttribute))
            //            {
            //                if (propertyValue == null)
            //                {
            //                    writer.WriteNull();
            //                }
            //                else
            //                {
            //                    var json = (string)propertyValue;
            //                    if (ValidateRawJsonStrings)
            //                    {
            //                        try
            //                        {
            //                            var token = JToken.Parse(json);
            //                            json = token.ToString();
            //                        }
            //                        catch (Exception)
            //                        {
            //                            json = "{}";
            //                        }
            //                    }
            //                    var valueToSerialize = JsonHelpers.MinifyJson(json);
            //                    writer.WriteRawValue(valueToSerialize);
            //                }
            //            }
            //            else
            //            {
            //                serializer.Serialize(writer, propertyValue);
            //            }
            //        }
            //        else
            //        {
            //            modelProps.Add(prop);
            //            continue;
            //        }
            //    }

            //    // Now do other stuff
            //    if (modelProps.Count() > 0)
            //    {
            //        writer.WritePropertyName("links");
            //        writer.WriteStartObject();
            //    }
            //    foreach (PropertyInfo prop in modelProps)
            //    {
            //        bool skip = false, iip = false;
            //        string lt = null;
            //        SerializeAsOptions sa = SerializeAsOptions.Ids;

            //        object[] attrs = prop.GetCustomAttributes(true);

            //        foreach (object attr in attrs)
            //        {
            //            Type attrType = attr.GetType();
            //            if (typeof(JsonIgnoreAttribute).IsAssignableFrom(attrType))
            //            {
            //                skip = true;
            //                continue;
            //            }
            //            if (typeof(IncludeInPayload).IsAssignableFrom(attrType))
            //                iip = ((IncludeInPayload)attr).Include;
            //            if (typeof(SerializeAs).IsAssignableFrom(attrType))
            //                sa = ((SerializeAs)attr).How;
            //            if (typeof(LinkTemplate).IsAssignableFrom(attrType))
            //                lt = ((LinkTemplate)attr).TemplateString;
            //        }
            //        if (skip) continue;

            //        writer.WritePropertyName(_modelManager.GetJsonKeyForProperty(prop));

            //        // Now look for enumerable-ness:
            //        if (typeof(IEnumerable<Object>).IsAssignableFrom(prop.PropertyType))
            //        {
            //            // Look out! If we want to SerializeAs a link, computing the property is probably 
            //            // expensive...so don't force it just to check for null early!
            //            if (sa != SerializeAsOptions.Link && prop.GetValue(value, null) == null)
            //            {
            //                writer.WriteStartArray();
            //                writer.WriteEndArray();
            //                continue;
            //            }

            //            switch (sa)
            //            {
            //                case SerializeAsOptions.Ids:
            //                    //writer.WritePropertyName(ContractResolver._modelManager.GetJsonKeyForProperty(prop));
            //                    IEnumerable<object> items = (IEnumerable<object>)prop.GetValue(value, null);
            //                    if (items == null)
            //                    {
            //                        writer.WriteValue((IEnumerable<object>)null); //TODO: Is it okay with the spec and Ember Data to return null for an empty array?
            //                        break; // LOOK OUT! Ending this case block early here!!!
            //                    }
            //                    this.WriteIdsArrayJson(writer, items, serializer);
            //                    if (iip)
            //                    {
            //                        Type itemType;
            //                        if (prop.PropertyType.IsGenericType)
            //                        {
            //                            itemType = prop.PropertyType.GetGenericArguments()[0];
            //                        }
            //                        else
            //                        {
            //                            // Must be an array at this point, right??
            //                            itemType = prop.PropertyType.GetElementType();
            //                        }
            //                        if (aggregator != null) aggregator.Add(itemType, items); // should call the IEnumerable one...right?
            //                    }
            //                    break;
            //                case SerializeAsOptions.Link:
            //                    if (lt == null) throw new JsonSerializationException("A property was decorated with SerializeAs(SerializeAsOptions.Link) but no LinkTemplate attribute was provided.");
            //                    //TODO: Check for "{0}" in linkTemplate and (only) if it's there, get the Ids of all objects and "implode" them.
            //                    string href = String.Format(lt, null, GetIdFor(value));
            //                    //writer.WritePropertyName(ContractResolver._modelManager.GetJsonKeyForProperty(prop));
            //                    //TODO: Support ids and type properties in "link" object
            //                    writer.WriteStartObject();
            //                    writer.WritePropertyName("href");
            //                    writer.WriteValue(href);
            //                    writer.WriteEndObject();
            //                    break;
            //                case SerializeAsOptions.Embedded:
            //                    // Not really supported by Ember Data yet, incidentally...but easy to implement here.
            //                    //writer.WritePropertyName(ContractResolver._modelManager.GetJsonKeyForProperty(prop));
            //                    //serializer.Serialize(writer, prop.GetValue(value, null));
            //                    this.Serialize(prop.GetValue(value, null), writeStream, writer, serializer, aggregator);
            //                    break;
            //            }
            //        }
            //        else
            //        {
            //            var propertyValue = prop.GetValue(value, null);

            //            // Look out! If we want to SerializeAs a link, computing the property is probably 
            //            // expensive...so don't force it just to check for null early!
            //            if (sa != SerializeAsOptions.Link && propertyValue == null)
            //            {
            //                writer.WriteNull();
            //                continue;
            //            }

            //            string objId = GetIdFor(propertyValue);

            //            switch (sa)
            //            {
            //                case SerializeAsOptions.Ids:
            //                    //writer.WritePropertyName(ContractResolver._modelManager.GetJsonKeyForProperty(prop));
            //                    serializer.Serialize(writer, objId);
            //                    if (iip)
            //                        if (aggregator != null)
            //                            aggregator.Add(prop.PropertyType, propertyValue);
            //                    break;
            //                case SerializeAsOptions.Link:
            //                    if (lt == null)
            //                        throw new JsonSerializationException(
            //                            "A property was decorated with SerializeAs(SerializeAsOptions.Link) but no LinkTemplate attribute was provided.");
            //                    string link = String.Format(lt, objId,
            //                        GetIdFor(value)); //value.GetType().GetProperty("Id").GetValue(value, null));

            //                    //writer.WritePropertyName(ContractResolver._modelManager.GetJsonKeyForProperty(prop));
            //                    writer.WriteStartObject();
            //                    writer.WritePropertyName("href");
            //                    writer.WriteValue(link);
            //                    writer.WriteEndObject();
            //                    break;
            //                case SerializeAsOptions.Embedded:
            //                    // Not really supported by Ember Data yet, incidentally...but easy to implement here.
            //                    //writer.WritePropertyName(ContractResolver._modelManager.GetJsonKeyForProperty(prop));
            //                    //serializer.Serialize(writer, prop.GetValue(value, null));
            //                    this.Serialize(prop.GetValue(value, null), writeStream, writer, serializer, aggregator);
            //                    break;
            //            }
            //        }

            //    }
            //    if (modelProps.Count() > 0)
            //    {
            //        writer.WriteEndObject();
            //    }

            //    writer.WriteEndObject();
        }

        #endregion

        #region Deserialization

        public Task<IPayload> DeserializeAsync(JsonReader reader, IFormatterLogger formatterLogger)
        {
            reader.Read();
            if (reader.TokenType != JsonToken.StartObject)
                throw new JsonSerializationException("Document root is not an object!");

            JObject metadata = null;
            IResourceObject primaryData = null;
            IEnumerable<IResourceObject> linkedData = null;
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    var propertyName = (string)reader.Value;
                    reader.Read(); // burn the PropertyName token
                    switch (propertyName)
                    {
                        case MetadataKey:
                            if (reader.TokenType != JsonToken.StartObject)
                                throw new BadRequestException(String.Format("The key for `{0}` must be an object.", MetadataKey));
                            metadata = (JObject)JToken.Load(reader);
                            break;
                        case RelatedDataKeyName:
                            linkedData = DeserializeLinkedData(reader);
                            break;
                        case PrimaryDataKeyName:
                            primaryData = DeserializeResourceObject(reader);
                            break;
                        default:
                            throw new BadRequestException(String.Format("The key `{0}` is not valid in the top-level context", propertyName));
                    }
                }
                else
                    reader.Skip();
            }

            if (primaryData == null)
                throw new BadRequestException(String.Format("Expected primary data located at the `{0}` key", PrimaryDataKeyName));

            var payload = new SingleResourcePayload(metadata);
            payload.PrimaryData.Add(primaryData);

            if (linkedData != null)
            {
                foreach (var relatedItem in linkedData)
                {
                    payload.RelatedData.Add(relatedItem);
                }
            }

            return Task.FromResult((IPayload)payload);
        }

        private IEnumerable<IResourceObject> DeserializeLinkedData(JsonReader reader)
        {
            if (reader.TokenType != JsonToken.StartArray)
                throw new BadRequestException(String.Format("Expected to find an array of resource objects, but got {0}", reader.TokenType));

            reader.Read(); // Burn the StartArray token

            var resourceObjects = new List<IResourceObject>();
            while (reader.TokenType != JsonToken.EndArray)
            {
                var resourceObject = DeserializeResourceObject(reader);
                resourceObjects.Add(resourceObject);
            }
            return resourceObjects;
        }

        private IResourceObject DeserializeResourceObject(JsonReader reader)
        {
            if (reader.TokenType != JsonToken.StartObject)
                throw new BadRequestException(String.Format("Expected to find a resource object, but got {0}", reader.TokenType));
            reader.Read(); // Burn the StartObject token

            var serializedObject = (JObject)JToken.Load(reader);

            JObject metadata = null;
            JProperty idProperty = null;
            JProperty resourceTypeNameProperty = null;
            IDictionary<string, JToken> dataAttributes = new Dictionary<string, JToken>();
            IDictionary<string, IRelationship> relationships = new Dictionary<string, IRelationship>();

            foreach (var property in serializedObject.Properties())
            {
                if (property.Name == ResourceObjectIdKey)
                {
                    idProperty = property;
                }
                else if (property.Name == ResourceObjectTypeNameKey)
                {
                    resourceTypeNameProperty = property;
                }
                else if (property.Name == MetadataKey)
                {
                    if (property.Type != JTokenType.Object)
                        throw new BadRequestException(String.Format("The property `{0}` must be an object.", MetadataKey));
                    metadata = (JObject)property.Value;
                }
                else if (property.Name == LinksKey)
                {
                    if (property.Type != JTokenType.Object)
                        throw new BadRequestException(String.Format("The property `{0}` must be an object.", MetadataKey));
                    relationships[property.Name] = ExtractRelationship((JObject)property.Value);
                }
                else
                {
                    dataAttributes.Add(property.Name, property.Value);
                }
            }

            // Get the id
            if (idProperty == null)
                throw new BadRequestException(
                    String.Format("The resource object does not have the required property `{0}`.", ResourceObjectIdKey));
            if (idProperty.Type != JTokenType.String)
                throw new BadRequestException(
                    String.Format("The property `{0}` must have a non-null string value.", ResourceObjectIdKey));
            var id = idProperty.Value<string>();

            // Get the type
            if (resourceTypeNameProperty == null)
                throw new BadRequestException(
                    String.Format("The resource object does not have the required property `{0}`.", ResourceObjectTypeNameKey));
            if (resourceTypeNameProperty.Type != JTokenType.String)
                throw new BadRequestException(
                    String.Format("The property `{0}` must have a non-null string value.", ResourceObjectTypeNameKey));
            var resourceTypeName = resourceTypeNameProperty.Value<string>();

            Type resourceType;
            try
            {
                resourceType = _modelManager.GetTypeByResourceTypeName(resourceTypeName);
            }
            catch (InvalidOperationException)
            {
                throw new BadRequestException(String.Format("The type `{0}` is not registered with the model manager.", resourceTypeName));
            }

            return new ResourceObject
            {
                Id = id,
                ResourceType = resourceType,
                DataAttributes = dataAttributes,
                Metadata = metadata,
                Relationships = relationships
            };

            //object retval = Activator.CreateInstance(resourceType);

            //foreach (var property in serializedObject.Properties())
            //{
            //    if (property == idProperty || property == resourceTypeNameProperty || property == metadataProperty)
            //        continue;

            //    PropertyInfo prop = _modelManager.GetPropertyForJsonKey(resourceType, property.Name);

            //    if (property.Name == LinksKey)
            //    {
            //        reader.Read(); // burn the PropertyName token
            //        //TODO: linked resources (Done??)
            //        DeserializeLinkedResources(retval, reader);
            //    }
            //    else if (prop != null)
            //    {
            //        if (!prop.PropertyType.CanWriteAsJsonApiAttribute())
            //        {
            //            reader.Read(); // burn the PropertyName token
            //            //TODO: Embedded would be dropped here!
            //            continue; // These aren't supposed to be here, they're supposed to be in "links"!
            //        }

            //        object propVal;
            //        Type enumType;
            //        if (prop.PropertyType == typeof(string) &&
            //            prop.GetCustomAttributes().Any(attr => attr is SerializeStringAsRawJsonAttribute))
            //        {
            //            reader.Read();
            //            if (reader.TokenType == JsonToken.Null)
            //            {
            //                propVal = null;
            //            }
            //            else
            //            {
            //                var token = JToken.Load(reader);
            //                var rawPropVal = token.ToString();
            //                propVal = JsonHelpers.MinifyJson(rawPropVal);
            //            }
            //        }
            //        else if (prop.PropertyType.IsGenericType &&
            //                    prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) &&
            //                    (enumType = prop.PropertyType.GetGenericArguments()[0]).IsEnum)
            //        {
            //            // Nullable enums need special handling
            //            reader.Read();
            //            propVal = reader.TokenType == JsonToken.Null
            //                ? null
            //                : Enum.Parse(enumType, reader.Value.ToString());
            //        }
            //        else if (prop.PropertyType == typeof(DateTimeOffset) ||
            //                    prop.PropertyType == typeof(DateTimeOffset?))
            //        {
            //            // For some reason 
            //            reader.ReadAsString();
            //            propVal = reader.TokenType == JsonToken.Null
            //                ? (object)null
            //                : DateTimeOffset.Parse(reader.Value.ToString());
            //        }
            //        else
            //        {
            //            reader.Read();
            //            propVal = DeserializeAttribute(prop.PropertyType, reader);
            //        }


            //        prop.SetValue(retval, propVal, null);

            //        // Tell the MetadataManager that we deserialized this property
            //        MetadataManager.Instance.SetMetaForProperty(retval, prop, true);

            //        // pop the value off the reader, so we catch the EndObject token below!.
            //        reader.Read();
            //    }
            //    else
            //    {
            //        // Unexpected/unknown property--Skip the propertyname and its value
            //        reader.Skip();
            //        if (reader.TokenType == JsonToken.StartArray || reader.TokenType == JsonToken.StartObject) reader.Skip();
            //        else reader.Read();
            //    }
            //}

            //return resourceObject;
        }

        /// <summary>
        /// Gets an IRelationship object from the serialized `links` JSON object.
        /// </summary>
        /// <param name="linksObject">The serialized `links` JSON object</param>
        /// <returns>A corresponding IRelationship</returns>
        private IRelationship ExtractRelationship(JObject linksObject)
        {
            string resourceTypeName = null;
            string[] ids = null;
            Tuple<string, string>[] data = null;
            foreach (var property in linksObject.Properties())
            {
                if (property.Name == LinksObjectIdKey)
                {
                    if (property.Value.Type == JTokenType.String)
                    {
                        ids = new[] { property.Value.Value<string>()};
                    }
                    else if (property.Value.Type == JTokenType.Array)
                    {
                        var array = (JArray) property.Value;

                        var idList = new List<string>();
                        foreach (var token in array)
                        {
                            if (token.Type != JTokenType.String)
                                throw new BadRequestException(
                                    String.Format(
                                        "If an array is provided as the value for the `{0}` property of a links object, each element must be a string.",
                                        LinksObjectIdKey));
                            idList.Add(token.Value<string>());
                        }
                        ids = idList.ToArray();
                    }
                    else
                    {
                        throw new BadRequestException(
                            String.Format(
                                "The value for the `{0}` property of a links object must be either a string or an array of strings.",
                                LinksObjectIdKey));
                    }
                }
                else if (property.Name == LinksObjectTypeKey)
                {
                    if (property.Value.Type != JTokenType.String)
                        throw new BadRequestException(
                            String.Format("The value for the `{0}` property of a links object must be a string.", LinksObjectTypeKey));

                    resourceTypeName = property.Value.Value<string>();
                }
                else if (property.Name == LinksObjectDataKey)
                {
                    if (property.Value.Type != JTokenType.Array)
                        throw new BadRequestException(String.Format("The value for the `{0}` property of a links object must be an array.", LinksObjectDataKey));

                    var dataItems = new List<Tuple<string, string>>();
                    foreach (var token in (JArray) property.Value)
                    {
                        if (token.Type != JTokenType.Object)
                            throw new BadRequestException(String.Format("Each element in the array for the `{0}` property of a links object must be an object.", LinksObjectDataKey));
                        var linkedResourceObj = (JObject) token;

                        var idProp = linkedResourceObj.Property(ResourceObjectIdKey);
                        string id;
                        if (idProp == null ||
                            idProp.Type != JTokenType.String ||
                            (id = idProp.Value.Value<string>()) == String.Empty)
                            throw new BadRequestException(
                                String.Format(
                                    "Each element in the array for the `{0}` property of a links object must contain a non-empty string-value property called `{1}`.",
                                    LinksObjectDataKey, ResourceObjectIdKey));

                        var relatedResourceTypeNameProp = linkedResourceObj.Property(ResourceObjectTypeNameKey);
                        string relatedResourceTypeName;
                        if (relatedResourceTypeNameProp == null ||
                            relatedResourceTypeNameProp.Type != JTokenType.String ||
                            (relatedResourceTypeName = relatedResourceTypeNameProp.Value.Value<string>()) ==
                            String.Empty)
                            throw new BadRequestException(
                                String.Format(
                                    "Each element in the array for the `{0}` property of a links object must contain a non-empty string-value property called `{1}`.",
                                    LinksObjectDataKey, ResourceObjectTypeNameKey));

                        dataItems.Add(Tuple.Create(id, relatedResourceTypeName));
                    }
                    data = dataItems.ToArray();
                }
            }

            Tuple<Type, string>[] relatedResources = null;
            if (resourceTypeName != null)
            {
                if (ids == null)
                    throw new BadRequestException(
                        String.Format("If `{0}` is specified in a links object, `{1}` must also be specified.",
                            LinksObjectTypeKey, LinksObjectIdKey));
                if (data != null)
                    throw new BadRequestException(
                        String.Format("If `{0}` is specified in a links object, `{1}` may not also be specified.",
                            LinksObjectTypeKey, LinksObjectDataKey));
                var resourceType = _modelManager.GetTypeByResourceTypeName(resourceTypeName);
                relatedResources = ids.Select(id => Tuple.Create(resourceType, id)).ToArray();
            }
            else if (ids != null)
            {
                throw new BadRequestException(
                    String.Format("If `{0}` is specified in a links object, `{1}` must also be specified.",
                        LinksObjectIdKey, LinksObjectTypeKey));
            }
            else if (data != null)
            {
                relatedResources =
                    data.Select(d => Tuple.Create(_modelManager.GetTypeByResourceTypeName(d.Item1), d.Item2)).ToArray();
            }

            throw new NotImplementedException();
            //PropertyInfo prop = _modelManager.GetPropertyForJsonKey(objectType, value);
            //    if (prop != null && !prop.PropertyType.CanWriteAsJsonApiAttribute())
            //    {
            //        if (_modelManager.IsSerializedAsMany(prop.PropertyType))
            //        {
            //            // Is a hasMany

            //            if (reader.TokenType != JsonToken.StartObject)
            //                throw new BadRequestException("The value of a to-many relationship must be an object.");

            //            JArray ids = null;
            //            string resourceType = null;
            //            JArray relatedObjects = null;

            //            while (reader.Read())
            //            {
            //                if (reader.TokenType == JsonToken.EndObject)
            //                    break;

            //                // Not sure what else could even go here, but if it's not a property name, throw an error.
            //                if (reader.TokenType != JsonToken.PropertyName)
            //                    throw new BadRequestException("Unexpected token: " + reader.TokenType);

            //                var propName = (string)reader.Value;
            //                reader.Read();

            //                if (propName == "ids")
            //                {
            //                    if (reader.TokenType != JsonToken.StartArray)
            //                        throw new BadRequestException("The value of `ids` must be an array.");

            //                    ids = JArray.Load(reader);
            //                }
            //                else if (propName == "type")
            //                {
            //                    if (reader.TokenType != JsonToken.String)
            //                        throw new BadRequestException("Unexpected value for `type`: " + reader.TokenType);

            //                    resourceType = (string)reader.Value;
            //                }
            //                else if (propName == "data")
            //                {
            //                    if (reader.TokenType != JsonToken.StartArray)
            //                        throw new BadRequestException("Unexpected value for `data`: " + reader.TokenType);

            //                    relatedObjects = JArray.Load(reader);
            //                }
            //                else
            //                {
            //                    throw new BadRequestException("Unexpected property name: " + propName);
            //                }
            //            }

            //            var relatedStubs = new List<object>();

            //            Type relType;
            //            if (prop.PropertyType.IsGenericType)
            //            {
            //                relType = prop.PropertyType.GetGenericArguments()[0];
            //            }
            //            else
            //            {
            //                // Must be an array at this point, right??
            //                relType = prop.PropertyType.GetElementType();
            //            }

            //            // According to the spec, either the type and ids or data must be specified
            //            if (relatedObjects != null)
            //            {
            //                if (ids != null)
            //                    throw new BadRequestException("If `data` is specified, then `ids` may not be.");

            //                if (resourceType != null)
            //                    throw new BadRequestException("If `data` is specified, then `type` may not be.");

            //                foreach (var relatedObject in relatedObjects)
            //                {
            //                    if (!(relatedObject is JObject))
            //                        throw new BadRequestException("Each element in the `data` array must be an object.");

            //                    var relatedObjectType = relatedObject["type"] as JValue;
            //                    if (relatedObjectType == null || relatedObjectType.Type != JTokenType.String)
            //                        throw new BadRequestException("Each element in the `data` array must have a string value for the key `type`.");

            //                    var relatedObjectId = relatedObject["id"] as JValue;
            //                    if (relatedObjectId == null || relatedObjectId.Type != JTokenType.String)
            //                        throw new BadRequestException("Each element in the `data` array must have a string value for the key `id`.");

            //                    var relatedObjectIdValue = relatedObjectId.Value<string>();
            //                    if (string.IsNullOrWhiteSpace(relatedObjectIdValue))
            //                        throw new BadRequestException("The value for `id` must be specified.");

            //                    var stub = GetById(relType, relatedObjectIdValue);
            //                    relatedStubs.Add(stub);
            //                }
            //            }
            //            else if (ids == null)
            //            {
            //                throw new BadRequestException("If `data` is not specified, then `ids` must be specified.");
            //            }
            //            else if (resourceType == null)
            //            {
            //                // We aren't doing anything with this value for now, but it needs to be present in the request payload.
            //                // We will need to reference it to properly support polymorphism.
            //                throw new BadRequestException("If `data` is not specified, then `type` must be specified.");
            //            }
            //            else
            //            {
            //                relatedStubs.AddRange(ids.Select(token => GetById(relType, token.ToObject<string>())));
            //            }

            //            IEnumerable<Object> hmrel = (IEnumerable<Object>)prop.GetValue(obj, null);
            //            if (hmrel == null)
            //            {
            //                // Hmm...now we have to create an object that fits this property. This could get messy...
            //                if (!prop.PropertyType.IsInterface && !prop.PropertyType.IsAbstract)
            //                {
            //                    // Whew...okay, just instantiate one of these...
            //                    hmrel = (IEnumerable<Object>)Activator.CreateInstance(prop.PropertyType);
            //                }
            //                else
            //                {
            //                    // Ugh...now we're really in trouble...hopefully one of these will work:
            //                    if (prop.PropertyType.IsGenericType)
            //                    {
            //                        if (prop.PropertyType.IsAssignableFrom(typeof(List<>).MakeGenericType(relType)))
            //                        {
            //                            hmrel = (IEnumerable<Object>)Activator.CreateInstance(typeof(List<>).MakeGenericType(relType));
            //                        }
            //                        else if (prop.PropertyType.IsAssignableFrom(typeof(HashSet<>).MakeGenericType(relType)))
            //                        {
            //                            hmrel = (IEnumerable<Object>)Activator.CreateInstance(typeof(HashSet<>).MakeGenericType(relType));
            //                        }
            //                        //TODO: Other likely candidates??
            //                        else
            //                        {
            //                            // punt!
            //                            throw new JsonReaderException(String.Format("Could not create empty container for relationship property {0}!", prop));
            //                        }
            //                    }
            //                    else
            //                    {
            //                        // erm...Array??!?
            //                        hmrel = (IEnumerable<Object>)Array.CreateInstance(relType, ids.Count);
            //                    }
            //                }
            //            }

            //            // We're having problems with how to generalize/cast/generic-ize this code, so for the time
            //            // being we'll brute-force it in super-dynamic language style...
            //            Type hmtype = hmrel.GetType();
            //            MethodInfo add = hmtype.GetMethod("Add");

            //            foreach (var stub in relatedStubs)
            //            {
            //                add.Invoke(hmrel, new[] { stub });
            //            }

            //            prop.SetValue(obj, hmrel);
            //        }
            //        else
            //        {
            //            // Is a belongsTo

            //            if (reader.TokenType == JsonToken.StartObject)
            //            {
            //                string id = null;
            //                string resourceType = null;

            //                while (reader.Read())
            //                {
            //                    if (reader.TokenType == JsonToken.EndObject)
            //                        break;

            //                    // Not sure what else could even go here, but if it's not a property name, throw an error.
            //                    if (reader.TokenType != JsonToken.PropertyName)
            //                        throw new BadRequestException("Unexpected token: " + reader.TokenType);

            //                    var propName = (string)reader.Value;
            //                    reader.Read();

            //                    if (propName == "id")
            //                    {
            //                        var idValue = reader.Value;

            //                        // The id must be a string.
            //                        if (!(idValue is string))
            //                            throw new BadRequestException("The value of the `id` property must be a string.");

            //                        id = (string)idValue;
            //                    }
            //                    else if (propName == "type")
            //                    {
            //                        // TODO: we don't do anything with this value yet, but we will need to in order to
            //                        // support polymorphic endpoints
            //                        resourceType = (string)reader.Value;
            //                    }
            //                }

            //                // The id must be specified.
            //                if (id == null)
            //                    throw new BadRequestException("Nothing was specified for the `id` property.");

            //                // The type must be specified.
            //                if (resourceType == null)
            //                    throw new BadRequestException("Nothing was specified for the `type` property.");

            //                Type relType = prop.PropertyType;

            //                prop.SetValue(obj, GetById(relType, id));
            //            }
            //            else if (reader.TokenType == JsonToken.Null)
            //            {
            //                prop.SetValue(obj, null);
            //            }
            //            else
            //            {
            //                throw new BadRequestException("The value of a to-one relationship must be an object or null.");
            //            }
            //        }

            //        // Tell the MetadataManager that we deserialized this property
            //        MetadataManager.Instance.SetMetaForProperty(obj, prop, true);
            //    }
            //    else
            //        reader.Skip();
            //}
        }

        #endregion

        #region Helpers

        private Type GetSingleType(Type type)
        {
            return _modelManager.IsSerializedAsMany(type) ? _modelManager.GetElementType(type) : type;
        }

        protected object GetById(Type type, string id)
        {
            // Only good for creating dummy relationship objects...
            object retval = Activator.CreateInstance(type);
            PropertyInfo idprop = _modelManager.GetIdProperty(type);
            idprop.SetValue(retval, System.Convert.ChangeType(id, idprop.PropertyType));
            return retval;
        }

        protected string GetValueForIdProperty(PropertyInfo idprop, object obj)
        {
            if (idprop != null)
            {
                return idprop.GetValue(obj).ToString();
            }
            return "NOIDCOMPUTABLE!";
        }

        protected string GetIdFor(object obj)
        {
            Type type = obj.GetType();
            PropertyInfo idprop = _modelManager.GetIdProperty(type);
            return GetValueForIdProperty(idprop, obj);
        }

        private void WriteIdsArrayJson(Newtonsoft.Json.JsonWriter writer, IEnumerable<object> value, Newtonsoft.Json.JsonSerializer serializer)
        {
            IEnumerator<Object> collectionEnumerator = (value as IEnumerable<object>).GetEnumerator();
            writer.WriteStartArray();
            while (collectionEnumerator.MoveNext())
            {
                var serializable = collectionEnumerator.Current;
                writer.WriteValue(this.GetIdFor(serializable));
            }
            writer.WriteEndArray();
        }

        #endregion
    }
}
