using Raylib_cs;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using FishMarkupLanguage;

namespace Harvesturr {
	public class MainMenuState : GUIState {
		public bool IsGameRunning {
			get {
				return GameEngine.IsGameRunning;
			}
		}

		public GUIButton FindButton(string ID) {
			return (GUIButton)FindControlByID(ID);
		}

		public override void Init() {
			GameEngine.PauseGame(true);

			GUIControl[] Controls = GUI.ParseFML("data/gui/main_menu.fml", this).ToArray();
			foreach (GUIControl C in Controls) 
				AddControl(C);
		}

		public override void ChangedStateTo() {
			foreach (GUIControl C in Controls) {
				if (C is GUIScriptControl ScriptCtrl)
					ScriptCtrl.Run(this);
			}
		}

		public void BtnContinue_OnClick(GUIControl Ctrl) {
			GUI.ChangeState(new InGameState() { PreserveCamera = true });
		}

		public void BtnNewGame_OnClick(GUIControl Ctrl) {
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

		public void BtnSettings_OnClick(GUIControl Ctrl) {

		}

		public void BtnQuit_OnClick(GUIControl Ctrl) {
			Environment.Exit(0);
		}
	}
}
