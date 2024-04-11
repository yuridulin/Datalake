using DatalakeDatabase.Tests.Attributes;
using DatalakeDatabase.Tests.Steps;

namespace DatalakeDatabase.Tests
{
	[TestCaseOrderer("DatalakeDatabase.Tests.Attributes.PriorityOrderer", "DatalakeDatabase.Tests")]
	public class Pipeline
	{
		[Fact, TestPriority(0)]
		public async Task CreationTest() => await Step001_DatabaseCreationAndSeed.CreationTest();
		
		[Fact, TestPriority(1)]
		public static async Task SeedTest() => await Step001_DatabaseCreationAndSeed.SeedTest();

		[Fact, TestPriority(2)]
		public static async Task GetManualSource() => await Step001_DatabaseCreationAndSeed.GetManualSource();
		
		[Fact, TestPriority(3)]
		public static async Task CreateManualTag() => await Step002_TagCreation.CreateManualTag();

		[Fact, TestPriority(4)]
		public static async Task GetManualTag() => await Step002_TagCreation.GetManualTag();

		[Fact, TestPriority(5)]
		public static async Task GetLiveValue() => await Step002_TagCreation.GetLiveValue();

		[Fact, TestPriority(6)]
		public static async Task WriteToTag() => await Step003_ManualWriteToTag.WriteToTag();
	}
}
