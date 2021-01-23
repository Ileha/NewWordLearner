using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NewWordLearner.Data
{
    [Serializable]
    public class Word
    {
        [JsonProperty("Word")]
        public string ForeignWord;
        [JsonProperty("Value")]
        public string Translate;

        [JsonProperty("Rate")]
        private float _learningRateShadow = 0;
        [JsonIgnore]
        public float LearningRate => 1.0f / (_learningRateShadow + 1.0f);

        [JsonConstructor]
        public Word() 
        {
        
        }

        public Word(string foreignWord, string translate) 
        {
            this.ForeignWord = foreignWord;
            this.Translate = translate;
        }

        public void IncreaseLearningRate()
        {
            _learningRateShadow++;
        }
        public void DropLearningRate()
        {
            _learningRateShadow = 0;
        }

        public override bool Equals(object obj) 
        {
            if (obj == null) { return false; }
            if (!(obj is Word)) { return false; }
            Word other = obj as Word;
            if (other == this) { return true; }
            return other.ForeignWord == ForeignWord;
        }

        public override int GetHashCode()
        {
            return ForeignWord.GetHashCode();
        }

        public override string ToString()
        {
            return $"{ForeignWord}\t{Translate}\t{_learningRateShadow}";
        }
    }
}
