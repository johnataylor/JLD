using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF;
using VDS.RDF.Writing;

namespace JLD
{
    class Program
    {
        static JToken Load(string filename)
        {
            JToken json;
            using (TextReader reader = new StreamReader(filename))
            {
                using (JsonReader jsonReader = new JsonTextReader(reader))
                {
                    json = JToken.Load(jsonReader);
                }
            }
            return json;
        }

        static void Test0()
        {
        }

        static void Test1()
        {
        }

        static void Test2()
        {
        }

        static void Test3()
        {
        }

        static void Test4()
        {
        }

        static void Test5()
        {
            Console.WriteLine(JsonLdProcessor.Expand((JObject)Load("Test7.json")));
        }

        static void Test6()
        {
            JObject json = (JObject)Load("Test6.json");

            IGraph graph = new Graph();
            JsonLd2Graph.Load(graph, json);

            CompressingTurtleWriter turtle = new CompressingTurtleWriter();
            turtle.CompressionLevel = 0;
            turtle.Save(graph, Console.Out);
        }

        static void Test7()
        {
            Console.WriteLine(JsonLdProcessor.Expand((JObject)Load("Demos\\person.json")));
            Console.WriteLine("-------- -------- -------- -------- --------");
            Console.WriteLine(JsonLdProcessor.Expand((JObject)Load("Demos\\event.json")));
            Console.WriteLine("-------- -------- -------- -------- --------");
            Console.WriteLine(JsonLdProcessor.Expand((JObject)Load("Demos\\place.json")));
            Console.WriteLine("-------- -------- -------- -------- --------");
            Console.WriteLine(JsonLdProcessor.Expand((JObject)Load("Demos\\product.json")));
            Console.WriteLine("-------- -------- -------- -------- --------");
            Console.WriteLine(JsonLdProcessor.Expand((JObject)Load("Demos\\recipe.json")));
            Console.WriteLine("-------- -------- -------- -------- --------");
            Console.WriteLine(JsonLdProcessor.Expand((JObject)Load("Demos\\library.json")));
            Console.WriteLine("-------- -------- -------- -------- --------");

            //  @index @container @list @set
            //  (a) expanded (b) flattened
        }

        static void Test8()
        {
            //Console.WriteLine(JsonLdProcessor.Expand((JObject)Load("Tests\\basic.json")));
            //Console.WriteLine("-------- -------- -------- -------- --------");
            //Console.WriteLine(JsonLdProcessor.Expand((JObject)Load("Tests\\basicWithContext.json")));
            //Console.WriteLine("-------- -------- -------- -------- --------");
            //Console.WriteLine(JsonLdProcessor.Expand((JObject)Load("Tests\\blankNodes.json")));
            //Console.WriteLine("-------- -------- -------- -------- --------");
            //Console.WriteLine(JsonLdProcessor.Expand((JObject)Load("Tests\\alias.json")));
            //Console.WriteLine("-------- -------- -------- -------- --------");
            //Console.WriteLine(JsonLdProcessor.Expand((JObject)Load("Tests\\list.json")));
            //Console.WriteLine("-------- -------- -------- -------- --------");
            //Console.WriteLine(JsonLdProcessor.Expand((JObject)Load("Tests\\namedlist.json")));
            //Console.WriteLine("-------- -------- -------- -------- --------");
            //Console.WriteLine(JsonLdProcessor.Expand((JObject)Load("Tests\\namedset.json")));
            //Console.WriteLine("-------- -------- -------- -------- --------");
            //Console.WriteLine(JsonLdProcessor.Expand((JObject)Load("Tests\\reverse.json")));
            //Console.WriteLine("-------- -------- -------- -------- --------");
            //Console.WriteLine(JsonLdProcessor.Expand((JObject)Load("Tests\\namedreverse.json")));
            //Console.WriteLine("-------- -------- -------- -------- --------");
            Console.WriteLine(JsonLdProcessor.Expand((JObject)Load("Tests\\namedgraph.json")));
            Console.WriteLine("-------- -------- -------- -------- --------");
        }

        static void Main(string[] args)
        {
            try
            {
                //Test0();
                //Test1();
                //Test2();
                //Test3();
                //Test4();
                //Test5();
                //Test6();

                //Test7();
                Test8();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
