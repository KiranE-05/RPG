﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RPG.Core;
using RPG.Core.HeadsUpDisplay;

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

		private readonly int _mapWidth;
		private readonly int _mapHeight;
		private readonly Texture2D _wallTexture;
		private readonly Texture2D _exitTexture;
		private readonly Texture2D _floorTexture;
		private readonly Texture2D _chestTexture;
		private readonly SpriteBatch _spriteBatch;

		private readonly Color[] floorTextureData;
		private readonly Color[] chestTextureData;

		private readonly int texWidth;
		private readonly int texHeight;

		private Raycaster(Texture2D wallTexture, Texture2D exitTexture, Texture2D floorTexture, Texture2D chestTexture, SpriteBatch spriteBatch)
		{
			_mapHeight = MapManager.Instance.GetMap().GetLength(0);
			_mapWidth = MapManager.Instance.GetMap().GetLength(1);
			_wallTexture = wallTexture;
			_exitTexture = exitTexture;
			_chestTexture = chestTexture;

			_floorTexture = floorTexture;
			chestTextureData = new Color[_chestTexture.Width * _chestTexture.Height];
			_chestTexture.GetData(chestTextureData);

			_spriteBatch = spriteBatch;


			texWidth = wallTexture.Width;
			texHeight = wallTexture.Height;

			floorTextureData = new Color[_floorTexture.Width * _floorTexture.Height];
			_floorTexture.GetData(floorTextureData);
		}

		public static void Initialize(Texture2D wallTexture, Texture2D exitTexture, Texture2D floorTexture, Texture2D chestTexture, SpriteBatch spriteBatch)
		{
			instance ??= new Raycaster(wallTexture, exitTexture, floorTexture, chestTexture, spriteBatch);
		}

		public void Render(int screenWidth, int screenHeight,
						   Vector2 playerPos, Vector2 playerDir, Vector2 cameraPlane)
		{
			float[] zBuffer = new float[screenWidth];
			for (int x = 0; x < screenWidth; x++)
			{
				float cameraX = 2 * x / (float)screenWidth - 1;
				Vector2 rayDir = playerDir + cameraPlane * cameraX;

				Vector2 mapCheck = new((int)playerPos.X, (int)playerPos.Y);
				Vector2 deltaDist = new(Math.Abs(1 / rayDir.X),	Math.Abs(1 / rayDir.Y));

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

					if (mapCheck.X < 0 || mapCheck.X >= _mapWidth || mapCheck.Y < 0 || mapCheck.Y >= _mapHeight)
						break;

					int tile = MapManager.Instance.GetMap()[(int)mapCheck.Y, (int)mapCheck.X];
					if (tile > 0 && tile != 3) // Skip chest as wall hit
						hit = true;

					steps++;
				}

				if (!hit)
					continue;

				// ← This is the fix
				Minimap.Instance.MarkVisible((int)mapCheck.X, (int)mapCheck.Y);

				int mapTile = MapManager.Instance.GetMap()[(int)mapCheck.Y, (int)mapCheck.X];

				Texture2D texToUse = mapTile switch
				{
					1 => _wallTexture,
					2 => _exitTexture,
					_ => null // Do not render chests here
				};

				float perpWallDist;

				if (side == 0)
					perpWallDist = (mapCheck.X - playerPos.X + (1 - step.X) / 2f) / rayDir.X;
				else
					perpWallDist = (mapCheck.Y - playerPos.Y + (1 - step.Y) / 2f) / rayDir.Y;

				perpWallDist = Math.Max(Math.Abs(perpWallDist), 0.1f);

				zBuffer[x] = perpWallDist;

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

					Rectangle sourceRect = new(texX, texY, 1, 1);
					Rectangle destRect = new(x, y, 1, 1);

					Color shade = (side == 1) ? Color.Gray : Color.White;

					if(texToUse != null)
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

			RenderChests(playerPos, playerDir, cameraPlane, screenWidth, screenHeight, zBuffer);
		}

		private void RenderChests(Vector2 playerPos, Vector2 playerDir, Vector2 cameraPlane, int screenWidth, int screenHeight, float[] zBuffer)
		{
			var map = MapManager.Instance.GetMap();
			int mapHeight = map.GetLength(0);
			int mapWidth = map.GetLength(1);

			float spriteSize = 0.75f; // Adjust for chest scale on screen

			for (int y = 0; y < mapHeight; y++)
			{
				for (int x = 0; x < mapWidth; x++)
				{
					if (map[y, x] != 3) continue; // Only draw chests (value 3)

					// Sprite world position: center of tile
					float spriteX = x + 0.5f;
					float spriteY = y + 0.5f;

					// Translate sprite position to relative to camera
					float dx = spriteX - playerPos.X;
					float dy = spriteY - playerPos.Y;

					// Inverse camera matrix
					float invDet = 1.0f / (cameraPlane.X * playerDir.Y - playerDir.X * cameraPlane.Y);

					float transformX = invDet * (playerDir.Y * dx - playerDir.X * dy);
					float transformY = invDet * (-cameraPlane.Y * dx + cameraPlane.X * dy);

					if (transformY <= 0) continue; // Sprite is behind the player

					int spriteScreenX = (int)((screenWidth / 2) * (1 + transformX / transformY));

					// Height and width of the sprite on screen
					int spriteHeight = Math.Abs((int)(screenHeight / transformY * spriteSize));
					int drawStartY = -spriteHeight / 2 + screenHeight / 2;
					int drawEndY = spriteHeight / 2 + screenHeight / 2;

					int spriteWidth = spriteHeight; // Assuming square sprite
					int drawStartX = -spriteWidth / 2 + spriteScreenX;
					int drawEndX = spriteWidth / 2 + spriteScreenX;

					drawStartY = Math.Clamp(drawStartY, 0, screenHeight - 1);
					drawEndY = Math.Clamp(drawEndY, 0, screenHeight - 1);
					drawStartX = Math.Clamp(drawStartX, 0, screenWidth - 1);
					drawEndX = Math.Clamp(drawEndX, 0, screenWidth - 1);

					for (int stripe = drawStartX; stripe < drawEndX; stripe++)
					{
						if (transformY < zBuffer[stripe]) // Occlusion check
						{
							int texX = (int)((stripe - drawStartX) * _chestTexture.Width / (drawEndX - drawStartX));

							for (int yPix = drawStartY; yPix < drawEndY; yPix++)
							{
								int texY = (int)((yPix - drawStartY) * _chestTexture.Height / (drawEndY - drawStartY));

								Color color = GetChestPixelColor(texX, texY);
								if (color.A > 32) // Skip transparent pixels
								{
									_spriteBatch.Draw(
										_chestTexture,
										new Rectangle(stripe, yPix, 1, 1),
										new Rectangle(texX, texY, 1, 1),
										color);
								}
							}
						}
					}
				}
			}
		}
		private Color GetChestPixelColor(int x, int y)
		{
			int index = y * _chestTexture.Width + x;
			return chestTextureData[index];
		}
	}
}
