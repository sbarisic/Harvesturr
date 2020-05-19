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

			return Arr[Random(0, Arr.Length)];
		}

		public static Vector2 Floor(Vector2 V) {
			return new Vector2((int)Math.Floor(V.X), (int)Math.Floor(V.Y));
		}

		public static int Rearrange<T>(T[] Arr) where T : class {
			int ILeft = 0;
			int IRight = 1;

			while (IRight < Arr.Length) {
				T Left = Arr[ILeft];
				T Right = Arr[IRight];

				if (Left == null && Right != null) {
					Swap(Arr, ILeft++, IRight++);
				} else if (Left == null && Right == null) {
					IRight++;
				} else if (Left != null && Right != null) {
					ILeft += 2;
					IRight += 2;
				} else if (Left != null && Right == null) {
					ILeft++;
					IRight++;
				}
			}

			return IndexOfFirstNull(Arr);
		}

		public static void Swap<T>(T[] Arr, int Left, int Right) {
			T Tmp = Arr[Left];
			Arr[Left] = Arr[Right];
			Arr[Right] = Tmp;
		}

		public static int IndexOfFirstNull<T>(T[] Arr) where T : class {
			for (int i = 0; i < Arr.Length; i++)
				if (Arr[i] == null)
					return i;

			return -1;
		}
	}
}
