using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace NewWordLearner.Views.official
{
    public class Notify : Window
    {
        private TextBlock text;
        private Button ok;
        
        public Notify()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }
        
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            
            text = this.FindControl<TextBlock>("text");
            ok = this.FindControl<Button>("ok");
        }

        #region Static

        public static async Task Create(string text, Window parent)
        {
            Notify notify = new Notify();
            
            
            var task = notify.ShowDialog(parent);
            notify.text.Text = text;
            notify.ok.Click += (object sender, RoutedEventArgs e) => notify.Close();

            await task;
        }

        #endregion
    }
}