using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harvesturr {
	public struct RaycastResult {
		public IntersectionResult Intersection;
		public GameUnit Unit;

		public RaycastResult(IntersectionResult Intersection, GameUnit Unit) {
			this.Intersection = Intersection;
			this.Unit = Unit;
		}
	}
}
