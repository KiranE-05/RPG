﻿using Microsoft.Xna.Framework;
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
		// 3 = chest (5 per level)
		public static int[,] GenerateRandomMap(int width, int height, Random rng = null)
		{
			if (width < 3 || height < 3)
				throw new ArgumentException("Width and height must be at least 3.");

			rng ??= new Random();
			int[,] map = new int[height, width];

			// Fill all cells with walls initially
			for (int y = 0; y < height; y++)
				for (int x = 0; x < width; x++)
					map[y, x] = 1;

			int centerX = width / 2;
			int centerY = height / 2;

			if (centerX % 2 == 0) centerX--;
			if (centerY % 2 == 0) centerY--;

			var stack = new Stack<(int x, int y)>();
			stack.Push((centerX, centerY));
			map[centerY, centerX] = 0;

			int[,] directions = new int[,] { { 0, -2 }, { 2, 0 }, { 0, 2 }, { -2, 0 } };

			while (stack.Count > 0)
			{
				var (x, y) = stack.Pop();
				var dirs = Enumerable.Range(0, 4).OrderBy(_ => rng.Next()).ToList();

				foreach (int i in dirs)
				{
					int dx = directions[i, 0];
					int dy = directions[i, 1];
					int nx = x + dx;
					int ny = y + dy;

					if (nx > 0 && nx < width - 1 && ny > 0 && ny < height - 1 && map[ny, nx] == 1)
					{
						map[ny, nx] = 0;
						map[y + dy / 2, x + dx / 2] = 0;
						stack.Push((nx, ny));
					}
				}
			}

			// Pick an exit point
			var possibleExits = new List<(int x, int y)>();
			for (int x = 1; x < width - 1; x++)
			{
				if (map[1, x] == 0) possibleExits.Add((x, 0));
				if (map[height - 2, x] == 0) possibleExits.Add((x, height - 1));
			}
			for (int y = 1; y < height - 1; y++)
			{
				if (map[y, 1] == 0) possibleExits.Add((0, y));
				if (map[y, width - 2] == 0) possibleExits.Add((width - 1, y));
			}

			if (possibleExits.Count > 0)
			{
				var (ex, ey) = possibleExits[rng.Next(possibleExits.Count)];
				map[ey, ex] = 2; // exit
			}

			// Ensure player start location is empty
			map[centerY, centerX] = 0;

			// === DEAD-END DETECTION FOR CHESTS ===
			List<(int x, int y)> deadEnds = [];
			for (int y = 1; y < height - 1; y++)
			{
				for (int x = 1; x < width - 1; x++)
				{
					if (map[y, x] != 0) continue;

					int neighborCount = 0;
					if (map[y - 1, x] == 0) neighborCount++;
					if (map[y + 1, x] == 0) neighborCount++;
					if (map[y, x - 1] == 0) neighborCount++;
					if (map[y, x + 1] == 0) neighborCount++;

					if (neighborCount == 1)
						deadEnds.Add((x, y));
				}
			}

			// Shuffle and pick up to 5 dead ends
			foreach (var (x, y) in deadEnds.OrderBy(_ => rng.Next()).Take(5))
				map[y, x] = 3; // Place treasure chest

			return map;
		}



		public static Vector2 FindSafeStartPosition()
		{
			var map = MapManager.Instance.GetMap();
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
