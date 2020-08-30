using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using NewWordLearner.Data;
using NewWordLearner.Views.official;

namespace NewWordLearner.Views
{
    public class MainWindow : Window
    {
        private TextBox title;
        private StackPanel project;
        private ComboBox yourLanguage;
        private ComboBox targetLanguage;
        private Language[] _languages;
        
        public MainWindow()
        {
            InitializeComponent();
            
            Init();
           
#if DEBUG
            this.AttachDevTools();
#endif
        }
        
        private async void Init()
        {
            App application = (App)Application.Current;

            _languages = application.AllLanguages.ToArray();
            
            targetLanguage.Items = _languages;
            yourLanguage.Items = _languages;
            
            project.Children.Clear();
            
            foreach ((string puth, string name) path in await application.GetAllProjects()) 
            {
                Button add = new Button() { Content = path.name.ToString() };
                add.Click += async (object sender, RoutedEventArgs e) =>
                {
                    Project project = await application.LoadProject(path.puth);
                    application.OpenWindowLearner(project);
                };
                project.Children.Add(add);
            }
        }

        public async void button_Click(object sender, RoutedEventArgs e)
        {
            Button create = sender as Button;
            create.IsEnabled = false;
            
            if (string.IsNullOrEmpty(title.Text))
            {
                await Notify.Create($"Enter the project name", this);
                create.IsEnabled = true;
                return;
            }
            else if (targetLanguage.SelectedIndex == yourLanguage.SelectedIndex)
            {
                await Notify.Create($"Select different languages", this);
                create.IsEnabled = true;
                return;
            }
            App application = (App)Application.Current;

            try
            {
                var project = await application.CreateNewProject(title.Text,
                    _languages[targetLanguage.SelectedIndex],
                    _languages[yourLanguage.SelectedIndex]);
                Init();
                create.IsEnabled = true;
                application.OpenWindowLearner(project);
            }
            catch (Exception exception)
            {
                await Notify.Create(exception.Message, this);
                create.IsEnabled = true;
                return;
            }
        }
        
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            
            project = this.FindControl<StackPanel>("project");
            yourLanguage = this.FindControl<ComboBox>("yourLanguage");
            targetLanguage = this.FindControl<ComboBox>("targetLanguage");
            title = this.FindControl<TextBox>("title");
        }
    }
}