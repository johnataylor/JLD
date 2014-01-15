using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF;

namespace JLD
{
    class JsonLd2Graph
    {
        public static void Load(IGraph output, JObject flattened)
        {
            JObject context = (JObject)flattened["@context"];

            IDictionary<string, string> types = new Dictionary<string, string>();

            foreach (JProperty term in context.Properties())
            {
                if (term.Value.Type == JTokenType.Object)
                {
                    types.Add(term.Name, (((JObject)term.Value)["@type"]).ToString());
                }
                else
                {
                    output.NamespaceMap.AddNamespace(term.Name, new Uri(term.Value.ToString()));
                }
            }

            JArray graph = (JArray)flattened["@graph"];

            foreach (JObject item in graph)
            {
                string id = item["@id"].ToString();

                INode s = id.StartsWith("_:")
                    ?
                    (INode)output.CreateBlankNode(id.Substring(2)) : (INode)output.CreateUriNode(new Uri(id));

                foreach (JProperty prop in ((JObject)item).Properties())
                {
                    if (prop.Name == "@id")
                    {
                        continue;
                    }

                    if (prop.Name == "@type")
                    {
                        INode p = output.CreateUriNode(new Uri("http://www.w3.org/1999/02/22-rdf-syntax-ns#type"));

                        if (prop.Value.Type == JTokenType.Array)
                        {
                            foreach (JToken type in (JArray)prop.Value)
                            {
                                INode o = output.CreateUriNode(type.ToString());
                                output.Assert(s, p, o);
                            }
                        }
                        else
                        {
                            INode o = output.CreateUriNode(prop.Value.ToString());
                            output.Assert(s, p, o);
                        }
                    }
                    else
                    {
                        INode p = output.CreateUriNode(prop.Name);

                        if (prop.Value.Type == JTokenType.Object)
                        {
                            string pid = ((JObject)prop.Value)["@id"].ToString();

                            INode o = pid.StartsWith("_:")
                                ?
                                (INode)output.CreateBlankNode(pid.Substring(2)) : (INode)output.CreateUriNode(new Uri(pid));

                            output.Assert(s, p, o);
                        }
                        else
                        {
                            string type = "string";
                            types.TryGetValue(prop.Name, out type);

                            INode o;
                            if (type == "@id")
                            {
                                string pv = prop.Value.ToString();

                                o = pv.StartsWith("http:") ? output.CreateUriNode(new Uri(pv)) : output.CreateUriNode(pv);
                            }
                            else
                            {
                                o = output.CreateLiteralNode(prop.Value.ToString());
                            }

                            output.Assert(s, p, o);
                        }
                    }
                }
            }
        }
    }
}
