using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Zeebe.Client.Impl.Responses;

namespace Zeebe.Client
{
    public class VariablesExtensionsTest
    {
        [TestCaseSource(nameof(ValidVariables))]
        public void ShouldDeserializeVariablesAsDictionary(string json, IDictionary<string, object> expected)
        {
            var variables = json.GetVariablesAsMap();
            Assert.AreEqual(expected.Keys, variables.Keys);
            Assert.AreEqual(expected.Values, variables.Values);
        }

        [TestCaseSource(nameof(InvalidVariables))]
        public void ShouldNotDeserializeInvalidJson(string json)
        {
            Assert.Throws<JsonSerializationException>(() => json.GetVariablesAsMap());
        }

        [TestCaseSource(nameof(ValidVariables))]
        public void ShouldDeserializeVariablesAsT(string json, IDictionary<string, object> _)
        {
            var variables = json.GetVariablesAsType<Dictionary<string, object>>();
            Assert.IsInstanceOf<Dictionary<string, object>>(variables);
        }

        private static IEnumerable<object[]> ValidVariables
        {
            get
            {
                yield return new object[]
                {
                    @"{'a' : 1, 'b' : 'any'}", new Dictionary<string, object> { { "a", 1 }, { "b", "any" } }
                };
                yield return new object[]
                {
                    @"{'a' : true, 'b' : { 'c' : 15 }}", new Dictionary<string, object> { { "a", true }, { "b", JObject.FromObject(new { c = 15 }) } }
                };
                yield return new object[]
                {
                    @"{'a' : null}", new Dictionary<string, object> { { "a", null } }
                };
                yield return new object[]
                {
                    @"{}", new Dictionary<string, object>()
                };
            }
        }

        private static IEnumerable<object[]> InvalidVariables
        {
            get
            {
                yield return new object[]
                {
                    @"'a' : 1"
                };
                yield return new object[]
                {
                    @"['a' : true, 'b' 15 ]"
                };
            }
        }
    }
}