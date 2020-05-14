using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Raylib_cs;

namespace Harvesturr {
	static class Utils {
		static Random Rnd = new Random();

		public static int Random(int IncMin, int ExMax) {
			return Rnd.Next(IncMin, ExMax);
		}

		public static Vector2 Random(Vector2 A, Vector2 B) {
			int X = Random((int)A.X, (int)B.X);
			int Y = Random((int)A.Y, (int)B.Y);
			return new Vector2(X, Y);
		}

		public static bool IsInside(Rectangle Rect, Vector2 Pos) {
			float Width = Rect.width + Rect.x;
			float Height = Rect.height + Rect.y;

			if (Pos.X < Rect.x || Pos.Y < Rect.y)
				return false;

			if (Pos.X >= Width || Pos.Y >= Height)
				return false;

			return true;
		}

		public static T Random<T>(T[] Arr) {
			if (Arr.Length == 0)
				return default(T);

			return Arr[Rnd.Next(0, Arr.Length)];
		}
	}
}
