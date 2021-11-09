using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;
using Raylib_cs;
using Raygui_cs;
using System.Numerics;
using System.Diagnostics;

namespace Harvesturr {
	enum DrawState {
		WORLD,
		SCREEN,
		RENDERTEXTURE,
		NONE
	}

	static class GameEngine {
		public static bool DebugView;
		public static bool DebugFast;
		public static bool DebugFastBuild;

		public static bool DebugDrawLaserRange;
		public static bool DebugPerformance;

		public static GameUnit[] GameUnits = new GameUnit[64];
		public static EffectPainter[] Effects = new EffectPainter[256];
		public static int Resources;
		public static bool IsGameRunning;

		// Used by picking functions, automatically increase in size
		public static GameUnit[] GameUnitsTemp = new GameUnit[0];
		public static RaycastResult[] RaycastResultTemp = new RaycastResult[0];

		public static Camera2D GameCamera;
		public static DrawState CurrentDrawState;

		public static int ScreenWidth;
		public static int ScreenHeight;

		public static Vector2 MousePosScreen;
		public static Vector2 MousePosWorld;
		public static float Zoom;
		public static bool DrawZoomDetails;
		public static float Time;

		// Right click mouse dragging
		public static Vector2 MouseDragStartLocation;
		public static Vector2 MouseDragStartMouse;
		public static bool IsMouseDragging;

		// Lockstep stuff
		public static Stopwatch LockstepTimer = Stopwatch.StartNew();
		public static int SimulatedSteps;

		// Timing stuff
		//static bool Paused = false;
		static Stopwatch GameTimer = Stopwatch.StartNew();

		static float NextWaveSpawnTime;

		/*public static void GUILoadStyle(string Name) {
			Raygui.GuiLoadStyle(string.Format("data/gui_styles/{0}/{0}.rgs", Name));
		}*/

		public static void ClearGameState() {
			IsGameRunning = false;
			GameTimer.Restart();

			foreach (GameUnit U in GetAllGameUnits(true))
				U.Destroy();

			for (int i = 0; i < Effects.Length; i++)
				Effects[i] = null;

			Resources = 0;
		}

		public static void SpawnStarters() {
			Spawn(new UnitMineral(new Vector2(-36, -20), true));
			Spawn(new UnitHarvester(new Vector2(10, -31)));
			Spawn(new UnitConduit(new Vector2(30, 0)));
			Spawn(new UnitSolarPanel(new Vector2(34, 50)));
		}

		public static void Lockstep(float StepInterval, float MaxTime) {
			Time = (float)GameTimer.Elapsed.TotalSeconds;
			float Elapsed = Time;

			float RequiredSteps = (Elapsed / StepInterval) - SimulatedSteps;
			int RequiredStepsInt = (int)Math.Floor(RequiredSteps);

			if (RequiredStepsInt <= 0) {
				Update(StepInterval, true);
				return;
			}

			LockstepTimer.Restart();

			for (int i = 0; i < RequiredStepsInt; i++) {
				Update(StepInterval, false);
				SimulatedSteps++;

				if (LockstepTimer.Elapsed.TotalSeconds > MaxTime)
					break;
			}
		}

		public static void Update(float Dt, bool Paused) {
			MousePosScreen = Raylib.GetMousePosition();
			MousePosWorld = Raylib.GetScreenToWorld2D(MousePosScreen, GameCamera);

			GUI.UpdateInput(Dt);
			GUI.UpdateGUI(Dt);

			if (!Paused) {
				GameMap.Update(Dt);

				for (int i = 0; i < GameUnits.Length; i++)
					if (GameUnits[i] != null) {
						if (GameUnits[i].Destroyed) {
							GameUnits[i] = null;
							continue;
						}

						GameUnits[i].IsMouseHover = Raylib.CheckCollisionPointRec(MousePosWorld, GameUnits[i].GetBoundingRect());
						GameUnits[i].Update(Dt);
					}

				if (NextWaveSpawnTime < Time) {
					NextWaveSpawnTime = Time + 10;
					// SpawnEnemyWave(Wave++);
				}
			}
		}

		public static void SpawnEnemyWave(int Wave) {
			int Count = (int)(((float)Wave - 5) / 5 * 2.0f);
			if (Count < 0)
				Count = 0;

			Console.WriteLine("Wave {0}, Count {1}", Wave, Count);

			for (int i = 0; i < Count; i++) {
				Vector2 Pt = Utils.RandomPointOnRect(GameMap.GetBounds());
				UnitAlienUfo Ufo = new UnitAlienUfo(Pt);
				Spawn(Ufo);
			}
		}

		public static void PauseGame(bool Pause) {
			bool Paused = !GameTimer.IsRunning;

			if (Paused && !Pause) {
				GameTimer.Start();
			} else if (!Paused && Pause) {
				GameTimer.Stop();
			}
		}

		public static void Spawn(GameUnit Unit) {
			for (int i = 0; i < GameUnits.Length; i++)
				if (GameUnits[i] == null) {
					GameUnits[i] = Unit;
					return;
				}

			const int BucketSize = 128;
			Array.Resize(ref GameUnits, GameUnits.Length + BucketSize);
			Spawn(Unit);
		}

		public static void DrawEffects(bool ScreenSpace) {
			for (int i = 0; i < Effects.Length; i++) {
				if (Effects[i] != null) {
					if (Effects[i].ScreenSpace != ScreenSpace)
						continue;

					if (Effects[i].EndTime > Time)
						Effects[i].Action();
					else
						Effects[i] = null;
				}
			}
		}

		public static void DrawWorld() {
			GameMap.DrawWorld();

			for (int i = 0; i < GameUnits.Length; i++)
				if (GameUnits[i] != null)
					GameUnits[i].DrawWorld();

			for (int i = 0; i < GameUnits.Length; i++)
				if (GameUnits[i] != null)
					GameUnits[i].DrawGUI();

			DrawEffects(false);
			GUI.DrawWorld();
		}

		public static void DrawScreen() {
			DrawEffects(true);
			GUI.DrawScreen();
		}

		public static void DrawDashedLine(Vector2 Start, Vector2 End, float Thick, float SegmentLength, Color Clr, float Offset = 0) {
			Vector2 A = Start;
			Vector2 B = End;
			Vector2 Dir = Vector2.Normalize(B - A);

			Vector2 StartOffset = Dir * (Offset % (SegmentLength * 2));
			A = A + StartOffset;

			if (StartOffset.Length() > SegmentLength)
				Raylib.DrawLineEx(Start, Start + StartOffset - (Dir * SegmentLength), Thick, Clr);

			do {
				B = A + Dir * SegmentLength;
				Raylib.DrawLineEx(A, B, Thick, Clr);

				A = B + Dir * SegmentLength;
			} while (Vector2.Distance(B, End) > (SegmentLength * 2));

			if (Vector2.Distance(B, End) > SegmentLength)
				Raylib.DrawLineEx(B + (Dir * SegmentLength), End, Thick, Clr);
		}

		public static IEnumerable<GameUnit> GetAllGameUnits(bool PickUnpickable = false) {
			for (int i = 0; i < GameUnits.Length; i++) {
				if (GameUnits[i] == null)
					continue;

				if (!GameUnits[i].Pickable && !PickUnpickable)
					continue;

				yield return GameUnits[i];
			}
		}

		public static IEnumerable<GameUnit> Pick(Vector2 WorldPos, bool PickUnpickable = false) {
			foreach (var U in GetAllGameUnits(PickUnpickable))
				if (Raylib.CheckCollisionPointRec(WorldPos, U.GetBoundingRect()))
					yield return U;
		}

		public static IEnumerable<GameUnit> Pick(Rectangle Rect, bool PickUnpickable = false) {
			foreach (var U in GetAllGameUnits(PickUnpickable))
				if (Raylib.CheckCollisionRecs(Rect, U.GetBoundingRect()))
					yield return U;
		}

		public static IEnumerable<GameUnit> PickInRange(Vector2 WorldPos, float Range, bool PickUnpickable = false) {
			foreach (var U in GetAllGameUnits(PickUnpickable))
				if (Vector2.Distance(U.Position, WorldPos) < Range)
					yield return U;
		}

		public static void PickInRange(ref GameUnit[] Units, out int Count, Vector2 WorldPos, float Range, bool PickUnpickable = false) {
			int Idx = 0;

			foreach (var U in GetAllGameUnits(PickUnpickable))
				if (Vector2.Distance(U.Position, WorldPos) < Range) {
					if (Idx >= Units.Length)
						Array.Resize(ref Units, Units.Length + 32);

					Units[Idx++] = U;
				}

			for (int i = Idx; i < Units.Length; i++)
				Units[i] = null;

			Count = Idx;
		}

		public static bool Collide(GameUnit A, GameUnit B) {
			return Raylib.CheckCollisionRecs(A.GetBoundingRect(), B.GetBoundingRect());
		}

		public static bool Collide(Vector2 Pos, Vector2 Dir, Vector2 Center, float Radius, out Vector2 CollisionPoint) {
			Ray R = new Ray(new Vector3(Pos, 0), new Vector3(Dir, 0));
			Vector3 Point = Vector3.Zero;
			CollisionPoint = Vector2.Zero;

			if (Raylib.CheckCollisionRaySphereEx(R, new Vector3(Center, 0), Radius, ref Point)) {
				CollisionPoint = new Vector2(Point.X, Point.Y);
				return true;
			}

			return false;
		}

		public static bool Raycast(Vector2 StartPos, Vector2 EndPos, out RaycastResult Result) {
			PickInRange(ref GameUnitsTemp, out int Length, StartPos, Vector2.Distance(StartPos, EndPos));
			int TempPointIdx = 0;

			for (int i = 0; i < Length; i++) {
				if (TempPointIdx >= RaycastResultTemp.Length)
					Array.Resize(ref RaycastResultTemp, RaycastResultTemp.Length + 16);

				if (Utils.Intersects(StartPos, EndPos, GameUnitsTemp[i].GetBoundingRect(), out IntersectionResult IResult)) {
					RaycastResultTemp[TempPointIdx] = new RaycastResult(IResult, GameUnitsTemp[i]);
					TempPointIdx++;
				}
			}

			if (TempPointIdx > 0) {
				// TODO: Fucking optimize this shit
				Result = RaycastResultTemp.Take(TempPointIdx).OrderBy(P => Vector2.DistanceSquared(StartPos, P.Intersection.HitPoint)).First();
				return true;
			}

			Result = new RaycastResult(IntersectionResult.Empty, null);
			return false;
		}

		public static GameUnit PickNextAttackTarget(Vector2 Position, float Range, bool Nearest) {
			PickInRange(ref GameUnitsTemp, out int Length, Position, Range);

			for (int i = 0; i < Length; i++) {
				if (GameUnitsTemp[i] is GameUnitAlien || GameUnitsTemp[i] is UnitMineral)
					GameUnitsTemp[i] = null;
			}

			if (Nearest) {
				return GameUnitsTemp.Where(P => P != null).OrderBy(P => Vector2.DistanceSquared(Position, P.Position)).FirstOrDefault();
			} else {
				Length = Utils.Rearrange(GameUnitsTemp);
				if (Length <= 0)
					return null;

				return GameUnitsTemp[Utils.Random(0, Length)];
			}
		}

		public static GameUnit PickNextEnergyPacketTarget(GameUnit CurUnit, GameUnit Except1, GameUnit Except2) {
			GameUnit LinkedConduit = null;
			UnitConduit CurConduit = CurUnit as UnitConduit;

			if (CurConduit != null && CurConduit.GetLinkedConduit != null)
				LinkedConduit = CurConduit.GetLinkedConduit;

			PickInRange(ref GameUnitsTemp, out int Length, CurUnit.Position, UnitConduit.ConnectRangePower);

			for (int i = 0; i < Length; i++) {
				if (GameUnitsTemp[i] == Except1 || GameUnitsTemp[i] == Except2) {
					GameUnitsTemp[i] = null;
					continue;
				}

				if (GameUnitsTemp[i] is UnitMineral) {
					GameUnitsTemp[i] = null;
					continue;
				}

				if (GameUnitsTemp[i] is UnitConduit)
					continue;

				if (!GameUnitsTemp[i].CanAcceptEnergyPacket()) {
					GameUnitsTemp[i] = null;
					continue;
				} else
					return GameUnitsTemp[i];
			}

			if (Length == 0)
				return LinkedConduit;

			int MaxLen = Utils.Rearrange(GameUnitsTemp);
			if (MaxLen <= 0)
				return LinkedConduit;

			if (LinkedConduit != null)
				return LinkedConduit;

			return GameUnitsTemp[Utils.Random(0, MaxLen)];
		}

		public static Rectangle GetBoundingRect(Texture2D Tex, Vector2 WorldPos) {
			float X = WorldPos.X - (Tex.width / 2);
			float Y = WorldPos.Y - (Tex.height / 2);
			return new Rectangle(X, Y, Tex.width, Tex.height);
		}

		public static void DrawTextureCentered(Texture2D Tex, Vector2 WorldPos, float Rotation = 0, Color? Clr = null) {
			//Raylib.DrawTexture(Tex, (int)WorldPos.X - (Tex.width / 2), (int)WorldPos.Y - (Tex.height / 2), Clr ?? Color.WHITE);
			//Raylib.DrawTextureEx(Tex, new Vector2((int)WorldPos.X - (Tex.width / 2), (int)WorldPos.Y - (Tex.height / 2)), Rotation, Scale, Clr ?? Color.WHITE);
			Raylib.DrawTexturePro(Tex, new Rectangle(0, 0, Tex.width, Tex.height), new Rectangle(WorldPos.X, WorldPos.Y, Tex.width, Tex.height), new Vector2(Tex.width, Tex.height) / 2, Rotation, Clr ?? Color.WHITE);
		}

		public static void DrawCircle(Vector2 Pos, float Radius, Color Clr, bool Outline = true) {
			if (Outline)
				Raylib.DrawCircleLines((int)Pos.X, (int)Pos.Y, Radius, Clr);
			else
				Raylib.DrawCircle((int)Pos.X, (int)Pos.Y, Radius, Clr);
		}

		public static bool TryConsumeResources(int Amt) {
			int NewResources = Resources - Amt;

			if (NewResources >= 0) {
				Resources = NewResources;
				return true;
			}

			return false;
		}

		public static void AddResource(int Amt) {
			Resources += Amt;
		}

		public static void AddEffect(EffectPainter Painter) {
			for (int i = 0; i < Effects.Length; i++)
				if (Effects[i] == null) {
					Effects[i] = Painter;
					return;
				}
		}

		public static void AddEffect(Action Action, float Length, bool ScreenSpace = false) {
			AddEffect(new EffectPainter(Action, Length, ScreenSpace));
		}

		public static void AddLightningEffect(Vector2 WorldPos, Color Clr, float Length = 0.1f) {
			int ArmCount = (int)Math.Ceiling(1.5f * Zoom);
			int PartCount = (int)Math.Ceiling(1.0f * Zoom);
			float Len = 8 * Zoom;

			Vector2[] Points = new Vector2[ArmCount * PartCount];
			for (int i = 0; i < Points.Length; i++)
				Points[i] = Utils.Random(new Vector2(-Len), new Vector2(Len));

			AddEffect(() => {
				Vector2 WrldPos = Raylib.GetWorldToScreen2D(WorldPos, GameCamera);

				for (int Arm = 0; Arm < ArmCount; Arm++) {
					Vector2 LastPoint = WrldPos;

					for (int Part = 0; Part < PartCount; Part++) {
						int Idx = Arm * PartCount + Part;

						Raylib.DrawLineEx(LastPoint, LastPoint + Points[Idx], 2, Clr);
						LastPoint += Points[Idx];
					}
				}
			}, Length, true);
		}

		public static void DrawLinkLines(Vector2 Pos, float Radius, Color Clr, Func<IEnumerable<GameUnit>, IEnumerable<GameUnit>> Filter = null) {
			Raylib.DrawCircleLines((int)Pos.X, (int)Pos.Y, Radius, Clr);

			if (Filter != null)
				foreach (var C in Filter(GameEngine.PickInRange(Pos, Radius)))
					Raylib.DrawLineV(Pos, C.Position, Clr);
		}
	}
}
