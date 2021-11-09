using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Harvesturr {
	class MainMenuState : GUIState {
		public static int GUIButtonHeight = 40;
		public static int GUIButtonWidth = 200;

		public static int GUIPadding = 10;
		public static int GUIRectHeight = GUIButtonHeight + GUIPadding * 2;

		public override void Init() {
			GameEngine.PauseGame(true);

			int XOffset = 100;

			int YOffset = 100;
			int YSpacing = 50;

			if (GameEngine.IsGameRunning) {
				GUIButton BtnContinue = new GUIButton(GUI.GUIFontLarge, "Continue", XOffset, YOffset += YSpacing, GUIButtonWidth, GUIButtonHeight);
				BtnContinue.OnClick += BtnContinue_OnClick;
				Controls.Add(BtnContinue);
			}

			GUIButton BtnNewGame = new GUIButton(GUI.GUIFontLarge, "New Game", XOffset, YOffset += YSpacing, GUIButtonWidth, GUIButtonHeight);
			BtnNewGame.OnClick += BtnNewGame_OnClick;
			Controls.Add(BtnNewGame);

			GUIButton BtnSettings = new GUIButton(GUI.GUIFontLarge, "Settings", XOffset, YOffset += YSpacing, GUIButtonWidth, GUIButtonHeight);
			BtnSettings.OnClick += BtnSettings_OnClick;
			Controls.Add(BtnSettings);

			GUIButton BtnQuit = new GUIButton(GUI.GUIFontLarge, "Quit", XOffset, YOffset += YSpacing, GUIButtonWidth, GUIButtonHeight);
			BtnQuit.OnClick += BtnQuit_OnClick;
			Controls.Add(BtnQuit);
		}

		private void BtnContinue_OnClick() {
			GUI.ChangeState(new InGameState() { PreserveCamera = true });
		}

		private void BtnNewGame_OnClick() {
			GameMap.Load("test");
			GameEngine.IsGameRunning = true;

			if (GameEngine.DebugPerformance) {
				GameMap.DestroyAllMinerals();
				const float Dist = 50;

				Rectangle Rect = GameMap.GetBounds();
				Vector2 Pos = new Vector2(Rect.x, Rect.y) + new Vector2(10);
				int XCount = (int)(Rect.width / Dist);
				int YCount = (int)(Rect.height / Dist);

				for (int X = 0; X < XCount; X++)
					for (int Y = 0; Y < YCount; Y++)
						GameEngine.Spawn(new UnitConduit(Pos + new Vector2(X, Y) * Dist));
			}

			//Spawn(new UnitAlienUfo(Vector2.Zero));

			GameEngine.Resources = 50;

			if (GameEngine.DebugView)
				GameEngine.Resources = 10000000;

			GUI.ChangeState(new InGameState());
		}

		private void BtnSettings_OnClick() {

		}

		private void BtnQuit_OnClick() {
			Environment.Exit(0);
		}

		public override void RecalculatePositions() {

		}
	}
}
