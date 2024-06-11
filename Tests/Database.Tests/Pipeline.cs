using DatalakeDatabase.Tests.Attributes;
using DatalakeDatabase.Tests.Steps;

namespace DatalakeDatabase.Tests
{
	[TestCaseOrderer("DatalakeDatabase.Tests.Attributes.PriorityOrderer", "DatalakeDatabase.Tests")]
	public class Pipeline
	{
		[Fact, Priority(1)]
		public async Task T001_CreationTest() => await Step001_DatabaseCreationAndSeed.T1_1_CreationTest();
		
		[Fact, Priority(2)]
		public static async Task T002_SeedTest() => await Step001_DatabaseCreationAndSeed.T1_2_SeedTest();

		[Fact, Priority(3)]
		public static async Task T003_GetManualSource() => await Step001_DatabaseCreationAndSeed.T1_3_GetManualSource();

		[Fact, Priority(4)]
		public static async Task T004_CreateStaticUser() => await Step002_TagCreation.T2_0_CreateStaticUser();

		[Fact, Priority(5)]
		public static async Task T005_CreateManualTag() => await Step002_TagCreation.T2_1_CreateManualTag();

		[Fact, Priority(6)]
		public static async Task T006_GetManualTag() => await Step002_TagCreation.T2_2_GetManualTag();

		[Fact, Priority(7)]
		public static async Task T007_GetLiveValue() => await Step002_TagCreation.T2_3_GetLiveValue();

		[Fact, Priority(8)]
		public static async Task T008_WriteToTag() => await Step003_ManualWriteToTag.T021_WriteToTag();

		[Fact, Priority(9)]
		public static async Task T009_SeedValues() => await Step003_ManualWriteToTag.T3_1_SeedValues();

		[Fact, Priority(10)]
		public static async Task T010_ReadExactBetweenFirstAndSecond() => await Step004_HistoryRead.T4_1_ReadExactBetweenFirstAndSecond();
	}
}
