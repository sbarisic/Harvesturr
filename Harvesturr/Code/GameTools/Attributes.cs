using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Raylib_cs;
using System.Globalization;

namespace Harvesturr {
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	sealed class IsGameToolAttribute : Attribute {
		public int Index;

		public IsGameToolAttribute(int Index) {
			this.Index = Index;
		}

		public static IEnumerable<GameTool> CreateAllGameTools() {
			Type[] AllTypes = Assembly.GetExecutingAssembly().GetTypes();

			foreach (var T in AllTypes) {
				IsGameToolAttribute IsGameTool = T.GetCustomAttribute<IsGameToolAttribute>();

				if (IsGameTool != null) {
					GameTool ToolInstance = Activator.CreateInstance(T) as GameTool;
					ToolInstance.GameToolAttribute = IsGameTool;
					yield return ToolInstance;
				}
			}
		}
	}
}
