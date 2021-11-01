using Raylib_cs;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Harvesturr
{
    class GameUnit
    {
        public Vector2 Position;
        public string Name;
        public bool Pickable;
        public bool LightningOnDestroy;

        public bool IsMouseHover;

        public int MaxHealth;
        public int Health;
        public bool CanLinkEnergy;
        public float UpdateInterval;
        public float NextUpdateTime;
        public UnitEnergyPacket AwaitingPacket;

        protected Texture2DRef UnitTex;
        protected Color DrawColor;

        protected SoundRef Sfx_OnDestroy;

        public bool Destroyed
        {
            get;
            private set;
        }

        public GameUnit(string UnitName, Vector2 Position)
        {
            this.Position = Position;
            CanLinkEnergy = false;
            UpdateInterval = 1;
            UnitTex = ResMgr.LoadTexture(UnitName);
            Name = UnitName;
            Destroyed = false;
            DrawColor = Color.WHITE;
            Pickable = true;
            MaxHealth = 100;
            Health = MaxHealth;
            LightningOnDestroy = false;
        }

        public Rectangle GetBoundingRect()
        {
            return GameEngine.GetBoundingRect(UnitTex, Position);
        }

        public Vector2 GetUnitWidth()
        {
            return new Vector2(UnitTex.width, 0);
        }

        public Vector2 GetUnitHeight()
        {
            return new Vector2(0, UnitTex.height);
        }

        public Vector2 GetUnitSize()
        {
            return GetUnitWidth() + GetUnitHeight();
        }

        public virtual void Destroy(bool SuppressSfx = false)
        {
            if (Destroyed)
                return;

            if (!SuppressSfx)
                GameMusic.PlaySfx(this, Sfx_OnDestroy);

            Destroyed = true;

            if (LightningOnDestroy)
                GameEngine.AddLightningEffect(Position, Color.SKYBLUE);
        }

        public virtual void Update(float Dt)
        {
            if (Health <= 0)
            {
                Destroy();
                return;
            }

            if (NextUpdateTime < GameEngine.Time)
            {
                NextUpdateTime = GameEngine.Time + UpdateInterval;
                SlowUpdate();
            }
        }

        public virtual void ReceiveDamage(GameUnit AttackingUnit, int Damage)
        {
            Health -= Damage;
        }

        public virtual void SlowUpdate()
        {
        }

        public virtual void DrawWorld()
        {
            GameEngine.DrawTextureCentered(UnitTex, Position, Clr: DrawColor);
        }

        public virtual void DrawGUI()
        {
            if (GameEngine.DrawZoomDetails && Health < MaxHealth)
                GUI.DrawBar(Position - GetUnitHeight(), Health / (float)MaxHealth, Color.GREEN);
        }

        // Return true to find next target from current position, return false to destroy
        public virtual bool ConsumeEnergyPacket(UnitEnergyPacket Packet)
        {
            return true;
        }

        public virtual bool CanAcceptEnergyPacket()
        {
            return false;
        }
    }

}
