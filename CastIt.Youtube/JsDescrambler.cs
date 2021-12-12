using CastIt.Domain.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CastIt.Youtube
{
    internal class JsDescrambler
    {
        private readonly ILogger _logger;

        public JsDescrambler(ILogger logger)
        {
            _logger = logger;
        }

        public string JsDescramble(string s, string js)
        {
            //Look for the descrambler function's name (in this example its "pt")
            //if (k.s) { var l=k.sp, m=pt(decodeURIComponent(k.s)); f.set(l, encodeURIComponent(m))}
            //k.s(from stream map field "s") holds the input scrambled signature
            //k.sp(from stream map field "sp") holds a parameter name(normally
            //"signature" or "sig") to set with the output, descrambled signature
            string descramblerPattern = @"(?<=[,&|]).(=).+(?=\(decodeURIComponent)";
            var descramblerMatch = Regex.Match(js, descramblerPattern);
            if (string.IsNullOrEmpty(descramblerMatch.Value))
            {
                _logger.LogInformation($"{nameof(JsDescramble)}: Coudln't retrieve the descrambler function");
                return s;
            }

            string descrambler = descramblerMatch.Value.Substring(descramblerMatch.Value.IndexOf("=", StringComparison.Ordinal) + 1);
            if (SpecialCharsExists(descrambler))
            {
                _logger.LogInformation($"{nameof(JsDescramble)}: Descrambler = {descrambler} contains special chars, escaping the first one");
                descrambler = @$"\{descrambler}";
            }

            //Fetch the code of the descrambler function
            //Go = function(a){ a = a.split(""); Fo.sH(a, 2); Fo.TU(a, 28); Fo.TU(a, 44); Fo.TU(a, 26); Fo.TU(a, 40); Fo.TU(a, 64); Fo.TR(a, 26); Fo.sH(a, 1); return a.join("")};
            string rulesPattern = $"{descrambler}=function.+(?=;)";
            _logger.LogInformation($"{nameof(JsDescramble)}: Getting the rules by using the following pattern = {rulesPattern}");

            string rules = Regex.Match(js, rulesPattern).Value;
            if (string.IsNullOrEmpty(rules))
            {
                _logger.LogInformation($"{nameof(JsDescramble)}: Couldn't retrieve the rules function");
                return s;
            }

            //Get the name of the helper object providing transformation definitions
            string helperPattern = @"(?<=;).*?(?=\.)";
            var helperMatch = Regex.Match(rules, helperPattern);
            if (string.IsNullOrEmpty(helperMatch.Value))
            {
                _logger.LogInformation($"{nameof(JsDescramble)}: Couldn't retrieve signature transformation helper name");
                return s;
            }

            string helper = Regex.Split(helperMatch.Value, @"\W+")
                .GroupBy(g => g)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(helper))
            {
                _logger.LogInformation($"{nameof(JsDescramble)}: Couldn't retrieve signature transformation helper name from = {helperMatch.Value}");
                return s;
            }

            //Fetch the helper object code
            //var Fo ={ TR: function(a){ a.reverse()},TU: function(a, b){ var c = a[0]; a[0] = a[b % a.length]; a[b] = c},sH: function(a, b){ a.splice(0, b)} };
            string transformationsPattern = "var " + helper + @"=(.*?)+\n.+\n.+(?=};)";
            string transformations = Regex.Match(js, transformationsPattern).Value;

            //Parse the helper object to map available transformations
            var methods = Regex.Matches(transformations, "(..):.*?(?=})");
            var trans = new Dictionary<string, string>();

            foreach (var method in methods)
            {
                string m = method.ToString();
                string methodName = m.Split(":".ToCharArray())[0];
                if (m.Contains(".reverse("))
                {
                    trans.Add(methodName, "reverse");
                }
                else if (m.Contains(".splice("))
                {
                    trans.Add(methodName, "slice");
                }
                else if (m.Contains("var c="))
                {
                    trans.Add(methodName, "swap");
                }
            }

            //Parse descrambling rules, map them to known transformations
            //and apply them on the signature
            var rulesToApply = Regex.Matches(rules, helper + @"\..*?(?=;)");
            var commaSeparator = new[] { ',' };
            foreach (var rule in rulesToApply)
            {
                //zw.sH(a,8)
                string x = rule.ToString();

                //sH(a, 2)
                string transToApply = x.Split(".".ToCharArray()).Last();

                //sH
                string transName = transToApply.Substring(0, transToApply.IndexOf("(", StringComparison.Ordinal));
                switch (trans[transName])
                {
                    case "reverse":
                        s = new string(s.Reverse().ToArray());
                        break;
                    case "slice":
                        {
                            int value = int.Parse(transToApply.Split(commaSeparator).Last().Replace(")", ""));
                            s = s.Substring(value);
                            break;
                        }
                    case "swap":
                        {
                            int value = int.Parse(transToApply.Split(commaSeparator).Last().Replace(")", ""));
                            var c = s[0];
                            s = s.ReplaceAt(0, s[value % s.Length]);
                            s = s.ReplaceAt(value % s.Length, c);
                            break;
                        }
                }
            }

            return s;
        }

        private bool SpecialCharsExists(string input)
        {
            char[] one = input.ToCharArray();
            char[] two = new char[one.Length];
            int c = 0;
            foreach (var t in one)
            {
                if (char.IsLetterOrDigit(t))
                    continue;
                two[c] = t;
                c++;
            }

            Array.Resize(ref two, c);
            return two.Length > 0;
        }
    }
}
