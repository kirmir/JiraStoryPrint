using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;

namespace JiraStoryPrint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly string _outputFileName =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "JIRA Stories.htm");

        private readonly JiraStoryUpdater _htmlUpdater;

        public MainWindow()
        {
            InitializeComponent();

            _htmlUpdater = new JiraStoryUpdater();
            DataContext = _htmlUpdater;
        }

        private void inputBorderDrop(object sender, DragEventArgs e)
        {
            var htmlFile = e.GetFirstFileNameWithExtension("htm", "html");
            if (string.IsNullOrEmpty(htmlFile))
            {
                _htmlUpdater.Reset();
                return;
            }

            _htmlUpdater.OpenFile(htmlFile);

            if (_htmlUpdater.FileIsOpened)
            {
                _htmlUpdater.ProcessFile();
            }
        }

        private void aboutHyperlinkRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void outputBorderMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || !_htmlUpdater.FileIsProcessed)
            {
                return;
            }

            // Save file and drop it to the target location.
            _htmlUpdater.Html.Save(_outputFileName);

            DragDrop.DoDragDrop(this, new DataObject(DataFormats.FileDrop, new[] { _outputFileName }),
                                DragDropEffects.Move);
        }
    }
}
