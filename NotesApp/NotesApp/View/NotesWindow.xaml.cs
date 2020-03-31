using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Recognition;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NotesApp.View
{
    /// <summary>
    /// Interaction logic for NotesWindow.xaml
    /// </summary>
    public partial class NotesWindow : Window
    {
        SpeechRecognitionEngine recognizer;

        public NotesWindow()
        {
            InitializeComponent();

            

            //english culture
            RecognizerInfo ri = null;
            foreach (var i in SpeechRecognitionEngine.InstalledRecognizers())
            {
                if (i.Culture.TwoLetterISOLanguageName.Equals("en"))
                {
                    ri = i;
                }
            }
            recognizer = new SpeechRecognitionEngine(ri);

            //nie działa w Polsce
            //var currentCulture = SpeechRecognitionEngine.InstalledRecognizers().Where(r => r.Culture.Equals(Thread.CurrentThread.CurrentCulture)).FirstOrDefault();
            //recognizer = new SpeechRecognitionEngine(currentCulture);

            GrammarBuilder builder = new GrammarBuilder();
            builder.AppendDictation();
            builder.Culture = recognizer.RecognizerInfo.Culture; //for english culture
            Grammar grammar = new Grammar(builder);
            recognizer.LoadGrammar(grammar);
            recognizer.SetInputToDefaultAudioDevice();
            recognizer.SpeechRecognized += Recognizer_SpeechRecognized;
        }

        private void Recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            string recognizedText = e.Result.Text;
            contentRichTextBox.Document.Blocks.Add(new Paragraph(new Run(recognizedText)));
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void SpeechButton_Click(object sender, RoutedEventArgs e)
        {
            bool isButtonEnabled = (sender as ToggleButton).IsChecked ?? false;
            if(!isButtonEnabled)
            {
                recognizer.RecognizeAsync(RecognizeMode.Multiple);
            }
            else
            {
                recognizer.RecognizeAsyncStop();
            }
        }

        private void contentRichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int ammountOfCharacters = new TextRange(contentRichTextBox.Document.ContentStart, contentRichTextBox.Document.ContentEnd).Text.Length;

            statusTextBlock.Text = $"Document length: {ammountOfCharacters} characters";
        }

        private void boldButton_Click(object sender, RoutedEventArgs e)
        {
            contentRichTextBox.Selection.ApplyPropertyValue(Inline.FontWeightProperty, FontWeights.Bold);
        }
    }
}
