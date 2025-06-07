using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raycaster;

namespace RPG
{
	internal class Player
	{
		public Vector2 Position;
		public Vector2 Direction;
		public Vector2 CameraPlane;

		private int[,] map;

		public Player(Vector2 startPosition, int[,] map)
		{
			Position = startPosition;
			Direction = new Vector2(1, 0);
			CameraPlane = new Vector2(0, 0.66f);
			this.map = map;
		}

		public void Update(GameTime gameTime, int[,] map, Raycaster.Raycaster raycaster)
		{
			KeyboardState keyboard = Keyboard.GetState();

			Vector2 newPos = Position;
			float moveSpeed = 3.0f * (float)gameTime.ElapsedGameTime.TotalSeconds;

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

			int playerTileX = (int)Position.X;
			int playerTileY = (int)Position.Y;

			if (raycaster.GetMap()[playerTileY, playerTileX] == 2)
			{
				raycaster.LoadNextLevel();
				Position = new Vector2(2, 2); // Reset to a starting position
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

		private bool IsWalkable(Vector2 pos)
		{
			int x = (int)pos.X;
			int y = (int)pos.Y;

			if (x < 0 || x >= map.GetLength(1) || y < 0 || y >= map.GetLength(0))
				return false;

			return map[y, x] == 0;
		}
	}
}
