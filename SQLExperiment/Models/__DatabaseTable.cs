using System.Collections.Generic;

namespace SQLExperiment.Models
{
	public class __DatabaseTable
	{
		public List<__PrimaryKey> PrimaryKeys { get; set; }
		public string Table_Catalog { get; set; }
		public string TABLE_SCHEMA { get; set; }
		public string Table_Name { get; set; }
		public string TypeCode { get; set; }
		public List<__DatabaseTableColumn> Columns { get; set; }
	}
}
