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
	class GUIState {
		public static int GUIButtonHeight = 50;
		public static int GUIButtonWidth = 400;

		public static int GUIPadding = 10;
		public static int GUIRectHeight = GUIButtonHeight + GUIPadding * 2;



		public List<GUIControl> Controls = new List<GUIControl>();

		public int X;
		public int Y;

		public GUIState() {
			Controls = new List<GUIControl>();
		}

		public virtual void Init() {		
		}

		public virtual void RecalculatePositions() {		
		}

		public virtual void UpdateInput(float Dt) {
		}

		public virtual void Update(float Dt) {
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
