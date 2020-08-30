using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewWordLearner.Data;

namespace WordLearner.Translate
{
    public interface ITranslator
    {
        Task<string> Translate(String word, Project project);
    }
}
