using DatalakeDatabase.Tests.Attributes;
using DatalakeDatabase.Tests.Steps.Step001;
using DatalakeDatabase.Tests.Steps.Step002;

namespace DatalakeDatabase.Tests
{
	[TestCaseOrderer("DatalakeDatabase.Tests.Attributes.PriorityOrderer", "DatalakeDatabase.Tests")]
	public class Pipeline
	{
		[Fact, TestPriority(0)]
		public async Task CreationTest() => await DatabaseSchemaTests.CreationTest();
		
		[Fact, TestPriority(1)]
		public static async Task SeedTest() => await DatabaseSchemaTests.SeedTest();

		[Fact, TestPriority(2)]
		public static async Task GetManualSource() => await DatabaseSchemaTests.GetManualSource();
		
		[Fact, TestPriority(3)]
		public static async Task CreateManualTag() => await TagCreationTests.CreateManualTag();

		[Fact, TestPriority(4)]
		public static async Task GetManualTag() => await TagCreationTests.GetManualTag();

		[Fact, TestPriority(5)]
		public static async Task GetLiveValue() => await TagCreationTests.GetLiveValue();

	}
}
