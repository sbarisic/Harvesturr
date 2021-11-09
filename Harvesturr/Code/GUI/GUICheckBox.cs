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
	class GUICheckBox : GUIControl {
		NPatchInfo InfoDefault;
		NPatchInfo InfoHover;
		NPatchInfo InfoPress;
		NPatchInfo InfoDisabled;

		NPatchInfo InfoDefaultChecked;
		NPatchInfo InfoHoverChecked;
		NPatchInfo InfoPressChecked;
		NPatchInfo InfoDisabledChecked;

		public event OnClickFunc OnClick;

		public GUICheckBox(int X, int Y, int W, int H) : base() {
			this.X = X;
			this.Y = Y;
			this.W = W;
			this.H = H;			
		}

		NPatchInfo CreateInfo(int X, int Y, int W, int H) {
			NPatchInfo I = new NPatchInfo();
			I.sourceRec = new Rectangle(X, Y, W, H);
			I.layout = NPatchLayout.NPATCH_NINE_PATCH;
			I.top = 4;
			I.bottom = 4;
			I.left = 4;
			I.right = 4;
			return I;
		}

		public override bool Update() {
			if (base.Update()) {
				if (IsHovered && GUI.MouseLeftReleased)
					OnClick?.Invoke();

				return IsHovered;
			}

			return false;
		}

		public override void Draw() {
			/*NPatchInfo NPatch = InfoDefault;

			if (Disabled) {
				NPatch = InfoDisabled;
			} else {
				if (OnCheckToggle != null && (OnCheckToggle?.Invoke() ?? false))
					NPatch = InfoDefaultOn;

				if (IsHovered) {
					NPatch = InfoHover;

					if (GUI.MouseLeft)
						NPatch = InfoPress;
				}
			}

			Raylib.DrawTextureNPatch(GUI.TexButton, NPatch, new Rectangle(X, Y, W, H), Vector2.Zero, 0, Color.WHITE);

			if (Text != null) {
				Vector2 Pos = new Vector2(X, Y) + new Vector2(W, H) / 2;
				Vector2 Size = Raylib.MeasureTextEx(Font, Text, FontSize, FontSpacing);
				Pos = Pos - Size / 2;

				Raylib.DrawTextEx(Font, Text, Pos, FontSize, FontSpacing, FontColor);
			}*/
		}

		public override void CalcAutoWidth() {
			//Vector2 Size = Raylib.MeasureTextEx(Font, Text, FontSize, FontSpacing);
			//W = 20 + (int)Size.X;
		}
	}
}
