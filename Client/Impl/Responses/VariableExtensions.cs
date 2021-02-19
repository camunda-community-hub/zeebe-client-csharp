using System.Collections.Generic;
using Newtonsoft.Json;

namespace Zeebe.Client.Impl.Responses
{
    /// <summary>
    /// Defines some extensions to handle variables.
    /// </summary>
    public static class VariableExtensions
    {
        /// <summary>
        /// Deserialize a JSON string as a dictionary of keys - the variable name - and their values.
        /// Message variables must be a JSON object, as variables will be mapped in a
        /// key-value fashion. e.g. { "a": 1, "b": 2 } will create two key/value pairs, named "a" and
        /// "b" respectively, with their associated values. [{ "a": 1, "b": 2 }] would not be a
        /// valid argument, as the root of the JSON document is an array and not an object.
        /// </summary>
        /// <param name="variables">Variables serialized as json.</param>
        /// <param name="settings">Optional Json serializer settings.</param>
        /// <returns>A deserialized dictionary of variables from the JSON string.</returns>
        public static IReadOnlyDictionary<string, object> GetVariablesAsMap(this string variables, JsonSerializerSettings settings = null)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(variables, settings);
        }

        /// <summary>
        /// Deserialize variables as T.
        /// </summary>
        /// <typeparam name="T">The target .NET type.</typeparam>
        /// <param name="variables">Variables serialized as json.</param>
        /// <param name="settings">Optional Json serializer settings.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static T GetVariablesAsType<T>(this string variables, JsonSerializerSettings settings = null) where T : class
        {
            return JsonConvert.DeserializeObject<T>(variables, settings);
        }
    }
}