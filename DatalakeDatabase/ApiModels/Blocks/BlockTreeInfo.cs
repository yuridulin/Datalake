namespace DatalakeDatabase.ApiModels.Blocks;

public class BlockTreeInfo
{
	public int Id { get; set; }

	public required string Name { get; set; }

	public BlockTreeInfo[] Children { get; set; } = [];
}
