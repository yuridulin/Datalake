using Datalake.Database.Repositories;
using Datalake.PublicApi.Enums;

namespace Benchmark;

internal class Program
{
	static void Main(string[] _)
	{
#if DEBUG
		var bench = new CreateAccessRightsCacheBenchmark();
		bench.Setup();

		Console.Write("Расчет по новому ... ");
		var optimized = bench.Optimized();
		Console.WriteLine("завершен");
		Console.Write("Расчет по старинке ... ");
		var current = bench.Current();
		Console.WriteLine("завершен");

		int mismatchCount = 0;

		foreach (var user in bench._minimal.Users)
		{
			var currentUser = AccessRepository.GetAuthInfoTest(current, user.Guid);
			var optimizedUser = AccessRepository.GetAuthInfoTest(optimized, user.Guid);

			foreach (var block in bench._minimal.Blocks)
			{
				var currentRight = AccessRepository.HasAccessToBlockTest(current, currentUser, AccessType.Editor, block.Id);
				var optimizedRight = AccessRepository.HasAccessToBlockTest(optimized, optimizedUser, AccessType.Editor, block.Id);

				if (currentRight != optimizedRight)
				{
					//Console.WriteLine($"Несовпадение доступа {user.Name} к {tag.Name} - было {currentRight}, стало {optimizedRight}");
					mismatchCount++;
				}
			}

			foreach (var tag in bench._minimal.Tags)
			{
				var currentRight = AccessRepository.HasAccessToTagTest(current, currentUser, AccessType.Editor, tag.Guid);
				var optimizedRight = AccessRepository.HasAccessToTagTest(optimized, optimizedUser, AccessType.Editor, tag.Guid);

				if (currentRight != optimizedRight)
				{
					//Console.WriteLine($"Несовпадение доступа {user.Name} к {tag.Name} - было {currentRight}, стало {optimizedRight}");
					mismatchCount++;
				}
			}
		}

		Console.WriteLine($"Количество ошибок: {mismatchCount}");
#else
		BenchmarkDotNet.Running.BenchmarkRunner.Run<CreateAccessRightsCacheBenchmark>();
#endif
	}
}

