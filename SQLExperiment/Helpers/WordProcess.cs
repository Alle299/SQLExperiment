using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLExperiment.Helpers
{
    public static class WordProcess
    {
        public static string Capitalize(string word)
        {
            return word.Substring(0,1).ToUpper() + word.Substring(1,word.Length-1).ToLower();
        }
    }
}
