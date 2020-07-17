using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TextAnalyzer.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public SingleThreadedTextAnalyzer bta { get; set; }
        public MultithreadedTextAnalyzer mta { get; set; }

        public TextAnalyzerViewModel viewModel { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            viewModel = new TextAnalyzerViewModel();
            DataContext = viewModel;
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                viewModel.MTA = new MultithreadedTextAnalyzer();
                viewModel.STA = new SingleThreadedTextAnalyzer();
                var singlethreadedTxt = singleThreadedTextbox.Text;
                var multithreadedTxt = multiThreadedTextbox.Text;
                Task.Run(() => viewModel.MTA.AnalyzeText(multithreadedTxt));
                Task.Run(() => viewModel.STA.AnalyzeText(singlethreadedTxt));
                

                //MessageBox.Show("Text analysis was finished.", "Notification");
            }
            catch (Exception ex)
            {
                throw;
            }
           
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            if (openFileDialog.ShowDialog() == true)
            {
                var text = File.ReadAllText(openFileDialog.FileName, Encoding.Default);
                singleThreadedTextbox.Text = text;
                multiThreadedTextbox.Text = text;
            }
        }
    }

    public class TextAnalyzerViewModel: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private MultithreadedTextAnalyzer _mta;
        public MultithreadedTextAnalyzer MTA
        {
            get
            {
                return _mta;
            }
            set
            {
                _mta = value;
                OnPropertyChanged("MTA");
            }
        }

        private SingleThreadedTextAnalyzer _sta;
        public SingleThreadedTextAnalyzer STA
        {
            get
            {
                return _sta;
            }
            set
            {
                _sta = value;
                OnPropertyChanged("STA");
            }
        }

        // Create the OnPropertyChanged method to raise the event
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
