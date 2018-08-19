using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordCardsManager
{
    /// <summary>
    /// Содержит данные файла с парольной картой
    /// </summary>
    public class Pass
    {
        public string publicName = "default name";
        public string publicDescription = "default description";

        public bool encrypted;

        public int height = 9;
        public int width = 9;

        public string privateDescription;

        public char[,] matrix;

        public Pass()
        {

        }

        public Pass(int width, int height)
        {
            this.height = height;
            this.width = width;

            matrix = new char[height, width];

            Random rand = new Random();
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    char letter;
                    while (true)
                    {
                        letter = (char)rand.Next(0x0030, 0x007A);
                        if (char.IsLetterOrDigit(letter))
                            break;
                    }

                    matrix[i, j] = letter;
                    Console.Write(matrix[i, j]);
                }
                Console.WriteLine();
            }
        }
    }

    public struct Commands
    {
        public int line;
        public int column;
        public int count;
        public int vector;
        public string error;

        public Commands(int _line, int _column, int _count, int _vector, string _error)
        {
            line = _line;
            column = _column;
            count = _count;
            vector = _vector;
            error = _error;
        }
    }


}
