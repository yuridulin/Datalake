using DatalakeDatabase.Tests.Attributes;
using DatalakeDatabase.Tests.Steps;

namespace DatalakeDatabase.Tests
{
	[TestCaseOrderer("DatalakeDatabase.Tests.Attributes.PriorityOrderer", "DatalakeDatabase.Tests")]
	public class Pipeline
	{
		[Fact, Priority(0)]
		public async Task CreationTest() => await Step001_DatabaseCreationAndSeed.CreationTest();
		
		[Fact, Priority(1)]
		public static async Task SeedTest() => await Step001_DatabaseCreationAndSeed.SeedTest();

		[Fact, Priority(2)]
		public static async Task GetManualSource() => await Step001_DatabaseCreationAndSeed.GetManualSource();
		
		[Fact, Priority(3)]
		public static async Task CreateManualTag() => await Step002_TagCreation.CreateManualTag();

		[Fact, Priority(4)]
		public static async Task GetManualTag() => await Step002_TagCreation.GetManualTag();

		[Fact, Priority(5)]
		public static async Task GetLiveValue() => await Step002_TagCreation.GetLiveValue();

		[Fact, Priority(6)]
		public static async Task WriteToTag() => await Step003_ManualWriteToTag.WriteToTag();

		[Fact, Priority(7)]
		public static async Task SeedValues() => await Step003_ManualWriteToTag.SeedValues();
	}
}
