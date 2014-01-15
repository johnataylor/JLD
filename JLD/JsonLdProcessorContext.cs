using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF;

namespace JLD
{
    class JsonLdProcessorContext
    {
        Stack<Scope> _context;

        public JsonLdProcessorContext()
        {
            _context = new Stack<Scope>();
        }

        public void Push(JObject doc)
        {
            Scope currentScope = new Scope();

            JToken t;
            if (doc.TryGetValue("@context", out t))
            {
                JObject context = (JObject)t;
                foreach (JProperty prop in context.Properties())
                {
                    if (prop.Value.Type == JTokenType.Object)
                    {
                        currentScope.Add(prop.Name, new TermDefinition((JObject)prop.Value));
                    }
                    else
                    {
                        string value = prop.Value.ToString();

                        switch (prop.Name)
                        {
                            case "@base": 
                                currentScope.Base = value;
                                break;
                            case "@vocab":
                                currentScope.Vocab = value;
                                break;
                            case "@language":
                                currentScope.Language = value;
                                break;
                            default:
                                switch (value)
                                {
                                    case "@id":
                                    case "@type":
                                        //TODO: other keywords
                                        currentScope.AddAlias(prop.Name, value);
                                        break;
                                    default:
                                        currentScope.Add(prop.Name, new TermDefinition(value));
                                        break;
                                }
                                break;
                        }
                    }
                }

                currentScope.Expand(this);
            }

            _context.Push(currentScope);
        }

        public void Pop()
        {
            _context.Pop();
        }

        public bool TryLookUp(string name, out TermDefinition termDef)
        {
            foreach (Scope scope in _context)
            {
                if (scope.TryLookUp(name, out termDef))
                {
                    return true;
                }
            }

            termDef = null;
            return false;
        }

        public bool TryLookUpPrefix(string name, out TermDefinition termDef)
        {
            int pos = name.IndexOf(':');
            if (pos >= 0)
            {
                string prefix = name.Substring(0, pos);
                foreach (Scope scope in _context)
                {
                    if (scope.TryLookUp(prefix, out termDef))
                    {
                        termDef = new TermDefinition(termDef.Id + name.Substring(pos + 1));
                        return true;
                    }
                }
            }

            termDef = null;
            return false;
        }

        public bool IsEmpty
        {
            get { return _context.Count == 0; }
        }

        public string ExpandValue(string original)
        {
            int pos = original.IndexOf(':');
            if (pos >= 0)
            {
                string prefix = original.Substring(0, pos);
                TermDefinition termDef;
                if (TryLookUp(prefix, out termDef))
                {
                    return termDef.Id + original.Substring(pos + 1);
                }
            }
            else
            {
                TermDefinition termDef;
                if (TryLookUp(original, out termDef))
                {
                    return termDef.Id;
                }
            }
            return original;
        }

        public string Alias(string name)
        {
            string keyword;
            foreach (Scope scope in _context)
            {
                if (scope.TryLookUpAlias(name, out keyword))
                {
                    return keyword;
                }
            }
            return name;
        }

        class Scope
        {
            IDictionary<string, TermDefinition> _props;
            IDictionary<string, string> _alias;

            public Scope()
            {
                _props = new Dictionary<string, TermDefinition>();
                _alias = new Dictionary<string, string>();
            }
            public string Base
            {
                get;
                set;
            }
            public string Vocab
            {
                get;
                set;
            }
            public string Language
            {
                get;
                set;
            }
            public void Add(string name, TermDefinition value)
            {
                _props.Add(name, value);
            }
            public bool TryLookUp(string name, out TermDefinition value)
            {
                return _props.TryGetValue(name, out value);
            }

            public void Expand(JsonLdProcessorContext activeContext)
            {
                IDictionary<string, TermDefinition> expanded = new Dictionary<string, TermDefinition>();
                foreach (KeyValuePair<string, TermDefinition> prop in _props)
                {
                    TermDefinition expandedTermDefinition = new TermDefinition(prop.Value);

                    if (expandedTermDefinition.Type != null)
                    {
                        expandedTermDefinition.Type = activeContext.ExpandValue(ExpandValue(expandedTermDefinition.Type));
                    }

                    if (expandedTermDefinition.Id != null)
                    {
                        expandedTermDefinition.Id = activeContext.ExpandValue(ExpandValue(expandedTermDefinition.Id));
                    }

                    expanded.Add(prop.Key, expandedTermDefinition);
                }
                _props = expanded;
            }

            private string ExpandValue(string original)
            {
                int pos = original.IndexOf(':');
                if (pos >= 0)
                {
                    string prefix = original.Substring(0, pos);
                    TermDefinition termDef;
                    if (TryLookUp(prefix, out termDef))
                    {
                        return termDef.Id + original.Substring(pos + 1);
                    }
                }
                return original;
            }

            public void AddAlias(string name, string keyword)
            {
                _alias.Add(name, keyword);
            }

            public bool TryLookUpAlias(string name, out string keyword)
            {
                return _alias.TryGetValue(name, out keyword);
            }
        }
    }
}
