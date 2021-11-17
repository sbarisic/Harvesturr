using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harvesturr {
	public class EffectPainter {
		public bool ScreenSpace;
		public float EndTime;
		public Action Action;

		public EffectPainter(Action Action, float Length = 1, bool ScreenSpace = false) {
			this.Action = Action;
			this.ScreenSpace = ScreenSpace;
			EndTime = GameEngine.Time + Length;
		}
	}
}
