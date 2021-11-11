using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Harvesturr {
	class MainMenuState : GUIState {
		public override void Init() {
			GameEngine.PauseGame(true);

			int XOffset = 100;
			int YOffset = 100;
			int YSpacing = GUIButtonHeight + GUIPadding;

			GUIPanel Pnl = new GUIPanel();
			Pnl.Layout = GUIControlLayout.Absolute;
			Pnl.Left = 100;
			Pnl.Top = 100;

			{
				int BtnOffset = 0;

				GUIButton BtnContinue = new GUIButton(GUI.GUIFontLarge, "Continue");
				BtnContinue.SetPadding(GUIPadding);
				BtnContinue.Width = GUIButtonWidth;
				BtnContinue.Height = GUIButtonHeight;
				BtnContinue.OnClick += BtnContinue_OnClick;
				BtnContinue.Disabled = !GameEngine.IsGameRunning;
				Pnl.Controls.Add(BtnContinue);

				GUIButton BtnNewGame = new GUIButton(GUI.GUIFontLarge, "New Game");
				BtnContinue.SetPadding(GUIPadding);
				BtnNewGame.Width = GUIButtonWidth;
				BtnNewGame.Height = GUIButtonHeight;
				BtnNewGame.OnClick += BtnNewGame_OnClick;
				Pnl.Controls.Add(BtnNewGame);

				GUIButton BtnSettings = new GUIButton(GUI.GUIFontLarge, "Settings");
				BtnContinue.SetPadding(GUIPadding);
				BtnSettings.Width = GUIButtonWidth;
				BtnSettings.Height = GUIButtonHeight;
				BtnSettings.OnClick += BtnSettings_OnClick;
				BtnSettings.Disabled = true;
				Pnl.Controls.Add(BtnSettings);

				GUIButton BtnQuit = new GUIButton(GUI.GUIFontLarge, "Quit");
				BtnContinue.SetPadding(GUIPadding);
				BtnQuit.Width = GUIButtonWidth;
				BtnQuit.Height = GUIButtonHeight;
				BtnQuit.OnClick += BtnQuit_OnClick;
				Pnl.Controls.Add(BtnQuit);
			}

			Controls.Add(Pnl);


			/*GUICheckBox CB = new GUICheckBox(GUI.GUIFontLarge, "Test", 600, 200, 200, 50);
			CB.Checked = true;
			Controls.Add(CB);*/
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
