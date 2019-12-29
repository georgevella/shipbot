using System.IO;
using System.Linq;
using System.Text;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace Shipbot.Controller.Core.DeploymentSources
{
    public class YamlUtilities
    {
        public void ReadYamlStream(YamlStream yaml, string file)
        {
            using var stream = File.Open(file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            ReadYamlStream(yaml, stream);
        }

        public void ReadYamlStream(YamlStream yaml, Stream stream)
        {
            using var reader = new StreamReader(stream, Encoding.UTF8, true, 1024, leaveOpen: true);
            yaml.Load(reader);
        }

        public void WriteYamlStream(YamlStream yaml, string file)
        {
            using var stream = File.Open(file, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
            WriteYamlStream(yaml, stream);
        }

        public void WriteYamlStream(YamlStream yaml, Stream stream)
        {
            using var writer = new StreamWriter(stream, Encoding.UTF8, 1024,  true);
            
            if (yaml.Documents.Count == 1)
            {
                var serializer = new Serializer();
                serializer.Serialize(writer, yaml.Documents.First().RootNode);
            }
            else
            {
                yaml.Save(writer, false);
            }
            
            writer.Flush();
        }

        public string ExtractValueFromDoc(string tagPropertyPath, YamlDocument doc)
        {
            var parts = tagPropertyPath.Split('.');

            var node = doc.RootNode as YamlMappingNode;

            foreach (var part in parts)
            {
                if (node == null)
                    break;

                if (!node.Children.ContainsKey(part)) 
                    break;
                
                if (parts.Last().Equals(part))
                {
                    if (node.Children[part] is YamlScalarNode valueNode)
                    {
                        return valueNode.Value;
                    }
                }
                else
                {
                    node = node.Children[part] as YamlMappingNode;
                }
            }
            
            return null;
        }
        
        public void SetValueInDoc(string tagPropertyPath, YamlDocument doc, string newValue)
        {
            var parts = tagPropertyPath.Split('.');

            var node = doc.RootNode as YamlMappingNode;

            foreach (var part in parts)
            {
                if (node == null)
                    break;

                if (!node.Children.ContainsKey(part)) 
                    break;
                
                if (parts.Last().Equals(part))
                {
                    if (node.Children[part] is YamlScalarNode valueNode)
                    {
                        valueNode.Value = newValue;
                    }
                }
                else
                {
                    node = node.Children[part] as YamlMappingNode;
                }
            }
        }
    }
}