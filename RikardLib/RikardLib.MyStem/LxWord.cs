using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace RikardLib.MyStem
{
    public class LxWord
    {
        [JsonProperty("analysis")]
        public List<Lexeme> Lexemes { get; set; }
        
        [JsonProperty("text")]
        public string InputWord { get; set; }
    }

    public class Lexeme
    {
        private string grammarsRaw;

        [JsonProperty("lex")]
        public string BaseForm { get; set; }

        [JsonProperty("gr")]
        public string GrammarsRaw
        {
            get
            {
                return grammarsRaw;
            }

            set
            {
                grammarsRaw = value;

                SetGrammar(grammarsRaw);
            }
        }

        [JsonIgnore]
        public Grammar Grammar { get; private set; }

        [JsonProperty("wt")]
        public double Weight { get; set; }

        [JsonProperty("qual")]
        public string Quality { get; set; }

        private void SetGrammar(string raw)
        {
            var generalGr = raw.Split('=');
            var grs = generalGr[0].Split(',');
            Grammar = new Grammar();
            if (Enum.TryParse<POS>(grs[0], out POS pos))
            {
                Grammar.POS = pos;
            }
            for(int i = 1; i < grs.Length; i++)
            {
                if(Grammar.AddTps == AddTps.None)
                {
                    if (Enum.TryParse<AddTps>(grs[i], true, out AddTps addTps))
                    {
                        Grammar.AddTps = addTps;
                        break;
                    }
                }
            }
        }
    }

    public class Grammar
    {
        public POS POS { get; set; }  = POS.None;
        public AddTps AddTps { get; set; } = AddTps.None;
    }

    public enum POS
    {
        A,
        ADV,
        ADVPRO,
        ANUM,
        APRO,
        COM,
        CONJ,
        INTJ,
        NUM,
        PART,
        PR,
        S,
        SPRO,
        V,
        None
    }

    public enum AddTps
    {
        PARENTH,
        GEO,
        AWKW,
        PERSN,
        DIST,
        MF,
        OBSC,
        PATRN,
        PRAED,
        INFORM,
        RARE,
        OBSOL,
        FAMN,
        None
    }
}
