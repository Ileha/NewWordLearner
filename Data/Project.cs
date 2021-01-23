using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Assets.Roullete;
using Newtonsoft.Json;

namespace NewWordLearner.Data
{
    [Serializable]
    public class Project
    {
        [JsonIgnore]
        public String Name => _name; 
        [JsonIgnore]
        public Language TargetLanguage => _targetLanguage;
        [JsonIgnore]
        public Language YourLanguage => _yourLanguage;
        [JsonIgnore]
        public IEnumerable<Word> AllWords => _words;
        [JsonIgnore]
        public int WordCount => _words.Count;


        [JsonProperty("Name")]
        private String _name;
        [JsonProperty("TL")] 
        private Language _targetLanguage;
        [JsonProperty("YL")] 
        private Language _yourLanguage;
        [JsonProperty("Data")]
        private HashSet<Word> _words = new HashSet<Word>();
        [JsonIgnore]
        private Roullete<Word> _roullete = default;
        [JsonConstructor]
        private Project()
        {
        }

        public Project(String name, Language tLanguage, Language yLanguage)
        {
            this._name = name;
            _targetLanguage = tLanguage;
            _yourLanguage = yLanguage;
        }

        /// <summary>
        /// return random word
        /// </summary>
        /// <param name="value">data [0; 1]</param>
        /// <returns></returns>
        public Word PickWord(float value)
        {
            ComposeRoullete();
            return _roullete.PickIndividual(value * _roullete.maxValue);
        }

        /// <summary>
        /// use after change word fitness
        /// </summary>
        public void UpdateRoullete()
        {
            if (!ComposeRoullete())
            {
                _roullete.UpdateFitness();
            }
        }

        public bool AddWord(Word word)
        {
            if (ContainsWord(word)) return false;

            ComposeRoullete();
            
            _words.Add(word);
            
            _roullete.Add(word);
            
            return true;
        }

        public bool ContainsWord(Word word)
        {
            return _words.Contains(word);
        }

        private bool ComposeRoullete()
        {
            if (_roullete == default)
            {
                _roullete = new Roullete<Word>(_words, word => word.LearningRate);
                return true;
            }
            return false;
        }

        #region Static
        // private static BinaryFormatter _formatter = new BinaryFormatter();
        
        public static Task<Project> LoadProject(string path)
        {
            return Task.Run(() =>
            {
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(fs))
                    {
                        using (JsonTextReader jsonReader = new JsonTextReader(reader))
                        {
                            JsonSerializer ser = new JsonSerializer();
                            Project result = ser.Deserialize<Project>(jsonReader);
                            return result;
                        }
                    }
                }
            });
        }
        
        public static Task SaveProject(Project project, string path)
        {
            return Task.Run(() =>
            {
                using (FileStream fs =
                    new FileStream(
                        path,
                        FileMode.Create))
                {
                    using (StreamWriter writer = new StreamWriter(fs))
                    {
                        using (JsonTextWriter jsonWriter = new JsonTextWriter(writer))
                        {
                            JsonSerializer ser = new JsonSerializer();
                            ser.Serialize(jsonWriter, project);
                            jsonWriter.Flush();
                        }
                    }
                }
            });
        }
        
        #endregion

        public override string ToString()
        {
            return $"{Name} {YourLanguage} - {TargetLanguage}";
        }
    }
}
