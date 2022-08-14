using System;
using Terahard.Assessment.Core;

namespace Terahard.Assessment.Test
{
	static class Program
	{
		[STAThread]
		static void Main()
		{
			Application.Instance.Run(new TestContext());
		}
	}
}

/** var s1 = Stopwatch.StartNew();
for (int i = 0; i < _max; i++)
{
}
s1.Stop();
var s2 = Stopwatch.StartNew();
for (int i = 0; i < _max; i++)
{
}
s2.Stop();
Console.WriteLine(((double)(s1.Elapsed.TotalMilliseconds * 1000000) /
	_max).ToString("0.00 ns"));
Console.WriteLine(((double)(s2.Elapsed.TotalMilliseconds * 1000000) /
	_max).ToString("0.00 ns"));
Console.Read(); **/