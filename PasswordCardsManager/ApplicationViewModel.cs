using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;

namespace PasswordCardsManager
{
    /// <summary>
    /// 1. Хранение полученных результатов и служебных данных
    /// 2. Работа с любыми пользовательскими представлениями - 
    ///     - формы
    ///     - иконки
    ///     - комбинации клавиш
    /// </summary>
    class ApplicationViewModel : INotifyPropertyChanged
    {
        public MainWindow mainWindow;

        System.Windows.Forms.NotifyIcon notifyIcon;

        static List<int> idRegisteredHotKeys;

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool ShowWindow(int hWnd, int nCmdShow);

        private string textMessage;
        public string TextMessage
        {
            get { return textMessage; }
            set
            {
                textMessage = value;
                OnPropertyChanged("TextMessage");
            }
        }

        private string commandString;
        public string CommandString
        {
            get { return commandString; }
            set
            {
                var input = value;

                string empty   = "^$";
                string numbers = "[\\d]{1,3}";
                string spaces  = "[,|.| ]{1}";

                string newline = "|^";
                string endline = "$";

                string pattern = empty
                    + newline + numbers + endline
                    + newline + numbers + spaces + endline
                    + newline + numbers + spaces + numbers + endline
                    + newline + numbers + spaces + numbers + spaces + endline
                    + newline + numbers + spaces + numbers + spaces + numbers + endline
                    + newline + numbers + spaces + numbers + spaces + numbers + spaces + endline
                    + newline + numbers + spaces + numbers + spaces + numbers + spaces + numbers + endline;

                Regex rgx = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                var matches = rgx.IsMatch(input);
                if (!matches)
                {
                    return;
                }

                if (commandString != input)
                {
                    commandString = input;
                    GetPass.Execute(null);
                }

                OnPropertyChanged("CommandString");
            }
        }

        private bool isResult;
        public bool IsResult
        {
            get
            {
                return isResult;
            }
            set
            {
                isResult = value;
            }
        }
        
        private string result;
        public string Result
        {
            get
            {
                return result;
            }
            set
            {
                result = value;
            }
        }

        public void ClearResult()
        {
            isResult = false;
            result = null;
            TextMessage = defautResultText;

            CommandString = "";
        }

        private string defautResultText = "Ctrl+Enter to copy and hidden; Ready <X.Y.L.r>"; //"Ctrl + C to copy; Ctrl+Enter to copy and hidden; Ready...";
        private string defautErrorText = "no value; waiting for the right command";

        private RelayCommand getPass;
        public RelayCommand GetPass
        {
            get
            {
                return getPass ??
                  (getPass = new RelayCommand(obj =>
                  {
                      string error = ""; // todo подумать как использовать, сейчас никуда не выводится

                      var matrixString = PasswordCardManager.GetPass(CommandString, error);

                      isResult = !string.IsNullOrEmpty(matrixString);

                      if (string.IsNullOrEmpty(CommandString))
                      {
                          TextMessage = defautResultText;
                      }
                      else if (isResult)
                      {
                          result = matrixString;
                          TextMessage = matrixString;
                      }
                      else
                      {
                          result = null;
                          TextMessage = defautErrorText;
                      }
                  }));
            }
        }

        public ApplicationViewModel()
        {
            RegisterHotKeys();

            CreateTrayMenu();

            TextMessage = defautResultText;
        }

        void UnInit()
        {
            notifyIcon.Dispose();

            UnRegisterHotKeys();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        #region Трей меню
        private void CreateTrayMenu()
        {
            var components = new System.ComponentModel.Container();

            var contextMenu = new System.Windows.Forms.ContextMenu();
            var menuItemOpen = new System.Windows.Forms.MenuItem();
            var menuItemSettings = new System.Windows.Forms.MenuItem();
            var menuItemSeparete = new Separator();
            var menuItemExit = new System.Windows.Forms.MenuItem();

            // Initialize menuItem1
            menuItemOpen.Index = 0;
            menuItemOpen.Text = "O&pen";
            menuItemOpen.Click += new System.EventHandler(this.notifyIcon_Open);

            // todo
            //menuItemSettings.Index = 1;
            //menuItemSettings.Text = "S&ettings";
            //menuItemSettings.Click += new System.EventHandler(this.notifyIcon_Settings);

            menuItemExit.Index = 3;
            menuItemExit.Text = "E&xit";
            menuItemExit.Click += new System.EventHandler(this.notifyIcon_Exit);

            // Initialize contextMenu
            contextMenu.MenuItems.Add(menuItemOpen);
            contextMenu.MenuItems.Add("-");
            contextMenu.MenuItems.Add(menuItemExit);

            // Create the NotifyIcon.
            notifyIcon = new System.Windows.Forms.NotifyIcon(components);

            // The Icon property sets the icon that will appear
            // in the systray for this application.
            notifyIcon.Icon = Properties.Resources.icon;

            // The ContextMenu property sets the menu that will
            // appear when the systray icon is right clicked.
            notifyIcon.ContextMenu = contextMenu;

            // The Text property sets the text that will be displayed,
            // in a tooltip, when the mouse hovers over the systray icon.
            notifyIcon.Text = "Password cards system";
            notifyIcon.Visible = true;

            // Handle the DoubleClick event to activate the form.
            notifyIcon.DoubleClick += new System.EventHandler(this.notifyIcon_Open);
        }

        private void notifyIcon_Open(object Sender, EventArgs e)
        {
            // Show the form when the user double clicks on the notify icon.

            // Set the WindowState to normal if the form is minimized.
            if (mainWindow.WindowState == WindowState.Minimized)
                mainWindow.WindowState = WindowState.Normal;

            if (!mainWindow.IsVisible)
                mainWindow.Show();

            mainWindow.Activate();
        }

        private void notifyIcon_Exit(object Sender, EventArgs e)
        {
            UnInit();

            System.Windows.Application.Current.Shutdown();
        }
        #endregion

        #region Комбинации клавиш

        void RegisterHotKeys()
        {
            if (idRegisteredHotKeys != null
                && idRegisteredHotKeys.Count() > 0)
                throw new Exception("Повторная регистрация горячих клавиш запрещена (метод RegisterHotKeys)");

            idRegisteredHotKeys = new List<int>();

            // Скопировать результат
            //var id = HotKeyManager.RegisterHotKey(Keys.C, KeyModifiers.Control);
            // Скопировать результат и закрыть форму
            var id2 = HotKeyManager.RegisterHotKey(Keys.Enter, KeyModifiers.Control);
            // Скрыть/показать форму
            var id3 = HotKeyManager.RegisterHotKey(Keys.T, KeyModifiers.Alt);

            HotKeyManager.HotKeyPressed += new EventHandler<HotKeyEventArgs>(HotKeyManager_Pressed);

            //idRegisteredHotKeys.Add(id);
            idRegisteredHotKeys.Add(id2);
            idRegisteredHotKeys.Add(id3);
        }

        private static void UnRegisterHotKeys()
        {
            foreach (var item in idRegisteredHotKeys)
            {
                HotKeyManager.UnregisterHotKey(item);
            }
        }

        void HotKeyManager_Pressed(object sender, HotKeyEventArgs e)
        {
            if (e.Key == Keys.C)
            {
                mainWindow.Dispatcher.Invoke(
                () =>
                {
                    if (mainWindow.IsVisible
                        && mainWindow.IsActive
                        && (mainWindow.DataContext as ApplicationViewModel).IsResult)
                    {
                        System.Windows.Clipboard.SetDataObject((mainWindow.DataContext as ApplicationViewModel).Result);
                    }
                }
                );
            }
            else if (e.Key == Keys.Enter)
            {
                CopyAndHide();
            }
            else if (e.Key == Keys.T)
            {
                ShowHide();
            }
        }

        void CopyAndHide()
        {
            mainWindow.Dispatcher.Invoke(
                () =>
                {
                    if (mainWindow.IsVisible
                        && mainWindow.IsActive
                        && (mainWindow.DataContext as ApplicationViewModel).IsResult)
                    {
                        System.Windows.Clipboard.SetDataObject((mainWindow.DataContext as ApplicationViewModel).Result);
                        ShowHide();
                        (mainWindow.DataContext as ApplicationViewModel).ClearResult();
                    }
                }
                );
        }

        void ShowHide()
        {
            mainWindow.Dispatcher.Invoke(
                () =>
                {
                    var isVisible = mainWindow.IsVisible;

                    var wins = App.Current.Windows;

                    // Если главное окно видимо - скрываем все окна, иначе показываем только главное

                    foreach (Window item in wins)
                    {
                        if (isVisible)
                        {
                            item.Hide();
                        }
                        else if (item == mainWindow)
                        {
                            item.Show();
                            (item as MainWindow).SetFocusCommandBox();
                        }
                    }
                }
            );
        }

        #endregion
    }

    public class RelayCommand : ICommand
    {
        private Action<object> execute;
        private Func<object, bool> canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return this.canExecute == null || this.canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            this.execute(parameter);
        }
    }
}
