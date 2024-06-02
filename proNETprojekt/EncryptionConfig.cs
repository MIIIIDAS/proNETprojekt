using System;
using System.IO;
using System.Xml.Serialization;

namespace proNETprojekt
{
    public class EncryptionConfig
    {
        public string EncryptionKey { get; set; }
        public string InitializationVector { get; set; }

        public static EncryptionConfig LoadConfigFromFile(string filePath)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(EncryptionConfig));
                using (var stream = File.OpenRead(filePath))
                {
                    return (EncryptionConfig)serializer.Deserialize(stream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading encryption configuration: " + ex.Message);
                return new EncryptionConfig();
            }
        }

        public static void SaveConfigToFile(string filePath, EncryptionConfig config)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(EncryptionConfig));
                using (var stream = File.Create(filePath))
                {
                    serializer.Serialize(stream, config);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving encryption configuration: " + ex.Message);
            }
        }
    }
}
