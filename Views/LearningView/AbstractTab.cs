using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using DynamicData;
using NewWordLearner.Data;
using NewWordLearner.System;
using NewWordLearner.Views.official;
using ReactiveUI;

namespace NewWordLearner.Views
{
    #region Abstract

    public abstract class AbstractTab : ReactiveObject
    {
        public AbstractTab()
        {
            
        }

        public abstract void OnOpenTab(TabItem tabItem);
    }

    public abstract class LearnTab : AbstractTab
    {
        #region Data

        public string _word = String.Empty;
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

        public ReactiveCommand<Unit, Task> _cancel = default;
        public ReactiveCommand<Unit, Task> Cancel
        {
            get => _cancel;
            protected set =>  this.RaiseAndSetIfChanged(ref _cancel, value);
        }
        
        #endregion
        
        private int _wordCount = -1;
        protected abstract TabItem OriginalTab { get; }
        protected abstract int CurrentProjectWordCount { get; }

        protected void DisableAllButtons()
        {
            foreach (Button button in Options)
            {
                button.IsEnabled = false;
            }
            Cancel = default;
        }
        
        public override void OnOpenTab(TabItem tabItem)
        {
            if (tabItem != OriginalTab) return;
            
            Console.WriteLine($"{this} update");

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
        protected override TabItem OriginalTab { get; }
        protected override int CurrentProjectWordCount => _data.Project.WordCount;

        private MainWindowViewModel _data;

        public LearningTab(MainWindowViewModel dataModel, Window parent)
        {
            _data = dataModel;
            OriginalTab = parent.FindControl<TabItem>("Straightforward");
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
        protected override TabItem OriginalTab { get; }
        protected override int CurrentProjectWordCount => _data.Project.WordCount;
        
        private MainWindowViewModel _data;
        public ReverseLearningTab(MainWindowViewModel dataModel, Window parent)
        {
            _data = dataModel;
            OriginalTab = parent.FindControl<TabItem>("Reverse");
        }

        protected override void OnUpdateLearnTab()
        {
            async void OnRightWord(Word word)
            {
                DisableAllButtons();
                Word = $"You are right {word.Translate} is {word.ForeignWord}";
                word.IncreaseLearningRate();
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
        protected override TabItem OriginalTab { get; }
        protected override int CurrentProjectWordCount => _data.Project.WordCount;
        private MainWindowViewModel _data;
        public WordConstructTab(MainWindowViewModel dataModel, Window parent)
        {
            _data = dataModel;
            OriginalTab = parent.FindControl<TabItem>("Construct");
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
                Word = $"You are wrong {rightWord.Translate} WRITE AS {rightWord.ForeignWord}" +
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
                int index = 0;

                void OnNext(char character, Button button)
                {
                    if (character == wordSample[index])
                    {
                        index++;
                        Word = $"{preposition}{wordSample.Substring(0, index)}";
                        Options.Remove(button);
                        if (index == wordSample.Length)
                        {
                            OnRightWord(selected);
                        }
                    }
                    else
                    {
                        Word = $"{preposition}{wordSample.Substring(0, index)}{character}";
                        OnWrong(selected, Word);
                    }
                }

                Options.Clear();
                Options
                    .AddRange
                    (
                        wordSample
                            .GetCharsFromString()
                            .Select((character, i) =>
                            {
                                Button button = new Button()
                                {
                                    Content = $" {character} "
                                };
                                button.Click += (sender, args) => OnNext(character, button);
                                return button;
                            })
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
    public class WordControlTab : AbstractTab
    {
        private TabItem _original;
        
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
        
        public WordControlTab(MainWindowViewModel dataModel, Window parent)
        {
            _data = dataModel;
            _original = parent.FindControl<TabItem>("WordsControl");

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

        public override void OnOpenTab(TabItem tabItem)
        {
            if (tabItem != _original) return;
            
            Console.WriteLine("WordControlTab update");
            
            HoleWords.Clear();
            HoleWords.AddRange(_data.Project.AllWords);
        }
    }
}