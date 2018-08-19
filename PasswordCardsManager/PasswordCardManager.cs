using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace PasswordCardsManager
{
    /// <summary>
    /// 1. Получение данных по их координатам
    /// 2. Работа с файлами - чтение, запись, шифрование
    /// </summary>
    public static class PasswordCardManager
    {
        private static bool isInit;

        public static Pass pass;

        public static void Init()
        {
            if (isInit)
                throw new Exception("Повторный вызов PasswordCardManager.Init запрещен");

            pass = Read();

            if (pass is null)
            {
                pass = new Pass(99, 99);
                Save(pass, "1234");
            }

            isInit = true;
        }

        public static string GetPass(string command, string error)
        {
            if (String.IsNullOrEmpty(command))
                return null;

            char[] forsplit = new char[] { ' ', '.', ',', ';' };
            string[] stringCommands = command.Split(forsplit, StringSplitOptions.RemoveEmptyEntries);

            if (stringCommands.Count() < 2)
            {
                error += Environment.NewLine;
                error += "Wrong command";
                return null;
            }

            bool simpleMode = stringCommands.Count() == 2;

            Commands commands = GetCommands(stringCommands, pass.width, pass.height, simpleMode);
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
                    int[] res = NextCoord(commands.line, commands.column, pass.height, pass.width, commands.vector, simpleMode);
                    commands.column = res[0];
                    commands.line = res[1];
                }

                matrixString += pass.matrix[commands.line, commands.column];
            }

            return matrixString;
        }

        // Получает следующую координату, сдвигает переданные строку и колонку на одну позицию
        public static int[] NextCoord(int line, int column, int height, int width, int vector, bool simpleMode)
        {
            int returnColumn = column;
            int returnLine = line;
            // vector [0..7] - направление считывания символов. 
            // 0 - слева на право, значение по умолчанию
            // 1 - слева на право и вниз
            // 2 - вниз
            // ...
            // 4 - справа на лево.
            switch (vector)
            {
                case 0:
                    returnColumn++;
                    break;
                case 1:
                    returnLine++;
                    returnColumn++;
                    break;
                case 2:
                    returnLine++;
                    break;
                case 3:
                    returnLine++;
                    returnColumn--;
                    break;
                case 4:
                    returnColumn--;
                    break;
                case 5:
                    returnLine--;
                    returnColumn--;
                    break;
                case 6:
                    returnLine--;
                    break;
                case 7:
                    returnLine--;
                    returnColumn++;
                    break;
                default:
                    Console.WriteLine("Wrong vector");
                    break;
            }

            // В простом режиме реализуется перевод строки
            if (simpleMode)
            {
                if (returnColumn == width)
                {
                    returnColumn = 0;
                    returnLine++;
                }
                if (returnColumn < 0)
                {
                    returnColumn = width - 1;
                    returnLine--;
                }

                if (returnLine == height)
                    returnLine = 0;
                if (returnLine < 0)
                    returnLine = height - 1;
            }
            // В сложном режиме реализуется закольцовка по указанному вектору
            else
            {
                if (returnColumn == width)
                    returnColumn = 0;
                if (returnColumn < 0)
                    returnColumn = width - 1;

                if (returnLine == height)
                    returnLine = 0;
                if (returnLine < 0)
                    returnLine = height - 1;
            }

            int[] returnValue = new int[2] { returnColumn, returnLine };
            return returnValue;
        }

        public static int inverseVector(int vector)
        {
            if (vector + 4 <= 7)
                return vector + 4;
            else
                return vector - 4;
        }

        public static Commands GetCommands(string[] commands, int width, int height, bool simpleMode)
        {
            int line, column, count, vector = 0;
            bool resA, resB, resC;
            string error = "";

            if (simpleMode)
            {
                line = 1;
                resA = true;
                resB = int.TryParse(commands[0], out column);
                resC = int.TryParse(commands[1], out count);
            }
            else
            {
                resA = int.TryParse(commands[0], out line);
                resB = int.TryParse(commands[1], out column);
                resC = int.TryParse(commands[2], out count);
            }

            // Отрицательные координаты означают необходимость выбрать "с конца"
            if (line < 0)
                line = height - line;
            if (column < 0)
                column = width - column;

            // В простом режиме стартовая позиция может быть выше длины первой строки - матрица разворачивается
            if (simpleMode && column > width)
            {
                int ln = column / height; // тип int отбрасывает дробную часть
                column = column - (ln * height);

                ln++;
                line = ln;
            }

            // Пользователь вводит информацию в позициях, а не в индексах, переводим в индексы
            line--;
            column--;

            // Вектор выбора символов. По умолчанию - слева на право.
            if (commands.Count() > 3)
            {
                int.TryParse(commands[3], out vector);
            }

            // Если количество выбираемых символов отрицательно - значит нужно выбирать в обратную от заданного вектора сторону.
            if (count < 0)
            {
                vector = inverseVector(vector);
                count = -count;
            }

            if (!resA || !resB || !resC)
            {
                error = "Wrong parse commands";
            }
            else if (
                (line < 0 || line >= height)
                || (column < 0 || column >= width)
                || (vector < 0 || vector > 7)
                || (count <= 0 || count > (width * height))
                )
            {
                error = "Wrong values";
            }

            Commands returnValue = new Commands(line, column, count, vector, error);

            return returnValue;
        }

        public static Pass Read()
        {
            if (File.Exists("pass.map"))
            {
                StreamReader stream = new StreamReader("pass.map");

                var publicName = stream.ReadLine();
                var publicDescription = stream.ReadLine();
                var encrypted = bool.Parse(stream.ReadLine());
                var height = int.Parse(stream.ReadLine());
                var width = int.Parse(stream.ReadLine());

                var textFile = stream.ReadToEnd();

                var stringToCheckEncrypt = textFile.Substring(0, 13);

                bool decryptSuccess = true;
                if (encrypted)
                {
                    textFile = StringCipher.Decrypt(textFile, "1234");
                    decryptSuccess = textFile.StartsWith("check encrypt");
                }

                string[] separator = { Environment.NewLine };
                var lines = textFile.Split(separator, StringSplitOptions.RemoveEmptyEntries);

                char[,] matrix = new char[height, width];

                int h = 0;
                for (int i = 1; i < height + 1; i++)
                {
                    int w = 0;

                    var line = lines[i];

                    foreach (char item in line)
                    {
                        matrix[h, w] = item;
                        w++;
                        Console.Write(item);
                    }

                    h++;

                    Console.WriteLine();
                }

                Pass returnValue = new Pass();
                returnValue.height = height;
                returnValue.width = width;
                returnValue.matrix = matrix;

                return returnValue;
            }

            return null;
        }

        public static void Save(Pass pass, string password)
        {
            bool encrypt = !string.IsNullOrEmpty(password);

            StringBuilder publicInfo = new StringBuilder();
            publicInfo.AppendLine(pass.publicName);
            publicInfo.AppendLine(pass.publicDescription);
            publicInfo.AppendLine(encrypt.ToString());

            publicInfo.AppendLine(pass.height.ToString());
            publicInfo.AppendLine(pass.width.ToString());

            StringBuilder privateInfo = new StringBuilder();
            privateInfo.AppendLine("check encrypt");
            privateInfo.AppendLine(pass.privateDescription);

            for (int i = 0; i < pass.height; i++)
            {
                string line = "";
                for (int j = 0; j < pass.width; j++)
                {
                    line += pass.matrix[i, j];
                }
                privateInfo.AppendLine(line);
            }

            var toSavePublic = publicInfo.ToString();
            var toSavePrivate = privateInfo.ToString();

            if (true)
            {
                toSavePrivate = StringCipher.Encrypt(toSavePrivate, password);
            }

            StreamWriter stream = new StreamWriter(@"pass.map", false, Encoding.UTF8);
            stream.Write(toSavePublic);
            stream.Write(toSavePrivate);
            stream.Close();
        }
    }
}
