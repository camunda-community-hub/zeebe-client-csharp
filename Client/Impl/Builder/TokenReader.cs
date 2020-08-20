using System.IO;
using YamlDotNet.Serialization;

namespace Zeebe.Client.Impl.Builder
{
    using YamlDotNet.RepresentationModel;
    public class TokenReader
    {
        private readonly string path;

        public TokenReader(string path)
        {
            this.path = path;
            var serializer = new DeserializerBuilder().Build();

            char[] result;
            using (StreamReader reader = File.OpenText(path))
            {
                var obj = serializer.Deserialize(reader);
            }
        }
    }
}