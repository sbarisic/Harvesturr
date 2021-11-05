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
				return false;

			IsHovered = Utils.IsInside(new Rectangle(X, Y, W, H), GUI.MousePos);
			return IsHovered;
		}

		public virtual void Draw() {
		}

		public virtual void CalcAutoWidth() {
		}
	}

	public delegate void OnClickFunc();
	public delegate bool CheckToggleFunc();


	class GUIButton : GUIControl {
		NPatchInfo InfoDefault;
		NPatchInfo InfoHover;
		NPatchInfo InfoPress;
		NPatchInfo InfoDisabled;
		NPatchInfo InfoDefaultOn;

		public string Text;
		public Font Font;
		public int FontSize;
		public Color FontColor;
		public float FontSpacing;

		public event OnClickFunc OnClick;
		public event CheckToggleFunc OnCheckToggle;

		public GUIButton(string Text, int X, int Y, int W, int H) : base() {
			this.X = X;
			this.Y = Y;
			this.W = W;
			this.H = H;
			this.Text = Text;

			Font = GUI.GUIFont;
			FontSize = Font.baseSize;
			FontColor = Color.WHITE;
			FontSpacing = 1;

			InfoDefault = CreateInfo(0, 0, 64, 20);
			InfoHover = CreateInfo(0, 20, 64, 20);
			InfoPress = CreateInfo(0, 40, 64, 20);
			InfoDisabled = CreateInfo(0, 60, 64, 20);
			InfoDefaultOn = CreateInfo(0, 80, 64, 20);
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
			NPatchInfo NPatch = InfoDefault;

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
			}
		}

		public override void CalcAutoWidth() {
			Vector2 Size = Raylib.MeasureTextEx(Font, Text, FontSize, FontSpacing);
			W = 20 + (int)Size.X;
		}
	}
}
