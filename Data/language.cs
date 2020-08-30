using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewWordLearner.Data
{
    [Serializable]
    public class Language
    {
        public string LanguageTitle { get; private set; }
        public string Code { get; private set; }

        public Language(string title, string code) {
            LanguageTitle = title;
            this.Code = code;
        }

        public override string ToString()
        {
            return LanguageTitle;
        }
    }
}
