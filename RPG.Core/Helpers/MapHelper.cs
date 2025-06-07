using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPG.Core.Helpers
{
	public static class MapHelper
	{
		// Generate a randomized map:
		// 0 = empty space
		// 1 = wall
		// 2 = exit (exactly one)
		public static int[,] GenerateRandomMap(int width, int height, Random rng = null)
		{
			if (width < 3 || height < 3)
				throw new ArgumentException("Width and height must be at least 3 to have an exit.");

			rng ??= new Random();

			int[,] map = new int[height, width];

			// Fill outer edges with walls
			for (int x = 0; x < width; x++)
			{
				map[0, x] = 1;
				map[height - 1, x] = 1;
			}
			for (int y = 0; y < height; y++)
			{
				map[y, 0] = 1;
				map[y, width - 1] = 1;
			}

			// Fill inner cells randomly with walls or empty space
			for (int y = 1; y < height - 1; y++)
			{
				for (int x = 1; x < width - 1; x++)
				{
					// Randomly wall (30%) or empty (70%)
					map[y, x] = (rng.NextDouble() < 0.3) ? 1 : 0;
				}
			}

			// Find all wall positions inside map to pick exit from
			var wallPositions = new System.Collections.Generic.List<(int x, int y)>();
			for (int y = 1; y < height - 1; y++)
			{
				for (int x = 1; x < width - 1; x++)
				{
					if (map[y, x] == 1)
						wallPositions.Add((x, y));
				}
			}

			if (wallPositions.Count == 0)
			{
				// No walls inside, just create an exit at center
				int centerX = width / 2;
				int centerY = height / 2;
				map[centerY, centerX] = 2;
			}
			else
			{
				// Pick random wall to become exit
				var exitPos = wallPositions[rng.Next(wallPositions.Count)];
				map[exitPos.y, exitPos.x] = 2;
			}

			return map;
		}

		public static Vector2 FindSafeStartPosition(int[,] map)
		{
			int width = map.GetLength(1);
			int height = map.GetLength(0);

			int centerX = width / 2;
			int centerY = height / 2;

			// If center is free, return center + 0.5f for center of cell
			if (map[centerY, centerX] == 0)
				return new Vector2(centerX + 0.5f, centerY + 0.5f);

			// Otherwise, search in a spiral around the center for an empty cell
			int maxRadius = Math.Max(width, height);
			for (int radius = 1; radius < maxRadius; radius++)
			{
				for (int dy = -radius; dy <= radius; dy++)
				{
					for (int dx = -radius; dx <= radius; dx++)
					{
						int nx = centerX + dx;
						int ny = centerY + dy;

						if (nx >= 0 && nx < width && ny >= 0 && ny < height)
						{
							if (map[ny, nx] == 0)
								return new Vector2(nx + 0.5f, ny + 0.5f);
						}
					}
				}
			}

			// Fallback: if no empty cell found, return (1.5, 1.5) or some default safe spot
			return new Vector2(1.5f, 1.5f);
		}
	}
}
