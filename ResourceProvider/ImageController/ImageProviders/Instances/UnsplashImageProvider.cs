using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;

namespace NewWordLearner.ImageController.ImageProviders.Instances
{
    #region Exceptions

    public class UnsplashKeyNotFound : IImageProviderExceptions
    {
        public UnsplashKeyNotFound() : base($"Unsplash API key not found")
        {
            
        }
    }

    #endregion
    
    public class UnsplashImageProvider : IImageProvider
    {
        [Serializable]
        internal class ImageApiData
        {
            [Serializable]
            internal class Result
            {
                [Serializable]
                internal class Urls
                {
                    // public string raw;
                    // public string full;
                    // public string regular;
                    // public string small;
                    public string thumb;
                }
                public Urls urls;
            }

            public Result[] results;
        }

        public static readonly DataContractJsonSerializer JsonFormatter = new DataContractJsonSerializer(typeof(ImageApiData));
        public const string URL = "https://api.unsplash.com/search/photos?query={0}&client_id={1}&per_page={2}";
        public const string CONFIG_KEY = "UnsplashAPIKey";
        private string Key;

        public UnsplashImageProvider()
        {
            Key = App.Instance.GetConfigValue(CONFIG_KEY);
        }

        protected override async Task<Stream> GetDataStream(string word)
        {
            if (string.IsNullOrEmpty(Key))
            {
                throw new UnsplashKeyNotFound();
            }

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(String.Format(URL, word, Key, 1));
            HttpWebResponse response = (await request.GetResponseAsync()) as HttpWebResponse;
            string requestURL;
            using (Stream dataStream = response.GetResponseStream())
            {
                ImageApiData data = (JsonFormatter.ReadObject(dataStream) as ImageApiData);
                if (data.results.Length == 0)
                {
                    throw new ImageNotFound(word);
                }
                requestURL = data.results[0].urls.thumb;
            }
            response.Close();
            
            request = (HttpWebRequest)WebRequest.Create(requestURL);
            return ((await request.GetResponseAsync()) as HttpWebResponse).GetResponseStream();
        }
    }
}