using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System;
using RPG.Core.Helpers;

namespace RPG.Core.Hero
{
	public class Player
	{
		public Vector2 Position;
		public Vector2 Direction;
		public Vector2 CameraPlane;

	    public PlayerStats Stats;

		private static Player instance;
		public static Player Instance
		{
			get
			{
				if (instance == null)
					throw new InvalidOperationException("Player must be initialized first using Player.Initialize().");
				return instance;
			}
		}

		public static void Initialize(Vector2 startPosition)
		{
			instance ??= new Player(startPosition);
		}

		private Player(Vector2 startPosition)
		{
			Position = startPosition;
			Direction = new Vector2(1, 0);
			CameraPlane = new Vector2(0, 0.66f);
			Stats = new PlayerStats();
		}

		public void Update(GameTime gameTime)
		{
			KeyboardState keyboard = Keyboard.GetState();

			Vector2 newPos = Position;
			float moveSpeed = 1.80f * (float)gameTime.ElapsedGameTime.TotalSeconds;

			// Movement
			if (keyboard.IsKeyDown(Keys.W))
			{
				Vector2 step = Direction * moveSpeed;
				if (IsWalkable(newPos + new Vector2(step.X, 0))) newPos.X += step.X;
				if (IsWalkable(newPos + new Vector2(0, step.Y))) newPos.Y += step.Y;
			}
			if (keyboard.IsKeyDown(Keys.S))
			{
				Vector2 step = -Direction * moveSpeed;
				if (IsWalkable(newPos + new Vector2(step.X, 0))) newPos.X += step.X;
				if (IsWalkable(newPos + new Vector2(0, step.Y))) newPos.Y += step.Y;
			}
			if (keyboard.IsKeyDown(Keys.A))
			{
				Vector2 step = -CameraPlane * moveSpeed;
				if (IsWalkable(newPos + new Vector2(step.X, 0))) newPos.X += step.X;
				if (IsWalkable(newPos + new Vector2(0, step.Y))) newPos.Y += step.Y;
			}
			if (keyboard.IsKeyDown(Keys.D))
			{
				Vector2 step = CameraPlane * moveSpeed;
				if (IsWalkable(newPos + new Vector2(step.X, 0))) newPos.X += step.X;
				if (IsWalkable(newPos + new Vector2(0, step.Y))) newPos.Y += step.Y;
			}

			Position = newPos;

			Vector2 checkPos = Position + Direction * 0.5f; // 0.5 units in front of the player
			int checkX = (int)checkPos.X;
			int checkY = (int)checkPos.Y;

			if (checkX >= 0 && checkX < MapManager.Instance.GetMap().GetLength(1) &&
				checkY >= 0 && checkY < MapManager.Instance.GetMap().GetLength(0) &&
				MapManager.Instance.GetMap()[checkY, checkX] == 2)
			{
				MapManager.Instance.Regenerate(25, 25);
				Position = MapHelper.FindSafeStartPosition(); // Reset to a starting position
				Direction = new Vector2(1, 0);
				CameraPlane = new Vector2(0, 0.66f);
			}
		}

		public void Rotate(float angle)
		{
			float cos = (float)Math.Cos(angle);
			float sin = (float)Math.Sin(angle);

			float oldDirX = Direction.X;
			Direction.X = Direction.X * cos - Direction.Y * sin;
			Direction.Y = oldDirX * sin + Direction.Y * cos;

			float oldPlaneX = CameraPlane.X;
			CameraPlane.X = CameraPlane.X * cos - CameraPlane.Y * sin;
			CameraPlane.Y = oldPlaneX * sin + CameraPlane.Y * cos;
		}

		private static bool IsWalkable(Vector2 pos)
		{
			int x = (int)pos.X;
			int y = (int)pos.Y;

			if (x < 0 || x >= MapManager.Instance.GetMap().GetLength(1) || y < 0 || y >= MapManager.Instance.GetMap().GetLength(0))
				return false;

			return MapManager.Instance.GetMap()[y, x] == 0;
		}
	}
}
