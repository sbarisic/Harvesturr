using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Raylib_cs;
namespace Harvesturr {
	struct IntersectionResult {
		public static readonly IntersectionResult Empty = new IntersectionResult();

		public Vector2 Direction;
		public Vector2 HitPoint;
		public Vector2 Normal;
		public Vector2 Reflection;

		public IntersectionResult(Vector2 Direction, Vector2 HitPoint, Vector2 Normal, Vector2 Reflection) {
			this.Direction = Direction;
			this.HitPoint = HitPoint;
			this.Normal = Normal;
			this.Reflection = Reflection;
		}
	}

	static unsafe class Utils {
		static Random Rnd = new Random();

		public static int Random(int IncMin, int ExMax) {
			return Rnd.Next(IncMin, ExMax);
		}

		public static float Random(float Min, float Max) {
			return Min + (float)(Rnd.NextDouble() * (Max - Min));
		}

		public static Vector2 Random(Vector2 A, Vector2 B) {
			int X = Random((int)A.X, (int)B.X);
			int Y = Random((int)A.Y, (int)B.Y);

			return new Vector2(X, Y);
		}

		public static Vector2 RandomDir() {
			float Angle = (float)(Rnd.Next(0, 360) * (Math.PI / 180));
			return new Vector2((float)Math.Cos(Angle), (float)Math.Sin(Angle));
		}

		// TODO
		/*public static Vector2 Spread(Vector2 Normal, float AngleDeg) {
			float Angle = (float)(Math.Atan2(Normal.Y, Normal.X) * (180.0 / Math.PI));
			Angle = Angle + Rnd.
		}*/

		public static Vector2 RandomPoint(float Radius, bool Uniform = false) {
			float Angle = (float)(Rnd.NextDouble() * Math.PI * 2);
			float Rad;

			if (Uniform)
				Rad = (float)(Math.Sqrt(Rnd.NextDouble()) * Radius);
			else
				Rad = (float)(Rnd.NextDouble() * Radius);

			float X = (float)(Rad * Math.Cos(Angle));
			float Y = (float)(Rad * Math.Sin(Angle));

			return new Vector2(X, Y);
		}

		public static bool RandomBool() {
			return Rnd.Next(0, 2) > 0;
		}

		public static Vector2 RandomPointOnCircle(float Radius) {
			float Angle = Rnd.Next(360);
			return new Vector2(Radius * (float)Math.Cos(Angle), Radius * (float)Math.Sin(Angle));
		}

		public static Vector2 RandomPointOnRect(float X, float Y, float W, float H) {
			if (RandomBool())
				return new Vector2(Random(X, X + W), RandomBool() ? Y : Y + H);

			return new Vector2(RandomBool() ? X : X + W, Random(Y, Y + H));
		}

		public static Vector2 RandomPointOnRect(Rectangle Rect) {
			return RandomPointOnRect(Rect.x, Rect.y, Rect.width, Rect.height);
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

		public static bool Intersects(Vector2 A1, Vector2 A2, Vector2 B1, Vector2 B2, ref IntersectionResult Result) {
			Vector2 B = A2 - A1;
			Vector2 D = B2 - B1;
			float bDotDPerp = B.X * D.Y - B.Y * D.X;

			// if b dot d == 0, it means the lines are parallel so have infinite intersection points
			if (bDotDPerp == 0)
				return false;

			Vector2 C = B1 - A1;
			float T = (C.X * D.Y - C.Y * D.X) / bDotDPerp;
			if (T < 0 || T > 1)
				return false;

			float U = (C.X * B.Y - C.Y * B.X) / bDotDPerp;
			if (U < 0 || U > 1)
				return false;

			Vector2 Dir = Vector2.Normalize(A2 - A1);
			Vector2 Normal = Vector2.Zero;
			Vector2 Ref = Vector2.Reflect(Dir, Normal);

			Result = new IntersectionResult(Dir, A1 + T * B, Normal, Ref);
			return true;
		}

		public static bool Intersects(Vector2 Start, Vector2 End, Rectangle Rect, out IntersectionResult Result) {
			IntersectionResult ResA = new IntersectionResult();
			IntersectionResult ResB = new IntersectionResult();

			ref IntersectionResult CurRes = ref ResA;

			Vector2 A1 = new Vector2(Rect.x, Rect.y);
			Vector2 A2 = A1 + new Vector2(Rect.width, 0);

			Vector2 B1 = new Vector2(Rect.x, Rect.y);
			Vector2 B2 = B1 + new Vector2(0, Rect.height);

			Vector2 C1 = B2;
			Vector2 C2 = C1 + new Vector2(Rect.width, 0);

			Vector2 D1 = A2;
			Vector2 D2 = D1 + new Vector2(0, Rect.height);

			int IntersectCount = 0;

			if (Intersects(Start, End, A1, A2, ref CurRes)) {
				CurRes = ref ResB;
				IntersectCount++;
			}

			if (Intersects(Start, End, B1, B2, ref CurRes)) {
				CurRes = ref ResB;
				IntersectCount++;
			}

			if (Intersects(Start, End, C1, C2, ref CurRes)) {
				CurRes = ref ResB;
				IntersectCount++;
			}

			if (Intersects(Start, End, D1, D2, ref CurRes)) {
				IntersectCount++;
			}

			if (IntersectCount <= 0) {
				Result = new IntersectionResult();
				return false;
			} else if (IntersectCount == 1) {
				Result = ResA;
				return true;
			} else if (IntersectCount == 2) {
				if (Vector2.DistanceSquared(Start, ResA.HitPoint) < Vector2.DistanceSquared(Start, ResB.HitPoint))
					Result = ResA;
				else
					Result = ResB;

				return true;
			} else
				throw new Exception("Wat");
		}

		public static string PtrToStringUTF8(IntPtr Ptr) {
			byte* pStringUtf8 = (byte*)Ptr;
			int len = 0;

			while (pStringUtf8[len] != 0)
				len++;

			return Encoding.UTF8.GetString(pStringUtf8, len);
		}
	}
}
