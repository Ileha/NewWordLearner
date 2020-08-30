using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using System.Net;
using System.IO;
using NewWordLearner.Data;

namespace WordLearner.Translate.google
{
    public class GoogleTranslator : ITranslator {
        public static readonly DataContractJsonSerializer JsonFormatter = new DataContractJsonSerializer(typeof(object[]));
        public const string URL = "https://translate.googleapis.com/translate_a/single?client=gtx&sl={0}&tl={1}&dt=t&q={2}";

        public async Task<string> Translate(string word, Project project) 
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(String.Format(URL, project.TargetLanguage.Code, project.YourLanguage.Code, word));
            HttpWebResponse response = (await request.GetResponseAsync()) as HttpWebResponse;
            string responseData;
            using (Stream dataStream = response.GetResponseStream()) 
            {
                responseData = (((JsonFormatter.ReadObject(dataStream) as object[])[0] as object[])[0] as object[])[0] as string;
            }
            response.Close();

            return responseData;
        }
    }
}
