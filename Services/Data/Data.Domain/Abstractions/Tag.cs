using Datalake.Contracts.Public.Enums;

namespace Datalake.Data.Domain.Abstractions;

public class Tag
{
	public int Id { get; private set; }

	public Guid Guid { get; private set; }

	public TagType Type { get; private set; }

	public TagResolution Resolution { get; private set; }

	public int SourceId { get; private set; }

	public CalculationSettings? CalculationSettings { get; private set; }
}

public class CalculationSettings
{
	public string Formula { get; private set; } = string.Empty;

	public IEnumerable<CalculationInput> Inputs { get; private set; } = Enumerable.Empty<CalculationInput>();
}

public class CalculationInput
{
	public int TagId { get; private set; }

	public string Variable {  get; private set; } = string.Empty;
}
