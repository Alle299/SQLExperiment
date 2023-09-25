using SQLExperiment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace SQLExperiment.Helpers
{
    public static class MigrationCommandLines
    {

        public static List<string> commands = new List<string> { "clear", "add", "remove", "alter", "rename" };
        public static List<string> entities = new List<string> { "table", "field" };
        public static List<string> fillerWords = new List<string> { "named", "a", "i", "want" };
        public static List<string> ReservedKeywords = new List<string> {
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
        public static List<string> invalidValues = new List<string> { "_build" };
        public static Dictionary<string, string> synonyms = new Dictionary<string, string> {
                { "create", "add"},
                { "give", "add"},
                { "build", "add"},
                { "make", "add"},
                { "column", "field"},
                { "delete", "remove"},
                { "change", "alter"},
            };
        public static List<string> dataType = new List<string> { "Binary", "Boolean" ,"Byte", "Currency", "Custom","Date", "DateTime", "DateTime2", "DateTimeOffset",
                "Decimal", "Double", "FixedLengthAnsiString", "FixedLengthString", "Float", "Guid", "Int16", "Int32", "Int64", "String", "Time", "XML", 
        };
        public static List<string> specialAttributeKeywords = new List<string>
        {
            "to",
        };

        /// <summary>
        /// Main processing function.
        /// </summary>
        /// <param name="__migrationCommand"></param>
        /// <param name="commandLine"></param>
        /// <returns></returns>
        public static __MigrationCommand ProcessLine(__MigrationCommand __migrationCommand, string commandLine)
        {
            // Declerations


            // Synonym case insensitive replacements
            foreach (var synonym in synonyms)
            {
                commandLine = Regex.Replace(commandLine, synonym.Key, synonym.Value, RegexOptions.IgnoreCase);
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
                __migrationCommand.CommandLines = new List<string>();
                // tables = new List<__DatabaseTable>();
                __migrationCommand.MigrationsCommands = new List<string>();
                return __migrationCommand;
            }

            // Get action from CommandWords
            var findActions = commandWords.ConvertAll(d => d.ToLower()).Where(i => commands.Contains(i)).ToList();
            if (findActions.Count != 1)
            {
                Console.WriteLine("Too many or too few commands in one line! ");
                return __migrationCommand;
            }

            // Get entities from CommandWords
            var findEntities = commandWords.ConvertAll(d => d.ToLower()).Where(i => entities.Contains(i)).ToList();
            if (findEntities.Count != 1)
            {
                Console.WriteLine("Too many or too few entities in one line! ");
                return __migrationCommand;
            }

            // Make sure theres at least one word after the found command
            int whereIsAction = commandWords.ConvertAll(d => d.ToLower()).FindIndex(a => a.Contains(findActions[0]));
            // Convert Action into lower case
            commandWords[whereIsAction] = commandWords[whereIsAction].ToLower();
            if (whereIsAction == commandWords.Count - 1)
            {
                Console.WriteLine("No identifier after command! ");
                return __migrationCommand;
            }

            // Make sure theres at least one word before or after the found entity, that is not an action.
            int whereIsEntity = commandWords.ConvertAll(d => d.ToLower()).FindIndex(a => a.Contains(findEntities[0]));
            // Convert Action into lower case
            commandWords[whereIsEntity] = commandWords[whereIsEntity].ToLower();
            if (whereIsEntity == commandWords.Count - 1 && whereIsAction == whereIsEntity - 1)
            {
                Console.WriteLine("No valid identifier before or after entity! ");
                return __migrationCommand;
            }

            // Find the value - if the word before the entity is the action, then the value must be the word after the entity.
            int whereIsValue = whereIsAction == whereIsEntity - 1 ? whereIsValue = whereIsEntity + 1 : whereIsEntity - 1;
            // Check to make sure the value is not an invalid value
            if (invalidValues.Contains(commandWords[whereIsValue]))
            {
                Console.WriteLine($"Protected field or table name {commandWords[whereIsValue]}.");
                return __migrationCommand;
            }

            // check to make sure the value ios not on the Reserved Keywords (Transact-SQL) list.
            if (ReservedKeywords.Contains(commandWords[whereIsValue]))
            {
                Console.WriteLine($"Value is in the Reserved Keywords list ( {commandWords[whereIsValue]} ).");
                return __migrationCommand;
            }

            // Get index of written datatype in Datatype list.
            var whereIsDataType = dataType.ConvertAll(d => d.ToLower()).FindIndex(a => commandWords.Contains(a.ToLower()));

            // Find first numerical value if any
            var whereIsNummerical = commandWords.FindIndex(a => Regex.IsMatch(a, @"^\d+$"));

            // Get SpecialAttributeKeywords and values
            var attributeKeywords = new Dictionary<string, string>();
            try
            {

                var commandAttributes = commandWords.ConvertAll(d => d.ToLower()).FindAll(c => specialAttributeKeywords.Contains(c));
                foreach (var attribute in commandAttributes)
                {
                    var findAttributeIndex = commandWords.FindIndex(a => a == attribute) + 1;
                    attributeKeywords.Add(attribute, commandWords[findAttributeIndex]);
                }
            } catch
            {
                Console.WriteLine($"Bad attribute value for attribute {attributeKeywords.Last().Key }.");
                return __migrationCommand;
            }

            #endregion

            // Command Validation
            try
            {
                var error = true;

                #region  Add Table command
                if (commandWords.Contains("add") &&
                    commandWords.Contains("table") &&
                    commandWords[whereIsValue].Contains('.') == false )
                {
                    // vaidate the Value (Table name)
                    var regex = new Regex(@"^[\p{L}_][\p{L}\p{N}@$#_]{0,127}$");
                    if (regex.IsMatch(commandWords[whereIsValue]) == false)
                    {
                        Console.WriteLine($"SQL invalid table name {commandWords[whereIsValue]}.");
                        return __migrationCommand;
                    }
                    var tableName = WordProcess.CapitalizeFirstLetter(commandWords[whereIsValue]);

                    StringBuilder sb = new StringBuilder();
                    sb.Append($"Create.Table(\"{tableName}\")" + Environment.NewLine);
                    sb.Append($"   .WithColumn(\"{tableName}ID\").AsGuid().PrimaryKey().Indexed()" + Environment.NewLine);
                    sb.Append("   .WithColumn(\"Name\").AsString(50)" + Environment.NewLine);
                    sb.Append("// MetadataFields" + Environment.NewLine);
                    sb.Append("   .WithColumn(\"Created\").AsDateTime().NotNullable()" + Environment.NewLine);
                    sb.Append("   .WithColumn(\"CreatedBy\").AsGuid().NotNullable()" + Environment.NewLine);
                    sb.Append("   .WithColumn(\"Modified\").AsDateTime().Nullable()" + Environment.NewLine);
                    sb.Append("   .WithColumn(\"ModifiedBy\").AsGuid().Nullable()" + Environment.NewLine);
                    sb.Append("   .WithColumn(\"Removed\").AsDateTime().Nullable()" + Environment.NewLine);
                    sb.Append("   .WithColumn(\"RemovedBy\").AsGuid().Nullable();" + Environment.NewLine + Environment.NewLine);

                    __migrationCommand.MigrationsCommands.Add(sb.ToString());
                    error = false;
                }
                #endregion

                #region Rename Table command
                if (commandWords.Contains("rename") &&
                    commandWords.Contains("table") &&
                    commandWords[whereIsValue].Contains('.') == false )
                {
                    var tableName = WordProcess.CapitalizeFirstLetter(commandWords[whereIsValue]);
                    var to = WordProcess.CapitalizeFirstLetter(attributeKeywords["to"]);
                    if (String.IsNullOrEmpty(to))
                    {
                        Console.WriteLine($"Bad or missing 'To' attribute value.");
                        return __migrationCommand;
                    }
                    __migrationCommand.MigrationsCommands.Add($"Rename.Table(\"{tableName}\").To(\"{to}\");");
                    error = false;
                }
                #endregion

                //Rename.Table("Alle").To("Alle");

                #region Add Field command
                if (commandWords.Contains("add") &&
                    commandWords.Contains("field") &&
                    whereIsDataType > -1 &&
                    commandWords[whereIsValue].Contains('.')
                    )
                {
                    var nullable = commandWords.Contains("field") ? ".Nullable()" : "";
                    var values = commandWords[whereIsValue].Split('.');
                    var size = whereIsNummerical > -1 ? commandWords[whereIsNummerical] : "";
                    __migrationCommand.MigrationsCommands.Add($"Create.Column(\"{values[1]}\").OnTable(\"{values[0]}\").As{dataType[whereIsDataType]}({size}){nullable};" + Environment.NewLine);
                    error = false;
                }
                #endregion

                #region Alter Field command
                if (commandWords.Contains("alter") &&
                    commandWords.Contains("field") &&
                    whereIsDataType > -1 &&
                    commandWords[whereIsValue].Contains('.')
                    )
                {
                    var nullable = commandWords.Contains("field") ? ".Nullable()" : "";
                    var values = commandWords[whereIsValue].Split('.');
                    var size = whereIsNummerical > -1 ? commandWords[whereIsNummerical] : "";

                    __migrationCommand.MigrationsCommands.Add($"Alter.Table(\"{values[0]}\").AlterColumn(\"{values[1]}\").As{dataType[whereIsDataType]}({size}){nullable};");
                    error = false;
                }
                #endregion

                #region Rename Field command
                if (commandWords.Contains("rename") &&
                    commandWords.Contains("field") &&
                    commandWords[whereIsValue].Contains('.')
                    )
                {
                    var values = commandWords[whereIsValue].Split('.');
                    var to = WordProcess.CapitalizeFirstLetter(attributeKeywords["to"]);
                    if(String.IsNullOrEmpty(to))
                    {
                        Console.WriteLine($"Bad or missing 'To' attribute value.");
                        return __migrationCommand;
                    }
                    __migrationCommand.MigrationsCommands.Add($"Rename.Column(\"{WordProcess.CapitalizeFirstLetter(values[1])}\").OnTable(\"{WordProcess.CapitalizeFirstLetter(values[0])}\").To(\"{to}\");");
                    error = false;
                }
                #endregion

                    //Rename.Table("Alle").To("Alle");
                    //Rename.Column("Alle").OnTable("Alle").To("Alle");

                    // ------------------------------------------------
                if (error)
                {
                    Console.WriteLine("Invallid command! ");
                    return __migrationCommand;
                }
                else
                {
                    Console.WriteLine("Verified! ");
                    __migrationCommand.CommandLines.Add(commandLine);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Broken command! {ex.InnerException.Message}");
                return __migrationCommand;
            }


            return __migrationCommand;
        }
    }
}
