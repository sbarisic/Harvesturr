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
	class GUIPanel : GUIControl {
		NPatchInfo InfoDefault;

		public List<GUIControl> Controls;

		public GUIPanel() : base() {
			Controls = new List<GUIControl>();
			InfoDefault = CreateInfo(0, 0, 64, 20);
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
			if (Disabled)
				return;

			foreach (GUIControl C in Controls)
				C.Update();

			base.Update();
		}

		public override void Draw() {
			NPatchInfo NPatch = InfoDefault;
			Raylib.DrawTextureNPatch(GUI.TexPanel, NPatch, new Rectangle(X, Y, W, H), Vector2.Zero, 0, Color.WHITE);

			foreach (GUIControl C in Controls)
				C.Draw();
			
			base.Draw();
		}

		public override void AutoSize() {
			if (Controls.Count <= 0)
				return;

			int MinX = Controls[0].X;
			int MinY = Controls[0].Y;
			int MaxX = MinX;
			int MaxY = MinY;

			foreach (GUIControl C in Controls) {
				MinX = Math.Min(MinX, C.X);
				MinY = Math.Min(MinY, C.Y);

				MaxX = Math.Max(MaxX, C.X + C.W);
				MaxY = Math.Max(MaxY, C.Y + C.H);
			}

			X = MinX;
			Y = MinY;
			W = MaxX - MinX;
			H = MaxY - MinY;
		}
	}
}