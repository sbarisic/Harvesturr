using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Harvesturr {
	class GameUnitAlien : GameUnit {
		protected float AttackRange;
		protected int AttackDamage;

		protected bool Rotate;
		protected float Rotation;
		protected float RotationSpeed;

		const float Friction = 0.9f;
		protected Vector2 Velocity;
		protected Vector2 MoveDirection;
		protected float MoveSpeed;

		protected GameUnit AttackTarget;

		public GameUnitAlien(string UnitName, Vector2 Position) : base(UnitName, Position) {
			AttackRange = 128;
			AttackDamage = 1;

			UpdateInterval = 2;
			Rotate = true;
			RotationSpeed = 0;
			MoveSpeed = 0;
		}

		public override void Update(float Dt) {
			base.Update(Dt);

			if (Rotate && RotationSpeed != 0)
				Rotation = GameEngine.Time * RotationSpeed;

			Position += (MoveDirection * MoveSpeed * Dt) + (Velocity * Dt);
			Velocity = Velocity * Friction;
		}

		public override void SlowUpdate() {
			base.SlowUpdate();

			if (AttackTarget == null)
				AttackTarget = GameEngine.PickNextAttackTarget(Position, AttackRange);

			MoveDirection = CalculateMoveDirection();
		}

		public virtual Vector2 CalculateMoveDirection() {
			if (AttackTarget == null)
				return Vector2.Zero;

			return Vector2.Normalize(AttackTarget.Position - Position);
		}

		public override void DrawWorld() {
			GameEngine.DrawTextureCentered(UnitTex, Position, Rotation: Rotation, Clr: DrawColor);
		}

		public override void DrawGUI() {
			base.DrawGUI();

			if (GameEngine.DrawZoomDetails && GameEngine.DebugView)
				GameEngine.DrawCircle(Position, AttackRange, Color.PINK);
		}

		public void ApplyForce(Vector2 Vec) {
			Velocity += Vec;
		}
	}

	class UnitAlienUfo : GameUnitAlien {
		public const string UNIT_NAME = "ufo";

		public UnitAlienUfo(Vector2 Position) : base(UNIT_NAME, Position) {
			RotationSpeed = 48;
			MoveSpeed = 8;
		}
	}
}
