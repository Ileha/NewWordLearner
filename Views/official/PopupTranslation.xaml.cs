using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using NewWordLearner.Data;
using NewWordLearner.Views.official;
using ReactiveUI;
using WordLearner.Translate;
using WordLearner.Translate.google;

namespace NewWordLearner.Views.official
{
    public partial class PopupTranslation : Window
    {
        public class TranslationViewModel : ReactiveObject
        {
            public string _heder = string.Empty;
            public string Heder
            {
                get => _heder; 
                set => this.RaiseAndSetIfChanged(ref _heder, value);
            }
            
            public string _word = string.Empty;
            public string Word
            {
                get => _word; 
                set => this.RaiseAndSetIfChanged(ref _word, value);
            }

            public ReactiveCommand<Unit, Unit> _ok = default;
            public ReactiveCommand<Unit, Unit> Ok
            {
                get => _ok;
                set =>  this.RaiseAndSetIfChanged(ref _ok, value);
            }
            
            public ReactiveCommand<Unit, Unit> _cancel = default;
            public ReactiveCommand<Unit, Unit> Cancel
            {
                get => _cancel;
                set =>  this.RaiseAndSetIfChanged(ref _cancel, value);
            }
        }
        
        private TranslationViewModel _commonDataModel;

        public PopupTranslation() 
        {
            InitializeComponent();
            
            _commonDataModel = new TranslationViewModel();
            DataContext = _commonDataModel;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        #region Static

        public static async Task<string> Create(string foreignWord, Project project, Window parent)
        {
            TaskCompletionSource<string> hiddenTask = new TaskCompletionSource<string>();
            
            Task<string> translate = new GoogleTranslator().Translate(foreignWord, project);
            
            PopupTranslation translation = new PopupTranslation();

            translation.ShowDialog(parent);
            
            translation.IsEnabled = false;
            translation._commonDataModel.Heder = $"Translation for word {foreignWord}";
            translation.Closed += (sender, args) => hiddenTask.TrySetCanceled();
            translation._commonDataModel.Cancel = ReactiveCommand.Create(() =>
            {
                hiddenTask.TrySetCanceled();
            });
            translation._commonDataModel.Ok = ReactiveCommand.Create(() =>
            {
                hiddenTask.TrySetResult(translation._commonDataModel.Word);
            });
            translation._commonDataModel.Word = "hold a sec";

            try
            {
                translation._commonDataModel.Word = await translate;
            }
            catch (Exception e)
            {
                translation._commonDataModel.Word = "something went wrong in translation";
                await Task.Delay(1000);
                translation._commonDataModel.Word = string.Empty;
            }
            
            translation.IsEnabled = true;
            
            try
            {
                return await hiddenTask.Task;
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                translation.Close();
            }
        }

        #endregion
    }
}
