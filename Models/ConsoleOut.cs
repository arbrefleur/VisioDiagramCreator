﻿using System;

namespace OmnicellBlueprintingTool.Models
{
	public static class ConsoleOut
	{
		public static void writeLine(string message)
		{
#if DEBUG
			Console.WriteLine(message);
#endif
		}
	}
}
