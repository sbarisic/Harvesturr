﻿using System;
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
	public class GUIButton : GUIControl {
		NPatchInfo InfoDefault;
		NPatchInfo InfoHover;
		NPatchInfo InfoPress;
		NPatchInfo InfoDisabled;
		NPatchInfo InfoDefaultOn;

		public string Text {
			get; set;
		}

		public Font Font {
			get; set;
		}

		public int FontSize {
			get; set;
		}

		public Color FontColor {
			get; set;
		}

		public float FontSpacing {
			get; set;
		}

		public int FontPadding {
			get; set;
		}


		public event OnClickFunc OnClick;
		public event CheckToggleFunc OnCheckToggle;

		public GUIButton(Font Font, string Text) : base() {
			this.Text = Text;

			this.Font = Font;
			FontSize = Font.baseSize;
			FontColor = Color.WHITE;
			FontSpacing = 1;
			FontPadding = 15;

			InfoDefault = CreateInfo(0, 0, 64, 20);
			InfoHover = CreateInfo(0, 20, 64, 20);
			InfoPress = CreateInfo(0, 40, 64, 20);
			InfoDisabled = CreateInfo(0, 60, 64, 20);
			InfoDefaultOn = CreateInfo(0, 80, 64, 20);
		}

		public GUIButton() : this(GUI.GUIFontLarge, null) {
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

			if (CheckClicked())
				OnClick?.Invoke(this);
		}

		public override void Draw() {
			CalculateXYWH(out int X, out int Y, out int W, out int H);
			NPatchInfo NPatch = InfoDefault;

			if (Disabled) {
				NPatch = InfoDisabled;
			} else {
				if (OnCheckToggle != null && (OnCheckToggle?.Invoke(this) ?? false))
					NPatch = InfoDefaultOn;

				if (IsHovered) {
					NPatch = InfoHover;

					if (GUI.MouseLeftDown)
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

			base.Draw();
		}

		/*public override void CalcAutoWidth() {
			Vector2 Size = Raylib.MeasureTextEx(Font, Text, FontSize, FontSpacing);
			W = (int)Size.X + FontPadding * 2;
		}*/
	}
}
