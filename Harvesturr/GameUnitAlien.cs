using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Harvesturr {
	class GameUnitAlien : GameUnit {
		protected float AttackRange; // Scan for enemy range
		protected float AttackDamageRange; // Deal damage range
		protected int AttackDamage;
		protected float AttackInterval;

		protected float NextAttackTime;

		protected bool Rotate;
		protected float Rotation;
		protected float RotationSpeed;

		const float Friction = 0.9f;
		protected Vector2 Velocity;
		protected Vector2 MoveDirection;
		protected float MoveSpeed;

		protected GameUnit AttackTarget;

		bool EnemyInDamageRange;

		public GameUnitAlien(string UnitName, Vector2 Position) : base(UnitName, Position) {
			//AttackRange = 128;
			AttackRange = 4000;

			AttackDamageRange = 16;
			AttackDamage = 5;
			AttackInterval = 1;

			UpdateInterval = 2;
			Rotate = true;
			RotationSpeed = 0;
			MoveSpeed = 0;

			MaxHealth = 50;
			Health = 50;
		}

		public override void Update(float Dt) {
			base.Update(Dt);

			if (Rotate && RotationSpeed != 0)
				Rotation = GameEngine.Time * RotationSpeed;

			// Uncomment to move
			Position += (MoveDirection * MoveSpeed * Dt) + (Velocity * Dt);
			Velocity = Velocity * Friction;

			if (AttackTarget != null && AttackTarget.Destroyed)
				AttackTarget = null;

			if (AttackTarget != null) {
				if (Vector2.Distance(Position, AttackTarget.Position) <= AttackDamageRange) {
					EnemyInDamageRange = true;

					if (NextAttackTime < GameEngine.Time) {
						NextAttackTime = GameEngine.Time + AttackInterval;
						Attack(AttackTarget);
					}
				} else {
					EnemyInDamageRange = false;
				}
			}
		}

		public override void SlowUpdate() {
			base.SlowUpdate();

			if (AttackTarget == null)
				AttackTarget = GameEngine.PickNextAttackTarget(Position, AttackRange, true);

			MoveDirection = CalculateMoveDirection();
		}

		public virtual void Attack(GameUnit Target) {
			// Console.WriteLine("Attacking!");

			GameMusic.PlaySfx(this, GameMusic.Sfx_Hit);
			Target.ReceiveDamage(this, AttackDamage);
		}

		public virtual Vector2 CalculateMoveDirection() {
			if (AttackTarget == null || EnemyInDamageRange)
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
