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
using Flexbox;
using System.Reflection;

namespace Harvesturr {
	public delegate void OnClickFunc(GUIControl Ctrl);
	public delegate bool CheckToggleFunc(GUIControl Ctrl);

	public class GUIControl {
		public const int SIZE_AUTO = -1;

		List<GUIControl> Controls;
		public Node FlexNode;

		public bool Disabled {
			get; set;
		}

		public bool IsHovered {
			get {
				CalculateXYWH(out int X, out int Y, out int W, out int H);
				return Utils.IsInside(new Rectangle(X, Y, W, H), GUI.MousePos);
			}
		}

		public virtual void SetAttribute(string Name, object Value, GUIState State) {
			if (Name == "Style") {
				ApplyStyle((string)Value);
				return;
			}

			Type T = GetType();
			Type StateT = State.GetType();

			PropertyInfo Prop = T.GetProperty(Name);
			if (Prop != null) {
				Prop.SetValue(this, Value);
				return;
			}

			EventInfo Event = T.GetEvent(Name);
			if (Event != null) {
				MethodInfo MI = StateT.GetMethod((string)Value, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
				Delegate EventDelegate = Delegate.CreateDelegate(Event.EventHandlerType, State, MI);
				Event.AddEventHandler(this, EventDelegate);
				return;
			}

			throw new NotImplementedException();
		}

		public GUIControl() {
			Disabled = false;
			Controls = new List<GUIControl>();
			FlexNode = Flex.CreateDefaultNode();
		}

		public virtual void ApplyStyle(string Style) {
			FlexNode.nodeStyle.Apply(Style);
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

		public virtual void AddControl(GUIControl Ctrl) {
			FlexNode.AddChild(Ctrl.FlexNode);
			Controls.Add(Ctrl);
		}

		public virtual void Update() {
			if (Disabled)
				return;

			foreach (GUIControl C in Controls)
				C.Update();
		}

		public virtual void CalculateXYWH(out int X, out int Y, out int W, out int H) {
			X = (int)FlexNode.LayoutGetX();
			Y = (int)FlexNode.LayoutGetY();
			W = (int)FlexNode.LayoutGetWidth();
			H = (int)FlexNode.LayoutGetHeight();
		}

		/*public virtual void AddPadding(int Padding) {
			X -= Padding;
			Y -= Padding;
			W += Padding * 2;
			H += Padding * 2;
		}*/

		public virtual void Draw() {
			CalculateXYWH(out int X, out int Y, out int W, out int H);
			//Raylib.DrawRectangleLines(X, Y, W, H, Color.RED);

			foreach (GUIControl C in Controls)
				C.Draw();
		}

		/*public virtual void AutoSize() {
		}

		public virtual void CalcAutoWidth() {
		}*/
	}
}
