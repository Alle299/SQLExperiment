namespace SQLExperiment.Models
{
	public class __DatabaseTableColumn
	{
		public string ColumnName { get; set; }

		public bool IsPrimaryKey { get; set; }

		public bool IsForeignKey { get; set; }
		public string ForeignKeyObjectName { get; set; }
		public bool IsIdentity { get; set; }

		public int? seed_value { get; set; }
		public int? increment_value { get; set; }

		public int OrdinalPosition { get; set; }
		public bool IsNullable { get; set; }
		public string DataType { get; set; }
		public int DataTypeInt { get; set; }
		public int? CharacterMaximumLength { get; set; }
	}
}