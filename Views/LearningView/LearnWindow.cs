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

        public MainWindowViewModel(LearnWindow parent)
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

        public Action<Key> OnButtonUp;
        public Action<Key> OnButtonDown;
        
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

        protected override void OnKeyDown(KeyEventArgs e)
        {
            OnButtonDown?.Invoke(e.Key);
            e.Handled = true;
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            OnButtonUp?.Invoke(e.Key);
            e.Handled = true;
        }
    }
}