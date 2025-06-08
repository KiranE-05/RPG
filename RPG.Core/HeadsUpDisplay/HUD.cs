using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RPG.Core.Hero;
using System;

namespace RPG.Core.HeadsUpDisplay
{
    public class HUD
    {
        private static HUD instance;
        public static HUD Instance
        {
            get
            {
                if (instance == null)
                    throw new InvalidOperationException("HUD not initialized. Call HUD.Initialize().");
                return instance;
            }
        }

        private SpriteFont _font;
        private SpriteBatch _spriteBatch;
        private readonly Texture2D _background;
        private int _panelHeight = 64;

        private HUD(SpriteBatch spriteBatch, SpriteFont font, GraphicsDevice graphicsDevice)
        {
            _spriteBatch = spriteBatch;

            // Use built-in default font if none provided
            _font = font; // You must expose this from Game1

            _background = new Texture2D(graphicsDevice, 1, 1);
            _background.SetData(new[] { new Color(0, 0, 0, 200) }); // semi-transparent black
        }

        public static void Initialize(SpriteBatch spriteBatch, SpriteFont font, GraphicsDevice graphicsDevice)
        {
            instance ??= new HUD(spriteBatch, font, graphicsDevice);
        }

        public void Draw(int screenWidth, int screenHeight)
        {
            var stats = Player.Instance.Stats;
            Rectangle panelRect = new Rectangle(0, screenHeight - _panelHeight, screenWidth, _panelHeight);
            _spriteBatch.Draw(_background, panelRect, Color.Black);

            Vector2 position = new Vector2(20, screenHeight - _panelHeight + 10);
            int spacing = 140;

            _spriteBatch.DrawString(_font, $"Level: {stats.Level}", position, Color.White);
            _spriteBatch.DrawString(_font, $"HP: {stats.CurrentHP}/{stats.MaxHP}", position + new Vector2(spacing, 0), Color.Red);
            _spriteBatch.DrawString(_font, $"Mana: {stats.CurrentMana}/{stats.MaxMana}", position + new Vector2(spacing * 2, 0), Color.Cyan);
            _spriteBatch.DrawString(_font, $"Endurance: {stats.CurrentEndurance}/{stats.MaxEndurance}", position + new Vector2(spacing * 3, 0), Color.Orange);
            _spriteBatch.DrawString(_font, $"XP: {stats.CurrentXP}/{stats.XPToLevelUp}", position + new Vector2(spacing * 4, 0), Color.Green);
        }
    }
}
