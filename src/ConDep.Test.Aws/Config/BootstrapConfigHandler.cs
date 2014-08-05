using System.IO;
using System.Resources;
using ConDep.Test.Aws.Logging;
using Newtonsoft.Json;

namespace ConDep.Test.Aws.Config
{
    public class BootstrapConfigHandler
    {
        private JsonSerializerSettings _jsonSettings;
        private string _filePath;

        public BootstrapConfigHandler(string bootstrapId)
        {
            _filePath = GetFilePath(bootstrapId);
        }

        private static string GetFilePath(string bootstrapId)
        {
            return Path.Combine(@"C:\temp\", bootstrapId + ".json");
        }

        public void Write(dynamic config)
        {
            File.WriteAllText(_filePath, ConvertToJsonText(config));
        }

        public Ec2BootstrapConfig GetTypedEnvConfig()
        {
            ValidatePath(_filePath);

            using (var fileStream = File.OpenRead(_filePath))
            {
                return GetTypedEnvConfig(fileStream);
            }
        }

        public static void DeleteConfigFile(string bootstrapId)
        {
            var fileName = GetFilePath(bootstrapId);
            Logger.Info("Deleting {0}", fileName);
            ValidatePath(fileName);
            File.Delete(fileName);
            Logger.Info("Config {0} deleted", fileName);
        }

        private static void ValidatePath(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(string.Format("[{0}] not found.", filePath), filePath);
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
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    Formatting = Formatting.Indented,
                });
            }
        }


    }
}