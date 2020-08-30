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

namespace NewWordLearner.Data
{
    [Serializable]
    public class Project
    {
        public String Name { get; private set; }
        public Language TargetLanguage { get; private set; }
        public Language YourLanguage { get; private set; }
        public IEnumerable<Word> AllWords => _words;
        public int WordCount => _words.Count;
        [DataMember]
        private HashSet<Word> _words = new HashSet<Word>();
        [NonSerialized]
        private Roullete<Word> _roullete = default;

        public Project(String name, Language tLanguage, Language yLanguage)
        {
            this.Name = name;
            TargetLanguage = tLanguage;
            YourLanguage = yLanguage;
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
        private static BinaryFormatter _formatter = new BinaryFormatter();
        
        public static Task<Project> LoadProject(string path)
        {
            return Task.Run(() =>
            {
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    return (Project)_formatter.Deserialize(fs);
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
                        FileMode.OpenOrCreate))
                {
                    _formatter.Serialize(fs, project);
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
