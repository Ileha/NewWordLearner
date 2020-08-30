using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Assets.Roullete;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using DynamicData;
using NewWordLearner.Data;
using ReactiveUI;

namespace NewWordLearner.Views
{
    public class MainWindowViewModel : ReactiveObject
    {
        public App Application { get; private set; }
        public Project Project => Application.Project;

        public LearningTab LearningTab { get; private set; }
        public ReverseLearningTab ReverseLearningTab { get; private set; }
        public WordConstructTab WordConstructTab { get; private set; }
        public WordControlTab WordControlTab { get; private set; }

        public IEnumerable<AbstractTab> AllTabs()
        {
            yield return LearningTab;
            yield return ReverseLearningTab;
            yield return WordConstructTab;
            yield return WordControlTab;
        }

        public MainWindowViewModel(Window parent)
        {
            Application = (App)Avalonia.Application.Current;
            
            LearningTab = new LearningTab(this, parent);
            ReverseLearningTab = new ReverseLearningTab(this, parent);
            WordConstructTab = new WordConstructTab(this, parent);
            WordControlTab = new WordControlTab(this, parent);
        }
        
        private string _title = "";
        public string Title
        {
            get => _title; 
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }
    }
    
    public class LearnWindow : Window
    {
        private MainWindowViewModel _commonDataModel;
        private TaskCompletionSource<bool> _hiddenLoading = new TaskCompletionSource<bool>();
        public Task LoadingTask => _hiddenLoading.Task;
        
        public LearnWindow()
        {
            InitializeComponent();
            
            _commonDataModel = new MainWindowViewModel(this);
            DataContext = _commonDataModel;
            
            Init();

            _hiddenLoading.TrySetResult(true);
        }
        
        private async void Init()
        {
            _commonDataModel.Title = _commonDataModel.Application.Project.ToString();

            

            // roullete = new Roullete<Word>(project.words, (w) => w.learningRate);                //создание рандомайзера на основе весов
            //
            // learnings[0] = new StraightLearning(roullete, project, straightforward, panel1, word1, cancel1);
            // learnings[1] = new ReverseLearning(roullete, project, reverse, panel2, word2, cancel2);
            // learnings[2] = new WordConstruct(roullete, project, contruct, panel3, word3, cancel3);
            //
            // for (int i = 0; i < learnings.Length; i++) {
            //     learnings[i].Update();
            //     tabs.Add(learnings[i].tab, learnings[i]);
            // }
            //
            // Size = App.FONT_SIZE;
        }

        protected override async void OnClosed(EventArgs e)
        {
            await _commonDataModel.Application.SaveProject(_commonDataModel.Project);
            Console.WriteLine("saved");
        }

        public async void OnTabSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await LoadingTask;
            
            if (e.Source is TabControl tabControl)
            {
                TabItem ti = tabControl.SelectedItem as TabItem;
                foreach (var tabData in _commonDataModel.AllTabs())
                {
                    tabData.OnOpenTab(ti);
                }
                
                // Console.WriteLine($"on change {e}");
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}