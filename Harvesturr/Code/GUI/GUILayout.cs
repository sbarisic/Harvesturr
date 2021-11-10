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

namespace Harvesturr {
	class GUILayout {
		public List<GUIControl> Controls;

		public int X;
		public int Y;

		public int W;
		public int H;

		public GUILayout() {
			Controls = new List<GUIControl>();
		}

		public void CalcAutoWidth() {
			foreach (GUIControl C in Controls)
				C.CalcAutoWidth();
		}

		public void CalcHorizontalLayout(int Padding) {
			int X = this.X;

			foreach (GUIControl C in Controls) {
				C.Y = Y;
				C.X = X;
				X += C.W + Padding;
			}

			X -= Padding;
			W = X - this.X;
		}
	}
}
