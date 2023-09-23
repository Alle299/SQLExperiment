using SQLExperiment.Helpers;
using SQLExperiment.Models;
using System;
using System.Collections;
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
            string commandLine = "";
            List<string> commandLines = new List<string>();
            List<string> commands = new List<string>{ "clear", "add", "remove" };
            List<string> entities = new List<string> { "table", "field" };
            List<string> fillerWords = new List<string> { "named", "a", "i", "want" };
            List<string> invalidValues = new List<string> { "_build" };
            Dictionary<string,string> synonyms = new Dictionary<string, string> {
                { "create", "add"},
                { "give", "add"},
                { "build", "add"},
                { "make", "add"},
                { "delete", "remove"},
            };
            var tables = new List<__DatabaseTable>();


            // Presentation
            Console.WriteLine("---------------------------------------------------------------------------------------------");
            Console.WriteLine("  A simple experiment to see if I cane make DB migration code based on used plain text input.");
            Console.WriteLine("");
            Console.WriteLine("  Add - table,field");
            Console.WriteLine("  Clear - resets commands");
            Console.WriteLine("  Print - Ends the loop");
            Console.WriteLine("---------------------------------------------------------------------------------------------");
            Console.WriteLine("");

            // Command Loop
            while (commandLine.ToLower() != "print")
            {

                // Type your command line and enter.
                Console.WriteLine($" ({commandLines.Count}) Entrer Command:");
                commandLine = Console.ReadLine().ToLower();

                // Synonym replacements
                foreach (var synonym in synonyms)
                {
                    commandLine = commandLine.Replace(synonym.Key,synonym.Value);
                }

                // Break up command line
                var commandWords = commandLine.Split(' ').ToList();

                // ---------------------------------------------

                // Filler word removal
                commandWords = commandWords.Where(i => !fillerWords.Contains(i)).ToList();

                #region Command validation
                // Reset and start over
                if (commandLine.ToLower() == "clear")
                {
                    // Reset all work
                    commandLines = new List<string>();
                    tables = new List<__DatabaseTable>();
                    continue;
                }

                // Get action from CommandWords
                var findActions = commandWords.Where(i => commands.Contains(i)).ToList();
                if (findActions.Count != 1)
                {
                    Console.WriteLine("Too many or too few commands in one line! ");
                    continue;
                }

                // Get entities from CommandWords
                var findEntities = commandWords.Where(i => entities.Contains(i)).ToList();
                if (findEntities.Count != 1)
                {
                    Console.WriteLine("Too many or too few entities in one line! ");
                    continue;
                }

                // Make sure theres at least one word after the found command
                int whereIsAction = commandWords.FindIndex(a => a.Contains(findActions[0]));
                if (whereIsAction == commandWords.Count-1)
                {
                    Console.WriteLine("No identifier after command! ");
                    continue;
                }

                // Make sure theres at least one word before or after the found entity, thast is not an action.
                int whereIsEntity = commandWords.FindIndex(a => a.Contains(findEntities[0]));
                if (whereIsEntity == commandWords.Count - 1 && whereIsAction == whereIsEntity - 1)
                {
                    Console.WriteLine("No valid identifier before or after entity! ");
                    continue;
                }

                // Find the value - if the word before the entity is the action, then the value must be the word after the entity.
                int whereIsValue = whereIsAction == whereIsEntity - 1 ? whereIsValue = whereIsEntity + 1 : whereIsEntity - 1;
                // Check to make sure the value is not an invalid value
                if (invalidValues.Contains(commandWords[whereIsValue]))
                {
                    Console.WriteLine($"Protected field or table name {commandWords[whereIsValue]}.");
                    continue;
                }

                #endregion



                // Command Validation
                try
                {
                    var error = true;

                    // Add Table command
                    if (commandWords.Contains("add") &&
                        commandWords.Contains("table") )
                    {
                        var table = new __DatabaseTable();
                        table.Table_Name = WordProcess.CapitalizeFirstLetter(commandWords[whereIsValue]);
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
                        tables.Add(table);
                        error = false;
                    }

                    // Add Filed command
                    if (commandWords.Contains("add") &&
                        commandWords.Contains("field"))
                    {
                        var databaseTableColumn = new __DatabaseTableColumn { 
                            ColumnName = WordProcess.CapitalizeFirstLetter(commandWords[whereIsValue]) ,
                            DataType ="String",
                            IsNullable = true,
                            CharacterMaximumLength = 50
                        };
                        // Check for field attribute values
                        if (commandWords.Contains("datetime"))
                        {
                            databaseTableColumn.DataType = "DateTime";
                            databaseTableColumn.CharacterMaximumLength = null;
                        }
                        if (commandWords.Contains("guid"))
                        {
                            databaseTableColumn.DataType = "Guid";
                            databaseTableColumn.CharacterMaximumLength = null;
                        }
                        if (commandWords.Contains("notnullable"))
                        {
                            databaseTableColumn.IsNullable = false;
                        }

                        tables[0].Columns.Insert(tables[0].Columns.Count - 6, databaseTableColumn);
                        error = false;
                    }

                    // ------------------------------------------------
                    if (error)
                    {
                        Console.WriteLine("Invallid command! ");
                        continue;
                    }
                    else
                    {
                        Console.WriteLine("Verified! ");
                        commandLines.Add(commandLine);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Broken command! {ex.InnerException.Message}");
                    continue;
                }
           

                // Table Output
                if (tables.Count > 0)
                {
                    Console.WriteLine("-- Table & colums --");
                    foreach (var table in tables)
                    {
                        Console.WriteLine($"  Table: {table.Table_Name}");
                        foreach (var column in table.Columns)
                        {
                            Console.WriteLine($"  {(column.IsPrimaryKey == true ? "pk" : "  ")} {column.ColumnName} - {column.DataType}({column.CharacterMaximumLength}), {(column.IsNullable ? "Nullable" : "        ")}");
                        }
                        Console.WriteLine("");
                    }
                }
            }

            // Migration class code Output
            if (tables.Count > 0)
            {
                Console.WriteLine("-- Migration class code Output --");
                foreach (var table in tables)
                {
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
            }

            // Quit
            Console.WriteLine("Press any key to quit");
            var key = Console.ReadKey();
        }
    }
}
