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
using NewWordLearner.Data;
using NewWordLearner.ImageController.ImageProviders;
using NewWordLearner.ImageController.ImageProviders.Instances;
using NewWordLearner.ResourceProvider.SoundsController;
using NewWordLearner.System;
using NewWordLearner.Views;

namespace NewWordLearner
{
    public class App : Application
    {
        public const int Complicate = 7;
        public const int MinWordCount = 5;
        public const String LanguagesPath = "./languages.xml";
        public const String ProjectExtention = ".dat";
        public const string ProjectsDataPath = "./data";
        public const string ImageStorePath = "./Images";
        public const string SoundStorePath = "./Sounds";
        
        public Project Project { get; private set; }
        public ImageController.ImageController ImageController { get; private set; }
        public SoundsController SoundsController { get; private set; }

        public IEnumerable<Language> AllLanguages 
        { 
            get 
            {
                return _languages.Values;
            }
        }

        // public KeyboardController KeyboardController { get; private set; }

        private Dictionary<string, Language> _languages = new Dictionary<string, Language>();
        private Regex _projectsFiles = new Regex($"(?<name>[^/]+){ProjectExtention}$");

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        public override void OnFrameworkInitializationCompleted()
        {
            // KeyboardController = new KeyboardController();
            
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
            
            XDocument xLang = LoadLanguages();

            foreach (XElement element in xLang.Root.Elements()) 
            {
                Language add = new Language(
                    element.Value.ToString(),
                    element.Attribute("code").Value.ToString()
                );
                _languages.Add(add.LanguageTitle, add);
            }

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }
            
            base.OnFrameworkInitializationCompleted();
        }

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
            string path = $"{ProjectsDataPath}/{project.Name}{ProjectExtention}";
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

        private XDocument LoadLanguages() 
        {
            XDocument res = null;
            try 
            {
                res = XDocument.Load(LanguagesPath);
            }
            catch (Exception err) 
            {
                res = new XDocument(
                    new XElement("languages",
                        new XElement("language", "English", new XAttribute("code", "en"))
                    )
                );
                res.Save(LanguagesPath);
            }

            return res;
        }
   }
}