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

		static void Main(string[] args) {
			if (!SetDllDirectory("native/" + ((IntPtr.Size == 8) ? "x64" : "x86")))
				throw new Exception("Failed to set DLL directory");

			Start();
		}

		const int MaxGameUnits = 4096;
		static GameUnit[] GameUnits = new GameUnit[MaxGameUnits];
		//static GameUnit[] GameUnitsTemp = new GameUnit[MaxGameUnits];

		static EffectPainter[] Effects = new EffectPainter[128];

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
			GameUnits = new GameUnit[MaxGameUnits];

			GameMap.Load("test");
			for (int i = 0; i < 100; i++)
				Spawn(new UnitMineral(GameMap.RandomMineralPoint(), Utils.Random(0, 100) > 80));

			GameTools.AddRange(IsGameToolAttribute.CreateAllGameTools());
			Resources = 50;

			if (Debugger.IsAttached)
				Resources += 999999;

			// Test

			/*UnitConduit ConduitA = new UnitConduit(new Vector2(314.844f, 167.216f));
			UnitConduit ConduitB = new UnitConduit(new Vector2(380.955f, 166.661f));
			UnitConduit ConduitC = new UnitConduit(new Vector2(348.513f, 215.875f));

			Spawn(ConduitA);
			Spawn(ConduitB);
			Spawn(ConduitC);
			Spawn(new UnitEnergyPacket(ConduitA, ConduitB));*/


			while (!Raylib.WindowShouldClose()) {
				ScreenWidth = Raylib.GetScreenWidth();
				ScreenHeight = Raylib.GetScreenHeight();
				Update(Raylib.GetFrameTime());

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

			throw new Exception("Could not find free unit slot");
		}

		static void DrawEffects(bool ScreenSpace) {
			float Time = (float)Raylib.GetTime();

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

			Raylib.DrawText(string.Format("{0} / {1}", GameUnits.Count(U => U != null), GameUnits.Length), 2, 2, 20, Color.WHITE);

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

		public static Rectangle GetBoundingRect(Texture2D Tex, Vector2 WorldPos) {
			float X = WorldPos.X - (Tex.width / 2);
			float Y = WorldPos.Y - (Tex.height / 2);
			return new Rectangle(X, Y, Tex.width, Tex.height);
		}

		public static void DrawTextureCentered(Texture2D Tex, Vector2 WorldPos, Color? Clr = null) {
			Raylib.DrawTexture(Tex, (int)WorldPos.X - (Tex.width / 2), (int)WorldPos.Y - (Tex.height / 2), Clr ?? Color.WHITE);
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

	class EffectPainter {
		public bool ScreenSpace;
		public float EndTime;
		public Action Action;

		public EffectPainter(Action Action, float Length = 1, bool ScreenSpace = false) {
			this.Action = Action;
			this.ScreenSpace = ScreenSpace;
			EndTime = (float)Raylib.GetTime() + Length;
		}
	}
}
