using Raylib_cs;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Harvesturr
{
	class UnitAlienUfo : GameUnitAlien
	{
		public const string UNIT_NAME = "ufo";

		public UnitAlienUfo(Vector2 Position) : base(UNIT_NAME, Position)
		{
			RotationSpeed = 48;
			MoveSpeed = 8;
		}
	}
}
