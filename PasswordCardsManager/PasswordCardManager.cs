using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace PasswordCardsManager
{
    public static class PasswordCardManager
    {
        public static Pass pass;

        //public static System.Windows.Application WinApp { get; private set; }
        public static MainWindow mainWindow;

        static List<int> idRegisteredHotKeys;

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool ShowWindow(int hWnd, int nCmdShow);

        public static void Init()
        {
            idRegisteredHotKeys = new List<int>();

            pass = Pass.Read();

            if (pass is null)
            {
                pass = new Pass(9, 9);
                Pass.Save(pass, "1234");
            }

            RegisterHotKeys();
        }

        public static void UnInit()
        {
            UnRegisterHotKeys();
        }

        private static void RegisterHotKeys()
        {
            // Скопировать результат
            var id = HotKeyManager.RegisterHotKey(Keys.C, KeyModifiers.Control);
            // Скопировать результат и закрыть форму
            var id2 = HotKeyManager.RegisterHotKey(Keys.Enter, KeyModifiers.Control);
            // Скрыть/показать форму
            var id3 = HotKeyManager.RegisterHotKey(Keys.T, KeyModifiers.Alt);

            HotKeyManager.HotKeyPressed += new EventHandler<HotKeyEventArgs>(HotKeyManager_Pressed);

            idRegisteredHotKeys.Add(id);
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

        static void HotKeyManager_Pressed(object sender, HotKeyEventArgs e)
        {
            if (e.Key == Keys.C)
            {
                if (mainWindow.IsVisible
                    && mainWindow.isResult)
                {
                    App.Current.Dispatcher.Invoke(
                    () =>
                        {
                            System.Windows.Clipboard.SetDataObject(mainWindow.result);
                        }
                    );
                }
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

        public static bool CopyAndHide()
        {
            if (mainWindow.IsVisible
                && mainWindow.isResult)
            {
                App.Current.Dispatcher.Invoke(
                    () =>
                        {
                            System.Windows.Clipboard.SetDataObject(mainWindow.result);
                            ShowHide();
                            mainWindow.ClearResult();

                            return true;
                        }
                    );
            }

            return false;
        }

        private static void ShowHide()
        {
            App.Current.Dispatcher.Invoke(
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
                        else if(item == mainWindow)
                        {
                            item.Show();
                            (item as MainWindow).SetFocusCommandBox();
                        }
                    }
                }
            );
        }

        public static string GetPass(string command, string error)
        {
            if (String.IsNullOrEmpty(command))
                return null;

            char[] forsplit = new char[] { ' ', '.', ',', ';' };
            string[] stringCommands = command.Split(forsplit);

            if (stringCommands.Count() < 2)
            {
                error += Environment.NewLine;
                error += "Wrong command";
                return null;
            }

            bool simpleMode = stringCommands.Count() == 2;

            Commands commands = Pass.GetCommands(stringCommands, pass.width, pass.height, simpleMode);
            if (!String.IsNullOrEmpty(commands.error))
            {
                error += Environment.NewLine;
                error += commands.error;
                return null;
            }

            string matrixString = "";

            for (int i = 0; i < commands.count; i++)
            {
                if (i > 0)
                {
                    int[] res = Pass.NextCoord(commands.line, commands.column, pass.height, pass.width, commands.vector, simpleMode);
                    commands.column = res[0];
                    commands.line = res[1];
                }

                matrixString += pass.matrix[commands.line, commands.column];
            }

            return matrixString;
        }
    }
}
