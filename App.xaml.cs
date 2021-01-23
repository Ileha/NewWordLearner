using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Newtonsoft.Json;
using NewWordLearner.Data;
using NewWordLearner.ImageController.ImageProviders;
using NewWordLearner.ImageController.ImageProviders.Instances;
using NewWordLearner.ResourceProvider.SoundsController;
using NewWordLearner.Extentions;
using NewWordLearner.Views;

namespace NewWordLearner
{
    public class App : Application
    {
        public const int Complicate = 7;
        public const int MinWordCount = 5;
        
        public const string LanguagesPathJSON = "./languages.json";
        public const string ProjectConfigJSON = "./config.json";
        public const string ProjectExtentionJSON = ".json";
        
        // public const string LanguagesPath = "./languages.xml";
        public const string ProjectsDataPath = "./data";
        
        // public const string ProjectExtention = ".dat";
        public const string ImageStorePath = "./Images";
        public const string SoundStorePath = "./Sounds";
        // public const string ProjectConfig = "./config.xml";
        
        public Project Project { get; private set; }
        public ImageController.ImageController ImageController { get; private set; }
        public SoundsController SoundsController { get; private set; }

        private Regex _projectsFiles = new Regex($"(?<name>[^/]+){ProjectExtentionJSON}$");

        #region Config

        private Dictionary<string, string> _config;

        public string GetConfigValue(string name)
        {
            if (_config.TryGetValue(name, out string _res))
            {
                return _res;
            }

            return null;
        }

        private void LoadConfig()
        {
            try
            {
                using (FileStream fs = new FileStream(ProjectConfigJSON, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(fs))
                    {
                        using (JsonTextReader jsonReader = new JsonTextReader(reader))
                        {
                            JsonSerializer ser = new JsonSerializer();
                            _config = ser.Deserialize<Dictionary<string, string>>(jsonReader);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _config = new Dictionary<string, string>();
                using (FileStream fs = new FileStream(ProjectConfigJSON, FileMode.Create))
                {
                    using (StreamWriter writer = new StreamWriter(fs))
                    {
                        using (JsonTextWriter jsonWriter = new JsonTextWriter(writer))
                        {
                            JsonSerializer ser = new JsonSerializer();
                            ser.Serialize(jsonWriter, _config);
                            jsonWriter.Flush();
                        }
                    }
                }
            }
        }

        #endregion

        #region Languages

        public IEnumerable<Language> AllLanguages 
        { 
            get 
            {
                return _languages.Values;
            }
        }

        private Dictionary<string, Language> _languages = new Dictionary<string, Language>();

        // private Language GetLanguageByTitle(string title) => _languages[title];
        
        private void LoadLanguages() 
        {
            try
            {
                using (FileStream fs = new FileStream(LanguagesPathJSON, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(fs))
                    {
                        using (JsonTextReader jsonReader = new JsonTextReader(reader))
                        {
                            JsonSerializer ser = new JsonSerializer();
                            var _data = ser.Deserialize<Language[]>(jsonReader);
                            _languages = _data.ToDictionary(_lang => _lang.LanguageTitle);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _languages = new Dictionary<string, Language>();
                var _add = new Language("English", "en");
                _languages.Add(_add.LanguageTitle, _add);
                using (FileStream fs = new FileStream(LanguagesPathJSON, FileMode.Create))
                {
                    using (StreamWriter writer = new StreamWriter(fs))
                    {
                        using (JsonTextWriter jsonWriter = new JsonTextWriter(writer))
                        {
                            JsonSerializer ser = new JsonSerializer();
                            ser.Serialize(jsonWriter, _languages.Values);
                            jsonWriter.Flush();
                        }
                    }
                }
            }
        }
        
        #endregion
        
        #region Singleton

        private static App instance;

        public static App Instance
        {
            get
            {
                return instance;
            }
        }

        #endregion

        public App()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                throw new Exception("you can't create some instances of App");
            }
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        public override async void OnFrameworkInitializationCompleted()
        {
            LoadConfig();
            
            if (!Directory.Exists(ProjectsDataPath)) 
            {
                Directory.CreateDirectory(ProjectsDataPath);
            }
            
            if (!Directory.Exists(ImageStorePath)) 
            {
                Directory.CreateDirectory(ImageStorePath);
            }
            
            if (!Directory.Exists(SoundStorePath))
            {
                Directory.CreateDirectory(SoundStorePath);
            }
            
            ImageController = new ImageController.ImageController(ImageStorePath, new IImageProvider.AgregatorImageProvider(new UnsplashImageProvider(), new GoogleImageProvider()));
            SoundsController = new SoundsController(SoundStorePath, new GoogleSoundProvider());
            
            LoadLanguages();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }
            
            base.OnFrameworkInitializationCompleted();
        }

        // private async Task Move2JSON()
        // {
        //     await Task.Run(() =>
        //     {
        //         using (FileStream fs =
        //             new FileStream(
        //                 ProjectConfigJSON,
        //                 FileMode.Truncate))
        //         {
        //             using (StreamWriter writer = new StreamWriter(fs))
        //             {
        //                 using (JsonTextWriter jsonWriter = new JsonTextWriter(writer))
        //                 {
        //                     JsonSerializer ser = new JsonSerializer();
        //                     ser.Serialize(jsonWriter, _config);
        //                     jsonWriter.Flush();
        //                 }
        //             }
        //         }
        //     });
        //     
        //     await Task.Run(() =>
        //     {
        //         using (FileStream fs =
        //             new FileStream(
        //                 LanguagesPathJSON,
        //                 FileMode.Truncate))
        //         {
        //             using (StreamWriter writer = new StreamWriter(fs))
        //             {
        //                 using (JsonTextWriter jsonWriter = new JsonTextWriter(writer))
        //                 {
        //                     JsonSerializer ser = new JsonSerializer();
        //                     ser.Serialize(jsonWriter, AllLanguages);
        //                     jsonWriter.Flush();
        //                 }
        //             }
        //         }
        //     });
        //     
        //     await Task.WhenAll
        //     (
        //         (await GetAllProjects()).Select(async _data =>
        //         {
        //             await SaveProjectJSON(await LoadProject(_data.puth));
        //         })
        //     );
        //
        // }

        #region SpecialWindows

        public void OpenWindowLearner(Project project)
        {
            SetCurrentProject(project);
            SetCurrentWindow(new LearnWindow());
        }

        #endregion
        
        #region Windows

        private Stack<Window> _stack = new Stack<Window>();
        
        public void SetCurrentWindow(Window window)
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow.Hide();
                _stack.Push(desktop.MainWindow);
                desktop.MainWindow = window;
                window.Show();
                window.Closing += (s, e) =>
                {
                    Window beforeWindow = _stack.Pop();
                    beforeWindow.Show();
                    desktop.MainWindow = beforeWindow;
                };
            }
        }

        #endregion
        
        #region Projects

        public void SetCurrentProject(Project project)
        {
            Project = project;
        }

        public async Task<Project> CreateNewProject(String name, Language tLanguage, Language yLanguage)
        {
            if ((await GetAllProjects()).Where(data => data.name == name).Count() > 0)
            {
                throw new Exception($"project with name {name} just exist");
            }
            Project result = new Project(name, tLanguage, yLanguage);
            await SaveProject(result);
            return result;
        }
        
        public Task<Project> LoadProject(string path)
        {
            return Project.LoadProject(path);
        }

        Dictionary<string, SemaphoreSlim> _pathSemaphores = new Dictionary<string, SemaphoreSlim>();
        public async Task SaveProject(Project project)
        {
            string path = $"{ProjectsDataPath}/{project.Name}{ProjectExtentionJSON}";
            SemaphoreSlim semaphoreSlim = default;
            if (!_pathSemaphores.TryGetValue(project.Name, out semaphoreSlim))
            {
                semaphoreSlim = new SemaphoreSlim(1, 1);
                _pathSemaphores.Add(project.Name, semaphoreSlim);
            }
            
            await semaphoreSlim.WaitAsync();
            try
            {
                await Project.SaveProject(project, path);
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public Task<(string puth, string name)[]> GetAllProjects()
        {
            return Task.Run(() =>
            {
                return Directory.EnumerateFiles($"{ProjectsDataPath}")
                    .WhereSelect((string path, out (string, string) data) =>
                    {
                        Match match = _projectsFiles.Match(path);
                        if (match.Success)
                        {
                            data = (path, match.Groups["name"].ToString());
                            return true;
                        }

                        data = default;
                        return false;
                    })
                    .ToArray();
            });
        }

        #endregion
   }
}