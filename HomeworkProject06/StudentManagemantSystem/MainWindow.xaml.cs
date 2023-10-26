﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using CommunityToolkit.Mvvm.Messaging;
using StudentManagemantSystem.Models;
using StudentManagemantSystem.ViewModels;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using Wpf.Ui.Markup;
using Wpf.Ui.Mvvm.Interfaces;

namespace StudentManagemantSystem;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : UiWindow, IRecipient<string>
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = this;
        Loaded += OnMainWindowLoaded;
        ViewModel = viewModel;
        ViewModel.SnackbarService.SetSnackbarControl(Snackbar);
        _navigateButtons = new()
        {
            StudentsNavigateButton,
            ClassesNavigateButton,
            SchoolsNavigateButton
        };
        WeakReferenceMessenger.Default.Register(this);
    }

    public MainWindowViewModel ViewModel { get; }

    private void OnMainWindowLoaded(object sender, RoutedEventArgs e)
    {
        Watcher.Watch(this, BackgroundType.Acrylic);
        OnNavigateButtonClicked(StudentsNavigateButton, new());
    }

    private void OnDataGridAutoGeneratingColumn(object sender, System.Windows.Controls.DataGridAutoGeneratingColumnEventArgs e)
    {
        e.Column.Header = ((PropertyDescriptor)e.PropertyDescriptor).DisplayName;
    }

    readonly List<Button> _navigateButtons;

    private void OnNavigateButtonClicked(object sender, RoutedEventArgs e)
    {
        foreach (var button in _navigateButtons)
        {
            button.IconFilled = false;
            button.Background = Brushes.Transparent;
            button.Foreground = (Brush)App.Current.Resources["TextFillColorPrimaryBrush"];
        }
        if (sender is Button currentButton)
        {
            currentButton.Background = currentButton.MouseOverBackground;
            var animation = new ColorAnimation(Accent.SystemAccent, TimeSpan.FromSeconds(0.2));
            var storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            Storyboard.SetTarget(currentButton, animation);
            Storyboard.SetTargetProperty(animation, new("Foreground.Color"));
            storyboard.Begin(currentButton);
            currentButton.IconFilled = true;
            ViewModel.EntityTypeName = (string)currentButton.Tag;
            DataGrid.SetBinding(System.Windows.Controls.ItemsControl.ItemsSourceProperty, string.Join('.', nameof(ViewModel), $"{ViewModel.EntityTypeName}Collection"));
            DataGrid.Items.Refresh();
        }
    }

    private void OnDataGridAutoGeneratedColumns(object sender, EventArgs e)
    {
        DataGrid.Columns[0].IsReadOnly = true;
        foreach (var column in DataGrid.Columns)
        {
            var header = (string)column.Header;
            if (header.Contains("集合"))
                column.Visibility = Visibility.Collapsed;
            else if (header.Contains("所属") && header.Contains("名称"))
                column.IsReadOnly = true;
        }
    }

    public void Receive(string message)
    {
        if (message == "Saved")
            DataGrid.Items.Refresh();
    }

    private void OnSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (ViewModel.IsNotBusy)
        {
            ViewModel.SelectedItems = DataGrid.SelectedItems.Count == 0 ? default : DataGrid.SelectedItems;
        }

        //if (ViewModel.IsNotBusy)
        //{
        //    if (DataGrid.SelectedItems.Count == 0)
        //    {
        //        ViewModel.HasSelection = false;
        //        ViewModel.StudentSelectedCollection.Clear();
        //        ViewModel.ClassSelectedCollection.Clear();
        //        ViewModel.SchoolSelectedCollection.Clear();
        //        return;
        //    }
        //    ViewModel.HasSelection = true;
        //    switch (ViewModel.EntityTypeName)
        //    {
        //        case nameof(Student):
        //            ViewModel.StudentSelectedCollection = (IList<Student>?)DataGrid.SelectedItems;
        //            ViewModel.ClassSelectedCollection.Clear();
        //            ViewModel.SchoolSelectedCollection.Clear();
        //            break;
        //        case nameof(Class):
        //            ViewModel.ClassSelectedCollection = (IList<Class>?)DataGrid.SelectedItems;
        //            ViewModel.StudentSelectedCollection.Clear();
        //            ViewModel.SchoolSelectedCollection.Clear();
        //            break;
        //        case nameof(School):
        //            ViewModel.SchoolSelectedCollection = (IList<School>?)DataGrid.SelectedItems;
        //            ViewModel.StudentSelectedCollection.Clear();
        //            ViewModel.ClassSelectedCollection.Clear();
        //            break;
        //        default:
        //            throw new NotImplementedException();
        //    }
        //}
    }
}
