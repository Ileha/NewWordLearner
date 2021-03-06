using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using DynamicData;
using NewWordLearner.Data;
using NewWordLearner.ResourceProvider.SoundsController;
using NewWordLearner.System;
using NewWordLearner.Views.official;
using ReactiveUI;
using System.Collections.Generic;
using Avalonia.Input;

namespace NewWordLearner.Views
{
    #region Abstract

    public abstract class AbstractTab : ReactiveObject
    {
        protected Action<KeyEventArgs> KeyboardInput = default;
        protected TabItem OriginalTab { get; private set; }
        public bool IsClosed { get; private set; } = true;

        public AbstractTab(TabItem tabItem)
        {
            OriginalTab = tabItem;
            OriginalTab.KeyUp += (sender, args) => KeyboardInput?.Invoke(args);
            OriginalTab.KeyDown += (sender, args) => args.Handled = true;
        }
        public void OnOpenTab(TabItem tabItem)
        {
            if (tabItem != OriginalTab)
            {
                if (!IsClosed)
                {
                    Console.WriteLine($"{this} close");
                    OnCloseTab();
                    IsClosed = true;
                }
                return;
            }

            if (IsClosed)
            {
                Console.WriteLine($"{this} open");
                OnOpenTab();
                IsClosed = false;
            }
        }

        protected virtual void OnOpenTab()
        {
            
        }
        protected virtual void OnCloseTab()
        {
            
        }
    }

    public abstract class LearnTab : AbstractTab
    {
        #region Data

        private string _word = String.Empty;
        public string Word
        {
            get => _word; 
            protected set => this.RaiseAndSetIfChanged(ref _word, value);
        }
        private ObservableCollection<Button> _options = new ObservableCollection<Button>();
        public ObservableCollection<Button> Options
        {
            get => _options;
            protected set => this.RaiseAndSetIfChanged(ref _options, value);
        }

        private ReactiveCommand<Unit, Task> _cancel = default;
        public ReactiveCommand<Unit, Task> Cancel
        {
            get => _cancel;
            protected set =>  this.RaiseAndSetIfChanged(ref _cancel, value);
        }

        private bool _muted = true;
        public bool Muted
        {
            get => _muted; 
            protected set => this.RaiseAndSetIfChanged(ref _muted, value);
        }

        private IBitmap _imagePath = default;
        public IBitmap ImagePath
        {
            get => _imagePath; 
            protected set => this.RaiseAndSetIfChanged(ref _imagePath, value);
        }
        
        #endregion

        private int _wordCount = -1;
        
        protected abstract int CurrentProjectWordCount { get; }

        public LearnTab(TabItem tabItem) : base(tabItem)
        {
            
        }
        
        protected async void ApplyImage(Task<IBitmap> imageLoader)
        {
            try
            {
                ImagePath = default;
                ImagePath = await imageLoader;
            }
            catch (Exception e)
            {
                Console.WriteLine($"something bad occured in image loading\n{e}");
            }
        }

        protected virtual void DisableAllButtons()
        {
            foreach (Button button in Options)
            {
                button.IsEnabled = false;
            }
            Cancel = default;
        }

        protected override void OnOpenTab()
        {
            if (CurrentProjectWordCount != _wordCount)
            {
                if (CurrentProjectWordCount >= App.MinWordCount)
                {
                    if (_wordCount < App.MinWordCount)
                    {    
                        //TODO update
                        OnUpdateLearnTab();
                    }
                }
                else
                {
                    //TODO clear tab
                    OnClearLearnTab();
                }
            }

            _wordCount = CurrentProjectWordCount;
        }

        protected virtual void OnUpdateLearnTab()
        {
            
        }
        
        protected virtual void OnClearLearnTab()
        {
            
        }
    }

    #endregion

    public class LearningTab : LearnTab
    {
        protected override int CurrentProjectWordCount => _data.Project.WordCount;

        private ReactiveCommand<Unit, Unit> _playSound = default;
        public ReactiveCommand<Unit, Unit> PlaySound
        {
            get => _playSound;
            protected set =>  this.RaiseAndSetIfChanged(ref _playSound, value);
        }
        
        private MainWindowViewModel _data;

        public LearningTab(MainWindowViewModel dataModel, LearnWindow parent) : base(parent.FindControl<TabItem>("Straightforward"))
        {
            _data = dataModel;
            // OriginalTab = parent.FindControl<TabItem>("Straightforward");
            
            // Cancel = ReactiveCommand.Create(async () =>
            // {
            //     ImagePath = new Bitmap("./Images/2D-Roguelike.jpg");
            // });
        }

        protected void OnPlaySound(Word selected)
        {
            try
            {
                _data.Application.SoundsController.PlaySound(new WordSoundKey(selected.ForeignWord,
                    _data.Project.TargetLanguage.Code));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected override void OnUpdateLearnTab()
        {
            async void OnRightWord(Word word)
            {
                DisableAllButtons();
                Word = $"You are right {word.ForeignWord} is {word.Translate}";
                word.IncreaseLearningRate();
                // _data.Application.SaveProject(_data.Project);
                await Task.Delay(3000);
                _data.Project.UpdateRoullete();
                SelectNextWord();
            }
            
            async void OnWrongWord(Word wrongWord, Word rightWord)
            {
                DisableAllButtons();
                Word = $"You are wrong {rightWord.ForeignWord} is NOT {wrongWord.Translate}" +
                       $"\n{rightWord.ForeignWord} is {rightWord.Translate}";
                wrongWord.DropLearningRate();
                rightWord.DropLearningRate();
                // _data.Application.SaveProject(_data.Project);
                await Task.Delay(3000);
                _data.Project.UpdateRoullete();
                SelectNextWord();
            }
            
            async void OnCancel(Word rightWord)
            {
                DisableAllButtons();
                Word = $"That's a pitty " +
                       $"\n{rightWord.ForeignWord} is {rightWord.Translate}";
                rightWord.DropLearningRate();
                // _data.Application.SaveProject(_data.Project);
                await Task.Delay(3000);
                _data.Project.UpdateRoullete();
                SelectNextWord();
            }

            void SelectNextWord()
            {
                Word selected = _data.Project.PickWord((float) MyRandom.MyRandom.GetRandomDouble());
                
                Button rightButton = new Button()
                {
                    Content = selected.Translate
                };
                rightButton.Click += (sender, args) => OnRightWord(selected);
                
                Word = selected.ForeignWord;
                ApplyImage(_data.Application.ImageController.GetResource(selected.ForeignWord));
                if (Muted)
                {
                    OnPlaySound(selected);
                }

                Options.Clear();
                Options
                    .AddRange
                    (
                        _data
                            .Project
                            .AllWords
                            .Except(IEnumerableExtentions.FromParams(selected))
                            .Shuffle()
                            .Take(Math.Min(App.Complicate - 1, _data.Project.WordCount))
                            .Select(word =>
                            {
                                Button button = new Button()
                                {
                                    Content = word.Translate
                                };
                                button.Click += (sender, args) => OnWrongWord(word, selected);
                                return button;
                            })
                            .Concat(IEnumerableExtentions.FromParams(rightButton))
                            .Shuffle()
                    );

                Cancel = ReactiveCommand.Create(async () =>
                {
                    OnCancel(selected);
                });
                
                PlaySound = ReactiveCommand.Create(() =>
                {
                    OnPlaySound(selected);
                });
            }

            SelectNextWord();
        }

        protected override void OnClearLearnTab()
        {
            Cancel = default;
            Word = $"Add {App.MinWordCount} or more words to start learning";
            Options.Clear();
        }
    }
    public class ReverseLearningTab : LearnTab
    {
        protected override int CurrentProjectWordCount => _data.Project.WordCount;
        
        private MainWindowViewModel _data;
        public ReverseLearningTab(MainWindowViewModel dataModel, LearnWindow parent) : base(parent.FindControl<TabItem>("Reverse"))
        {
            _data = dataModel;
        }

        protected void PlaySound(Word selected)
        {
            try
            {
                _data.Application.SoundsController.PlaySound(new WordSoundKey(selected.ForeignWord,
                    _data.Project.TargetLanguage.Code));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        
        protected override void OnUpdateLearnTab()
        {
            async void OnRightWord(Word word)
            {
                DisableAllButtons();
                Word = $"You are right {word.Translate} is {word.ForeignWord}";
                word.IncreaseLearningRate();
                if (Muted)
                {
                    PlaySound(word);
                }
                // _data.Application.SaveProject(_data.Project);
                await Task.Delay(3000);
                _data.Project.UpdateRoullete();
                SelectNextWord();
            }
            
            async void OnWrongWord(Word wrongWord, Word rightWord)
            {
                DisableAllButtons();
                Word = $"You are wrong {rightWord.Translate} is NOT {wrongWord.ForeignWord}" +
                       $"\n{rightWord.Translate} is {rightWord.ForeignWord}";
                wrongWord.DropLearningRate();
                rightWord.DropLearningRate();
                // _data.Application.SaveProject(_data.Project);
                await Task.Delay(3000);
                _data.Project.UpdateRoullete();
                SelectNextWord();
            }
            
            async void OnCancel(Word rightWord)
            {
                DisableAllButtons();
                Word = $"That's a pitty " +
                       $"\n{rightWord.Translate} is {rightWord.ForeignWord}";
                rightWord.DropLearningRate();
                // _data.Application.SaveProject(_data.Project);
                await Task.Delay(3000);
                _data.Project.UpdateRoullete();
                SelectNextWord();
            }

            void SelectNextWord()
            {
                Word selected = _data.Project.PickWord((float) MyRandom.MyRandom.GetRandomDouble());
                
                Button rightButton = new Button()
                {
                    Content = selected.ForeignWord
                };
                rightButton.Click += (sender, args) => OnRightWord(selected);
                
                Word = selected.Translate;
                ApplyImage(_data.Application.ImageController.GetResource(selected.ForeignWord));
                
                Options.Clear();
                Options
                    .AddRange
                    (
                        _data
                            .Project
                            .AllWords
                            .Except(IEnumerableExtentions.FromParams(selected))
                            .Shuffle()
                            .Take(Math.Min(App.Complicate - 1, _data.Project.WordCount))
                            .Select(word =>
                            {
                                Button button = new Button()
                                {
                                    Content = word.ForeignWord
                                };
                                button.Click += (sender, args) => OnWrongWord(word, selected);
                                return button;
                            })
                            .Concat(IEnumerableExtentions.FromParams(rightButton))
                            .Shuffle()
                    );

                Cancel = ReactiveCommand.Create(async () =>
                {
                    OnCancel(selected);
                });
            }

            SelectNextWord();
        }

        protected override void OnClearLearnTab()
        {
            Cancel = default;
            Word = $"Add {App.MinWordCount} or more words to start learning";
            Options.Clear();
        }
    }
    public class WordConstructTab : LearnTab
    {
        protected override int CurrentProjectWordCount => _data.Project.WordCount;
        private MainWindowViewModel _data;
        public WordConstructTab(MainWindowViewModel dataModel, LearnWindow parent) : base(parent.FindControl<TabItem>("Construct"))
        {
            _data = dataModel;
        }
        
        protected void PlaySound(Word selected)
        {
            try
            {
                _data.Application.SoundsController.PlaySound(new WordSoundKey(selected.ForeignWord,
                    _data.Project.TargetLanguage.Code));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected override void DisableAllButtons()
        {
            KeyboardInput = null;
            base.DisableAllButtons();
        }

        protected override void OnUpdateLearnTab()
        {
            async void OnCancel(Word rightWord)
            {
                DisableAllButtons();
                Word = $"That's a pitty " +
                       $"\n{rightWord.Translate} is {rightWord.ForeignWord}";
                rightWord.DropLearningRate();
                // _data.Application.SaveProject(_data.Project);
                await Task.Delay(3000);
                _data.Project.UpdateRoullete();
                SelectNextWord();
            }
            
            async void OnWrong(Word rightWord, string data)
            {
                DisableAllButtons();
                Word = $"You are wrong {rightWord.Translate} WRITE AS:\n{rightWord.ForeignWord}" +
                       $"\nYour instance:\n{data}";
                rightWord.DropLearningRate();
                // _data.Application.SaveProject(_data.Project);
                await Task.Delay(3000);
                _data.Project.UpdateRoullete();
                SelectNextWord();
            }
            
            async void OnRightWord(Word word)
            {
                DisableAllButtons();
                Word = $"You are right {word.Translate} is {word.ForeignWord}";
                word.IncreaseLearningRate();
                if (Muted)
                {
                    PlaySound(word);
                }
                // _data.Application.SaveProject(_data.Project);
                await Task.Delay(3000);
                _data.Project.UpdateRoullete();
                SelectNextWord();
            }
            
            void SelectNextWord()
            {
                Word selected = _data.Project.PickWord((float) MyRandom.MyRandom.GetRandomDouble());
                string preposition = $"{selected.Translate} is ";
                string wordSample = selected.ForeignWord.Trim();
                Word = preposition;
                ApplyImage(_data.Application.ImageController.GetResource(selected.ForeignWord));

                Regex _suitForWord = new Regex("^\\w$");
                
                IEnumerable<(char _char, int index)> _chars = wordSample
                    .GetCharsFromString()
                    .Select((_char, i) => (_char, i))
                    .Where(_data => _suitForWord.IsMatch(_data._char.ToString()));
                
                IEnumerator<(char _char, int index)> _charSequence = _chars
                    .GetEnumerator();

                Options.Clear();

                Dictionary<Button, char> _dictionaryButton = default;
                
                var _temp = _chars
                    .Select(_data =>
                    {
                        Button button = new Button()
                        {
                            Content = $" {_data._char} "
                        };
                        button.Click += (sender, args) => OnNextCharacter(_data._char, button);
                        return (button, _data);
                    })
                    .Shuffle()
                    .ToArray();

                _dictionaryButton = _temp.ToDictionary(_data => _data.button, _data=>_data._data._char);
                
                void OnNextCharacter(char insttance, Button button = default)
                {
                    if (button == default)
                    {
                        Regex _regex = new Regex($"(?i:{insttance})");
                        button = Options
                            .Where(button => _dictionaryButton.TryGetValue(button, out var content) && _regex.IsMatch(content.ToString()))
                            .FirstOrDefault();
                    }

                    if (_charSequence.MoveNext())
                    {
                        if (button != default && _dictionaryButton.TryGetValue(button, out var content) && content == _charSequence.Current._char)
                        {
                            Word = $"{preposition}{wordSample.Substring(0, _charSequence.Current.index+1)}";
                            Options.Remove(button);
                            if (_charSequence.Current.index >= wordSample.Length-1)
                            {
                                OnRightWord(selected);
                            }
                        }
                        else
                        {
                            string _word = $"{wordSample.Substring(0, _charSequence.Current.index).Trim()}{insttance}";
                            Word = $"{preposition}{_word}";
                            OnWrong(selected, _word);
                        }
                    }
                    else
                    {
                        OnRightWord(selected);
                    }
                }

                Options.AddRange
                (
                    _temp
                        .Select(_data => _data.button)
                );

                KeyboardInput = args =>
                {
                    if (!_suitForWord.IsMatch(args.Key.ToString()))
                    {
                        return;
                    }
                    
                    args.Handled = true;
                    OnNextCharacter(args.Key.ToString().ToLower()[0]);
                };
            
                Cancel = ReactiveCommand.Create(async () =>
                {
                    OnCancel(selected);
                });
            }
            
            SelectNextWord();
        }

        protected override void OnClearLearnTab()
        {
            Cancel = default;
            Word = $"Add {App.MinWordCount} or more words to start learning";
            Options.Clear();
        }
    }
    public class WordControlTab : AbstractTab
    {
        private ObservableCollection<Word> _holeWords = new ObservableCollection<Word>();
        public ObservableCollection<Word> HoleWords
        {
            get => _holeWords;
            set => this.RaiseAndSetIfChanged(ref _holeWords, value);
        }
        public string _word = string.Empty;
        public string Word
        {
            get => _word; 
            private set => this.RaiseAndSetIfChanged(ref _word, value);
        }

        public ReactiveCommand<Unit, Task> _addWord;
        public ReactiveCommand<Unit, Task> AddWord
        {
            get => _addWord;
            private set =>  this.RaiseAndSetIfChanged(ref _addWord, value);
        }

        private MainWindowViewModel _data;

        public WordControlTab(MainWindowViewModel dataModel, LearnWindow parent) : base(parent.FindControl<TabItem>("WordsControl"))
        {
            _data = dataModel;
            
            AddWord = ReactiveCommand.Create(async () =>
            {
                try
                {
                    Word word = new Word(Word, String.Empty);
                    if (dataModel.Project.ContainsWord(word))
                    {
                        Console.WriteLine("just added");
                        await Notify.Create($"word {word.ForeignWord} just added", parent);
                        return;
                    }

                    string data = await PopupTranslation.Create(Word, dataModel.Project, parent);
                    word.Translate = data;
                    if (!dataModel.Project.AddWord(word))
                    {
                        Console.WriteLine("just added");
                        await Notify.Create($"word {word.ForeignWord} just added", parent);
                        return;
                    }
                    
                    HoleWords.Add(word);
                    Word = string.Empty;
                    await dataModel.Application.SaveProject(dataModel.Project);
                    Console.WriteLine("saved");
                }
                catch (Exception e)
                {
                    Console.WriteLine("canceled");
                }
            });
        }

        protected override void OnOpenTab()
        {
            HoleWords.AddRange(_data.Project.AllWords);
        }

        protected override void OnCloseTab()
        {
            HoleWords.Clear();
        }
    }
}