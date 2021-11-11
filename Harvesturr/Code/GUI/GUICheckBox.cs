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

		public bool Checked;

		public string Text;
		public Font Font;
		public int FontSize;
		public Color FontColor;
		public float FontSpacing;
		public int FontPadding;

		public GUICheckBox(Font Font, string Text, int X, int Y, int W, int H) : base() {
			this.X = X;
			this.Y = Y;
			this.W = W;
			this.H = H;

			this.Text = Text;
			this.Font = Font;
			FontSize = Font.baseSize;
			FontColor = Color.WHITE;
			FontSpacing = 1;
			FontPadding = 15;

			InfoDefault = CreateInfo(20, 0, 20, 20);
			InfoHover = CreateInfo(20, 20, 20, 20);
			InfoPress = CreateInfo(20, 40, 20, 20);
			InfoDisabled = CreateInfo(20, 60, 20, 20);

			InfoDefaultChecked = CreateInfo(0, 0, 20, 20);
			InfoHoverChecked = CreateInfo(0, 20, 20, 20);
			InfoPressChecked = CreateInfo(0, 40, 20, 20);
			InfoDisabledChecked = CreateInfo(0, 60, 20, 20);
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

		public override void Update() {
			base.Update();

			if (Disabled)
				return;

			if (CheckClicked()) {
				Checked = !Checked;
				OnClick?.Invoke();
			}
		}

		public override void Draw() {
			NPatchInfo NPatch = InfoDefault;

			if (Disabled) {
				if (Checked)
					NPatch = InfoDisabledChecked;
				else
					NPatch = InfoDisabled;
			} else {
				if (Checked)
					NPatch = InfoDefaultChecked;

				if (IsHovered) {
					if (Checked)
						NPatch = InfoHoverChecked;
					else
						NPatch = InfoHover;

					if (GUI.MouseLeftDown) {
						if (Checked)
							NPatch = InfoPressChecked;
						else
							NPatch = InfoPress;
					}
				}
			}

			int WH = Math.Min(GUI.TexCheckbox.width, GUI.TexCheckbox.height);
			Raylib.DrawTextureNPatch(GUI.TexCheckbox, NPatch, new Rectangle(X + W - WH, Y + (H / 2) - WH / 2, WH, WH), Vector2.Zero, 0, Color.WHITE);

			if (Text != null) {
				Vector2 Pos = new Vector2(X, Y) + new Vector2(W - WH, H) / 2;
				Vector2 Size = Raylib.MeasureTextEx(Font, Text, FontSize, FontSpacing);
				Pos = Pos - Size / 2;

				Raylib.DrawTextEx(Font, Text, Pos, FontSize, FontSpacing, FontColor);
			}

			base.Draw();
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
