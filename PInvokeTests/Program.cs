using Synergy.PInvoke;
using System;

namespace PInvokeTests
{
	class Program
	{
		static void Main(string[] args)
		{
			Mouse.Click(Mouse.MouseEventFlags.LeftDown);
			Console.WriteLine("Mouse clicked");
			Console.ReadLine();
		}
	}
}
