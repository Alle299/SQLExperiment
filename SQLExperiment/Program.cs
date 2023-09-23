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
            string commandLine = "";
            List<string> commandLines = new List<string>();
            List<string> commands = new List<string>{ "clear", "add", "remove" };
            List<string> entities = new List<string> { "table", "field" };
            List<string> fillerWords = new List<string> { "named", "a", "i", "want" };
            List<string> ReservedKeywords = new List<string> {
                "ADD", "EXTERNAL", "PROCEDURE", "ALL", "FETCH", "PUBLIC", "ALTER", "FILE", "RAISERROR", "AND",
                "FILLFACTOR", "READ", "ANY", "FOR", "READTEXT", "AS", "FOREIGN", "RECONFIGURE", "ASC",
                "FREETEXT", "REFERENCES", "AUTHORIZATION", "FREETEXTTABLE", "REPLICATION", "BACKUP",
                "FROM", "RESTORE", "BEGIN", "FULL", "RESTRICT", "BETWEEN", "FUNCTION", "RETURN", "BREAK", "GOTO",
                "REVERT", "BROWSE", "GRANT", "REVOKE", "BULK", "GROUP", "RIGHT", "BY", "HAVING",
                "ROLLBACK", "CASCADE", "HOLDLOCK", "ROWCOUNT", "CASE", "IDENTITY", "ROWGUIDCOL", "CHECK",
                "IDENTITY_INSERT", "RULE", "CHECKPOINT", "IDENTITYCOL", "SAVE", "CLOSE", "IF", "SCHEMA",
                "CLUSTERED", "IN", "SECURITYAUDIT", "COALESCE", "INDEX", "SELECT", "COLLATE", "INNER", "SEMANTICKEYPHRASETABLE",
                "COLUMN", "INSERT", "SEMANTICSIMILARITYDETAILSTABLE", "COMMIT", "INTERSECT", "SEMANTICSIMILARITYTABLE",
                "COMPUTE", "INTO", "SESSION_USER", "CONSTRAINT", "IS", "SET", "CONTAINS", "JOIN", "SETUSER",
                "CONTAINSTABLE", "KEY", "SHUTDOWN", "CONTINUE", "KILL", "SOME", "CONVERT", "LEFT", "STATISTICS",
                "CREATE", "LIKE", "SYSTEM_USER", "CROSS", "LINENO", "TABLE", "CURRENT", "LOAD", "TABLESAMPLE",
                "CURRENT_DATE", "MERGE", "TEXTSIZE", "CURRENT_TIME", "NATIONAL", "THEN", "CURRENT_TIMESTAMP",
                "NOCHECK", "TO", "CURRENT_USER", "NONCLUSTERED", "TOP", "CURSOR", "NOT", "TRAN",
                "DATABASE", "NULL", "TRANSACTION", "DBCC", "NULLIF", "TRIGGER", "DEALLOCATE", "OF", "TRUNCATE",
                "DECLARE", "OFF", "TRY_CONVERT", "DEFAULT", "OFFSETS", "TSEQUAL", "DELETE", "ON",
                "UNION", "DENY", "OPEN", "UNIQUE", "DESC", "OPENDATASOURCE", "UNPIVOT", "DISK",
                "OPENQUERY", "UPDATE", "DISTINCT", "OPENROWSET", "UPDATETEXT", "DISTRIBUTED", "OPENXML",
                "USE", "DOUBLE", "OPTION", "USER", "DROP", "OR", "VALUES", "DUMP", "ORDER", "VARYING",
                "ELSE", "OUTER", "VIEW", "END", "OVER", "WAITFOR", "ERRLVL", "PERCENT", "WHEN", "ESCAPE",
                "PIVOT", "WHERE", "EXCEPT", "PLAN", "WHILE", "EXEC", "PRECISION", "WITH", "EXECUTE",
                "PRIMARY", "WITHIN", "EXISTS", "PRINT", "WRITETEXT", "EXIT", "PROC",
             };

            List<string> invalidValues = new List<string> { "_build" };
            Dictionary<string,string> synonyms = new Dictionary<string, string> {
                { "create", "add"},
                { "give", "add"},
                { "build", "add"},
                { "make", "add"},
                { "column", "field"},
                { "delete", "remove"},
            };
            var tables = new List<__DatabaseTable>();


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
            Console.WriteLine("Exemples:");
            Console.WriteLine("'Create Customer Table' and 'add table customer' will produce the exact same result.");
            Console.WriteLine("---------------------------------------------------------------------------------------------");
            Console.WriteLine("");

            // Command Loop
            while (commandLine.ToLower() != "print")
            {

                // Type your command line and enter.
                Console.WriteLine($" ({commandLines.Count}) Enter Command:");
                commandLine = Console.ReadLine();
 
                // Synonym case insensitive replacements
                foreach (var synonym in synonyms)
                {
                    commandLine = Regex.Replace(commandLine, synonym.Key, synonym.Value, RegexOptions.IgnoreCase);

                    // commandLine = commandLine.Replace(synonym.Key,synonym.Value);
                }


                //string input = "hello WoRlD";
                //string result =
                //   Regex.Replace(input, "world", "csharp", RegexOptions.IgnoreCase);


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
                var findActions = commandWords.ConvertAll(d => d.ToLower()).Where(i => commands.Contains(i)).ToList();
                if (findActions.Count != 1)
                {
                    Console.WriteLine("Too many or too few commands in one line! ");
                    continue;
                }

                // Get entities from CommandWords
                var findEntities = commandWords.ConvertAll(d => d.ToLower()).Where(i => entities.Contains(i)).ToList();
                if (findEntities.Count != 1)
                {
                    Console.WriteLine("Too many or too few entities in one line! ");
                    continue;
                }

                // Make sure theres at least one word after the found command
                int whereIsAction = commandWords.ConvertAll(d => d.ToLower()).FindIndex(a => a.Contains(findActions[0]));
                // Convert Action into lower case
                commandWords[whereIsAction] = commandWords[whereIsAction].ToLower();
                if (whereIsAction == commandWords.Count-1)
                {
                    Console.WriteLine("No identifier after command! ");
                    continue;
                }

                // Make sure theres at least one word before or after the found entity, that is not an action.
                int whereIsEntity = commandWords.ConvertAll(d => d.ToLower()).FindIndex(a => a.Contains(findEntities[0]));
                // Convert Action into lower case
                commandWords[whereIsEntity] = commandWords[whereIsEntity].ToLower();
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

                // check to make sure the value ios not on the Reserved Keywords (Transact-SQL) list.
                if (ReservedKeywords.Contains(commandWords[whereIsValue]))
                {
                    Console.WriteLine($"Value is in the Reserved Keywords list ( {commandWords[whereIsValue]} ).");
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
                        // vaidate the Value (Table name)
                        var regex = new Regex(@"^[\p{L}_][\p{L}\p{N}@$#_]{0,127}$");
                        if ( regex.IsMatch(commandWords[whereIsValue]) == false )
                        {
                            Console.WriteLine($"SQL invalid table name {commandWords[whereIsValue]}.");
                            continue;
                        }

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
