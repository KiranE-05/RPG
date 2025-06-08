using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RPG.Core;
using System;

namespace Raycaster
{
	public class Minimap
	{
		private static Minimap instance;
		public static Minimap Instance
		{
			get
			{
				if (instance == null)
					throw new InvalidOperationException("Minimap must be initialized first using Minimap.Initialize().");
				return instance;
			}
		}

		private bool[,] visibility;
		private int _mapSize;
		private int mapHeight;
		private int tileSize;
		private Texture2D pixel;
		private SpriteBatch spriteBatch;

		private Minimap(int mapSize, int tileSize, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
		{
			_mapSize = mapSize;
			this.tileSize = tileSize;
			this.spriteBatch = spriteBatch;
			this.mapHeight = mapSize;
			visibility = new bool[mapSize, mapSize];

			pixel = new Texture2D(graphicsDevice, 1, 1);
			pixel.SetData(new[] { Color.White });
		}

		public static void Initialize(int mapSize, int tileSize, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
		{
			if (instance == null)
				instance = new Minimap(mapSize, tileSize, graphicsDevice, spriteBatch);
		}

		public void MarkVisible(int x, int y)
		{
			if (x >= 0 && x < _mapSize && y >= 0 && y < mapHeight)
				visibility[y, x] = true;
		}

		public void Draw(Vector2 playerPos, Vector2 playerDir)
		{
			int offsetX = 10;
			int offsetY = 10;
			int minimapWidth = _mapSize * tileSize;
			int minimapHeight = mapHeight * tileSize;

			// Background and border
			spriteBatch.Draw(pixel, new Rectangle(offsetX - 2, offsetY - 2, minimapWidth + 4, minimapHeight + 4), Color.Gold);
			spriteBatch.Draw(pixel, new Rectangle(offsetX, offsetY, minimapWidth, minimapHeight), Color.Gray);

			var map = MapManager.Instance.GetMap();

			for (int y = 0; y < mapHeight; y++)
			{
				for (int x = 0; x < _mapSize; x++)
				{
					if (!visibility[y, x]) continue;

					Color color = map[y, x] switch
					{
						0 => Color.Black,
						1 => Color.Gray,
						2 => Color.Green,
						_ => Color.DarkGray
					};

					spriteBatch.Draw(pixel,
						new Rectangle(offsetX + x * tileSize, offsetY + y * tileSize, tileSize, tileSize),
						color);
				}
			}

			// Player
			spriteBatch.Draw(pixel,
				new Rectangle(offsetX + (int)(playerPos.X * tileSize), offsetY + (int)(playerPos.Y * tileSize), tileSize / 2, tileSize / 2),
				Color.Yellow);

			// View direction
			Vector2 end = playerPos + playerDir * 2f;
			DrawLine((int)(playerPos.X * tileSize) + offsetX, (int)(playerPos.Y * tileSize) + offsetY,
					 (int)(end.X * tileSize) + offsetX, (int)(end.Y * tileSize) + offsetY,
					 Color.Yellow);
		}

		private void DrawLine(int x0, int y0, int x1, int y1, Color color)
		{
			int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
			int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
			int err = dx + dy, e2;

			while (true)
			{
				spriteBatch.Draw(pixel, new Rectangle(x0, y0, 1, 1), color);
				if (x0 == x1 && y0 == y1) break;
				e2 = 2 * err;
				if (e2 >= dy) { err += dy; x0 += sx; }
				if (e2 <= dx) { err += dx; y0 += sy; }
			}
		}

		public void ClearMap()
		{
			for (int y = 0; y < mapHeight; y++)
				for (int x = 0; x < _mapSize; x++)
					visibility[y, x] = false;
		}
	}
}
