using Raycaster;
using RPG.Core.Helpers;
using System;

namespace RPG.Core
{
	public class MapManager
	{
		private static MapManager instance;

		private int[,] map;
		private int width;
		private int height;

		public static MapManager Instance
		{
			get
			{
				if (instance == null)
					instance = new MapManager();
				return instance;
			}
		}

		private MapManager()
		{
			// Optional: set a default map size on initialization
			width = 20;
			height = 20;
			map = new int[height, width];
		}

		public void SetMap(int[,] newMap)
		{
			map = newMap;
			height = newMap.GetLength(0);
			width = newMap.GetLength(1);
		}

		public int[,] GetMap() => map;

		public void Regenerate(int width = 25, int height = 25)
		{
			// You can plug in your maze generation function here
			var generatedMap = MapHelper.GenerateRandomMap(width, height);
			SetMap(generatedMap);
			Minimap.Instance.ClearMap();
		}
	}
}
