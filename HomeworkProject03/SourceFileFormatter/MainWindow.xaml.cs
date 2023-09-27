﻿using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using Wpf.Ui.Mvvm.Services;

namespace SourceFileFormatter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Public Constructors

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            var snackbarService = new SnackbarService();
            snackbarService.SetSnackbarControl(Snackbar);
            ViewModel = new(new(), snackbarService);
        }

        #endregion Public Constructors

        #region Public Properties

        public MainWindowViewModel ViewModel { get; }

        #endregion Public Properties

        #region Private Methods

        private void OnWindowDrop(object sender, DragEventArgs e)
        {
            var array = e.Data.GetData(DataFormats.FileDrop) as Array;
            ViewModel.FilePath = array!.GetValue(0)!.ToString()!;
        }

        private void OnSelectButtonClicked(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog()
            {
                Title = "选择文件",
                Filter = "C#源文件(*.cs)|*.cs",
            };
            if (dialog.ShowDialog() == true)
            {
                ViewModel.FilePath = dialog.FileName;
            }
        }

        private void OnSaveButtonClicked(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog()
            {
                Title = "另存为",
                Filter = "C#源文件(*.cs)|*.cs",
                FileName = ViewModel.FilePath,
            };
            if (dialog.ShowDialog() == true)
            {
                using var stream = new FileStream(dialog.FileName, FileMode.Create, FileAccess.Write, FileShare.Read);
                using var writer = new StreamWriter(stream);
                writer.Write(ViewModel.Text);
            }
        }

        #endregion Private Methods
    }
}
