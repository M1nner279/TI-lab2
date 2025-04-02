using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Linq;
using System.Text;
using Avalonia.VisualTree;
using lab2.Models;
using lab2.ViewModels;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;

namespace lab2.Views;

public partial class MainWindow : Window
{
    private ScrollViewer _inputScrollViewer, _keyScrollViewer, _outputScrollViewer;
    private string _inputFilePath;
    private string _outputFilePath;
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
        InitializeEvents();
    }

    private void InitializeEvents()
    {
        SelectOutputButton.Click += BrowseOutputButton_Click;
        SelectInputButton.Click += BrowseInputButton_Click;
        TransformButton.Click += TransformButton_Click;

        ToStartButton.Click += GoToStartButton_Click;
        ToEndButton.Click += GoToEndButton_Click;
    }
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _inputScrollViewer = InputTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
        _keyScrollViewer = KeyStreamTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
        _outputScrollViewer = OutputTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
        if (_inputScrollViewer is not null)
        {
            _inputScrollViewer.ScrollChanged += OnScrollChanged;
        }
        if (_keyScrollViewer is not null)
        {
            _keyScrollViewer.ScrollChanged += OnScrollChanged;
        }
        if (_outputScrollViewer is not null)
        {
            _outputScrollViewer.ScrollChanged += OnScrollChanged;
        }
    }
    
    private void GoToStartButton_Click(object sender, RoutedEventArgs e)
    {
        _inputScrollViewer.Offset = new Vector(_inputScrollViewer.Offset.X, 0);
        _keyScrollViewer.Offset = new Vector(_keyScrollViewer.Offset.X, 0);
        _outputScrollViewer.Offset = new Vector(_outputScrollViewer.Offset.X, 0);
    }
    
    private void GoToEndButton_Click(object sender, RoutedEventArgs e)
    {
        double maxOffset = _inputScrollViewer.Extent.Height - _inputScrollViewer.Viewport.Height;
        _inputScrollViewer.Offset = new Vector(_inputScrollViewer.Offset.X, maxOffset);
        _keyScrollViewer.Offset = new Vector(_keyScrollViewer.Offset.X, maxOffset);
        _outputScrollViewer.Offset = new Vector(_outputScrollViewer.Offset.X, maxOffset);
    }
    
    private void OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        double newVerticalOffset = 0;
        if (sender is ScrollViewer sv)
        {
            newVerticalOffset = sv.Offset.Y;
        }
        
        if (sender is not null && !sender.Equals(_inputScrollViewer))
        {
            if (!DoubleUtil.AreClose(_inputScrollViewer.Offset.Y, newVerticalOffset))
                _inputScrollViewer.Offset = new Vector(_inputScrollViewer.Offset.X, newVerticalOffset);
        }
        if (sender is not null && !sender.Equals(_keyScrollViewer))
        {
            if (!DoubleUtil.AreClose(_keyScrollViewer.Offset.Y, newVerticalOffset))
                _keyScrollViewer.Offset = new Vector(_keyScrollViewer.Offset.X, newVerticalOffset);
        }
        if (sender is not null && !sender.Equals(_outputScrollViewer))
        {
            if (!DoubleUtil.AreClose(_outputScrollViewer.Offset.Y, newVerticalOffset))
                _outputScrollViewer.Offset = new Vector(_outputScrollViewer.Offset.X, newVerticalOffset);
        }
    }
    
    [Obsolete("Obsolete")]
    private async void BrowseInputButton_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog dialog = new OpenFileDialog();
        dialog.AllowMultiple = false;
        var result = await dialog.ShowAsync(this);
        if (result != null && result.Length > 0)
        {
            _inputFilePath = result[0];
        }
        InputFile.Text = _inputFilePath;
    }

    [Obsolete("Obsolete")]
    private async void BrowseOutputButton_Click(object sender, RoutedEventArgs e)
    {
        SaveFileDialog dialog = new SaveFileDialog();
        string result = await dialog.ShowAsync(this);
        if (result != null && result.Length > 0)
            _outputFilePath = result;
        OutputFile.Text = _outputFilePath;
    }
    
    

    private async void TransformButton_Click(object sender, RoutedEventArgs e)
    {
        //check key
        string input = "";
        if (KeyInput.Text is not null)
        {
            input = FilterInput(KeyInput.Text);
            KeyInput.Text = input;
        }
        if (input.Length < 32)
        {
            var messageBox = MessageBoxManager.GetMessageBoxStandard(new MessageBoxStandardParams
            {
                ContentTitle = "Ошибка",
                ContentMessage = "Ключ должен содержать ровно 32 символа '0' или '1'.",
                ButtonDefinitions = ButtonEnum.Ok,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            });
            await messageBox.ShowWindowDialogAsync(this);
            return;
        }
        
        
        //check inputFile
        if (string.IsNullOrEmpty(_inputFilePath) || !File.Exists(_inputFilePath))
        {
            var messageBox = MessageBoxManager.GetMessageBoxStandard(new MessageBoxStandardParams
            {
                ContentTitle = "Ошибка",
                ContentMessage = "Не указан файл для чтения",
                ButtonDefinitions = ButtonEnum.Ok,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            });
            await messageBox.ShowWindowDialogAsync(this);
            return;
        }
        
        //check outputFile
        if (string.IsNullOrEmpty(_outputFilePath))
        {
            var messageBox = MessageBoxManager.GetMessageBoxStandard(new MessageBoxStandardParams
            {
                ContentTitle = "Ошибка",
                ContentMessage = "Не указан файл для записи",
                ButtonDefinitions = ButtonEnum.Ok,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            });
            await messageBox.ShowWindowDialogAsync(this);
            return;
        }
        
        Encrypt proccess = new Encrypt();
        try
        {
            uint seed = Convert.ToUInt32(input, 2);
            proccess.Convert(_inputFilePath, _outputFilePath, seed);
        }
        catch (Exception ex)
        {
            var messageBox = MessageBoxManager.GetMessageBoxStandard(new MessageBoxStandardParams
            {
                ContentTitle = "Ошибка",
                ContentMessage = ex.Message,
                ButtonDefinitions = ButtonEnum.Ok,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            });
            await messageBox.ShowWindowDialogAsync(this);
            return;
        }
        string inputBinary, keyBinary, outputBinary;

        if (proccess.Input.Count <= 16)
        {
            // Если файл <= 16 байт, отображаем все данные без многоточия
            inputBinary = string.Join("\n", proccess.Input.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));
            keyBinary = string.Join("\n", proccess.Key.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));
            outputBinary = string.Join("\n", proccess.Result.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));
        }
        else
        {
            // Если файл > 16 байт, отображаем первые 16, многоточие и последние 16
            inputBinary = string.Join("\n", proccess.Input.Take(16).Select(b => Convert.ToString(b, 2).PadLeft(8, '0'))) 
                          + "\n\n...\n\n" 
                          + string.Join("\n", proccess.Input.Skip(16).Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));
            keyBinary = string.Join("\n", proccess.Key.Take(16).Select(b => Convert.ToString(b, 2).PadLeft(8, '0'))) 
                        + "\n\n...\n\n" 
                        + string.Join("\n", proccess.Key.Skip(16).Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));
            outputBinary = string.Join("\n", proccess.Result.Take(16).Select(b => Convert.ToString(b, 2).PadLeft(8, '0'))) 
                           + "\n\n...\n\n" 
                           + string.Join("\n", proccess.Result.Skip(16).Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));
        }
        
        //show input File
        InputTextBox.Text = inputBinary;
        //show key
        KeyStreamTextBox.Text = keyBinary;
        //show result
        OutputTextBox.Text = outputBinary;
    }
    
    private string FilterBinaryString(string input)
    {
        StringBuilder result = new StringBuilder();
        foreach (char c in input)
        {
            if (c == '0' || c == '1')
            {
                result.Append(c);
            }
        }
        return result.ToString();
    }
    public string FilterInput(string input)
    {
        input = FilterBinaryString(input);
        if (input.Length > 32)
        {
            input = input.Substring(0, 32);
        }
        return input;
    }
}