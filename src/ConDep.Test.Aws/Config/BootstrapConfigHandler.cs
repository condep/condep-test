using System.IO;
using Newtonsoft.Json;

namespace ConDep.Test.Aws.Config
{
    public class BootstrapConfigHandler
    {
        private readonly string _bootstrapId;
        private JsonSerializerSettings _jsonSettings;

        public BootstrapConfigHandler(string bootstrapId)
        {
            _bootstrapId = bootstrapId;
        }

        public void Write(string dirPath, dynamic config)
        {
            var filePath = Path.Combine(dirPath, _bootstrapId + ".json");
            File.WriteAllText(filePath, ConvertToJsonText(config));
        }

        public Ec2BootstrapConfig GetTypedEnvConfig(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(string.Format("[{0}] not found.", filePath), filePath);
            }

            using (var fileStream = File.OpenRead(filePath))
            {
                return GetTypedEnvConfig(fileStream);
            }
        }

        public Ec2BootstrapConfig GetTypedEnvConfig(Stream stream)
        {
            Ec2BootstrapConfig config;
            using (var memStream = GetMemoryStreamWithCorrectEncoding(stream))
            {
                using (var reader = new StreamReader(memStream))
                {
                    var json = reader.ReadToEnd();
                    config = JsonConvert.DeserializeObject<Ec2BootstrapConfig>(json, JsonSettings);
                }
            }
            return config;
        }

        private static MemoryStream GetMemoryStreamWithCorrectEncoding(Stream stream)
        {
            using (var r = new StreamReader(stream, true))
            {
                var encoding = r.CurrentEncoding;
                return new MemoryStream(encoding.GetBytes(r.ReadToEnd()));
            }
        }


        private string ConvertToJsonText(dynamic config)
        {
            return JsonConvert.SerializeObject(config, JsonSettings);
        }

        private JsonSerializerSettings JsonSettings
        {
            get
            {
                return _jsonSettings ?? (_jsonSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented,
                });
            }
        }


    }
}