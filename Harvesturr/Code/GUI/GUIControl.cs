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

	enum GUIControlLayout {
		Absolute,
		ParentRelative
	}

	class GUIControl {
		public const int SIZE_AUTO = -1;

		public int Left = 0;
		public int Top = 0;
		public int Bottom = 0;
		public int Right = 0;

		public int Width = 0;
		public int Height = 0;

		public GUIControlLayout Layout = GUIControlLayout.Absolute;

		public bool Disabled;

		public bool IsHovered {
			get {
				CalculateXYWH(out int X, out int Y, out int W, out int H);
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

		public void SetPadding(int Padding) {
			Top = Left = Bottom = Right = Padding;
		}

		public virtual void CalculateXYWH(out int X, out int Y, out int W, out int H) {
			GUIControl Parent = null;
			X = Y = W = H = -2;

			if (Layout == GUIControlLayout.Absolute) {
				X = Left;
				Y = Top;

				W = Width;
				H = Height;
			} else if (Layout == GUIControlLayout.ParentRelative) {
				Parent.CalculateXYWH(out int PX, out int PY, out int PW, out int PH);

				X = PX + Left;
				Y = PY + Top;

				W = PW - Left - Right;
				H = PH - Right - Bottom;
			}
		}

		/*public virtual void AddPadding(int Padding) {
			X -= Padding;
			Y -= Padding;
			W += Padding * 2;
			H += Padding * 2;
		}*/

		public virtual void Draw() {
			CalculateXYWH(out int X, out int Y, out int W, out int H);

			Raylib.DrawRectangleLines(X, Y, W, H, Color.RED);
		}

		/*public virtual void AutoSize() {
		}

		public virtual void CalcAutoWidth() {
		}*/
	}
}
