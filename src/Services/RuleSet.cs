using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using LinkGrab.Constants;

namespace LinkGrab.Services
{
    public class Rule
    {
        public Rule()
        {
            Attrs = new string[0];
            Render = RuleKeys.RenderAuto;
        }

        public string Name { get; set; }
        public Regex Match { get; set; }
        public string Element { get; set; }
        public string[] Attrs { get; set; }
        public string Render { get; set; }

        public bool IsComplete { get { return Match != null && Element != null && Attrs.Length > 0; } }
    }

    // Reads rules.ini, the file that says where each site keeps its download link.
    public static class RuleSet
    {
        public static Rule For(string url)
        {
            return Load(AppFiles.Path(AppFileNames.Rules)).FirstOrDefault(r => r.Match.IsMatch(url));
        }

        static List<Rule> Load(string path)
        {
            if (!File.Exists(path)) throw new Exception(string.Format(Strings.ErrorMissingFile, path));

            var rules = new List<Rule>();
            Rule rule = null;
            foreach (string raw in File.ReadAllLines(path))
            {
                string line = raw.Trim();
                if (line.Length == 0 || line[0] == RuleKeys.Comment) continue;

                if (line[0] == '[' && line[line.Length - 1] == ']')
                {
                    rule = new Rule { Name = line.Substring(1, line.Length - 2) };
                    rules.Add(rule);
                    continue;
                }

                int separator = line.IndexOf('=');
                if (separator < 0 || rule == null) throw new Exception(string.Format(Strings.ErrorBadRuleLine, line));
                Assign(rule, line.Substring(0, separator).Trim().ToLowerInvariant(), line.Substring(separator + 1).Trim());
            }

            Rule incomplete = rules.FirstOrDefault(r => !r.IsComplete);
            if (incomplete != null) throw new Exception(string.Format(Strings.ErrorIncompleteRule, incomplete.Name));
            return rules;
        }

        static void Assign(Rule rule, string key, string value)
        {
            switch (key)
            {
                case RuleKeys.Match: rule.Match = new Regex(value, RegexOptions.IgnoreCase); break;
                case RuleKeys.Element: rule.Element = value; break;
                case RuleKeys.Attr: rule.Attrs = value.Split(',').Select(a => a.Trim()).ToArray(); break;
                case RuleKeys.Render: rule.Render = value.ToLowerInvariant(); break;
                default: throw new Exception(string.Format(Strings.ErrorUnknownRuleKey, key));
            }
        }
    }
}
