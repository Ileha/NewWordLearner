using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using NewWordLearner.Extentions;

namespace NewWordLearner.ImageController.ImageProviders.Instances
{
    public class GoogleImageProvider : IImageProvider
    {
        //https://www.google.ru/search?tbm=isch&q=to+fall+back+on
        public const string URL = "https://www.google.ru/search?tbm={0}&q={1}";
        private const string TBM = "isch";
        
        protected override async Task<Stream> GetDataStream(string word)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(String.Format(URL, TBM, PrepareWord(word)));
            HttpWebResponse response = (await request.GetResponseAsync()) as HttpWebResponse;
            
            HtmlDocument htmlDocument = new HtmlDocument();
            using (Stream dataStream = response.GetResponseStream())
            {
                htmlDocument.Load(dataStream);
            }
            response.Close();
            
            if (htmlDocument.ParseErrors != null && htmlDocument.ParseErrors.Count() > 0 || htmlDocument.DocumentNode == null)
            {
                throw new ImageNotFound(word);
            }
            HtmlNode body = htmlDocument.DocumentNode
                .SelectSingleNode("//body").ChildNodes.Where(x => x.Name == "div")
                .Skip(2).First();

            var _data = body
                .SelectNodes(".//img[@alt and @class and @src]")
                .SelectMany(img => img.Attributes.AttributesWithName("src"))
                .Select(src => src.Value)
                .Where(src => _isHttp.IsMatch(src))
                .Shuffle()
                .ToArray();

            if (_data.Length == 0)
            {
                throw new ImageNotFound(word);
            }

            request = (HttpWebRequest)WebRequest.Create(_data[0]);
            return ((await request.GetResponseAsync()) as HttpWebResponse).GetResponseStream();
        }

        Regex _regex = new Regex(@"[ ]+");
        Regex _isHttp = new Regex(@"^https:\/\/");
        private string PrepareWord(string word)
        {
            return _regex.Replace(word.Trim(), "+");
        }
    }
}