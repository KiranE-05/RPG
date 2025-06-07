using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RPG.Core;
using RPG.Core.Helpers;
using System;

namespace Raycaster
{
	public class Raycaster
	{
		private static Raycaster instance;
		public static Raycaster Instance
		{
			get
			{
				if (instance == null)
					throw new InvalidOperationException("Raycaster must be initialized first using Raycaster.Initialize().");
				return instance;
			}
		}

		private int mapWidth;
		private int mapHeight;
		private Texture2D wallTexture;
		private Texture2D exitTexture;
		private Texture2D _floorTexture;
		private SpriteBatch _spriteBatch;

		private Color[] floorTextureData;
		private int texWidth;
		private int texHeight;

		private Raycaster(Texture2D wallTexture, Texture2D exitTexture, Texture2D floorTexture, SpriteBatch spriteBatch)
		{
			mapHeight = MapManager.Instance.GetMap().GetLength(0);
			mapWidth = MapManager.Instance.GetMap().GetLength(1);
			this.wallTexture = wallTexture;
			this.exitTexture = exitTexture;
			this._floorTexture = floorTexture;
			this._spriteBatch = spriteBatch;

			texWidth = wallTexture.Width;
			texHeight = wallTexture.Height;

			floorTextureData = new Color[_floorTexture.Width * _floorTexture.Height];
			_floorTexture.GetData(floorTextureData);
		}

		public static void Initialize(Texture2D wallTexture, Texture2D exitTexture, Texture2D floorTexture, SpriteBatch spriteBatch)
		{
			if (instance == null)
				instance = new Raycaster(wallTexture, exitTexture, floorTexture, spriteBatch);
		}

		public void Render(int screenWidth, int screenHeight,
						   Vector2 playerPos, Vector2 playerDir, Vector2 cameraPlane)
		{
			for (int x = 0; x < screenWidth; x++)
			{
				float cameraX = 2 * x / (float)screenWidth - 1;
				Vector2 rayDir = playerDir + cameraPlane * cameraX;

				Vector2 mapCheck = new Vector2((int)playerPos.X, (int)playerPos.Y);
				Vector2 deltaDist = new Vector2(
					Math.Abs(1 / rayDir.X),
					Math.Abs(1 / rayDir.Y));

				Vector2 sideDist;
				Vector2 step;

				if (rayDir.X < 0)
				{
					step.X = -1;
					sideDist.X = (playerPos.X - mapCheck.X) * deltaDist.X;
				}
				else
				{
					step.X = 1;
					sideDist.X = (mapCheck.X + 1.0f - playerPos.X) * deltaDist.X;
				}

				if (rayDir.Y < 0)
				{
					step.Y = -1;
					sideDist.Y = (playerPos.Y - mapCheck.Y) * deltaDist.Y;
				}
				else
				{
					step.Y = 1;
					sideDist.Y = (mapCheck.Y + 1.0f - playerPos.Y) * deltaDist.Y;
				}

				bool hit = false;
				int side = 0;
				int maxSteps = 100;
				int steps = 0;

				while (!hit && steps < maxSteps)
				{
					Minimap.Instance.MarkVisible((int)mapCheck.X, (int)mapCheck.Y);

					if (sideDist.X < sideDist.Y)
					{
						sideDist.X += deltaDist.X;
						mapCheck.X += step.X;
						side = 0;
					}
					else
					{
						sideDist.Y += deltaDist.Y;
						mapCheck.Y += step.Y;
						side = 1;
					}

					if (mapCheck.X < 0 || mapCheck.X >= mapWidth || mapCheck.Y < 0 || mapCheck.Y >= mapHeight)
						break;

					if (MapManager.Instance.GetMap()[(int)mapCheck.Y, (int)mapCheck.X] > 0)
						hit = true;

					steps++;
				}

				if (!hit)
					continue;

				int mapTile = MapManager.Instance.GetMap()[(int)mapCheck.Y, (int)mapCheck.X];
				Texture2D texToUse = (mapTile == 2) ? exitTexture : wallTexture;

				float perpWallDist;

				if (side == 0)
					perpWallDist = (mapCheck.X - playerPos.X + (1 - step.X) / 2f) / rayDir.X;
				else
					perpWallDist = (mapCheck.Y - playerPos.Y + (1 - step.Y) / 2f) / rayDir.Y;

				perpWallDist = Math.Max(Math.Abs(perpWallDist), 0.1f);

				int lineHeight = (int)(screenHeight / perpWallDist);
				int drawStart = Math.Clamp(-lineHeight / 2 + screenHeight / 2, 0, screenHeight - 1);
				int drawEnd = Math.Clamp(lineHeight / 2 + screenHeight / 2, 0, screenHeight - 1);

				float wallX = (side == 0)
					? playerPos.Y + perpWallDist * rayDir.Y
					: playerPos.X + perpWallDist * rayDir.X;
				wallX -= (float)Math.Floor(wallX);

				int texX = (int)(wallX * texWidth);
				if ((side == 0 && rayDir.X > 0) || (side == 1 && rayDir.Y < 0))
					texX = texWidth - texX - 1;

				texX = Math.Clamp(texX, 0, texWidth - 1);

				for (int y = drawStart; y < drawEnd; y++)
				{
					int d = y * 256 - screenHeight * 128 + lineHeight * 128;
					int texY = ((d * texHeight) / lineHeight) / 256;
					texY = Math.Clamp(texY, 0, texHeight - 1);

					Rectangle sourceRect = new Rectangle(texX, texY, 1, 1);
					Rectangle destRect = new Rectangle(x, y, 1, 1);

					Color shade = (side == 1) ? Color.Gray : Color.White;

					_spriteBatch.Draw(texToUse, destRect, sourceRect, shade);
				}

				// Floor and ceiling
				float floorWallX, floorWallY;

				if (side == 0 && rayDir.X > 0)
				{
					floorWallX = mapCheck.X;
					floorWallY = mapCheck.Y + wallX;
				}
				else if (side == 0 && rayDir.X < 0)
				{
					floorWallX = mapCheck.X + 1.0f;
					floorWallY = mapCheck.Y + wallX;
				}
				else if (side == 1 && rayDir.Y > 0)
				{
					floorWallX = mapCheck.X + wallX;
					floorWallY = mapCheck.Y;
				}
				else
				{
					floorWallX = mapCheck.X + wallX;
					floorWallY = mapCheck.Y + 1.0f;
				}

				for (int y = drawEnd + 1; y < screenHeight; y++)
				{
					float currentDist = screenHeight / (2.0f * y - screenHeight);
					float weight = currentDist / perpWallDist;

					float floorX = weight * floorWallX + (1.0f - weight) * playerPos.X;
					float floorY = weight * floorWallY + (1.0f - weight) * playerPos.Y;

					int floorTexX = (int)(floorX * texWidth) % texWidth;
					int floorTexY = (int)(floorY * texHeight) % texHeight;

					if (floorTexX < 0) floorTexX += texWidth;
					if (floorTexY < 0) floorTexY += texHeight;

					Color floorColor = floorTextureData[floorTexX + floorTexY * texWidth];
					Color ceilingColor = floorColor;

					_spriteBatch.Draw(_floorTexture, new Rectangle(x, y, 1, 1), new Rectangle(floorTexX, floorTexY, 1, 1), floorColor);

					int ceilingY = screenHeight - y;
					if (ceilingY >= 0 && ceilingY < screenHeight)
					{
						_spriteBatch.Draw(_floorTexture, new Rectangle(x, ceilingY, 1, 1), new Rectangle(floorTexX, floorTexY, 1, 1), ceilingColor);
					}
				}
			}
		}
	}
}
