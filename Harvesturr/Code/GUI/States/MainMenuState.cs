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

		public void Print(string Str) {
			Console.WriteLine(Str);
		}

		public override void Init() {
			GameEngine.PauseGame(true);

			Scripting Scr = new Scripting();
			Scr.Compile(@"
				return BtnContinue_OnClick;
            ", GetType());

			object Ret = Scr.Run(this);

			GUIControl[] GUIControls = GUI.ParseFML("data/gui/main_menu.fml", this).ToArray();
			foreach (GUIControl C in GUIControls) {
				AddControl(C);
			}

			/*GUIPanel Pnl = new GUIPanel();
            Pnl.ApplyStyle(@"
				position: absolute;
				left: 100px;
				top: 100px;
				width: 30%;
				height: 40%;

				padding: 10px;
				align-items: center;
				flex-direction: column;
				justify-content: space-between;
			");

            {
                string ButtonStyle = "width: 100%; height: 23%;";

                GUIButton BtnContinue = new GUIButton(GUI.GUIFontLarge, "Continue");
                BtnContinue.ApplyStyle(ButtonStyle);
                BtnContinue.OnClick += BtnContinue_OnClick; // TODO: from script
                BtnContinue.Disabled = !GameEngine.IsGameRunning; // TODO: from script
                Pnl.AddControl(BtnContinue);

                GUIButton BtnNewGame = new GUIButton(GUI.GUIFontLarge, "New Game");
                BtnNewGame.ApplyStyle(ButtonStyle);
                BtnNewGame.OnClick += BtnNewGame_OnClick; // TODO: from script
                Pnl.AddControl(BtnNewGame);

                GUIButton BtnSettings = new GUIButton(GUI.GUIFontLarge, "Settings");
                BtnSettings.ApplyStyle(ButtonStyle);
                BtnSettings.OnClick += BtnSettings_OnClick; // TODO: from script
                BtnSettings.Disabled = true;
                Pnl.AddControl(BtnSettings);

                GUIButton BtnQuit = new GUIButton(GUI.GUIFontLarge, "Quit");
                BtnQuit.ApplyStyle(ButtonStyle);
                BtnQuit.OnClick += BtnQuit_OnClick; // TODO: from script
                Pnl.AddControl(BtnQuit);
            }

            AddControl(Pnl);*/
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
