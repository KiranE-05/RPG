using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Raycaster;
using RPG.Core;
using RPG.Core.Helpers;
using System;

namespace RPG
{
	public class Game1 : Game
	{
		private GraphicsDeviceManager _graphics;
		private SpriteBatch _spriteBatch;

		Texture2D stoneTexture;
		Texture2D exitTexture;
		Texture2D _playerDot;

		private readonly int _mapSize = 25;

		public Game1()
		{
			var gdm = new GraphicsDeviceManager(this);
			gdm.PreferredBackBufferHeight = 600;
			gdm.PreferredBackBufferWidth = 800;
			gdm.IsFullScreen = false;
			_graphics = gdm;
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

			Minimap.Initialize(_mapSize, 6, GraphicsDevice, _spriteBatch);
			MapManager.Instance.Regenerate(_mapSize);

			Player.Initialize(MapHelper.FindSafeStartPosition());

			stoneTexture = Content.Load<Texture2D>("Textures/stonebricks1");
			exitTexture = Content.Load<Texture2D>("Textures/stonebricks1_door");

			Raycaster.Raycaster.Initialize(stoneTexture, exitTexture, stoneTexture, _spriteBatch);

		}

		protected override void Update(GameTime gameTime)
		{
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
				Exit();

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

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.Black);
			_spriteBatch.Begin();

			Raycaster.Raycaster.Instance.Render(GraphicsDevice.Viewport.Width,
					GraphicsDevice.Viewport.Height,
					Player.Instance.Position,       // pass current player position
					Player.Instance.Direction,      // current player direction
					Player.Instance.CameraPlane);   // current camera plane

			Minimap.Instance.Draw(Player.Instance.Position, Player.Instance.Direction);

			_spriteBatch.End();
			base.Draw(gameTime);
		}

	}
}
