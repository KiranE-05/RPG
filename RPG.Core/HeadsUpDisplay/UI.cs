using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPG.Core.HeadsUpDisplay
{
	public class UI
	{
		private static UI _instance;
		private static int _screenWidth;
		private static int _screenHeight;

		public static UI Instance
		{
			get
			{
				if (_instance == null)
					throw new InvalidOperationException("Minimap must be initialized first using Minimap.Initialize().");
				return _instance;
			}
		}

		public static void Initialize(int screenWidth, int screenHeight)
		{
			_instance = new UI(screenWidth, screenHeight);
		}

		public UI(int screenWidth, int screenHeight)
		{
			_screenWidth = screenWidth;
			_screenHeight = screenHeight;
		}

		public void DrawUI()
		{
			HUD.Instance.Draw(_screenWidth, _screenHeight);
			Minimap.Instance.Draw();
		}
	}
}
