﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca
{
    class ConsoleOutput : OutputBase
    {
        #region Output color settings

        static readonly ConsoleColor Head = ConsoleColor.White;
        static readonly ConsoleColor Info = ConsoleColor.Gray;
        static readonly ConsoleColor Variable = ConsoleColor.Yellow;
        static readonly ConsoleColor Delimiter = ConsoleColor.Red;
        static readonly ConsoleColor Comment = ConsoleColor.Green;

        #endregion

        #region Private members

        private bool needPrefix = true;
        private string prefix = "";

        #endregion

        #region OutputBase implementation

        protected override void setIndentation(int level)
        {
            prefix = "".PadLeft(level * 2, ' ');
        }

        protected override void head(string text)
        {
            print(Head, text);
        }

        protected override void info(string text)
        {
            print(Info, text);
        }

        protected override void comment(string text)
        {
            print(Comment, text);
        }

        protected override void delimiter(string text)
        {
            print(Delimiter, text);
        }

        protected override void variable(string name)
        {
            print(Variable, name);
        }

        protected override void line()
        {
            Console.WriteLine();
            needPrefix = true;
        }
        #endregion

        #region Private utilities

        private void print(ConsoleColor color, string text)
        {
            Console.ForegroundColor = color;

            if (needPrefix)
            {
                text = prefix + text;
                needPrefix = false;
            }
            Console.Write(text);
        }

        #endregion

    }
}
