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

		public bool IsHovered {
			get {
				return Utils.IsInside(new Rectangle(X, Y, W, H), GUI.MousePos);
			}
		}

		public GUIControl() {
			Disabled = false;
		}

		bool PressedInside = false;
		public virtual bool CheckClicked() {
			if (Disabled) {
				PressedInside = false;
				return false;
			}

			if (!IsHovered) 
				PressedInside = false;

			if (PressedInside && IsHovered && GUI.MouseLeftReleased) {
				PressedInside = false;
				return true;
			}

			if (IsHovered && GUI.MouseLeftPressed)
				PressedInside = true;

			return false;
		}

		public virtual void Update() {
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
