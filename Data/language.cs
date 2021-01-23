using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NewWordLearner.Data
{
    [Serializable]
    public class Language
    {
        [JsonIgnore]
        public string LanguageTitle => _languageTitle;
        [JsonIgnore]
        public string Code => _code;

        [JsonProperty("LanguageTitle")]
        private string _languageTitle;
        [JsonProperty("Code")]
        private string _code;

        [JsonConstructor]
        private Language()
        {
        }

        public Language(string title, string code) {
            _languageTitle = title;
            _code = code;
        }

        public override string ToString()
        {
            return LanguageTitle;
        }
    }
}
