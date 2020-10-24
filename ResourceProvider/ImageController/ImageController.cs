using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using NewWordLearner.ImageController.ImageProviders;
using NewWordLearner.ResourceProvider;

namespace NewWordLearner.ImageController
{
    public class ImageController : CacheResourceProvider<IBitmap, string>
    {
        public ImageController(string path, IImageProvider provider, int amount = 20) : base(path, provider, amount)
        {
            
        }
    }
}