using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace Shipbot.Controller.Core.ApplicationSources
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
        
        const string AccessorPattern = @"(?<name>[\w\d\s]+)\[(?<index>[\w\d\""\']+)\]";

        private YamlScalarNode? VisitDocAndGetValueNode(string path, YamlDocument doc)
        {
            YamlMappingNode? GetMappingNodeFromArray(YamlSequenceNode sequenceNode, int index)
            {
                if (sequenceNode.Children.Count <= index)
                    return null;
                
                return sequenceNode.Children[index] as YamlMappingNode 
                       ?? throw new InvalidOperationException("Retrieved child from array is NOT a map type."); 
            }
                        
            YamlMappingNode? GetMappingNodeFromMap(YamlMappingNode mappingNode, string property)
            {
                return mappingNode.Children[property] as YamlMappingNode 
                       ?? throw new InvalidOperationException("Retrieved child from map is NOT a map type."); 
            }

            var parts = path.Split('.');

            var node = doc.RootNode as YamlMappingNode;

            foreach (var part in parts)
            {
                if (node == null)
                    break;

                string? mapAccessorKey = null;
                int? arrayAccessorIndex = null;
                string propertyName = part;

                // check if part is an array accessor
                var matches = Regex.Match(part, AccessorPattern);
                if (matches.Success)
                {
                    // we have an array accessor
                    propertyName = matches.Groups["name"].Value;
                    var rawAccessorIndex = matches.Groups["index"].Value;

                    if (int.TryParse(rawAccessorIndex, out var aai))
                    {
                        // index is an integer, assume we are going to read an array from property
                        arrayAccessorIndex = aai;
                    }
                    else
                    {
                        mapAccessorKey = rawAccessorIndex.Trim().Trim('\"', '\'');
                    }
                }

                if (!node.Children.ContainsKey(propertyName)) 
                    break;
                
                if (parts.Last().Equals(part))
                {
                    // we reached the end of the provided path, return the value we found
                    if (arrayAccessorIndex == null && mapAccessorKey == null)
                    {
                        // we are not trying to access a map or array
                        if (node.Children[propertyName] is YamlScalarNode valueNode)
                        {
                            return valueNode;
                        }
                    }
                    else
                    {
                        // we are reading a single value from within an array or a map
                        var childNode = node.Children[propertyName];
                        var valueNode = childNode switch
                        {
                            YamlSequenceNode arrayNode => arrayAccessorIndex != null
                                ? (YamlScalarNode)arrayNode.Children[arrayAccessorIndex.Value]
                                : throw new InvalidOperationException(
                                    "We are trying to access an array node, but index value is not an integer."),
                            YamlMappingNode mappingNode => mapAccessorKey != null
                                ? (YamlScalarNode)mappingNode.Children[mapAccessorKey]
                                : throw new InvalidOperationException(
                                    "We are trying to access a map node, but index value is not a string."),
                            _ => throw new InvalidOperationException("Found mismatching type")
                        };
                        
                        return valueNode;
                    }
                }
                else
                {
                    if (arrayAccessorIndex == null && mapAccessorKey == null)
                    {
                        node = node.Children[propertyName] as YamlMappingNode;
                    }
                    else
                    {
                        var childNode = node.Children[propertyName];
                        node = childNode switch
                        {
                            YamlSequenceNode arrayNode => arrayAccessorIndex != null
                                ? GetMappingNodeFromArray(arrayNode, arrayAccessorIndex.Value)
                                : throw new InvalidOperationException(
                                    "We are trying to access an array node, but index value is not an integer."),
                            YamlMappingNode mappingNode => mapAccessorKey != null
                                ? GetMappingNodeFromMap(mappingNode, mapAccessorKey)
                                : throw new InvalidOperationException(
                                    "We are trying to access a map node, but index value is not a string."),
                            _ => throw new InvalidOperationException("Found mismatching type")
                        };
                    }
                        
                }
            }
            
            return null;
        }

        public string? ExtractValueFromDoc(string tagPropertyPath, YamlDocument doc)
        {
            var valueNode = VisitDocAndGetValueNode(tagPropertyPath, doc);
            return valueNode?.Value;
        }
        
        public void SetValueInDoc(string tagPropertyPath, YamlDocument doc, string newValue)
        {
            var valueNode = VisitDocAndGetValueNode(tagPropertyPath, doc);
            if (valueNode != null)
                valueNode.Value = newValue;
        }
    }
}