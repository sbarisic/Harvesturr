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
		public bool Pickable;

		public int Health;
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
			Pickable = true;
			Health = 100;
		}

		public Rectangle GetBoundingRect() {
			return GameEngine.GetBoundingRect(UnitTex, Position);
		}

		public Vector2 GetUnitWidth() {
			return new Vector2(UnitTex.width, 0);
		}

		public Vector2 GetUnitHeight() {
			return new Vector2(0, UnitTex.height);
		}

		public Vector2 GetUnitSize() {
			return GetUnitWidth() + GetUnitHeight();
		}

		public virtual void Destroy() {
			Destroyed = true;
			GameEngine.AddLightningEffect(Position, Color.SKYBLUE);
		}

		public virtual void Update(float Dt) {
			if (Health <= 0) {
				Destroy();
				return;
			}

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

		public virtual void DrawGUI() {
			if (GameEngine.DrawZoomDetails && Health < 100)
				GameEngine.DrawBar(Position - GetUnitHeight(), Health / 100.0f, Color.GREEN);
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

		public int Heat;

		public UnitConduit LinkedConduit;

		public UnitConduit(Vector2 Position) : base(UNIT_NAME, Position) {
			UpdateInterval = 0.2f;
			CanLinkEnergy = true;
			Heat = 0;
		}

		public override void SlowUpdate() {
			Heat -= 2;

			if (Heat < 0)
				Heat = 0;
		}

		public override void DrawWorld() {
			DrawColor = Raylib.ColorFromNormalized(Vector4.Lerp(Vector4.One, new Vector4(1, 0.35f, 0.2f, 1), Heat / 100.0f));
			base.DrawWorld();
		}

		public override void DrawGUI() {
			base.DrawGUI();

			if (GameEngine.DrawZoomDetails) {
				if (LinkedConduit != null)
					GameEngine.DrawDashedLine(Position, LinkedConduit.Position, 1, 6, Color.YELLOW);

				GameEngine.DrawBar(Position - GetUnitHeight() / 2, Heat / 100.0f, DrawColor);
			}
		}

		public override void ConsumeEnergyPacket(UnitEnergyPacket Packet) {
			Heat++;

			if (Heat > 100) {
				Heat = 100;
				Packet.Destroy();
			}
		}

		public GameUnit PickNextEnergyPacketTarget(params GameUnit[] Except) {
			if (LinkedConduit != null) {
				if (LinkedConduit.Destroyed)
					LinkedConduit = null;
				else
					return LinkedConduit;
			}

			GameUnit[] UnitsInRange = GameEngine.PickInRange(Position, ConnectRangePower).ToArray();

			if (UnitsInRange.Length == 0)
				return null;

			for (int i = 0; i < UnitsInRange.Length; i++) {
				bool DoContinue = false;

				for (int j = 0; j < Except.Length; j++)
					if (UnitsInRange[i] == Except[j]) {
						UnitsInRange[i] = null;
						DoContinue = true;
						break;
					}

				if (DoContinue)
					continue;

				if (UnitsInRange[i] is UnitMineral) {
					UnitsInRange[i] = null;
					continue;
				}

				if (UnitsInRange[i] is UnitConduit)
					continue;

				if (!UnitsInRange[i].CanAcceptEnergyPacket()) {
					UnitsInRange[i] = null;
					continue;
				} else
					return UnitsInRange[i];
			}

			int MaxLen = Utils.Rearrange(UnitsInRange);
			if (MaxLen <= 0)
				return null;

			return UnitsInRange[Utils.Random(0, MaxLen)];
		}

		public override string ToString() {
			return string.Format("Heat: {0}", Heat);
		}
	}

	class UnitHarvester : GameUnit {
		public const string UNIT_NAME = "harvester";
		public const float ConnectRangeHarvest = 64;

		int EnergyCharges;
		float LastEnergyChargeUseTime;

		public UnitHarvester(Vector2 Position) : base(UNIT_NAME, Position) {
			EnergyCharges = 0;
			UpdateInterval = 5;
			CanLinkEnergy = true;
		}

		public override void Update(float Dt) {
			base.Update(Dt);
			DrawColor = Color.WHITE;

			if (EnergyCharges <= 0 && (((float)Raylib.GetTime() - LastEnergyChargeUseTime) > UpdateInterval))
				DrawColor = new Color(128, 128, 128, 190);
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
				GameEngine.AddEffect(() => Raylib.DrawLineEx(Position, TgtMineral.Position, 2, Color.GREEN), 0.25f);

				EnergyCharges--;

				if (EnergyCharges <= 0) {
					EnergyCharges = 0;
					LastEnergyChargeUseTime = (float)Raylib.GetTime();
				}

				GameEngine.AddResource(1);
			}
		}

		public override void ConsumeEnergyPacket(UnitEnergyPacket Packet) {
			EnergyCharges++;
			Packet.Destroy();
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
			//UpdateInterval = 0.5f;
			//UpdateInterval = 0.01f;

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
			Pickable = false;
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
			this.Target = null;
			GameUnit Next = null;
			Target.ConsumeEnergyPacket(this);

			if (Destroyed)
				return;

			if (Target is UnitConduit ConduitTarget) {
				Next = ConduitTarget.PickNextEnergyPacketTarget(Target, Previous);

				if (Next == null)
					Next = Previous;

				Previous = Target;
			}

			if (Next != null && Next.Destroyed)
				Next = null;

			if (Next == null) {
				if (this.Target == null)
					Destroy();

				return;
			}

			this.Target = Next;
			this.Target.AwaitingPacket = this;
		}
	}
}
