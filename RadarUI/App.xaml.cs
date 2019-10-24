using log4net;
using MahApps.Metro;
using Microsoft.Win32;
using RadarUI.Windows;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace RadarUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static MainWindow app;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Log.Debug("Started New Session Logging *******************************************************");


            ThemeManager.AddAppTheme("DarkTheme", new Uri("pack://application:,,,/Style/DarkTheme.xaml"));
            ThemeManager.AddAppTheme("LightTheme", new Uri("pack://application:,,,/Style/LightTheme.xaml"));

            EventManager.RegisterClassHandler(
                typeof(TextBox), TextBox.GotFocusEvent, new RoutedEventHandler(TextBox_GotFocus));
            EventManager.RegisterClassHandler(
                typeof(TextBox), TextBox.PreviewMouseDownEvent, new RoutedEventHandler(TextBox_PreviewMouseDown));

            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");


            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

            // For catching Global uncaught exception
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledExceptionOccured);

            Log.Info("Application Startup");
            LogMachineDetails();
            LoadPreviousSessionParameters();
            app = new MainWindow();
            app.Show();

        }

        static void UnhandledExceptionOccured(object sender, UnhandledExceptionEventArgs args)
        {
            // Here change path to the log.txt file
            var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) 
                + "\\Documents\\RadarDemo\\App.log";

            // Show a message before closing application
            var dialogService = new MvvmDialogs.DialogService();
            dialogService.ShowMessageBox((INotifyPropertyChanged)(app.DataContext),
                "Oops, something went wrong and the application must close. Please find a " +
                "report on the issue at: " + path + Environment.NewLine +
                "If the problem persist, please contact your local system supplier.",
                "Unhandled Error",
                MessageBoxButton.OK);

            Exception e = (Exception)args.ExceptionObject;
            Log.Fatal("Application has crashed", e);
        }

        private void LogMachineDetails()
        {
            var computer = new Microsoft.VisualBasic.Devices.ComputerInfo();

            string text = "OS: " + computer.OSPlatform + " v" + computer.OSVersion + " " + computer.OSFullName +
                          "; RAM: " + computer.TotalPhysicalMemory.ToString() +
                          "; Language: " + computer.InstalledUICulture.EnglishName;
            Log.Info(text);
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            Log.Debug("TextBox Got Focus - Select All");
            (sender as TextBox).SelectAll();
        }

        private void TextBox_PreviewMouseDown(object sender, RoutedEventArgs e)
        {
            Log.Debug("Mouse Down - Got Focus & Select All");
            TextBox textBox = sender as TextBox;

            if (!textBox.IsFocused)
            {
                textBox.Focus();
                textBox.SelectAll();
                e.Handled = true;
            }
        }

        private void LoadPreviousSessionParameters()
        {
            Log.Debug("Load Previous Session Parameters");
            RegistryKey subKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\RadarDemo\PrevSesParameters");
            if (subKey == null)
                subKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\RadarDemo\PrevSesParameters");

            ThemeManager.ChangeAppStyle(Application.Current,
                ThemeManager.GetAccent(subKey.GetValue("Accent", "Blue").ToString()),
                ThemeManager.GetAppTheme(subKey.GetValue("AppTheme", "DarkTheme").ToString()));
            subKey.Close();

        }
    }
}
