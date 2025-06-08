using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPG.Core.Hero
{
	public class PlayerStats
	{
		public int Level { get; set; }
		public int CurrentHP { get; set; }
		public int MaxHP { get; set; }
		public int CurrentMana { get; set; }
		public int MaxMana { get; set; }
		public int CurrentEndurance { get; set; }
		public int MaxEndurance { get; set; }
		public int CurrentXP { get; set; }
		public int XPToLevelUp { get; set; }
	}

}
