using System.Collections.Generic;
using System.Threading.Tasks;

namespace NewWordLearner.ResourceProvider
{
    public interface IResourceProvider<T, K>
    {
        Task<T> GetFromDisk(K word);
        /// <summary>
        /// have to save to disk and return data
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        Task<T> DownloadFromInternet(K word);
        bool ContainsOnDisk(K word);
    }

    public abstract class CacheResourceProvider<T, K>
    {
        internal class ResourceHolder<T, K>
        {
            public T Data;
            public K Word;
            public long Old = 0;
        }
        internal class BitmapHolderComparer<T, K> : IComparer<ResourceHolder<T, K>>
        {
            public int Compare(ResourceHolder<T, K> x, ResourceHolder<T, K> y)
            {
                return (int) (x.Old - y.Old);
            }
        }

        protected string Path => _path;
        private string _path;
        private int _amount { get; }
        private long _requests = 0;
        
        private SortedSet<ResourceHolder<T, K>> _fastData = new SortedSet<ResourceHolder<T, K>>(new BitmapHolderComparer<T, K>());
        private Dictionary<K, ResourceHolder<T, K>> _associations = new Dictionary<K, ResourceHolder<T, K>>();
        private IResourceProvider<T, K> _imageProvider;

        public CacheResourceProvider(string path, IResourceProvider<T, K> provider, int amount = 20)
        {
            _amount = amount;
            _path = path;
            _imageProvider = provider;
        }
        
        public async Task<T> GetResource(K word)
        {
            ResourceHolder<T, K> original = default;
            if (_associations.TryGetValue(word, out original))
            {
                _fastData.Remove(original);
                original.Old = _requests;
                _fastData.Add(original);
            }
            else
            {
                if (_imageProvider.ContainsOnDisk(word))
                {
                    original = new ResourceHolder<T, K>()
                    {
                        Data = await _imageProvider.GetFromDisk(word),
                        Word = word,
                        Old = _requests
                    };
                }
                else
                {
                    original = new ResourceHolder<T, K>()
                    {
                        Data = await _imageProvider.DownloadFromInternet(word),
                        Word = word,
                        Old = _requests
                    };
                }
                
                _fastData.Add(original);
                _associations.Add(word, original);

                if (_associations.Count > _amount)
                {
                    ResourceHolder<T, K> older = _fastData.Min;
                    _fastData.Remove(older);
                    _associations.Remove(older.Word);
                    OnUnloadResource(older.Data);
                }
            }

            _requests++;
            return original.Data;
        }

        protected virtual void OnUnloadResource(T Data)
        {
            
        }
    }
}