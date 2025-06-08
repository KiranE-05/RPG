using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace RPG.Core.HeadsUpDisplay
{
	public class InventoryUI
	{
		private static InventoryUI instance;
		public static InventoryUI Instance
		{
			get
			{
				if (instance == null)
					throw new InvalidOperationException("Minimap must be initialized first using Minimap.Initialize().");
				return instance;
			}
		}

		private Texture2D pixel;
		private SpriteBatch _spriteBatch;
		private SpriteFont _font;
		private GraphicsDevice _graphicsDevice;

		public static void Initialize(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
		{
			instance = new InventoryUI(graphicsDevice, spriteBatch);
		}

		private InventoryUI(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
		{
			_spriteBatch = spriteBatch;
			_graphicsDevice = graphicsDevice;

			pixel = new Texture2D(graphicsDevice, 1, 1);
			pixel.SetData(new[] { Color.White });
		}

		public void DrawInventoryUI()
		{
			int slotSize = 50;
			int padding = 5;
			int columns = 6;
			int rows = Inventory.InventorySize / columns;

			int startX = 100;
			int startY = 100;
			var inv = Inventory.Instance;

			// Background box
			_spriteBatch.Draw(GetPixelTexture(), new Rectangle(startX - 10, startY - 10,
				columns * (slotSize + padding) + 20, rows * (slotSize + padding) + 20), Color.Black * 0.8f);

			for (int i = 0; i < Inventory.InventorySize; i++)
			{
				int col = i % columns;
				int row = i / columns;

				Rectangle slotRect = new Rectangle(startX + col * (slotSize + padding), startY + row * (slotSize + padding), slotSize, slotSize);

				// Draw slot background
				_spriteBatch.Draw(GetPixelTexture(), slotRect, Color.DarkGray);

				var item = inv.GetItem(i);
				if (item != null)
				{
					// Draw item name centered
					Vector2 textSize = _font.MeasureString(item.Name);
					Vector2 textPos = new Vector2(slotRect.X + (slotSize - textSize.X) / 2, slotRect.Y + (slotSize - textSize.Y) / 2);
					_spriteBatch.DrawString(_font, item.Name, textPos, Color.White);
				}
			}
		}

		// Simple 1x1 white pixel texture helper
		private Texture2D pixelTexture;
		private Texture2D GetPixelTexture()
		{
			if (pixelTexture == null)
			{
				pixelTexture = new Texture2D(_graphicsDevice, 1, 1);
				pixelTexture.SetData(new[] { Color.White });
			}
			return pixelTexture;
		}
	}
}
