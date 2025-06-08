using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using RPG.Core;
using RPG.Core.HeadsUpDisplay;
using RPG.Core.Helpers;
using RPG.Core.Hero;
using System;
using static System.Net.Mime.MediaTypeNames;

namespace RPG
{
	public class Game1 : Game
	{
		private SpriteBatch _spriteBatch;

		Texture2D stoneTexture;
		Texture2D exitTexture;
		Texture2D chestTexture;
		Texture2D _playerDot;

		private bool inventoryVisible = false;

		private bool _paused = false;

		private static SpriteFont _defaultFont;

		private readonly int _mapSize = 25;

		KeyboardState prevKeyboardState;

		public Game1()
		{
			var gdm = new GraphicsDeviceManager(this)
			{
				PreferredBackBufferHeight = 600,
				PreferredBackBufferWidth = 800,
				IsFullScreen = false
			};
			Content.RootDirectory = "Content";
			IsMouseVisible = true;
		}

		protected override void Initialize()
		{
			IsMouseVisible = false;
			GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
			Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);

			_playerDot = new Texture2D(GraphicsDevice, 1, 1);
			_playerDot.SetData(new[] { Color.Red });

			base.Initialize();
		}

		protected override void LoadContent()
		{
			_spriteBatch = new SpriteBatch(GraphicsDevice);

			GraphicsDevice.Clear(Color.Black);
			_defaultFont = Content.Load<SpriteFont>("DefaultFont");
			HUD.Initialize(_spriteBatch, _defaultFont, GraphicsDevice);
			Minimap.Initialize(_mapSize, 6, GraphicsDevice, _spriteBatch);
			MapManager.Instance.Regenerate(_mapSize);

			Player.Initialize(MapHelper.FindSafeStartPosition());

			stoneTexture = Content.Load<Texture2D>("Textures/stonebricks1");
			exitTexture = Content.Load<Texture2D>("Textures/stonebricks1_door");
			chestTexture = Content.Load<Texture2D>("Textures/Chest");

			MedievalProceduralAudio generator = new();
			SoundEffect song = generator.GenerateSong(45);
			SoundEffectInstance instance = song.CreateInstance();
			instance.IsLooped = true;
			instance.Play();

			Raycaster.Raycaster.Initialize(stoneTexture, exitTexture, stoneTexture, chestTexture, _spriteBatch);
			Inventory.Initialize(_spriteBatch, _defaultFont, GraphicsDevice);
		}

		protected override void Update(GameTime gameTime)
		{
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
				Exit();
			
			KeyboardState kb = Keyboard.GetState();

			if (kb.IsKeyDown(Keys.I) && !prevKeyboardState.IsKeyDown(Keys.I))
			{
				inventoryVisible = !inventoryVisible;
				_paused = inventoryVisible;
			}

			prevKeyboardState = kb;

			// Show/hide mouse cursor depending on inventory state
			IsMouseVisible = inventoryVisible;

			if (!_paused)
			{
				// Only update game logic when inventory is closed
				UpdateGameLogic(gameTime);
			}

			base.Update(gameTime);
		}

		private void UpdateGameLogic(GameTime gameTime)
		{
			int centerX = GraphicsDevice.Viewport.Width / 2;
			MouseState mouse = Mouse.GetState();
			int deltaX = mouse.X - centerX;

			float sensitivity = 0.002f;
			float rotationAmount = deltaX * sensitivity;

			if (deltaX != 0)
			{
				Player.Instance.Rotate(rotationAmount);
				Mouse.SetPosition(centerX, mouse.Y);
			}

			Player.Instance.Update(gameTime);

		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.Black);
			_spriteBatch.Begin();

			DrawGameScene();

			if (inventoryVisible)
			{
				Inventory.Instance.DrawInventoryUI();
			}

			_spriteBatch.End();
			base.Draw(gameTime);
		}

		private void DrawGameScene()
		{
			Raycaster.Raycaster.Instance.Render(GraphicsDevice.Viewport.Width,
		GraphicsDevice.Viewport.Height,
		Player.Instance.Position,       // pass current player position
		Player.Instance.Direction,      // current player direction
		Player.Instance.CameraPlane);   // current camera plane

			Minimap.Instance.Draw(Player.Instance.Position, Player.Instance.Direction);
			// After raycast rendering:
			HUD.Instance.Draw(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, Player.Instance.Stats);
		}

		
	}
}
