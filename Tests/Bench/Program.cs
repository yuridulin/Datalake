using BenchmarkDotNet.Running;

namespace Bench;

internal class Program
{
	static void Main(string[] args)
	{
#if DEBUG
		// Создать тестовые данные
		Console.WriteLine("CreateTestData");
		var testState = Generator.CreateTestData(200, 1000, 400);
		Console.WriteLine("CreateTestData done");

		// Запустить оригинальную реализацию
		Console.WriteLine("Original");
		var originalState = Methods.Original(testState);
		Console.WriteLine("Original done");

		// Запустить оптимизированную реализацию
		Console.WriteLine("Optimized");
		var optimizedState = Methods.Optimized(testState);
		Console.WriteLine("Optimized done");

		// Сравнить результаты
		Console.WriteLine("CompareAccessStates");
		AccessStateComparer.CompareAccessStates(testState, originalState, optimizedState);
		Console.WriteLine("CompareAccessStates done");
#else
		BenchmarkRunner.Run<AccessComputations>();
#endif
	}
}
