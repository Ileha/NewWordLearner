using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ManagedBass;
using NotImplementedException = System.NotImplementedException;

namespace NewWordLearner.ResourceProvider.SoundsController
{
    public class CustomMediaPlayer : MediaPlayer
    {
        public ChannelInfo GetInfo()
        {
            ChannelInfo info;

            if (Bass.ChannelGetInfo(Handle, out info))
            {
                return info;
            }
            return default(ChannelInfo);
        }
    }
    
    public class WordSoundKey
    {
        public string Word { get; private set; }
        public string Code { get; private set; }

        public WordSoundKey(string word, string code)
        {
            Word = word;
            Code = code;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 23 + Word.GetHashCode();
            hash = hash * 23 + Code.GetHashCode();
            return hash;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) { return false; }
            if (this == obj) { return true; }
            if (!(obj is WordSoundKey)) { return false; }
            WordSoundKey cell = (WordSoundKey)obj;

            return cell.Word == Word && cell.Code == Code;
        }

        private static Regex _regex = new Regex(@"[/]");
        public string ToPath()
        {
            return Path.Combine(App.SoundStorePath, _regex.Replace($"{Word}_{Code}", "_"));
        }
    }

    public abstract class ISoundsProvider : IResourceProvider<CustomMediaPlayer, WordSoundKey>
    {
        public async Task<CustomMediaPlayer> GetFromDisk(WordSoundKey word)
        {
            // return Task.FromResult(new AudioFileReader(word.ToPath()));
            // return Task.FromResult(new SoundStream(File.OpenRead(word.ToPath()), _engine));

            CustomMediaPlayer Player = new CustomMediaPlayer();
            await Player.LoadAsync(word.ToPath()); // Load a file.
            ChannelInfo info = Player.GetInfo();
            Player.Volume = 1f;
            Player.Frequency = info.Frequency;
            return Player;
        }

        public async Task<CustomMediaPlayer> DownloadFromInternet(WordSoundKey word)
        {
            using (Stream dataStream = await GetDataStream(word))
            {
                using (FileStream fs = new FileStream(word.ToPath(), FileMode.OpenOrCreate))
                {
                    await dataStream.CopyToAsync(fs);
                }
            }

            return await GetFromDisk(word);
        }

        public bool ContainsOnDisk(WordSoundKey word)
        {
            return File.Exists(word.ToPath());
        }
        
        Regex _regex = new Regex(@"[/]");
        private string Word2Path(string word)
        {
            return Path.Combine(App.SoundStorePath, _regex.Replace(word, "_"));
        }
        
        protected abstract Task<Stream> GetDataStream(WordSoundKey word);
    }

    public class GoogleSoundProvider : ISoundsProvider
    {
        //https://translate.google.ru/translate_tts?ie=UTF-8&tl=en&client=gtx&q=emphaise line cat
        public const string URL = "https://translate.google.ru/translate_tts?ie={0}&tl={1}&client={2}&q={3}";
        public const string IE = "UTF-8";
        public const string CLIENT = "gtx";
        
        protected override async Task<Stream> GetDataStream(WordSoundKey word)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(String.Format(URL, IE, word.Code, CLIENT, word.Word));
            HttpWebResponse response = (await request.GetResponseAsync()) as HttpWebResponse;

            return response.GetResponseStream();
        }
    }

    public class SoundsController : CacheResourceProvider<CustomMediaPlayer, WordSoundKey>
    {
        // private AudioEngine _engine;
        // private WaveOutEvent _outputDevice;
        
        public SoundsController(string path, ISoundsProvider provider, int amount = 20) : base(path, provider, amount)
        {
            // _outputDevice = new WaveOutEvent();
            
            // _engine = AudioEngine.CreateDefault();
            // provider.SetEngine(_engine);
        }

        public async void PlaySound(WordSoundKey wordSoundKey)
        {
            // SoundStream _data = await GetResource(wordSoundKey);
            // _data.Play();
            
            // AudioFileReader _data = await GetResource(wordSoundKey);
            // _outputDevice.Stop();
            // _outputDevice.Init(_data);
            // _outputDevice.Play();

            CustomMediaPlayer player = await GetResource(wordSoundKey);
            player.Stop();
            player.Position = TimeSpan.Zero;
            player.Play();
        }

        protected override void OnUnloadResource(CustomMediaPlayer Data)
        {
            Data.Dispose();
        }
    }
}