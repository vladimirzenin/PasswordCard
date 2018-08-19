using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace PasswordCardsManager
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ///// <summary>
        ///// Результат существует и успешно получен
        ///// </summary>
        //public bool isResult { get; private set; }

        ///// <summary>
        ///// Результат
        ///// </summary>
        //public string result { get; private set; }

        //private string defautResultText = "Ctrl + C to copy; Ctrl+Enter to copy and hidden; Ready...";
        //private string defautErrorText = "no value; waiting for the right command";

        //private System.Windows.Forms.NotifyIcon notifyIcon;
        //private System.Windows.Forms.ContextMenu contextMenu1;
        //private System.Windows.Forms.MenuItem menuItem1;
        //private System.ComponentModel.IContainer components;

        public MainWindow()
        {
            // 3

            InitializeComponent();

            DataContext = new ApplicationViewModel();

            PasswordCardManager.mainWindow = this;

            CreateTrayMenu();

            SetFocusCommandBox();

            return;

            //name.Text = PasswordCardManager.pass.publicName;
            //description.Text = PasswordCardManager.pass.publicDescription;

            //UpdateResult();

            //StringBuilder privateInfo = new StringBuilder();

            //DataTable table = new DataTable();

            //for (int i = 1; i < Program.pass.height + 1; i++)
            //{
            //    DataGridTextColumn column = new DataGridTextColumn();
            //    column.Header = i;
            //    column.Width = 4;
            //    table.Columns.Add(i.ToString(), typeof(Char));
            //}



            //for (int i = 0; i < Program.pass.height; i++)
            //{
            //    string line = "";
            //    char[] items = new char[Program.pass.width];

            //    var row = table.NewRow();

            //    for (int j = 0; j < Program.pass.width; j++)
            //    {
            //        line += Program.pass.matrix[i, j];
            //        //items[j] = Program.pass.matrix[i, j];


            //        //row[(i + 1).ToString()] = Program.pass.matrix[i, j];
            //    }
            //    table.Rows.Add(row);
            //    //dataGrid.Items.Add(items);



            //    privateInfo.AppendLine(line);
            //}

            //dataGrid.DataContext = table;

            //PassCardText.Text = privateInfo.ToString();

            CreateTrayMenu();

            SetFocusCommandBox();
        }

        private void CreateTrayMenu()
        {
            var components = new System.ComponentModel.Container();

            var contextMenu = new System.Windows.Forms.ContextMenu();
            var menuItemOpen = new System.Windows.Forms.MenuItem();
            var menuItemSettings = new System.Windows.Forms.MenuItem();
            var menuItemAbout = new System.Windows.Forms.MenuItem();
            var menuItemSeparete = new System.Windows.Forms.MenuItem();
            var menuItemExit = new System.Windows.Forms.MenuItem();

            // Initialize contextMenu1
            contextMenu.MenuItems.AddRange(
                        new System.Windows.Forms.MenuItem[] 
                        { menuItemOpen, menuItemSettings, menuItemSeparete, menuItemAbout, menuItemExit });

            // Initialize menuItem1
            menuItemOpen.Index = 0;
            menuItemOpen.Text = "O&pen";
            menuItemOpen.Click += new System.EventHandler(this.notifyIcon_Open);

            menuItemSettings.Index = 1;
            menuItemSettings.Text = "S&ettings";
            menuItemSettings.Click += new System.EventHandler(this.notifyIcon_Settings);

            menuItemAbout.Index = 2;
            menuItemAbout.Text = "A&bout";
            menuItemAbout.Click += new System.EventHandler(this.notifyIcon_About);

            menuItemSeparete.Index = 3;

            menuItemExit.Index = 4;
            menuItemExit.Text = "E&xit";
            menuItemExit.Click += new System.EventHandler(this.notifyIcon_Exit);

            // Create the NotifyIcon.
            var notifyIcon = new System.Windows.Forms.NotifyIcon(components);

            // The Icon property sets the icon that will appear
            // in the systray for this application.
            notifyIcon.Icon = new Icon("icon.ico");

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
            if (this.WindowState == WindowState.Minimized)
                this.WindowState = WindowState.Normal;

            if (!this.IsVisible)
                this.Show();

            this.Activate();
        }

        private void notifyIcon_Settings(object Sender, EventArgs e)
        {

        }

        private void notifyIcon_About(object Sender, EventArgs e)
        {
            
        }

        private void notifyIcon_Exit(object Sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        //private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        //{
        //    var input = e.Text;
        //    string pattern = @"[0-9]|[.,; ]";
        //    Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
        //    MatchCollection matches = rgx.Matches(input);
        //    if (matches.Count == 0)
        //    {
        //        e.Handled = true;
        //    }

        //    // Нажатие enter
        //    if (input == "\r")
        //    {
        //        CopyAndClose();
        //    }
        //}

        //private void commandBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    UpdateResult();
        //}

        //private void UpdateResult()
        //{
        //    string error = ""; // todo подумать как использовать, сейчас никуда не выводится

        //    var matrixString = PasswordCardManager.GetPass(commandBox.Text, error);

        //    isResult = !string.IsNullOrEmpty(matrixString);

        //    if (string.IsNullOrEmpty(commandBox.Text))
        //    {
        //        resultLabel.Text = defautResultText;
        //    }
        //    else if (isResult)
        //    {
        //        result = matrixString;
        //        resultLabel.Text = matrixString;
        //    }
        //    else
        //    {
        //        result = null;
        //        resultLabel.Text = defautErrorText;
        //    }
        //}

        //private void CopyAndClose()
        //{
        //    var isSuccess = PasswordCardManager.CopyAndHide();
        //    if (!isSuccess)
        //    {
        //        resultLabel.Text = defautErrorText;
        //    }
        //}

        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    UpdateResult();

        //    CopyAndClose();
        //}

        private void Button_Click_Settings(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Hide();
            e.Cancel = true;
        }

        public void SetFocusCommandBox()
        {
            commandBox.Focus();
        }
    }
}
