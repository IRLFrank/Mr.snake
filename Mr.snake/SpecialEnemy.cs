using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Mr.snake;

namespace Mr_snake
{
    public class SpecialEnemy : ISpecialEnemy
    {
        private Vector2 _position;
        private Vector2 _direction;
        private bool _active;
        private Random _rnd;
        private int _size;
        private int _screenWidth;
        private int _screenHeight;
        public float Speed { get; set; }
        public int ScoreThreshold { get; private set; } = 5;

        public SpecialEnemy(Random rnd, int size, int screenWidth, int screenHeight)
        {
            _rnd = rnd;
            _size = size;
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
            _position = RandomPosition();
            _direction = new Vector2(1, 0);
            Speed = 10f; // Snížená rychlost fialového čtverečku (SpecialEnemy)
            _active = false;
        }

        private Vector2 RandomPosition()
        {
            int maxX = (_screenWidth - _size) / _size;
            int maxY = (_screenHeight - _size) / _size;
            return new Vector2(
                _rnd.Next(0, maxX + 1) * _size,
                _rnd.Next(0, maxY + 1) * _size
            );
        }

        public void Activate()
        {
            _active = true;
            _position = RandomPosition();
        }

        public void Update(float dt, Vector2 snakeHead, Vector2 snakeDir, ref bool isBlinking, ref bool isVisible, ref List<Vector2> snake, float knockbackDist, Game1 game)
        {
            if (!_active) return;

            Vector2 toSnake = snakeHead - _position;
            if (toSnake != Vector2.Zero)
                _direction = Vector2.Normalize(toSnake);

            _position += _direction * Speed * dt;

            _position.X = MathHelper.Clamp(_position.X, 0, _screenWidth - _size);
            _position.Y = MathHelper.Clamp(_position.Y, 0, _screenHeight - _size);

            Rectangle rectEnemy = new Rectangle((int)_position.X, (int)_position.Y, _size, _size);
            Rectangle rectHead = new Rectangle((int)snakeHead.X, (int)snakeHead.Y, _size, _size);

            if (rectEnemy.Intersects(rectHead) && !isBlinking)
            {
                if (snake.Count > 1) snake.RemoveAt(snake.Count - 1);
                Vector2 knockback = snake[0] - snakeDir * knockbackDist;
                knockback.X = MathHelper.Clamp(knockback.X, 0, _screenWidth - _size);
                knockback.Y = MathHelper.Clamp(knockback.Y, 0, _screenHeight - _size);
                snake[0] = knockback;
                isBlinking = true;
                isVisible = false;
            }
        }

        // Add the missing interface method implementation
        public void Update(float dt, Vector2 snakeHead, Vector2 snakeDir, ref bool isBlinking, ref bool isVisible, ref List<Vector2> snake, float knockbackDist)
        {
            // Call the main Update method, passing null for Game1 since the interface doesn't require it
            Update(dt, snakeHead, snakeDir, ref isBlinking, ref isVisible, ref snake, knockbackDist, null);
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
        {
            if (_active)
                spriteBatch.Draw(pixel, new Rectangle((int)_position.X, (int)_position.Y, _size, _size), Color.Purple);
        }
    }
}
