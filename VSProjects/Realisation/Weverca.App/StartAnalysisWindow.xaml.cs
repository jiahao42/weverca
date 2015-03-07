﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Weverca.App.Settings;

namespace Weverca.App
{
    /// <summary>
    /// Interaction logic for StartAnalysisWindow.xaml
    /// </summary>
    public partial class StartAnalysisWindow : Window
    {
        private MainWindow mainWindow;

        public string FileName { get; set; }
        public SecondPhaseType SecondPhaseType { get; set; }
        public MemoryModelType MemoryModelType { get; set; }
        public LoggingOutputType LoggingOutputType { get; set; }
        public LoggingStrategyType LoggingStrategyType { get; set; }

        public StartAnalysisWindow()
        {
            InitializeComponent();
        }

        public bool? ShowStartAnalysisDialog()
        {
            if (FileName != null && FileName != string.Empty)
            {
                fileNameText.Text = FileName;
            }

            if (SecondPhaseType != null)
            {
                secondPhaseCombo.SelectedIndex = (int)SecondPhaseType;
            }

            if (MemoryModelType != null)
            {
                memoryModelCombo.SelectedIndex = (int)MemoryModelType;
            }

            if (LoggingOutputType != null)
            {
                fileOutputCombo.SelectedIndex = (int)LoggingOutputType;
            }

            if (LoggingStrategyType != null)
            {
                loggingStrategyCombo.SelectedIndex = (int)LoggingStrategyType;
            }

            return base.ShowDialog();
        }

        private void browseFileNameButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog()
            {
                DefaultExt = ".php",
                Filter = "PHP source file (*.php)|*.php|Text file (*.txt)|*.txt|All files (*.*)|*.*",
                Title = "Analysed file"
            };

            if (dlg.ShowDialog() == true)
            {
                fileNameText.Text = dlg.FileName;
            }
        }

        private void startAnalysisButton_Click(object sender, RoutedEventArgs e)
        {
            if (testFileName())
            {
                FileName = fileNameText.Text;
                SecondPhaseType = (SecondPhaseType)((ComboBoxItem)secondPhaseCombo.SelectedItem).Tag;
                MemoryModelType = (MemoryModelType)((ComboBoxItem)memoryModelCombo.SelectedItem).Tag;
                LoggingOutputType = (LoggingOutputType)((ComboBoxItem)fileOutputCombo.SelectedItem).Tag;
                LoggingStrategyType = (LoggingStrategyType)((ComboBoxItem)loggingStrategyCombo.SelectedItem).Tag;

                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Selected file does not exist.\n\nPlease select valid PHP source file.", "Analysis can not be started", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private bool testFileName()
        {
            string fileName = fileNameText.Text;
            if (fileName != null && fileName != string.Empty)
            {
                return System.IO.File.Exists(fileName);
            }
            else
            {
                return false;
            }
        }
    }
}
