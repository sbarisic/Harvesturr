using Raylib_cs;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Harvesturr {
	class InGameState : GUIState {
		// public static Color GUIPanelColor;

		//public static int GUIButtonHeight = 40;
		//public static int GUIPadding = 10;
		//public static int GUIRectHeight = GUIButtonHeight + GUIPadding * 2;

		public bool PreserveCamera = false;

		GUIPanel BottomPanel;
		List<GameTool> GameTools;
		GameTool ActiveGameTool;

		public override void Init() {
			GameEngine.PauseGame(false);

			if (!PreserveCamera)
				GameEngine.GameCamera = new Camera2D(new Vector2(GameEngine.ScreenWidth, GameEngine.ScreenHeight) / 2, Vector2.Zero, 0, 2);


			BottomPanel = new GUIPanel();
			BottomPanel.ApplyStyle(@"
                position: absolute;
                left: 10%;
                right: 10%;
                bottom: 1%;
                height: 64px;

                padding: 5px;
                align-items: center;
                flex-direction: row;
                justify-content: flex-start;
            ");

			GameTools = new List<GameTool>(IsGameToolAttribute.CreateAllGameTools().OrderBy(T => T.GameToolAttribute.Index));

			for (int i = 0; i < GameTools.Count; i++) {
				GameTool T = GameTools[i];

				GUIButton Btn = new GUIButton(GUI.GUIFontLarge, T.Name);
				Btn.ApplyStyle("width: 180px; height: 100%;");
				Btn.OnCheckToggle += (Ctrl) => T.Active;
				Btn.OnClick += (Ctrl) => SelectTool(T);
				BottomPanel.AddControl(Btn);
			}

			SelectTool(GameTools.Where(T => T is GameToolPicker).FirstOrDefault());
			AddControl(BottomPanel);
		}

		void SelectTool(GameTool T) {
			if (ActiveGameTool == T)
				return;

			for (int j = 0; j < GameTools.Count; j++)
				GameTools[j].Active = false;

			T.Active = true;
			T.OnSelected();
			ActiveGameTool = T;
		}

		public override void UpdateInput(float Dt) {
			GameEngine.Zoom = GameEngine.GameCamera.zoom;
			GameEngine.DrawZoomDetails = GameEngine.Zoom >= 2;

			if (Raylib.IsKeyPressed(KeyboardKey.KEY_ESCAPE)) {
				GUI.ChangeState(new MainMenuState());
			}

			float Amt = 100 * Dt;

			if (Raylib.IsKeyDown(KeyboardKey.KEY_W))
				GameEngine.GameCamera.target += new Vector2(0, -Amt);

			if (Raylib.IsKeyDown(KeyboardKey.KEY_A))
				GameEngine.GameCamera.target += new Vector2(-Amt, 0);

			if (Raylib.IsKeyDown(KeyboardKey.KEY_S))
				GameEngine.GameCamera.target += new Vector2(0, Amt);

			if (Raylib.IsKeyDown(KeyboardKey.KEY_D))
				GameEngine.GameCamera.target += new Vector2(Amt, 0);

			if (!GameEngine.IsMouseDragging) {
				int Wheel = (int)Raylib.GetMouseWheelMove();
				if (Wheel != 0) {
					GameEngine.GameCamera.zoom += Wheel / 10.0f;

					if (GameEngine.GameCamera.zoom < 0.5f)
						GameEngine.GameCamera.zoom = 0.5f;

					if (GameEngine.GameCamera.zoom > 3)
						GameEngine.GameCamera.zoom = 3;
				}
			}

			if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_MIDDLE_BUTTON))
				GameEngine.GameCamera.zoom = 2;

			if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_RIGHT_BUTTON)) {
				GameEngine.MouseDragStartMouse = GameEngine.MousePosScreen;
				GameEngine.MouseDragStartLocation = GameEngine.GameCamera.target;
				GameEngine.IsMouseDragging = true;
			} else if (Raylib.IsMouseButtonReleased(MouseButton.MOUSE_RIGHT_BUTTON)) {
				GameEngine.IsMouseDragging = false;
			}

			if (GameEngine.IsMouseDragging) {
				Vector2 Delta = (GameEngine.MouseDragStartMouse - GameEngine.MousePosScreen) * (1.0f / GameEngine.GameCamera.zoom);
				GameEngine.GameCamera.target = GameEngine.MouseDragStartLocation + Delta;
			}
		}

		public override void Update(float Dt) {
			base.Update(Dt);

			foreach (GUIControl C in GetControls())
				if (C.IsHovered)
					return;

			ActiveGameTool?.Update(Dt);

			if (GameMap.IsInBounds(GameEngine.MousePosWorld)) {
				if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
					ActiveGameTool?.OnWorldMousePress(GameEngine.MousePosWorld, true);

				if (Raylib.IsMouseButtonReleased(MouseButton.MOUSE_LEFT_BUTTON))
					ActiveGameTool?.OnWorldMousePress(GameEngine.MousePosWorld, false);
			}
		}

		public override void DrawScreen() {
			// Raylib.DrawRectangle(0, GameEngine.ScreenHeight - GUIRectHeight, GameEngine.ScreenWidth, GUIRectHeight, GUIPanelColor);
			// Raylib.DrawRectangle(0, 0, GameEngine.ScreenWidth, 24, GUIPanelColor);

			if (GameEngine.DebugView) {
				float FrameTime = Raylib.GetFrameTime();
				float FPS = 1.0f / FrameTime;
				Raylib.DrawText(string.Format("{0} / {1} Units, {2:0.000} ms, {3:0.00} FPS", GameEngine.GameUnits.Count(U => U != null), GameEngine.GameUnits.Length, FrameTime, FPS), 2, 2, 20, Color.WHITE);
			}

			string ResourcesText = "R$ " + GameEngine.Resources;
			int TextWidth = Raylib.MeasureText(ResourcesText, 20);
			Raylib.DrawText(ResourcesText, GameEngine.ScreenWidth - TextWidth - 10, 2, 20, Color.GREEN);

			base.DrawScreen();
		}

		public override void DrawWorld() {
			ActiveGameTool?.DrawWorld();
		}
	}
}
