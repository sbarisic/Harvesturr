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
using Flexbox;

namespace Harvesturr {
	public class GUIState {
		public static int GUIButtonHeight = 50;
		public static int GUIButtonWidth = 400;

		public static int GUIPadding = 10;
		public static int GUIRectHeight = GUIButtonHeight + GUIPadding * 2;

		List<GUIControl> Controls = new List<GUIControl>();
		Node FlexRoot;

		public GUIState() {
			Controls = new List<GUIControl>();
			FlexRoot = Flex.CreateDefaultNode();
			FlexRoot.nodeStyle.Apply("display: flex;");
		}

		public virtual void Init() {
		}

		public virtual void UpdateInput(float Dt) {
		}

		public virtual void AddControl(GUIControl Ctrl) {
			Controls.Add(Ctrl);
			FlexRoot.AddChild(Ctrl.FlexNode);
		}

		public virtual IEnumerable<GUIControl> GetControls() {
			return Controls;
		}

		public virtual void Update(float Dt) {
			Flex.CalculateLayout(FlexRoot, GameEngine.ScreenWidth, GameEngine.ScreenHeight, Direction.LTR);

			for (int i = 0; i < Controls.Count; i++)
				Controls[i].Update();
		}

		public virtual void DrawScreen() {
			for (int i = 0; i < Controls.Count; i++)
				Controls[i].Draw();
		}

		public virtual void DrawWorld() {
		}
	}
}
