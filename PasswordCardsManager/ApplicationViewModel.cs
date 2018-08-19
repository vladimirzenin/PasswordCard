using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PasswordCardsManager
{
    class ApplicationViewModel : INotifyPropertyChanged
    {
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

        private string defautResultText = "Ctrl + C to copy; Ctrl+Enter to copy and hidden; Ready...";
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
            
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
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
