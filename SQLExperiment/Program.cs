using SQLExperiment.Helpers;
using SQLExperiment.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace SQLExperiment
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var __migrationCommand = new __MigrationCommand();
            __migrationCommand.MigrationsCommands = new List<string>();
            __migrationCommand.CommandLines = new List<string>();

            string commandLine = "";

            // Presentation
            Console.WriteLine("---------------------------------------------------------------------------------------------");
            Console.WriteLine("  A simple experiment to see if I cane make DB migration code based on used plain text input.");
            Console.WriteLine("  Commands work irregardles of case, synonyms replaces the actions to those below abd filler");
            Console.WriteLine("  are ignored.");
            Console.WriteLine("  Values can not be on the Transact-SQL Reserved Keywords list.");
            Console.WriteLine("");
            Console.WriteLine("  Add - table,field");
            Console.WriteLine("  Clear - resets commands");
            Console.WriteLine("  Print - Ends the loop");
            Console.WriteLine("");
            Console.WriteLine("  Exemples:");
            Console.WriteLine("  'Create Customer Table' and 'add table customer' will produce the exact same result.");
            Console.WriteLine("---------------------------------------------------------------------------------------------");
            Console.WriteLine("");

            // Command Loop
            while (commandLine.ToLower() != "print")
            {
                // Type your command line and enter.
                Console.WriteLine($" ({__migrationCommand.CommandLines.Count}) Enter Command:");
                commandLine = Console.ReadLine();

                // Process latest command line
                __migrationCommand = MigrationCommandLines.ProcessLine(__migrationCommand, commandLine);

                // Present what you got so far
                Console.WriteLine("");
                foreach ( var line in  __migrationCommand.MigrationsCommands )
                {
                    Console.WriteLine(line);
                }
                Console.WriteLine("");

            }

            // Migration class code Output
            Console.WriteLine("--- Print CommandLines ----------------------------------------------");
            Console.WriteLine("");
            foreach (var line in __migrationCommand.CommandLines)
            {
                Console.WriteLine(line);
            }
            Console.WriteLine("");
            Console.WriteLine("--- Print Code ----------------------------------------------");
            Console.WriteLine("");
            foreach (var line in __migrationCommand.MigrationsCommands)
            {
                Console.WriteLine(line);
            }
            Console.WriteLine("");

            // Quit
            Console.WriteLine("Press any key to quit");
            var key = Console.ReadKey();
        }
    }
}
