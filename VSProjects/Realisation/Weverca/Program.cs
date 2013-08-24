﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca
{
    class Program
    {
        /// <summary>
        /// Relative path to the trunk folder in SVN
        /// MUST BE CHANGED IN CASE OF CHANDES IN SVN DIRECTORY STRUCTURE
        /// </summary>
        public static readonly string TRUNK_PATH = @"..\..\..\..\..\";

        /// <summary>
        /// Directory with PHP sources
        /// </summary>
        public static readonly string PHP_SOURCES_DIR = TRUNK_PATH + @"PHP_sources\";

        /// <summary>
        /// Startup method for Weverca
        /// </summary>
        /// <param name="args">TODO: Specification of arguments</param>
        static void Main(string[] args)
        {
            if (args.Count() < 1)
            {
                Console.WriteLine("Missing argument");
                Console.WriteLine(@"Example of usage: weverca.exe ..\..\..\..\..\PHP_sources\test_programs\testfile.php ");
                Console.ReadKey();
                return;
            }
            
            //TODO: Resolve entry file
            var analyzedFile = args[0];

            //Process analysis
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var ppGraph = Analyzer.Run(analyzedFile);
            watch.Stop();

            //Build output
            var console = new ConsoleOutput();
            console.CommentLine(string.Format("Analysis completed in: {0}ms\n",watch.ElapsedMilliseconds));
            console.ProgramPointInfo("Start", ppGraph.Start);
            console.ProgramPointInfo("End", ppGraph.End);
                        
            Console.ReadKey();
        }
    }
}
