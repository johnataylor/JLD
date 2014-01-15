using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JLD
{
    public class TermDefinition
    {
        public TermDefinition()
        {
        }
        public TermDefinition(TermDefinition termDefinitionToBeCloned)
        {
            Id = termDefinitionToBeCloned.Id;
            Type = termDefinitionToBeCloned.Type;
            Language = termDefinitionToBeCloned.Language;
            Container = termDefinitionToBeCloned.Container;
            Reverse = termDefinitionToBeCloned.Reverse;
        }
        public TermDefinition(string id)
        {
            Id = id;
        }
        public TermDefinition(JObject obj)
        {
            JToken id;
            if (obj.TryGetValue("@id", out id))
            {
                Id = id.ToString();
            }

            JToken type;
            if (obj.TryGetValue("@type", out type))
            {
                Type = type.ToString();
            }

            JToken language;
            if (obj.TryGetValue("@language", out language))
            {
                Language = language.ToString();
            }

            JToken container;
            if (obj.TryGetValue("@container", out container))
            {
                Container = container.ToString();
            }

            JToken reverse;
            if (obj.TryGetValue("@reverse", out reverse))
            {
                Reverse = reverse.ToString();
            }
        }

        public string Id
        {
            get;
            set;
        }
        public string Type
        {
            get;
            set;
        }
        public string Language
        {
            get;
            set;
        }
        public string Container
        {
            get;
            set;
        }
        public string Reverse
        {
            get;
            set;
        }
    }
}
