using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Raylib_cs;
using System.Globalization;

namespace Harvesturr {
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	sealed class IsGameToolAttribute : Attribute {
		public IsGameToolAttribute() {
		}

		public static IEnumerable<GameTool> CreateAllGameTools() {
			Type[] AllTypes = Assembly.GetExecutingAssembly().GetTypes();

			foreach (var T in AllTypes) {
				bool HasAttribute = T.GetCustomAttribute<IsGameToolAttribute>() != null;

				if (HasAttribute) {
					GameTool ToolInstance = Activator.CreateInstance(T) as GameTool;
					yield return ToolInstance;
				}
			}
		}
	}

	class GameTool {
		public string Name;
		public bool Active;

		protected Texture2D? ToolGhost;

		public GameTool(string Name) {
			this.Name = Name;
			this.Active = false;
		}

		public virtual void OnSelected() {
			Console.WriteLine("Selected {0}", Name);
		}

		public virtual void OnWorldClick(Vector2 WorldPos) {
		}

		public virtual void Update(float Dt) {
		}

		public virtual void DrawWorld() {
		}
	}

	class GameToolBuilder : GameTool {
		protected int BuildCost;
		protected bool CurrentLocationValid;

		public GameToolBuilder(string Name, int BuildCost) : base(Name) {
			this.BuildCost = BuildCost;
		}

		public override void Update(float Dt) {
			CurrentLocationValid = IsValidLocation(GameEngine.MousePosWorld);
		}

		public override void DrawWorld() {
			if (ToolGhost == null)
				return;

			Color GhostColor = CurrentLocationValid ? Color.GREEN : Color.RED;
			GameEngine.DrawTextureCentered(ToolGhost.Value, GameEngine.MousePosWorld, Raylib.Fade(GhostColor, 0.5f));

			if (CurrentLocationValid)
				GameEngine.DrawTooltip(GameEngine.MousePosWorld, "R$ " + BuildCost, Color.GREEN);
		}

		public virtual void DrawLinkLines(float Radius, Color Clr, Func<IEnumerable<GameUnit>, IEnumerable<GameUnit>> Filter) {
			Vector2 Pos = GameEngine.MousePosWorld;
			Raylib.DrawCircleLines((int)Pos.X, (int)Pos.Y, Radius, Clr);

			foreach (var C in Filter(GameEngine.PickInRange(Pos, Radius)))
				Raylib.DrawLineV(Pos, C.Position, Clr);
		}

		public virtual bool IsValidLocation(Vector2 MousePosWorld) {
			if (ToolGhost == null)
				return true;

			Rectangle CollisionRect = GameEngine.GetBoundingRect(ToolGhost.Value, MousePosWorld);
			GameUnit[] Picks = GameEngine.Pick(CollisionRect).ToArray();

			if (Picks.Length != 0)
				return false;

			return true;
		}

		public override void OnWorldClick(Vector2 WorldPos) {
			if (CurrentLocationValid) {
				if (GameEngine.TryConsumeResources(BuildCost)) {
					OnSpawnSuccess(WorldPos);
				}
			}
		}

		public virtual void OnSpawnSuccess(Vector2 WorldPos) {

		}
	}

	[IsGameTool]
	class GameToolPicker : GameTool {
		public GameToolPicker() : base("Picker") {
		}

		public override void OnWorldClick(Vector2 WorldPos) {
			Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "new Vector2({0:F3}f, {1:F3}f)", WorldPos.X, WorldPos.Y));
		}

		public override void DrawWorld() {
			GameUnit PickedUnit = GameEngine.Pick(GameEngine.MousePosWorld).FirstOrDefault();

			if (PickedUnit == null)
				return;

			GameEngine.DrawTooltip(GameEngine.MousePosWorld, PickedUnit.ToString());
		}
	}

	[IsGameTool]
	class GameToolConduit : GameToolBuilder {
		public GameToolConduit() : base("Conduit", 5) {
			ToolGhost = ResMgr.LoadTexture(UnitConduit.UNIT_NAME);
		}

		public override void DrawWorld() {
			base.DrawWorld();
			DrawLinkLines(UnitConduit.ConnectRangePower, Color.YELLOW, F => F.Where(U => U.CanLinkEnergy));
		}

		public override void OnSpawnSuccess(Vector2 WorldPos) {
			GameEngine.Spawn(new UnitConduit(WorldPos));
		}
	}

	[IsGameTool]
	class GameToolHarvester : GameToolBuilder {
		const float ConnectRangeHarvest = 64;

		public GameToolHarvester() : base("Harvester", 8) {
			ToolGhost = ResMgr.LoadTexture(UnitHarvester.UNIT_NAME);
		}

		public override void DrawWorld() {
			/*Vector2 Pos = GameEngine.MousePosWorld;
			GameEngine.DrawCircle(Pos, UnitConduit.ConnectRangePower, Color.YELLOW);
			GameEngine.DrawCircle(Pos, ConnectRangeHarvest, Color.GREEN);

			UnitConduit[] Conduits = GameEngine.PickInRange(Pos, UnitConduit.ConnectRangePower).OfType<UnitConduit>().ToArray();
			UnitMineral[] Minerals = GameEngine.PickInRange(Pos, ConnectRangeHarvest).OfType<UnitMineral>().ToArray();

			foreach (var C in Conduits)
				Raylib.DrawLineV(Pos, C.Position, Color.YELLOW);

			foreach (var M in Minerals)
				Raylib.DrawLineV(Pos, M.Position, Color.GREEN);*/

			DrawLinkLines(UnitConduit.ConnectRangePower, Color.YELLOW, Enumerable.OfType<UnitConduit>);
			DrawLinkLines(UnitHarvester.ConnectRangeHarvest, Color.GREEN, Enumerable.OfType<UnitMineral>);
			base.DrawWorld();
		}

		public override void OnSpawnSuccess(Vector2 WorldPos) {
			GameEngine.Spawn(new UnitHarvester(WorldPos));
		}
	}

	[IsGameTool]
	class GameToolSolarPanel : GameToolBuilder {
		public GameToolSolarPanel() : base("Solar Panel", 10) {
			ToolGhost = ResMgr.LoadTexture(UnitSolarPanel.UNIT_NAME);
		}

		public override void DrawWorld() {
			DrawLinkLines(UnitConduit.ConnectRangePower, Color.YELLOW, Enumerable.OfType<UnitConduit>);
			base.DrawWorld();
		}

		public override void OnSpawnSuccess(Vector2 WorldPos) {
			GameEngine.Spawn(new UnitSolarPanel(WorldPos));
		}
	}
}
