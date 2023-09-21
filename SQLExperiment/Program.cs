using SQLExperiment.Helpers;
using SQLExperiment.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLExperiment
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string command = "";
            List<string> commands = new List<string>();
            var table = new __DatabaseTable();


            // Presentation
            Console.WriteLine("---------------------------------------------------------------------------------------------");
            Console.WriteLine("  A simple experiment to see if I cane make DB migration code based on used plain text input.");
            Console.WriteLine("");
            Console.WriteLine("  Add - table");
            Console.WriteLine("  [Clear] - resets commands");
            Console.WriteLine("  [Print] - Ends the loop");
            Console.WriteLine("---------------------------------------------------------------------------------------------");
            Console.WriteLine("");

            // Command Loop
            while (command.ToLower() != "print")
            {

                // Type your username and press enter
                Console.WriteLine($" ({commands.Count}) Éntrer Command:");

                // Create a string variable and get user input from the keyboard and store it in the variable
                command = Console.ReadLine();
                var commandWords = command.Split(' ').ToList();


                // Print the value of the variable (userName), which will display the input value
                Console.WriteLine("Command is: " + command);

                // Commands
                if (command.ToLower() == "clear")
                {
                    // Reset all work
                    commands = new List<string>();
                    table = new __DatabaseTable();
                    continue;
                }
                else
                {
                    // Word processing
                    // Todo.. Remove filler words.
                    // Convert synonums to commands.

                    // Command Validation
                    try
                    {
                        var error = true;
                        // Add Table command
                        if (commandWords.Contains("add") &&
                            commandWords.Contains("table") &&
                            commandWords.Count > 2)
                        {
                            int whereIsTable = commandWords.FindIndex(a => a.Contains("table"));
                            table.Table_Name = WordProcess.Capitalize(commandWords[whereIsTable - 1]);
                            table.Columns = new List<__DatabaseTableColumn>
                            {
                                new __DatabaseTableColumn { IsPrimaryKey= true, ColumnName = table.Table_Name + "ID", DataType ="Guid", IsNullable=false},
                                new __DatabaseTableColumn { ColumnName = "Name", DataType ="String", IsNullable=true, CharacterMaximumLength =50},
                                new __DatabaseTableColumn { ColumnName = "Created", DataType = "DateTime", IsNullable=false},
                                new __DatabaseTableColumn { ColumnName = "CreatedBy", DataType = "Guid", IsNullable = false},
                                new __DatabaseTableColumn { ColumnName = "Modified", DataType = "DateTime", IsNullable=true},
                                new __DatabaseTableColumn { ColumnName = "ModifiedBy", DataType = "Guid", IsNullable=true},
                                new __DatabaseTableColumn { ColumnName = "Removed", DataType = "DateTime", IsNullable=true},
                                new __DatabaseTableColumn { ColumnName = "RemovedBy", DataType = "Guid", IsNullable=true},
                            };
                            error = false;
                        }


                        if (error)
                        {
                            Console.WriteLine("Invallid command! ");
                            continue;
                        }
                        else
                        {
                            Console.WriteLine("Verified! ");
                            commands.Add(command);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Broken command! {ex.InnerException.Message}");
                        continue;
                    }
                }

                // Table Output
                if (table.Table_Name != "")
                {
                    Console.WriteLine("-- Table & colums --");
                    Console.WriteLine($"  Table: {table.Table_Name}");
                    foreach (var column in table.Columns)
                    {
                        Console.WriteLine($"  {(column.IsPrimaryKey == true ? "pk" : "  ")} {column.ColumnName} - {column.DataType}, {(column.IsNullable ? "Nullable" : "        ")}");
                    }
                    Console.WriteLine("");
                }
            }

            // Migration class code Output
            if (table.Table_Name != "")
            {
                Console.WriteLine("-- Migration class code Output --");
                Console.WriteLine($"Create.Table(\"{table.Table_Name}\")");
                foreach (var column in table.Columns)
                {
                    if (column.ColumnName == "Created")
                    { Console.WriteLine("   // Metadata Fields"); }

                    var lineToBeWritten = $"   .WithColumn(\"{column.ColumnName}\").As{column.DataType}({column.CharacterMaximumLength})";
                    if (column.IsPrimaryKey)
                    {
                        lineToBeWritten = lineToBeWritten + ".PrimaryKey().Indexed()";
                    }
                    else
                    {
                        lineToBeWritten = lineToBeWritten + $".{(column.IsNullable == true ? "Nullable()" : "NotNullable()")}";
                    }

                    if (column == table.Columns.LastOrDefault())
                        lineToBeWritten = lineToBeWritten + ";";

                    Console.WriteLine(lineToBeWritten);
                }
                Console.WriteLine("");
            }

            // Quit
            Console.WriteLine("Press any key to quit");
            var key = Console.ReadKey();
        }
    }
}
