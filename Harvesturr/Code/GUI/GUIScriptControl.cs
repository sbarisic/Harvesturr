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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Reflection;

namespace Harvesturr {
	public class GUIScriptControl : GUIControl {
		Script<object> FMLScript;
		ScriptState<object> State;

		public GUIScriptControl() : base() {
			FlexNode = null;
			Controls = null;
		}

		public override void Draw() {
		}

		public override void Update() {
		}

		public override void CalculateXYWH(out int X, out int Y, out int W, out int H) {
			X = Y = W = H = 0;
		}

		public override void SetAttribute(string Name, object Value, GUIState State) {
			if (Name == "CSharp") {
				string SourceCode = (string)Value;
				Compile(SourceCode, State.GetType());
				return;
			}

			base.SetAttribute(Name, Value, State);
		}

		public void Compile(string SrcCode, Type GlobalsType) {
			ScriptOptions Options = ScriptOptions.Default;
			Options = Options.AddReferences(Assembly.GetExecutingAssembly());
			Options = Options.AddImports("System", nameof(Harvesturr));

			FMLScript = CSharpScript.Create<object>(SrcCode, Options, GlobalsType);
		}

		public void Run(object Globals) {
			State = FMLScript.RunAsync(Globals).Result;

			if (State.Exception != null)
				throw State.Exception;
		}

		public T GetReturnValue<T>() {
			return (T)State.ReturnValue;
		}

		public object GetReturnValue() {
			return State.ReturnValue;
		}
	}
}