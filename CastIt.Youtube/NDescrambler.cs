using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CastIt.Youtube
{
    internal enum TransformationType
    {
        Function,
        Integer,
        String,
        NParameter,
        Null
    }

    internal class NDescrambler
    {
        private const string ReverseTransform = "reverse";
        private const string AppendTransform = "append";
        private const string RemoveTransform = "remove";
        private const string SwapTransform = "swap";
        private const string RotateTransform = "rotate";
        private const string CompoundTransform = "compound";
        private const string Compound1Transform = "compound1";
        private const string Compound2Transform = "compound2";

        //function() { for (var d = 64, e =[]; ++d - e.length - 32;) { switch (d) { case 91: d = 44; continue; case 123: d = 65; break; case 65: d -= 18; continue; case 58: d = 96; continue; case 46: d = 95} e.push(String.fromCharCode(d))} return e}
        //"^function%(%){[^}]-case 58:d=96;",
        private const string Alphabetic1 = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ-_";

        //function() { for (var d = 64, e =[]; ++d - e.length - 32;) { switch (d) { case 58: d -= 14; case 91: case 92: case 93: continue; case 123: d = 47; case 94: case 95: case 96: continue; case 46: d = 95} e.push(String.fromCharCode(d))} return e}
        //function() { for (var d = 64, e =[]; ++d - e.length - 32;) switch (d) { case 46: d = 95; default: e.push(String.fromCharCode(d)); case 94: case 95: case 96: break; case 123: d -= 76; case 92: case 93: continue; case 58: d = 44; case 91: } return e}
        //"^function%(%){[^}]-case 58:d%-=14;",
        //"^function%(%){[^}]-case 58:d=44;",
        private const string Alphabetic2 = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";

        private readonly ILogger _logger;

        public NDescrambler(ILogger logger)
        {
            _logger = logger;
        }

        public string NDescramble(string nParam, string js)
        {
            if (string.IsNullOrWhiteSpace(js))
            {
                _logger.LogWarning($"{nameof(NDescramble)}: Js is null so we can't retrieve descramble the nparam");
                return null;
            }

            //Look for the descrambler function's name
            //a.D && (b = a.get("n")) && (b = lha(b), a.set("n", b))}};
            string descrambler = Regex.Match(js, @"[=%(,&|](...?)\(.?.\),.?.(set\(\""n\"")").Value;
            if (string.IsNullOrWhiteSpace(descrambler))
            {
                _logger.LogWarning($"{nameof(NDescramble)}: Could not retrieve the descrambler");
                return null;
            }

            //in this example we got = lha(b), a.set("n" but we need only the name, in this case: lha
            descrambler = descrambler[1..descrambler.IndexOf("(", StringComparison.OrdinalIgnoreCase)];

            //Fetch the code of the descrambler function
            //lha = function(a){ var b = a.split(""), c =[310282131, "KLf3", b, null, function(d, e){ d.push(e)},-45817231, [data and transformations...] ,1248130556]; c[3] = c; c[15] = c; c[18] = c; try { c[40](c[14], c[2]),c[25](c[48]),c[21](c[32], c[23]), [scripted calls...] ,c[25](c[33], c[3])} catch (d) { return "enhanced_except_4ZMBnuz-_w8_" + a} return b.join("")};
            string code = Regex.Match(js, descrambler + "=function.+?(=?};)", RegexOptions.Singleline).Value;
            if (string.IsNullOrWhiteSpace(code))
            {
                _logger.LogWarning($"{nameof(NDescramble)}: Could not retrieve the code of the descrambler function");
                return null;
            }

            //Split code into two main sections:
            //1) data and transformations,
            //2) a script of calls
            string dataC = Regex.Match(code, @"(?=)\[(.+)\](?=;)", RegexOptions.Singleline).Value;
            string script = Regex.Match(code, "try{(.*)(?=catch)", RegexOptions.Singleline).Value;

            if (string.IsNullOrWhiteSpace(dataC) || string.IsNullOrWhiteSpace(script))
            {
                _logger.LogWarning($"{nameof(NDescramble)}: Could not retrieve the dataC or script code");
                return null;
            }

            //remove the square brackets
            dataC = dataC[1..];
            dataC = dataC[..^1];
            return GetFinalN(nParam, script, dataC);
        }

        private string GetFinalN(string nParam, string script, string dataC)
        {
            string[] dataCValues = GetTransformations(dataC);
            return GetFinalN(nParam, script, dataCValues.ToList());
        }

        private string GetFinalN(string nParam, string script, List<string> dataC)
        {
            _logger.LogInformation($"{nameof(GetFinalN)}: Retrieving the final n... Initial nParam = {nParam}");
            //Split "n" parameter into a table as descrambling operates on it
            //as one of several arrays
            var n = Regex.Matches(nParam, ".").Select(m => m.Value).ToList();

            var toEvaluate = Regex.Matches(script, @"c\[\d+\]\((.*?)\)", RegexOptions.Singleline)
                .Select(m => m.Value.Replace("\n", string.Empty))
                .ToList();

            //c[42](c[8], c[2]), c[42](c[17], c[44]), c[0](c[22], c[11], c[17]()),
            for (var index1 = 0; index1 < toEvaluate.Count; index1++)
            {
                string function = toEvaluate[index1];
                //var matches = Regex.Matches(function, @"c\[(\d+)\]");
                var matches = Regex.Matches(function, @"(\d+)");
                if (!matches.Any())
                {
                    throw new Exception("We should have something here");
                }

                int funcIndex = -1;
                var funcParamIndex = new List<int>();

                for (int i = 0; i < matches.Count; i++)
                {
                    int index = int.Parse(matches[i].Value);
                    if (i == 0)
                    {
                        funcIndex = index;
                        continue;
                    }

                    funcParamIndex.Add(index);
                }

                if (funcIndex < 0)
                {
                    throw new Exception("Could not retrieve the func index");
                }

                if (funcIndex > dataC.Count - 1)
                {
                    throw new Exception("FuncIndex is out of range");
                }

                if (funcParamIndex.Count == 0)
                {
                    throw new Exception("At least one param must be provided");
                }

                ApplyFunction(funcIndex, funcParamIndex, dataC, n);
            }

            string finalN = string.Concat(n);
            _logger.LogInformation($"{nameof(GetFinalN)}: The final n is = {finalN}");
            return finalN;
        }

        private void ApplyFunction(int funcIndex, List<int> funcParamIndex, List<string> dataC, List<string> n)
        {
            string function = dataC[funcIndex];
            var funcParams = new List<object>();
            foreach (int index in funcParamIndex)
            {
                string value = dataC[index];
                var param = GetFunctionParam(value, dataC, n);
                funcParams.Add(param);
            }

            var table = funcParams[0] as List<string>;
            switch (GetTransformationType(function))
            {
                case TransformationType.Function:
                    switch (function)
                    {
                        case ReverseTransform:
                            Reverse(table);
                            break;
                        case AppendTransform:
                            Append(table, funcParams[1].ToString());
                            break;
                        case RemoveTransform:
                            Remove(table, (int)funcParams[1]);
                            break;
                        case SwapTransform:
                            Swap(table, (int)funcParams[1]);
                            break;
                        case RotateTransform:
                            Rotate(table, (int)funcParams[1]);
                            break;
                        case CompoundTransform:
                            Compound(table, funcParams[1].ToString(), funcParams[2].ToString());
                            break;
                        case Compound1Transform:
                            Compound1(table, funcParams[1].ToString());
                            break;
                        case Compound2Transform:
                            Compound2(table, funcParams[1].ToString());
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private object GetFunctionParam(string value, List<string> dataC, List<string> n)
        {
            return GetTransformationType(value) switch
            {
                TransformationType.Function => value switch
                {
                    Compound1Transform => Alphabetic1,
                    Compound2Transform => Alphabetic2,
                    _ => value
                },
                TransformationType.Integer => int.Parse(value),
                TransformationType.String => value,
                TransformationType.NParameter => n,
                TransformationType.Null => dataC,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value)
            };
        }

        private TransformationType GetTransformationType(string val)
        {
            return val switch
            {
                ReverseTransform => TransformationType.Function,
                AppendTransform => TransformationType.Function,
                RemoveTransform => TransformationType.Function,
                SwapTransform => TransformationType.Function,
                RotateTransform => TransformationType.Function,
                CompoundTransform => TransformationType.Function,
                Compound1Transform => TransformationType.Function,
                Compound2Transform => TransformationType.Function,
                "b" => TransformationType.NParameter,
                "null" => TransformationType.Null,
                _ => int.TryParse(val, out _) ? TransformationType.Integer : TransformationType.String
            };
        }

        private string[] GetTransformations(string dataC)
        {
            string updated = dataC;
            CheckForReverseMatch(ref updated);
            CheckForAppendMatch(ref updated);
            CheckForRemoveMatch(ref updated);
            CheckForSwap(ref updated);
            CheckForRotateMatch(ref updated);
            CheckForCompound(ref updated);
            CheckForCompound1(ref updated);
            CheckForCompound2(ref updated);

            string[] items = updated.Split(",");
            for (int i = 0; i < items.Length; i++)
            {
                items[i] = items[i]
                    .Replace("\"", string.Empty)
                    .Replace("\n", string.Empty);
            }
            return items;
        }

        private void CheckForReverseMatch(ref string dataC)
        {
            string pattern = @"function\(d\){.*?(})";
            ReplaceIfNeeded(ReverseTransform, pattern, ref dataC);
        }

        private void CheckForAppendMatch(ref string dataC)
        {
            string pattern = @"function\((.*?)\){(.*?).push\((.)\).*?(})";
            ReplaceIfNeeded(AppendTransform, pattern, ref dataC);
        }

        private void CheckForRemoveMatch(ref string dataC)
        {
            string pattern = @"function\((.*?)\).*?(splice\(.,.\)})";
            ReplaceIfNeeded(RemoveTransform, pattern, ref dataC);
        }

        private void CheckForSwap(ref string dataC)
        {
            string[] possiblePatterns =
            {
                @"function\((.*?)\).*?(var f=).*?(})",
                @"function\((.*?)\).*?(splice\(0,1).*?(})"
            };

            foreach (string pattern in possiblePatterns)
            {
                ReplaceIfNeeded(SwapTransform, pattern, ref dataC);
            }
        }

        private void CheckForRotateMatch(ref string dataC)
        {
            string pattern = @"function\((.*?)\).*?(unshift).*?}(?=,)";
            ReplaceIfNeeded(RotateTransform, pattern, ref dataC);
        }

        private void CheckForCompound(ref string dataC)
        {
            string pattern = @"function\(.,.,.\).*?(\)\)})";
            ReplaceIfNeeded(CompoundTransform, pattern, ref dataC);
        }

        private void CheckForCompound1(ref string dataC)
        {
            string pattern = @"function\((.*?)\).*?(case 58:.=96).*?}(?=,)";
            ReplaceIfNeeded(Compound1Transform, pattern, ref dataC);
        }

        private void CheckForCompound2(ref string dataC)
        {
            string[] possiblePatterns =
            {
                @"function\((.*?)\).*?(case 58:.=44).*?}(?=,)",
                @"function\((.*?)\).*?(case 58:.-=14).*?}(?=,)"
            };
            foreach (string pattern in possiblePatterns)
            {
                ReplaceIfNeeded(Compound2Transform, pattern, ref dataC);
            }
        }

        private void ReplaceIfNeeded(string operation, string pattern, ref string dataC)
        {
            var matches = Regex.Matches(dataC, pattern);
            foreach (Match match in matches)
            {
                if (!string.IsNullOrWhiteSpace(match.Value))
                {
                    dataC = dataC.Replace(match.Value, operation);
                }
            }
        }

        private void Reverse(List<string> table)
        {
            //--function(d){ d.reverse()}
            //--function(d){ for (var e = d.length; e;) d.push(d.splice(--e, 1)[0])}
            //"^function%(d%)"
            table.Reverse();
        }

        private void Append(List<string> table, string val)
        {
            //function(d, e){ d.push(e)}
            //"^function%(d,e%){d%.push%(e%)},",
            table.Add(val);
        }

        private void Remove(List<string> table, int i)
        {
            //function(d, e){ e = (e % d.length + d.length) % d.length; d.splice(e, 1)}
            //"^[^}]-;d%.splice%(e,1%)},",
            int x = (i % table.Count + table.Count) % table.Count;
            table.RemoveAt(x);
        }

        private void Swap(List<string> table, int i)
        {
            //--function(d, e){ e = (e % d.length + d.length) % d.length; var f = d[0]; d[0] = d[e]; d[e] = f}
            //--function(d, e){ e = (e % d.length + d.length) % d.length; d.splice(0, 1, d.splice(e, 1, d[0])[0])}
            //"^[^}]-;var f=d%[0%];d%[0%]=d%[e%];d%[e%]=f},",
            //"^[^}]-;d%.splice%(0,1,d%.splice%(e,1,d%[0%]%)%[0%]%)},",
            int x = (i % table.Count + table.Count) % table.Count;
            string temp = table[0];
            table[0] = table[x];
            table[x] = temp;
        }

        private void Rotate(List<string> table, int shift)
        {
            int newShift = ((shift % table.Count) + table.Count) % table.Count;
            for (int i = newShift; i > 0; i--)
            {
                string value = table.Last();
                table.RemoveAt(table.Count - 1);
                table.Insert(0, value);
            }
            //--function(d, e){ for (e = (e % d.length + d.length) % d.length; e--;) d.unshift(d.pop())}
            //--function(d, e){ e = (e % d.length + d.length) % d.length; d.splice(-e).reverse().forEach(function(f){ d.unshift(f)})}
            //"^[^}]-d%.unshift%(d.pop%(%)%)},",
            //"^[^}]-d%.unshift%(f%)}%)},",
        }

        private void Compound(List<string> nTab, string str, string alphabet)
        {
            int alphabetLength = alphabet.Length;
            List<char> strArray = str.ToCharArray().ToList();

            for (int i = 0; i < nTab.Count; i++)
            {
                string current = nTab[i];
                int part = alphabet.IndexOf(current) - alphabet.IndexOf(strArray[i]) + i + alphabetLength--;
                char letter = alphabet[part % alphabet.Length];
                nTab[i] = letter.ToString();
                strArray.Add(letter);
            }

            //function(d, e, f){ var h = f.length; d.forEach(function(l, m, n){ this.push(n[m] = f[(f.indexOf(l) - f.indexOf(this[m]) + m + h--) % f.length])},e.split(""))}
            //"^function%(d,e,f%)",
        }

        private void Compound1(List<string> nTab, string str)
        {
            Compound(nTab, str, Alphabetic1);
            //--function(d, e){ for (var f = 64, h =[]; ++f - h.length - 32;) switch (f) { case 58: f = 96; continue; case 91: f = 44; break; case 65: f = 47; continue; case 46: f = 153; case 123: f -= 58; default: h.push(String.fromCharCode(f))}[compound... ] }
            //"^function%(d,e%){[^}]-case 58:f=96;",
        }

        private void Compound2(List<string> nTab, string str)
        {
            Compound(nTab, str, Alphabetic2);
            //--function(d, e){ for (var f = 64, h =[]; ++f - h.length - 32;) { switch (f) { case 58: f -= 14; case 91: case 92: case 93: continue; case 123: f = 47; case 94: case 95: case 96: continue; case 46: f = 95} h.push(String.fromCharCode(f))}[compound... ] }
            //--function(d, e){ for (var f = 64, h =[]; ++f - h.length - 32;) switch (f) { case 46: f = 95; default: h.push(String.fromCharCode(f)); case 94: case 95: case 96: break; case 123: f -= 76; case 92: case 93: continue; case 58: f = 44; case 91: }[compound... ] }
            //"^function%(d,e%){[^}]-case 58:f%-=14;",
            //"^function%(d,e%){[^}]-case 58:f=44;",
        }
    }
}
