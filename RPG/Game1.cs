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

		private Raycaster.Raycaster raycaster;
		private MapManager mapManager;
		private Player player;

		private int _mapWidth = 20;
		private int _mapHeight = 20;

		public int[,] map;

		public Game1()
		{
			var gdm = new GraphicsDeviceManager(this);
			gdm.PreferredBackBufferHeight = 600;
			gdm.PreferredBackBufferWidth = 800;
			gdm.IsFullScreen = true;
			_graphics = gdm;
			Content.RootDirectory = "Content";
			IsMouseVisible = true;
		}

		protected override void Initialize()
		{
			IsMouseVisible = false;
			GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
			Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);

			// Access the shared map
			MapManager.Instance.Regenerate(_mapWidth, _mapHeight);
			var map = MapManager.Instance.GetMap();

			player = new Player(MapHelper.FindSafeStartPosition(MapManager.Instance.GetMap()));

			_playerDot = new Texture2D(GraphicsDevice, 1, 1);
			_playerDot.SetData(new[] { Color.Red });

			base.Initialize();
		}

		protected override void LoadContent()
		{
			_spriteBatch = new SpriteBatch(GraphicsDevice);

			GraphicsDevice.Clear(Color.Black);

			stoneTexture = Content.Load<Texture2D>("Textures/stonebricks1");
			exitTexture = Content.Load<Texture2D>("Textures/stonebricks1_door");

			raycaster = new Raycaster.Raycaster(stoneTexture, exitTexture, stoneTexture, _spriteBatch);

			Minimap.Initialize(_mapWidth, _mapHeight, 4, GraphicsDevice, _spriteBatch);
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
				player.Rotate(rotationAmount);
				Mouse.SetPosition(centerX, mouse.Y);
			}

			player.Update(gameTime);

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);
			_spriteBatch.Begin();

			raycaster.Render(GraphicsDevice.Viewport.Width,
					GraphicsDevice.Viewport.Height,
					player.Position,       // pass current player position
					player.Direction,      // current player direction
					player.CameraPlane);   // current camera plane

			Minimap.Instance.Draw(player.Position, player.Direction);

			_spriteBatch.End();
			base.Draw(gameTime);
		}

	}
}
