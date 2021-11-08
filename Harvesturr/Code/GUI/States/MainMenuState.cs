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
		public static int GUIPadding = 10;
		public static int GUIRectHeight = GUIButtonHeight + GUIPadding * 2;

		public override void Init() {
			GUIButton BtnNewGame = new GUIButton(GUI.GUIFontLarge, "New Game", 100, 100, 200, GUIButtonHeight);
			BtnNewGame.Font = GUI.GUIFontLarge;
			BtnNewGame.OnClick += BtnNewGame_OnClick;
			Controls.Add(BtnNewGame);

			GUIButton BtnSettings = new GUIButton(GUI.GUIFontLarge, "Settings", 100, 150, 200, GUIButtonHeight);
			BtnSettings.Font = GUI.GUIFontLarge;
			BtnSettings.OnClick += BtnSettings_OnClick;
			Controls.Add(BtnSettings);

			GUIButton BtnQuit = new GUIButton(GUI.GUIFontLarge, "Quit", 100, 200, 200, GUIButtonHeight);
			BtnQuit.Font = GUI.GUIFontLarge;
			BtnQuit.OnClick += BtnQuit_OnClick;
			Controls.Add(BtnQuit);
		}

		private void BtnNewGame_OnClick() {
			foreach (GameUnit U in GameEngine.GetAllGameUnits(true)) 
				U.Destroy();

			GameMap.Load("test");

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
