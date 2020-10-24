using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using NewWordLearner.ResourceProvider;

namespace NewWordLearner.ImageController.ImageProviders
{
    #region Exception

    public abstract class IImageProviderExceptions : Exception
    {
        public IImageProviderExceptions(string message) : base(message)
        {
        }
    }
    
    public class ImageNotFound : IImageProviderExceptions
    {
        public ImageNotFound(string word) : base($"image for {word} hasn't found")
        {
            
        }
    }

    #endregion

    public abstract class IImageProvider : IResourceProvider<IBitmap, string>
    {
        public class AgregatorImageProvider : IImageProvider
        {
            private IImageProvider[] _providers;

            public AgregatorImageProvider(params IImageProvider[] providers)
            {
                _providers = providers;
            }

            protected override async Task<Stream> GetDataStream(string word)
            {
                foreach (IImageProvider provider in _providers)
                {
                    try
                    {
                        return await provider.GetDataStream(word);
                    }
                    catch (IImageProviderExceptions e)
                    {

                    }
                }
                throw new ImageNotFound(word);
            }
        }
        
        public async Task<IBitmap> DownloadFromInternet(string word)
        {
            using (Stream dataStream = await GetDataStream(word))
            {
                using (FileStream fs = new FileStream(Word2Path(word), FileMode.OpenOrCreate))
                {
                    await dataStream.CopyToAsync(fs);
                }
            }

            return await GetFromDisk(word);
        }

        public Task<IBitmap> GetFromDisk(string word)
        {
            return Task.FromResult<IBitmap>(new Bitmap(Word2Path(word)));
        }
        
        public bool ContainsOnDisk(string word)
        {
            return File.Exists(Word2Path(word));
        }

        Regex _regex = new Regex(@"[/]");
        private string Word2Path(string word)
        {
            return Path.Combine(App.ImageStorePath, _regex.Replace(word, "_"/*@"\$0"*/));
        }

        protected abstract Task<Stream> GetDataStream(string word);
    }
}