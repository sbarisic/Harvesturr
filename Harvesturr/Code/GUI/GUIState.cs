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

		protected List<GUIControl> Controls = new List<GUIControl>();
		protected Node FlexRoot;

		public GUIState() {
			Controls = new List<GUIControl>();
			FlexRoot = Flex.CreateDefaultNode();
			FlexRoot.nodeStyle.Apply("display: flex;");
		}

		public virtual void Init() {
		}

		public virtual void ChangedStateTo() {
		}

		public virtual void ChangedStateFrom() {
		}

		public virtual void UpdateInput(float Dt) {
		}

		public virtual void AddControl(GUIControl Ctrl) {
			Controls.Add(Ctrl);

			if (Ctrl.FlexNode != null)
				FlexRoot.AddChild(Ctrl.FlexNode);
		}

		public virtual bool TryFindControlByID(string ID, out GUIControl Control) {
			foreach (GUIControl C in Controls) {
				if (C.ID == ID) {
					Control = C;
					return true;
				}

				if (C.TryFindControlByID(ID, out Control))
					return true;
			}

			Control = null;
			return false;
		}

		public virtual GUIControl FindControlByID(string ID) {
			if (TryFindControlByID(ID, out GUIControl Control))
				return Control;

			return null;
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
