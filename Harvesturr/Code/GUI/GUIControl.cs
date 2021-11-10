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
	public delegate void OnClickFunc();
	public delegate bool CheckToggleFunc();

	class GUIControl {
		public int X;
		public int Y;
		public int W;
		public int H;

		public bool Disabled;
		public bool IsHovered;

		public GUIControl() {
			Disabled = false;
		}

		public virtual bool Update() {
			if (Disabled)
				IsHovered = false;
			else
				IsHovered = Utils.IsInside(new Rectangle(X, Y, W, H), GUI.MousePos);

			return IsHovered;
		}

		public virtual void AddPadding(int Padding) {
			X -= Padding;
			Y -= Padding;
			W += Padding * 2;
			H += Padding * 2;
		}

		public virtual void Draw() {
		}

		public virtual void AutoSize() {
		}

		public virtual void CalcAutoWidth() {
		}
	}
}
