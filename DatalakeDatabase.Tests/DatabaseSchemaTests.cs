using DatalakeDatabase.Tests.Attributes;

namespace DatalakeDatabase.Tests
{
	[TestCaseOrderer("DatalakeDatabase.Tests.Attributes.PriorityOrderer", "DatalakeDatabase.Tests")]
	public class DatabaseSchemaTests
	{
		[Fact, TestPriority(0)]
		public async Task CreationTest()
		{
			using var context = Setup.CreateEfContext();

			await context.Database.EnsureDeletedAsync();
			await context.Database.EnsureCreatedAsync();
		}

		[Fact, TestPriority(1)]
		public async Task SeedTest()
		{
			using var db = Setup.CreateDbContext();

			await db.EnsureDataCreatedAsync();
		}
	}
}