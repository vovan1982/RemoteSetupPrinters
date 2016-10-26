using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RemoteSetupPrinters.ReportWindow
{
    /// <summary>
    /// Логика взаимодействия для ReportWindow.xaml
    /// </summary>
    public partial class ReportWindow : Window
    {
        #region Конструктор
        public ReportWindow()
        {
            InitializeComponent();
        }

        public ReportWindow(string reportText)
        {
            InitializeComponent();
            int infoMessageCount = 0;
            string[] arrInput = reportText.Split('#');
            for (int i = 0; i < arrInput.Length; i++)
            {
                string type = arrInput[i].Substring(0, arrInput[i].IndexOf("\n")).TrimEnd('\r', '\n');
                switch (type)
                {
                    case "error":
                        string errorData = arrInput[i].Substring(7);
                        if (!string.IsNullOrWhiteSpace(errorData))
                        {
                            errorExpander.Header = "Во время подключения возникли ошибки!!";
                            errorExpander.Foreground = Brushes.Red;
                            errorDataText.Text = errorData;
                        }
                        break;
                    case "offline":
                        string offlineData = arrInput[i].Substring(9);
                        if (!string.IsNullOrWhiteSpace(offlineData))
                        {
                            offlineExpander.Header = "Компьютеры не в сети " + offlineData.Split(',').Length;
                            offlineTextData.Text = offlineData;
                        }
                        break;
                    case "connect":
                        string connectData = arrInput[i].Substring(9);
                        string[] arrConnectData = connectData.Split('=');
                        Expander newExpander = createExpanderConnect(arrConnectData[0].TrimEnd('\r', '\n'), arrConnectData[1]);
                        if (arrConnectData[1].Contains("Ошибка") || arrConnectData[1].Contains("Код  ошибки"))
                        {
                            newExpander.Foreground = Brushes.Red;
                            newExpander.Header = newExpander.Header + " есть ошибки!!";
                        }
                        rootStackPanel.Children.Add(newExpander);
                        break;
                    case "info":
                        string infoData = arrInput[i].Substring(6);
                        if (!string.IsNullOrWhiteSpace(infoData))
                        {
                            if (infoMessageCount <= 0)
                                infoTextData.Text = "";
                            infoMessageCount++;
                            infoExpander.Header = "Информационные сообщения " + infoMessageCount;
                            infoTextData.Text += infoData;
                        }
                        break;
                }
            }
        }
        #endregion
        #region Методы
        private Expander createExpanderConnect(string header, string data)
        {
            Expander dynamicExpander = new Expander();
            dynamicExpander.Header = header;
            dynamicExpander.Foreground = Brushes.Green;
            Border dinamicBorder = new Border();
            dinamicBorder.BorderBrush = Brushes.Black;
            dinamicBorder.BorderThickness = new Thickness(1);
            dinamicBorder.Height = 350;
            dinamicBorder.Margin = new Thickness(10, 0, 8, 0);
            ScrollViewer dinamicScrollViewer = new ScrollViewer();
            dinamicScrollViewer.Margin = new Thickness(1);
            TextBox dinamicTextBox = new TextBox();
            dinamicTextBox.IsReadOnly = true;
            dinamicTextBox.TextWrapping = TextWrapping.Wrap;
            dinamicTextBox.Foreground = Brushes.Black;
            dinamicTextBox.Text = data;
            dinamicScrollViewer.Content = dinamicTextBox;
            dinamicBorder.Child = dinamicScrollViewer;
            dynamicExpander.Content = dinamicBorder;
            return dynamicExpander;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion
    }
}
