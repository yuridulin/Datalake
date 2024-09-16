using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datalake.ApiClasses.Models.Tables;

public class HistoryTableInfo
{
	public string Name { get; set; }

	public DateTime Date { get; set; }

	public bool HasIndex { get; set; }
}
