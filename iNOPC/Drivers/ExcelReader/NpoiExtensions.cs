using NPOI.SS.UserModel;

namespace ExcelReader
{
	public static class NpoiExtensions
	{
		public static object GetFormattedCellValue(this ICell cell, IFormulaEvaluator eval = null)
		{
			if (cell != null)
			{
				switch (cell.CellType)
				{
					case CellType.String:
						return cell.StringCellValue;

					case CellType.Numeric:
						if (DateUtil.IsCellDateFormatted(cell))
						{
							return cell.DateCellValue;
						}
						else
						{
							return cell.NumericCellValue;
						}

					case CellType.Boolean:
						return cell.BooleanCellValue;

					case CellType.Formula:
						if (eval != null)
							return GetFormattedCellValue(eval.EvaluateInCell(cell));
						else
							return cell.CellFormula;

					case CellType.Error:
						return FormulaError.ForInt(cell.ErrorCellValue).String;
				}
			}
			// null or blank cell, or unknown cell type
			return string.Empty;
		}
	}
}
