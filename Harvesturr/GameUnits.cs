using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Harvesturr {
	class GameUnit {
		Texture2D UnitTex;
		public Vector2 Position;
		public string Name;

		public bool CanLinkEnergy;
		public float UpdateInterval;
		public float NextUpdateTime;
		public UnitEnergyPacket AwaitingPacket;

		protected Color DrawColor;

		public bool Destroyed {
			get;
			private set;
		}

		public GameUnit(string UnitName, Vector2 Position) {
			this.Position = Position;
			CanLinkEnergy = false;
			UpdateInterval = 1;
			UnitTex = ResMgr.LoadTexture(UnitName);
			Name = UnitName;
			Destroyed = false;
			DrawColor = Color.WHITE;
		}

		public Rectangle GetBoundingRect() {
			return GameEngine.GetBoundingRect(UnitTex, Position);
		}

		public virtual void Destroy() {
			Destroyed = true;
		}

		public virtual void Update(float Dt) {
			float CurTime = (float)Raylib.GetTime();

			if (NextUpdateTime < CurTime) {
				NextUpdateTime = CurTime + UpdateInterval;
				SlowUpdate();
			}
		}

		public virtual void SlowUpdate() {
		}

		public virtual void DrawWorld() {
			GameEngine.DrawTextureCentered(UnitTex, Position, DrawColor);
		}

		public virtual void ConsumeEnergyPacket(UnitEnergyPacket Packet) {
		}

		public virtual bool CanAcceptEnergyPacket() {
			return false;
		}
	}

	class UnitMineral : GameUnit {
		public const string UNIT_NAME_Megamineral = "megamineral";
		public const string UNIT_NAME_Mineral = "mineral";

		//public const int ConnectRangeHarvest = 64;
		public int MineralCount;

		public UnitMineral(Vector2 Position, bool Megamineral = false) : base(Megamineral ? UNIT_NAME_Megamineral : UNIT_NAME_Mineral, Position) {
			if (Megamineral)
				MineralCount = Utils.Random(300, 400);
			else
				MineralCount = Utils.Random(20, 40);
			//MineralCount = 5;
		}

		public bool HarvestMineral() {
			bool Success = false;

			if (MineralCount > 0) {
				MineralCount--;
				Success = true;
			}

			if (MineralCount <= 0)
				Destroy();

			return Success;
		}

		public override string ToString() {
			return "Minerals: " + MineralCount;
		}
	}

	class UnitConduit : GameUnit {
		public const string UNIT_NAME = "conduit";
		public const float ConnectRangePower = 96;

		public UnitConduit(Vector2 Position) : base(UNIT_NAME, Position) {
			CanLinkEnergy = true;
		}

		public GameUnit PickNextEnergyPacketTarget(IEnumerable<GameUnit> Except = null) {
			IEnumerable<GameUnit> UnitsInRange = GameEngine.PickInRange(Position, ConnectRangePower);
			UnitsInRange = UnitsInRange.Except(UnitsInRange.OfType<UnitMineral>());

			IEnumerable<UnitConduit> ConduitsInRange = UnitsInRange.OfType<UnitConduit>();
			UnitsInRange = UnitsInRange.Except(ConduitsInRange);

			foreach (var U in UnitsInRange)
				if (U.CanAcceptEnergyPacket())
					return U;

			if (Except != null)
				ConduitsInRange = ConduitsInRange.Except(Except.Select(U => U as UnitConduit));

			return Utils.Random(ConduitsInRange.ToArray());
		}
	}

	class UnitHarvester : GameUnit {
		public const string UNIT_NAME = "harvester";
		public const float ConnectRangeHarvest = 64;

		int EnergyCharges;

		const float LineLifetime = 0.25f;
		float LineEndTime;
		Vector2 LineEnd;

		public UnitHarvester(Vector2 Position) : base(UNIT_NAME, Position) {
			EnergyCharges = 0;
			UpdateInterval = 5;
			CanLinkEnergy = true;
		}

		public override void Update(float Dt) {
			base.Update(Dt);
			DrawColor = EnergyCharges > 0 ? Color.WHITE : Raylib.Fade(Color.BLACK, 0.5f);
		}

		public override void SlowUpdate() {
			if (EnergyCharges <= 0)
				return;

			UnitMineral[] Minerals = GameEngine.PickInRange(Position, ConnectRangeHarvest).OfType<UnitMineral>().ToArray();
			if (Minerals.Length == 0) {
				Destroy();
				GameEngine.AddResource(2);
				return;
			}

			UnitMineral TgtMineral = Utils.Random(Minerals);
			if (TgtMineral == null || TgtMineral.Destroyed)
				return;

			if (TgtMineral.HarvestMineral()) {
				LineEnd = TgtMineral.Position;
				LineEndTime = (float)Raylib.GetTime() + LineLifetime;
				EnergyCharges--;
				GameEngine.AddResource(1);
			}
		}

		public override void DrawWorld() {
			base.DrawWorld();

			if (Raylib.GetTime() < LineEndTime)
				Raylib.DrawLineEx(Position, LineEnd, 2, Color.GREEN);

			GameEngine.DrawTooltip(Position, EnergyCharges.ToString());
		}

		public override void ConsumeEnergyPacket(UnitEnergyPacket Packet) {
			EnergyCharges += 2;
		}

		public override bool CanAcceptEnergyPacket() {
			if (AwaitingPacket != null && !AwaitingPacket.Destroyed)
				return false;

			return EnergyCharges <= 0;
		}
	}

	class UnitSolarPanel : GameUnit {
		public const string UNIT_NAME = "solarpanel";

		public UnitSolarPanel(Vector2 Position) : base(UNIT_NAME, Position) {
			UpdateInterval = 2;
			CanLinkEnergy = true;
		}

		public override void SlowUpdate() {
			UnitConduit RandomTarget = Utils.Random(GameEngine.PickInRange(Position, UnitConduit.ConnectRangePower).OfType<UnitConduit>().ToArray());

			if (RandomTarget == null)
				return;

			GameEngine.Spawn(new UnitEnergyPacket(Position, RandomTarget));
		}
	}

	class UnitEnergyPacket : GameUnit {
		public const string UNIT_NAME = "energy";

		public GameUnit Previous;
		public GameUnit Target;

		public UnitEnergyPacket(Vector2 Position, GameUnit Target) : base(UNIT_NAME, Position) {
			this.Target = Target;
		}

		public UnitEnergyPacket(UnitConduit Source, GameUnit Target) : this(Source.Position, Target) {
			Previous = Source;
		}

		public override void Update(float Dt) {
			base.Update(Dt);
			float MoveSpeed = 96;

			if (Target.Destroyed) {
				Destroy();
				return;
			}

			Vector2 TargetPos = Target.Position;

			if (Vector2.Distance(Position, TargetPos) < 2) {
				OnTargetReached(Target);
				return;
			}

			Vector2 Dir = Vector2.Normalize(TargetPos - Position);
			Position += Dir * MoveSpeed * Dt;
		}

		void OnTargetReached(GameUnit Target) {
			GameUnit Next = null;

			if (Target is UnitConduit ConduitTarget) {
				Next = ConduitTarget.PickNextEnergyPacketTarget(new GameUnit[] { Target, Previous });

				if (Next == null)
					Next = Previous;

				Previous = Target;
			} else
				Target.ConsumeEnergyPacket(this);

			if (Next == null || Next.Destroyed) {
				Destroy();
				return;
			}

			this.Target = Next;
			Next.AwaitingPacket = this;
		}
	}
}
