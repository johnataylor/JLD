using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JLD
{
    class JsonLdProcessor
    {
        public static JArray Expand(JObject original)
        {
            JsonLdProcessorContext activeContext = new JsonLdProcessorContext();

            JToken token = InnerExpand(activeContext, null, original);
            if (token == null)
            {
                return null;
            }
            if (token.Type == JTokenType.Array)
            {
                return (JArray)token;
            }
            JArray result = new JArray();
            result.Add(token);
            return result;
        }

        private static void AddToValueArray(JObject target, string name, JToken value)
        {
            JArray array;
            JToken token;
            if (target.TryGetValue(name, out token))
            {
                array = (JArray)token;
            }
            else
            {
                array = new JArray();
                target.Add(name, array);
            }
            if (value.Type == JTokenType.Array)
            {
                foreach (JToken item in value)
                {
                    array.Add(item);
                }
            }
            else
            {
                array.Add(value);
            }
        }

        private static void AddToObject(JObject obj, TermDefinition termDefinition, JToken expandedValue)
        {
            if (termDefinition.Container != null)
            {
                JObject container = new JObject();
                AddToValueArray(container, termDefinition.Container, expandedValue);
                obj.Add(termDefinition.Id, container);
            }
            else if (termDefinition.Reverse != null)
            {
                JObject reverse = new JObject();
                AddToValueArray(reverse, termDefinition.Reverse, expandedValue);
                obj.Add("@reverse", reverse);
            }
            else
            {
                AddToValueArray(obj, termDefinition.Id, expandedValue);
            }
        }

        private static JToken InnerExpand(JsonLdProcessorContext activeContext, TermDefinition activePropertyTermDefinition, JToken original)
        {
            switch (original.Type)
            {
                case JTokenType.Array:
                {
                    JArray originalArray = (JArray)original;
                    JArray expanded = new JArray();
                    foreach (JToken property in originalArray)
                    {
                        expanded.Add(InnerExpand(activeContext, activePropertyTermDefinition, property));
                    }
                    return expanded;
                }
                case JTokenType.Object:
                {
                    JObject originalObj = (JObject)original;
                    JToken expanded;

                    activeContext.Push(originalObj);

                    JToken graph;
                    if (originalObj.TryGetValue("@graph", out graph))
                    {
                        expanded = InnerExpand(activeContext, null, graph);

                        JToken id;
                        if (originalObj.TryGetValue("@id", out id))
                        {
                            JObject namedGraph = new JObject();
                            namedGraph.Add("@graph", expanded);
                            namedGraph.Add("@id", id);

                            //TODO: other graph level properties

                            expanded = namedGraph;
                        }
                    }
                    else
                    {
                        JObject obj = new JObject();

                        foreach (JProperty property in originalObj.Properties())
                        {
                            string name = activeContext.Alias(property.Name);

                            switch (name)
                            {
                                case "@id":
                                    obj.Add("@id", property.Value);
                                    break;

                                case "@context":
                                    break;

                                case "@type":
                                    {
                                        string expandedName = "@type";
                                        JToken expandedValue = InnerExpand(activeContext, null, property.Value);
                                        AddToValueArray(obj, expandedName, expandedValue);
                                        break;
                                    }

                                default:
                                    {
                                        TermDefinition termDefinition;
                                        if (activeContext.TryLookUp(name, out termDefinition))
                                        {
                                            if (termDefinition.Id == null)
                                            {
                                                TermDefinition prefixTermDefinition;
                                                if (activeContext.TryLookUpPrefix(name, out prefixTermDefinition))
                                                {
                                                    termDefinition.Id = prefixTermDefinition.Id;
                                                }
                                            }
                                            JToken expandedValue = InnerExpand(activeContext, termDefinition, property.Value);
                                            AddToObject(obj, termDefinition, expandedValue);
                                        }
                                        else if (activeContext.TryLookUpPrefix(name, out termDefinition))
                                        {
                                            JToken expandedValue = InnerExpand(activeContext, termDefinition, property.Value);
                                            AddToObject(obj, termDefinition, expandedValue);
                                        }
                                        else
                                        {
                                            termDefinition = new TermDefinition(name);
                                            JToken expandedValue = InnerExpand(activeContext, termDefinition, property.Value);
                                            AddToObject(obj, termDefinition, expandedValue);
                                        }
                                    }
                                    break;
                            }
                        }

                        expanded = obj;
                    }

                    activeContext.Pop();

                    return expanded;
                }
                default:
                {
                    if (activePropertyTermDefinition == null)
                    {
                        string expandedValue = activeContext.ExpandValue(original.ToString());
                        return new JValue(expandedValue);
                    }
                    else
                    {
                        JObject expanded = new JObject();

                        if (activePropertyTermDefinition.Type == "@id")
                        {
                            string expandedValue = activeContext.ExpandValue(original.ToString());
                            expanded.Add("@id", expandedValue);
                        }
                        else if (activePropertyTermDefinition.Type == "@vocab")
                        {
                            expanded.Add("@id", original);
                        }
                        else
                        {
                            expanded.Add("@value", original);

                            if (activePropertyTermDefinition.Type != null)
                            {
                                expanded.Add("@type", activePropertyTermDefinition.Type);
                            }
                            if (activePropertyTermDefinition.Language != null)
                            {
                                expanded.Add("@language", activePropertyTermDefinition.Language);
                            }
                        }

                        return expanded;
                    }
                }
            }
        }
    }
}
