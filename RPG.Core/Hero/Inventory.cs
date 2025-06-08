using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace RPG.Core
{
	public class Inventory
	{
		private static Inventory instance;
		private SpriteBatch _spriteBatch;
		private SpriteFont _font;
		private GraphicsDevice _graphicsDevice;

		public static Inventory Instance
		{
			get
			{
				if (instance == null)
					throw new InvalidOperationException("Inventory has not been initialized");
				return instance;
			}
		}

		public const int InventorySize = 24;
		private Item[] items;

		public static void Initialize(SpriteBatch spriteBatch, SpriteFont font, GraphicsDevice graphicsDevice)
		{
			instance = new Inventory(spriteBatch, font, graphicsDevice);
		}
		private Inventory(SpriteBatch spriteBatch, SpriteFont font, GraphicsDevice graphicsDevice)
		{
			items = new Item[InventorySize];
			_spriteBatch = spriteBatch;
			_font = font;
			_graphicsDevice = graphicsDevice;
		}

		// Get item at slot (0-based)
		public Item GetItem(int slot)
		{
			if (slot < 0 || slot >= InventorySize)
				throw new ArgumentOutOfRangeException(nameof(slot));
			return items[slot];
		}

		// Add item to first empty slot, returns slot index or -1 if full
		public int AddItem(Item item)
		{
			if (item == null)
				throw new ArgumentNullException(nameof(item));

			for (int i = 0; i < InventorySize; i++)
			{
				if (items[i] == null)
				{
					items[i] = item;
					return i;
				}
			}
			return -1; // Inventory full
		}

		// Remove item from slot, returns removed item or null if empty
		public Item RemoveItem(int slot)
		{
			if (slot < 0 || slot >= InventorySize)
				throw new ArgumentOutOfRangeException(nameof(slot));

			Item removed = items[slot];
			items[slot] = null;
			return removed;
		}

		// Check if inventory is full
		public bool IsFull()
		{
			foreach (var item in items)
			{
				if (item == null)
					return false;
			}
			return true;
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

	// Example Item class - customize as needed
	public class Item
	{
		public string Name { get; set; }
		public int Id { get; set; }
		// Add other properties like stack size, type, stats, etc.

		public Item(string name, int id)
		{
			Name = name;
			Id = id;
		}
	}
}
