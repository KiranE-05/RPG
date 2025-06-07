using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Raycaster
{
	public class Minimap
	{
		private bool[,] visibility;
		private int mapWidth;
		private int mapHeight;
		private int tileSize;
		private Texture2D pixel;
		private SpriteBatch spriteBatch;

		public Minimap(int width, int height, int tileSize, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
		{
			this.mapWidth = width;
			this.mapHeight = height;
			this.tileSize = tileSize;
			this.spriteBatch = spriteBatch;
			visibility = new bool[height, width];

			pixel = new Texture2D(graphicsDevice, 1, 1);
			pixel.SetData(new[] { Color.White });
		}

		public void MarkVisible(int x, int y)
		{
			if (x >= 0 && x < mapWidth && y >= 0 && y < mapHeight)
				visibility[y, x] = true;
		}

		public void Draw(int[,] map, Vector2 playerPos, Vector2 playerDir)
		{
			int offsetX = 10;
			int offsetY = 10;
			int minimapWidth = mapWidth * tileSize;
			int minimapHeight = mapHeight * tileSize;

			// Draw background
			spriteBatch.Draw(pixel, new Rectangle(offsetX - 2, offsetY - 2, minimapWidth + 4, minimapHeight + 4), Color.Gold); // Border
			spriteBatch.Draw(pixel, new Rectangle(offsetX, offsetY, minimapWidth, minimapHeight), Color.Gray); // Background

			for (int y = 0; y < mapHeight; y++)
			{
				for (int x = 0; x < mapWidth; x++)
				{
					if (!visibility[y, x]) continue;

					Color color = map[y, x] switch
					{
						0 => Color.Black,
						1 => Color.Gray,
						2 => Color.Red,
						_ => Color.DarkGray
					};

					spriteBatch.Draw(pixel,
						new Rectangle(offsetX + x * tileSize, offsetY + y * tileSize, tileSize, tileSize),
						color);
				}
			}

			// Draw player
			spriteBatch.Draw(pixel,
				new Rectangle(offsetX + (int)(playerPos.X * tileSize), offsetY + (int)(playerPos.Y * tileSize), tileSize / 2, tileSize / 2),
				Color.Yellow);

			// Draw viewing direction
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
	}
}
