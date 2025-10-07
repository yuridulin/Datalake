using Datalake.Contracts.Public.Enums;

namespace Datalake.Data.Application.Models.Tags;

public record TagCalculationSettingsDto
{
	public required string ExpressionFormula { get; init; }

	public required IEnumerable<TagCalculationInputDto> ExpressionVariables { get; init; }

	public record TagCalculationInputDto
	{
		public required int SourceTagId { get; init; }

		public required TagType SourceTagType { get; init; }

		public required string VariableName { get; init; }
	}
}
