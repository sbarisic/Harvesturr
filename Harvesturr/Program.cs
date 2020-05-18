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

		static Camera2D GameCamera;
		static GameUnit[] GameUnits;
		static int GUIRectHeight = 50;
		static DrawState CurrentDrawState;

		static Color GUIPanelColor;
		static int ScreenWidth;
		static int ScreenHeight;
		static int Resources;

		public static Vector2 MousePosScreen;
		public static Vector2 MousePosWorld;

		// Right click mouse dragging
		static Vector2 MouseDragStartLocation;
		static Vector2 MouseDragStartMouse;
		static bool IsMouseDragging;

		static List<GameTool> GameTools = new List<GameTool>();

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
			GameUnits = new GameUnit[4096];

			GameMap.Load("test");
			for (int i = 0; i < 100; i++)
				Spawn(new UnitMineral(GameMap.RandomMineralPoint(), Utils.Random(0, 100) > 80));

			GameTools.AddRange(IsGameToolAttribute.CreateAllGameTools());
			Resources = 50;

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

			for (int i = 0; i < GameTools.Count; i++)
				if (GameTools[i].Active)
					GameTools[i].Update(Dt);

			if (Utils.IsInside(new Rectangle(0, 0, ScreenWidth, ScreenHeight - GUIRectHeight), MousePosScreen)) {
				if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
					WorldClick(MousePosWorld);
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

		public static void WorldClick(Vector2 WorldPos) {
			if (!GameMap.IsInBounds(WorldPos))
				return;


			for (int i = 0; i < GameTools.Count; i++)
				if (GameTools[i].Active)
					GameTools[i].OnWorldClick(WorldPos);
		}

		static void DrawWorld() {
			GameMap.DrawWorld();

			for (int i = 0; i < GameUnits.Length; i++)
				if (GameUnits[i] != null)
					GameUnits[i].DrawWorld();

			for (int i = 0; i < GameTools.Count; i++)
				if (GameTools[i].Active)
					GameTools[i].DrawWorld();
		}

		static void DrawScreen() {
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

		public static IEnumerable<GameUnit> Pick(Vector2 WorldPos) {
			for (int i = 0; i < GameUnits.Length; i++) {
				if (GameUnits[i] == null)
					continue;

				if (Raylib.CheckCollisionPointRec(WorldPos, GameUnits[i].GetBoundingRect()))
					yield return GameUnits[i];
			}
		}

		public static IEnumerable<GameUnit> Pick(Rectangle Rect) {
			for (int i = 0; i < GameUnits.Length; i++) {
				if (GameUnits[i] == null)
					continue;

				if (Raylib.CheckCollisionRecs(Rect, GameUnits[i].GetBoundingRect()))
					yield return GameUnits[i];
			}
		}

		public static IEnumerable<GameUnit> PickInRange(Vector2 WorldPos, float Range) {
			for (int i = 0; i < GameUnits.Length; i++) {
				if (GameUnits[i] == null)
					continue;

				if (Vector2.Distance(GameUnits[i].Position, WorldPos) < Range)
					yield return GameUnits[i];
			}
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
}
