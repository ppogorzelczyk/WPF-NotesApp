using Microsoft.WindowsAzure.Storage;
using NotesApp.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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
        NotesVM viewModel;

        public NotesWindow()
        {
            InitializeComponent();

            viewModel = Resources["vm"] as NotesVM;
            container.DataContext = viewModel;
            viewModel.SelectedNoteChanged += ViewModel_SelectedNoteChanged;

            #region Speech Recognition
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
            #endregion

            var fontFamilies = Fonts.SystemFontFamilies.OrderBy(f => f.Source);
            fontFamilyComboBox.ItemsSource = fontFamilies;

            List<double> fontSizes = new List<double>() { 8, 9, 10, 11, 12, 14, 16, 28, 36, 48, 72 };
            fontSizeComboBox.ItemsSource = fontSizes;
        }

        private async void ViewModel_SelectedNoteChanged(object sender, EventArgs e)
        {
            contentRichTextBox.Document.Blocks.Clear();
            if (viewModel.SelectedNote != null && !string.IsNullOrEmpty(viewModel.SelectedNote.FileLocation))
            {
                Stream rtfFileStream = null;
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync(viewModel.SelectedNote.FileLocation);
                    rtfFileStream = await response.Content.ReadAsStreamAsync();
                    
                    TextRange range = new TextRange(contentRichTextBox.Document.ContentStart, contentRichTextBox.Document.ContentEnd);
                    range.Load(rtfFileStream, DataFormats.Rtf);
                }
            }
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            if (string.IsNullOrEmpty(App.UserId))
            {
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.ShowDialog();
            }
            else
            {
                viewModel.ReadNotebooks();
            }
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

        private void contentRichTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            var selectedState = contentRichTextBox.Selection.GetPropertyValue(Inline.FontWeightProperty);
            boldButton.IsChecked = (selectedState != DependencyProperty.UnsetValue) && selectedState.Equals(FontWeights.Bold);

            var selectedStyle = contentRichTextBox.Selection.GetPropertyValue(Inline.FontStyleProperty);
            italicButton.IsChecked = (selectedStyle != DependencyProperty.UnsetValue) && selectedStyle.Equals(FontStyles.Italic);

            var selectedDecoration = contentRichTextBox.Selection.GetPropertyValue(Inline.TextDecorationsProperty);
            underlineButton.IsChecked = (selectedDecoration != DependencyProperty.UnsetValue) && selectedDecoration.Equals(TextDecorations.Underline);

            fontFamilyComboBox.SelectedItem = contentRichTextBox.Selection.GetPropertyValue(Inline.FontFamilyProperty);
            if (contentRichTextBox.Selection.GetPropertyValue(Inline.FontSizeProperty) == DependencyProperty.UnsetValue)
            {
                fontSizeComboBox.Text = string.Empty;
            }
            else
            {
                fontSizeComboBox.Text = contentRichTextBox.Selection.GetPropertyValue(Inline.FontSizeProperty).ToString();
            }
        }

        private void fontFamilyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (fontFamilyComboBox.SelectedItem != null)
            {
                contentRichTextBox.Selection.ApplyPropertyValue(Inline.FontFamilyProperty, fontFamilyComboBox.SelectedItem);
            }
        }

        private void fontSizeComboBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (contentRichTextBox.Selection.GetPropertyValue(Inline.FontSizeProperty) != DependencyProperty.UnsetValue
                && !string.IsNullOrWhiteSpace(fontSizeComboBox.Text))
            {
                contentRichTextBox.Selection.ApplyPropertyValue(Inline.FontSizeProperty, fontSizeComboBox.Text);
            }
        }

        private async void saveFileButton_Click(object sender, RoutedEventArgs e)
        {
            string fileName = $"{viewModel.SelectedNote.Id}.rtf";
            string rtfFile = System.IO.Path.Combine(Environment.CurrentDirectory, fileName);

            using(FileStream fs = new FileStream(rtfFile, FileMode.Create))
            {
                TextRange range = new TextRange(contentRichTextBox.Document.ContentStart, contentRichTextBox.Document.ContentEnd);
                range.Save(fs, DataFormats.Rtf);
            }

            string fileUrl = await UploadFile(rtfFile, fileName);
            viewModel.SelectedNote.FileLocation = fileUrl;

            viewModel.UpdateSelectedNote();
        }

        private async Task<string> UploadFile(string rtfFileLocation, string fileName)
        {
            string fileUrl = string.Empty;
            var account = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=evernotecloneappstorage;AccountKey=uYOCz9r4tnBWQSEKn8b0ZYTM21X1V2b9dZszHhkyAlKdzVfpthuUC/ome+LI4FLiBsMuEajKuXQYiYhxpiph5g==;EndpointSuffix=core.windows.net");
            var client = account.CreateCloudBlobClient();
            var container = client.GetContainerReference("notes");
            await container.CreateIfNotExistsAsync();
            var blob = container.GetBlockBlobReference(fileName);

            using (FileStream fs = new FileStream(rtfFileLocation, FileMode.Open))
            {
                await blob.UploadFromStreamAsync(fs);
                fileUrl = blob.Uri.OriginalString;
            }

            return fileUrl;
        }
    }
}
