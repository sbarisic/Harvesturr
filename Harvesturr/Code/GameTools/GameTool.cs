using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Raylib_cs;
using System.Globalization;

namespace Harvesturr
{
    class GameTool
    {
        public string Name;
        public bool Active;

        protected Texture2DRef ToolGhost;

        protected Vector2 MouseClickPos;
        protected bool InMouseClick;

        public GameTool(string Name)
        {
            this.Name = Name;
            this.Active = false;
        }

        public virtual void OnSelected()
        {
            Console.WriteLine("Selected {0}", Name);
        }

        public virtual void OnWorldMousePress(Vector2 WorldPos, bool Press)
        {
            if (Press)
                OnWorldClick(WorldPos);

            if (Press)
            {
                MouseClickPos = WorldPos;
                InMouseClick = true;
            }
            else if (!Press && InMouseClick)
            {
                InMouseClick = false;

                Vector2 EndPos = GameEngine.MousePosWorld;
                if (MouseClickPos != EndPos)
                    OnMouseDrag(MouseClickPos, EndPos, EndPos - MouseClickPos);
            }
        }

        public virtual void OnMouseDrag(Vector2 WorldStart, Vector2 WorldEnd, Vector2 DragNormal)
        {
        }

        public virtual void OnWorldClick(Vector2 WorldPos)
        {
        }

        public virtual void Update(float Dt)
        {
        }

        public virtual void DrawWorld()
        {
        }
    }
}
