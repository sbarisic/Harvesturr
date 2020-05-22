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
		NONE
	}

	static class GameEngine {
		[DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern bool SetDllDirectory(string FileName);

		public static bool DebugView;
		public static bool DebugFast;

		public static bool DebugPerformance;

		static void Main(string[] args) {
			if (!SetDllDirectory("native/" + ((IntPtr.Size == 8) ? "x64" : "x86")))
				throw new Exception("Failed to set DLL directory");

			bool DebugAll = args.Contains("--debug-all");

			DebugView = DebugAll || args.Contains("--debug");
			DebugPerformance = DebugAll || args.Contains("--performance");
			DebugFast = DebugAll || args.Contains("--fast");

			Start();
		}

		static GameUnit[] GameUnits = new GameUnit[64];
		static EffectPainter[] Effects = new EffectPainter[256];

		// Used by picking functions, automatically increase in size
		static GameUnit[] GameUnitsTemp = new GameUnit[0];
		static RaycastResult[] RaycastResultTemp = new RaycastResult[0];

		static Camera2D GameCamera;
		static int GUIRectHeight = 50;
		static DrawState CurrentDrawState;

		static Color GUIPanelColor;
		static int ScreenWidth;
		static int ScreenHeight;
		static int Resources;

		public static Vector2 MousePosScreen;
		public static Vector2 MousePosWorld;
		public static float Zoom;
		public static bool DrawZoomDetails;
		public static float Time;

		// Right click mouse dragging
		static Vector2 MouseDragStartLocation;
		static Vector2 MouseDragStartMouse;
		static bool IsMouseDragging;

		static List<GameTool> GameTools = new List<GameTool>();
		static GameTool ActiveGameTool;


		static void GUILoadStyle(string Name) {
			Raygui.GuiLoadStyle(string.Format("data/gui_styles/{0}/{0}.rgs", Name));
		}

		static void Start() {
			const int Width = 1366;
			const int Height = 768;

			Raylib.InitWindow(Width, Height, "Harvesturr");
			Raylib.SetTargetFPS(60);

			GUILoadStyle("jungle");

			GUIPanelColor = Raylib.Fade(Color.BLACK, 0.8f);
			GameCamera = new Camera2D(new Vector2(Width, Height) / 2, Vector2.Zero, 0, 2);

			GameMap.Load("test");

			if (DebugPerformance) {
				const float Dist = 50;

				Rectangle Rect = GameMap.GetBounds();
				Vector2 Pos = new Vector2(Rect.x, Rect.y) + new Vector2(10);
				int XCount = (int)(Rect.width / Dist);
				int YCount = (int)(Rect.height / Dist);

				for (int X = 0; X < XCount; X++)
					for (int Y = 0; Y < YCount; Y++)
						Spawn(new UnitConduit(Pos + new Vector2(X, Y) * Dist));
			} else
				for (int i = 0; i < 100; i++)
					Spawn(new UnitMineral(GameMap.RandomMineralPoint(), Utils.Random(0, 100) > 80));

			Spawn(new UnitAlienUfo(Vector2.Zero));

			GameTools.AddRange(IsGameToolAttribute.CreateAllGameTools());
			Resources = 50;

			if (DebugView)
				Resources = int.MaxValue;

			// Test

			/*UnitConduit ConduitA = new UnitConduit(new Vector2(314.844f, 167.216f));
			UnitConduit ConduitB = new UnitConduit(new Vector2(380.955f, 166.661f));
			UnitConduit ConduitC = new UnitConduit(new Vector2(348.513f, 215.875f));

			Spawn(ConduitA);
			Spawn(ConduitB);
			Spawn(ConduitC);
			Spawn(new UnitEnergyPacket(ConduitA, ConduitB));*/


			while (!Raylib.WindowShouldClose()) {
				float FrameTime = Raylib.GetFrameTime();
				Time = (float)Raylib.GetTime();

				ScreenWidth = Raylib.GetScreenWidth();
				ScreenHeight = Raylib.GetScreenHeight();

				if (FrameTime < 0.5f)
					Update(FrameTime);
				else
					Console.WriteLine("Skipping update, frame time {0} s", FrameTime);

				Raylib.BeginDrawing();
				Raylib.ClearBackground(Color.SKYBLUE);

				Raylib.BeginMode2D(GameCamera);
				CurrentDrawState = DrawState.WORLD;
				DrawWorld();
				Raylib.EndMode2D();

				CurrentDrawState = DrawState.SCREEN;
				DrawScreen();
				Raylib.EndDrawing();

				CurrentDrawState = DrawState.NONE;
			}

			Raylib.CloseWindow();
		}

		static void Update(float Dt) {
			MousePosScreen = Raylib.GetMousePosition();
			MousePosWorld = Raylib.GetScreenToWorld2D(MousePosScreen, GameCamera);
			Zoom = GameCamera.zoom;
			DrawZoomDetails = Zoom >= 2;

			float Amt = 100 * Dt;

			if (Raylib.IsKeyDown(KeyboardKey.KEY_W))
				GameCamera.target += new Vector2(0, -Amt);

			if (Raylib.IsKeyDown(KeyboardKey.KEY_A))
				GameCamera.target += new Vector2(-Amt, 0);

			if (Raylib.IsKeyDown(KeyboardKey.KEY_S))
				GameCamera.target += new Vector2(0, Amt);

			if (Raylib.IsKeyDown(KeyboardKey.KEY_D))
				GameCamera.target += new Vector2(Amt, 0);

			if (!IsMouseDragging) {
				int Wheel = Raylib.GetMouseWheelMove();
				if (Wheel != 0) {
					GameCamera.zoom += Wheel / 10.0f;

					if (GameCamera.zoom < 0.5f)
						GameCamera.zoom = 0.5f;

					if (GameCamera.zoom > 3)
						GameCamera.zoom = 3;

					Console.WriteLine("Zoom: {0}", GameCamera.zoom);
				}
			}

			if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_MIDDLE_BUTTON))
				GameCamera.zoom = 2;

			if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_RIGHT_BUTTON)) {
				MouseDragStartMouse = MousePosScreen;
				MouseDragStartLocation = GameCamera.target;
				IsMouseDragging = true;
			} else if (Raylib.IsMouseButtonReleased(MouseButton.MOUSE_RIGHT_BUTTON)) {
				IsMouseDragging = false;
			}

			if (IsMouseDragging) {
				Vector2 Delta = (MouseDragStartMouse - MousePosScreen) * (1.0f / GameCamera.zoom);
				GameCamera.target = MouseDragStartLocation + Delta;
			}

			GameMap.Update(Dt);

			for (int i = 0; i < GameUnits.Length; i++)
				if (GameUnits[i] != null) {
					if (GameUnits[i].Destroyed) {
						GameUnits[i] = null;
						continue;
					}

					GameUnits[i].Update(Dt);
				}

			ActiveGameTool?.Update(Dt);

			if (Utils.IsInside(new Rectangle(0, 0, ScreenWidth, ScreenHeight - GUIRectHeight), MousePosScreen) && GameMap.IsInBounds(MousePosWorld)) {
				if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
					ActiveGameTool?.OnWorldMousePress(MousePosWorld, true);

				if (Raylib.IsMouseButtonReleased(MouseButton.MOUSE_LEFT_BUTTON))
					ActiveGameTool?.OnWorldMousePress(MousePosWorld, false);
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

		static void DrawEffects(bool ScreenSpace) {
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

		static void DrawWorld() {
			GameMap.DrawWorld();

			for (int i = 0; i < GameUnits.Length; i++)
				if (GameUnits[i] != null)
					GameUnits[i].DrawWorld();

			for (int i = 0; i < GameUnits.Length; i++)
				if (GameUnits[i] != null)
					GameUnits[i].DrawGUI();

			DrawEffects(false);

			ActiveGameTool?.DrawWorld();
		}

		static void DrawScreen() {
			DrawEffects(true);

			Raylib.DrawRectangle(0, ScreenHeight - GUIRectHeight, ScreenWidth, GUIRectHeight, GUIPanelColor);
			Raylib.DrawRectangle(0, 0, ScreenWidth, 24, GUIPanelColor);

			if (DebugView) {
				float FrameTime = Raylib.GetFrameTime();
				float FPS = 1.0f / FrameTime;
				Raylib.DrawText(string.Format("{0} / {1} Units, {2:0.000} ms, {3:0.00} FPS", GameUnits.Count(U => U != null), GameUnits.Length, FrameTime, FPS), 2, 2, 20, Color.WHITE);
			}

			string ResourcesText = "R$ " + Resources;
			int TextWidth = Raylib.MeasureText(ResourcesText, 20);
			Raylib.DrawText(ResourcesText, ScreenWidth - TextWidth - 10, 2, 20, Color.GREEN);

			int ButtonCount = 0;
			for (int i = 0; i < GameTools.Count; i++) {
				GameTool T = GameTools[i];
				bool Checked = AddButton(ref ButtonCount, T.Name, T.Active);

				if (Checked && !T.Active) {
					for (int j = 0; j < GameTools.Count; j++)
						GameTools[j].Active = false;

					T.Active = Checked;
					T.OnSelected();
					ActiveGameTool = T;
				}
			}
		}

		static bool AddButton(ref int ButtonCount, string Text, bool Active) {
			int ButtonHeight = (int)(GUIRectHeight * 0.8f);
			int ButtonWidth = 80;
			int ButtonPadding = 10;

			int ButtonX = ButtonPadding + (ButtonWidth * ButtonCount + ButtonPadding * ButtonCount);
			int ButtonY = (ScreenHeight - GUIRectHeight) + (GUIRectHeight - ButtonHeight) / 2;

			bool Ret = Raygui.GuiToggle(new Rectangle(ButtonX, ButtonY, ButtonWidth, ButtonHeight), Text, Active);
			ButtonCount++;
			return Ret;
		}

		public static void DrawTooltip(Vector2 Pos, string Text, Color? Clr = null, bool Offset = true) {
			if (CurrentDrawState == DrawState.NONE)
				throw new Exception("Can not tooltip outside drawing functions");

			if (Offset)
				Pos += new Vector2(8);

			if (CurrentDrawState == DrawState.WORLD) {
				Pos = Raylib.GetWorldToScreen2D(Pos, GameCamera);
				Raylib.EndMode2D();
			}

			int FontSize = 10;
			int YPadding = 2;
			int XPadding = 5;

			string[] Lines = Text.Trim().Split('\n');
			int MaxWidth = Lines.Select(L => Raylib.MeasureText(L, FontSize)).Max();


			Raylib.DrawRectangle((int)Pos.X, (int)Pos.Y, MaxWidth + XPadding * 2, Lines.Length * FontSize + (Lines.Length + 2) * YPadding, GUIPanelColor);

			for (int i = 0; i < Lines.Length; i++)
				Raylib.DrawText(Lines[i], (int)Pos.X + XPadding, (int)Pos.Y + YPadding * (i + 1) + FontSize * i, FontSize, Clr ?? Color.WHITE);

			if (CurrentDrawState == DrawState.WORLD)
				Raylib.BeginMode2D(GameCamera);
		}

		public static void DrawBar(Vector2 Pos, float Amt, Color Clr) {
			if (CurrentDrawState == DrawState.NONE)
				throw new Exception("Can not tooltip outside drawing functions");

			if (CurrentDrawState == DrawState.WORLD) {
				Pos = Raylib.GetWorldToScreen2D(Pos, GameCamera);
				Raylib.EndMode2D();
			}

			const int Padding = 2;
			const int Width = 48;
			const int Height = 8;

			Pos -= new Vector2(Width / 2, Height / 2);

			if (Amt < 0)
				Amt = 0;
			else if (Amt > 1)
				Amt = 1;

			Raylib.DrawRectangle((int)Pos.X, (int)Pos.Y, Width, Height, GUIPanelColor);
			Raylib.DrawRectangle((int)Pos.X + Padding, (int)Pos.Y + Padding, (int)((Width - Padding * 2) * Amt), Height - Padding * 2, Clr);

			if (CurrentDrawState == DrawState.WORLD)
				Raylib.BeginMode2D(GameCamera);
		}

		public static void DrawDashedLine(Vector2 Start, Vector2 End, float Thick, float SegmentLength, Color Clr) {
			Vector2 A = Start;
			Vector2 B = End;
			Vector2 Dir = Vector2.Normalize(B - A);

			do {
				B = A + Dir * SegmentLength;
				Raylib.DrawLineEx(A, B, Thick, Clr);
				A = B + Dir * SegmentLength;
			} while (Vector2.Distance(B, End) > (SegmentLength * 2));
		}

		static IEnumerable<GameUnit> GetAllGameUnits(bool PickUnpickable = false) {
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

		public static GameUnit PickNextAttackTarget(Vector2 Position, float Range) {
			PickInRange(ref GameUnitsTemp, out int Length, Position, Range);

			for (int i = 0; i < Length; i++) {
				if (GameUnitsTemp[i] is GameUnitAlien || GameUnitsTemp[i] is UnitMineral)
					GameUnitsTemp[i] = null;
			}

			Length = Utils.Rearrange(GameUnitsTemp);
			if (Length <= 0)
				return null;

			return GameUnitsTemp[Utils.Random(0, Length)];
		}

		public static GameUnit PickNextEnergyPacketTarget(UnitConduit CurConduit, GameUnit Except1, GameUnit Except2) {
			if (CurConduit.LinkedConduit != null) {
				if (CurConduit.LinkedConduit.Destroyed)
					CurConduit.LinkedConduit = null;
				else
					return CurConduit.LinkedConduit;
			}

			PickInRange(ref GameUnitsTemp, out int Length, CurConduit.Position, UnitConduit.ConnectRangePower);

			if (Length == 0)
				return null;

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

			int MaxLen = Utils.Rearrange(GameUnitsTemp);
			if (MaxLen <= 0)
				return null;

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

		static void AddEffect(EffectPainter Painter) {
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
	}

	static class GameMap {
		static Texture2D MapTex;

		static int X;
		static int Y;
		static int Width;
		static int Height;

		public static Rectangle GetBounds() {
			return new Rectangle(X, Y, Width, Height);
		}
		public static void Load(string MapName) {
			MapTex = ResMgr.LoadTexture(MapName);

			Width = MapTex.width;
			Height = MapTex.height;

			X = -(Width / 2);
			Y = -(Height / 2);
		}

		public static void Update(float Dt) {
		}

		public static void DrawWorld() {
			Raylib.DrawTexture(MapTex, X, Y, Color.WHITE);
		}

		public static bool IsInBounds(Vector2 Pos) {
			return Utils.IsInside(new Rectangle(X, Y, Width, Height), Pos);
		}

		public static Vector2 RandomPoint() {
			Vector2 Pos = new Vector2(X, Y);
			Vector2 Pos2 = Pos + new Vector2(Width, Height);

			return Utils.Random(Pos, Pos2);
		}

		public static Vector2 RandomMineralPoint() {
			Vector2 Pt = Vector2.Zero;

			while (Vector2.Distance(Vector2.Zero, Pt) < 120)
				Pt = RandomPoint();

			return Pt;
		}
	}

	struct RaycastResult {
		public IntersectionResult Intersection;
		public GameUnit Unit;

		public RaycastResult(IntersectionResult Intersection, GameUnit Unit) {
			this.Intersection = Intersection;
			this.Unit = Unit;
		}
	}

	class EffectPainter {
		public bool ScreenSpace;
		public float EndTime;
		public Action Action;

		public EffectPainter(Action Action, float Length = 1, bool ScreenSpace = false) {
			this.Action = Action;
			this.ScreenSpace = ScreenSpace;
			EndTime = GameEngine.Time + Length;
		}
	}
}
