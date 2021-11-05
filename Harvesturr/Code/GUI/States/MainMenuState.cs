using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
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
			Controls.Add(BtnNewGame);

			GUIButton BtnSettings = new GUIButton(GUI.GUIFontLarge, "Settings", 100, 150, 200, GUIButtonHeight);
			BtnSettings.Font = GUI.GUIFontLarge;
			Controls.Add(BtnSettings);

			GUIButton BtnQuit = new GUIButton(GUI.GUIFontLarge, "Quit", 100, 200, 200, GUIButtonHeight);
			BtnQuit.Font = GUI.GUIFontLarge;
			Controls.Add(BtnQuit);
		}

		public override void RecalculatePositions() {
		}
	}
}
