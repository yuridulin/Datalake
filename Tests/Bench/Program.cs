using BenchmarkDotNet.Running;

namespace Bench;

internal class Program
{
	static void Main(string[] args)
	{
#if DEBUG
		var state = Generator.CreateTestData(200, 1000, 400);
		var deepseek = Methods.Deepseek(state);
		var copilot = Methods.Copilot(state);

		AccessStateComparer.CompareAccessStates(state, deepseek, copilot);
#else
		BenchmarkRunner.Run<AccessComputations>();
#endif
	}
}
