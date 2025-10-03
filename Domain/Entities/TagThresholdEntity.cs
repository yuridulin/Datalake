using Datalake.Domain.Interfaces;

namespace Datalake.Domain.Entities;

public record class TagThresholdEntity : IWithIdentityKey
{
	private TagThresholdEntity() { }

	public TagThresholdEntity(int tagId, float input, float output)
	{
		TagId = tagId;
		InputValue = input;
		OutputValue = output;
	}

	public int Id { get; private set; }

	public int TagId { get; private set; }

	public float InputValue { get; private set; }

	public float OutputValue { get; private set; }
}
