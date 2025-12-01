using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Mr_snake  
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D _pixel;

        // Had
        private List<Vector2> _snake = new List<Vector2>();
        private Vector2 _dir = new Vector2(1, 0);
        private const int size = 40;
        private const float speed = 200f;

        // Jídlo
        private Vector2 _redPos;
        private Vector2 _yellowPos;
        private float _yellowTimer = 0f;
        private const float _yellowMoveInterval = 10f;

        // Překážky
        private List<Vector2> _obstacles = new List<Vector2>();
        private List<Vector2> _obstacleDirs = new List<Vector2>();
        private const int obstacleCount = 5;
        private float _obstacleSpeed = 100f;

        // Denní cyklus
        private float _dayTimer = 0f;
        private const float _dayCycleInterval = 10f;
        private Color _bgColor = Color.Yellow;
        private Color _targetBgColor = Color.Goldenrod;

        // Štít
        private Vector2 _shieldPos;
        private bool _shieldActive = false;
        private float _shieldSpawnTimer = 0f;
        private float _shieldActiveTimer = 0f;
        private const float _shieldSpawnInterval = 15f;
        private const float _shieldEffectDuration = 10f;

        // Hra
        private int _score = 0;
        private bool _wasColliding = false;
        private bool _isBlinking = false;
        private float _blinkTimer = 0f;
        private float _blinkDuration = 0.5f;
        private float _blinkInterval = 0.1f;
        private float _blinkIntervalTimer = 0f;
        private bool _isVisible = true;
        private float _knockbackDist = 60f;

        private Random _rnd = new Random();

        // Speciální nepřítel jako objekt
        private SpecialEnemy _specialEnemy;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 600;
            _graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            _snake.Clear();
            _snake.Add(new Vector2(300, 100));
            _dir = new Vector2(1, 0);

            _redPos = RandomPosition();
            _yellowPos = RandomPosition();

            _obstacles.Clear();
            _obstacleDirs.Clear();
            for (int i = 0; i < obstacleCount; i++)
            {
                _obstacles.Add(RandomPosition());
                Vector2 dir;
                do dir = new Vector2(_rnd.Next(-1, 2), _rnd.Next(-1, 2));
                while (dir == Vector2.Zero);
                _obstacleDirs.Add(dir);
            }

            _specialEnemy = new SpecialEnemy(_rnd, size, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);

            _isBlinking = false;
            _isVisible = true;

            _score = 0;
            _yellowTimer = 0f;

            _dayTimer = 0f;
            _bgColor = Color.Yellow;
            _targetBgColor = Color.Goldenrod;

            _shieldActive = false;
            _shieldSpawnTimer = 0f;
            _shieldActiveTimer = 0f;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _pixel = new Texture2D(GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        private Vector2 RandomPosition()
        {
            int maxX = (_graphics.PreferredBackBufferWidth - size) / size;
            int maxY = (_graphics.PreferredBackBufferHeight - size) / size;
            return new Vector2(
                _rnd.Next(0, maxX + 1) * size,
                _rnd.Next(0, maxY + 1) * size
            );
        }

        protected override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var kstate = Keyboard.GetState();
            if (kstate.IsKeyDown(Keys.Escape))
                Exit();

            // Blikání hada
            if (_isBlinking)
            {
                _blinkTimer += dt;
                _blinkIntervalTimer += dt;
                if (_blinkIntervalTimer >= _blinkInterval)
                {
                    _isVisible = !_isVisible;
                    _blinkIntervalTimer = 0f;
                }
                if (_blinkTimer >= _blinkDuration)
                {
                    _isBlinking = false;
                    _isVisible = true;
                }
            }

            // Ovládání hada
            Vector2 move = Vector2.Zero;
            if (kstate.IsKeyDown(Keys.Up)) move = new Vector2(0, -1);
            else if (kstate.IsKeyDown(Keys.Down)) move = new Vector2(0, 1);
            else if (kstate.IsKeyDown(Keys.Left)) move = new Vector2(-1, 0);
            else if (kstate.IsKeyDown(Keys.Right)) move = new Vector2(1, 0);

            if (move != Vector2.Zero)
                _dir = move;

            if (move != Vector2.Zero)
            {
                Vector2 newHead = _snake[0] + _dir * speed * dt;
                newHead.X = MathHelper.Clamp(newHead.X, 0, _graphics.PreferredBackBufferWidth - size);
                newHead.Y = MathHelper.Clamp(newHead.Y, 0, _graphics.PreferredBackBufferHeight - size);

                for (int i = _snake.Count - 1; i > 0; i--)
                    _snake[i] = _snake[i - 1];
                _snake[0] = newHead;

                // Červené jídlo
                Rectangle rectRed = new Rectangle((int)_redPos.X, (int)_redPos.Y, size, size);
                Rectangle rectHead = new Rectangle((int)_snake[0].X, (int)_snake[0].Y, size, size);
                bool redCollision = rectRed.Intersects(rectHead);
                if (redCollision && !_wasColliding)
                {
                    for (int i = 0; i < 3; i++)
                        _snake.Add(_snake[_snake.Count - 1]);
                    _redPos = RandomPosition();
                    _score++;

                    if (_score >= _specialEnemy.ScoreThreshold)
                    {
                        _specialEnemy.Activate();
                    }
                }
                _wasColliding = redCollision;

                // Žluté jídlo
                Rectangle rectYellow = new Rectangle((int)_yellowPos.X, (int)_yellowPos.Y, size, size);
                if (rectYellow.Intersects(rectHead))
                {
                    for (int i = 0; i < 2; i++)
                        _snake.Add(_snake[_snake.Count - 1]);
                    _yellowPos = RandomPosition();
                    _yellowTimer = 0f;
                }

                // Kolize s překážkami
                for (int i = 0; i < _obstacles.Count; i++)
                {
                    Rectangle rectObs = new Rectangle((int)_obstacles[i].X, (int)_obstacles[i].Y, size, size);
                    if (rectObs.Intersects(rectHead) && !_isBlinking && !_shieldActive)
                    {
                        if (_snake.Count > 1) _snake.RemoveAt(_snake.Count - 1);
                        Vector2 knockback = _snake[0] - _dir * _knockbackDist;
                        knockback.X = MathHelper.Clamp(knockback.X, 0, _graphics.PreferredBackBufferWidth - size);
                        knockback.Y = MathHelper.Clamp(knockback.Y, 0, _graphics.PreferredBackBufferHeight - size);
                        _snake[0] = knockback;
                        _isBlinking = true;
                        _blinkTimer = 0f;
                        _blinkIntervalTimer = 0f;
                        _isVisible = false;
                    }
                }
            }
            else
            {
                _wasColliding = false;
            }

            // Pohyb překážek
            for (int i = 0; i < _obstacles.Count; i++)
            {
                Vector2 pos = _obstacles[i];
                Vector2 dir = _obstacleDirs[i];

                pos += dir * _obstacleSpeed * dt;

                if (pos.X < 0 || pos.X > _graphics.PreferredBackBufferWidth - size)
                    dir.X *= -1;
                if (pos.Y < 0 || pos.Y > _graphics.PreferredBackBufferHeight - size)
                    dir.Y *= -1;

                pos.X = MathHelper.Clamp(pos.X, 0, _graphics.PreferredBackBufferWidth - size);
                pos.Y = MathHelper.Clamp(pos.Y, 0, _graphics.PreferredBackBufferHeight - size);

                _obstacles[i] = pos;
                _obstacleDirs[i] = dir;
            }

            // Speciální nepřítel
            _specialEnemy.Update(dt, _snake[0], _dir, ref _isBlinking, ref _isVisible, ref _snake, _knockbackDist, this);

            // Žluté jídlo
            _yellowTimer += dt;
            if (_yellowTimer >= _yellowMoveInterval)
            {
                _yellowPos = RandomPosition();
                _yellowTimer = 0f;
            }

            // Denní cyklus
            _dayTimer += dt;
            if (_dayTimer >= _dayCycleInterval)
            {
                Color temp = _bgColor;
                _bgColor = _targetBgColor;
                _targetBgColor = temp;
                _dayTimer = 0f;

                // Změna rychlosti nepřátel podle dne/noci
                _obstacleSpeed = (_bgColor == Color.Yellow) ? 100f : 180f;
                _specialEnemy.Speed = (_bgColor == Color.Yellow) ? 50f : 70f; // Snížená rychlost fialového enemy
            }

            // Štít
            _shieldSpawnTimer += dt;
            if (!_shieldActive && _shieldSpawnTimer >= _shieldSpawnInterval)
            {
                _shieldPos = RandomPosition();
                _shieldActive = true;
                _shieldActiveTimer = 0f;
                _shieldSpawnTimer = 0f;
            }
            if (_shieldActive)
            {
                _shieldActiveTimer += dt;
                if (_shieldActiveTimer >= _shieldEffectDuration)
                    _shieldActive = false;
            }

            // Konec hry pokud had zmizí
            if (_snake.Count <= 0) Exit();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(_bgColor);
            _spriteBatch.Begin();

            // Červené jídlo
            _spriteBatch.Draw(_pixel, new Rectangle((int)_redPos.X, (int)_redPos.Y, size, size), Color.Red);
            // Žluté jídlo
            _spriteBatch.Draw(_pixel, new Rectangle((int)_yellowPos.X, (int)_yellowPos.Y, size, size), Color.Yellow);

            // Had
            if (_isVisible)
                foreach (var s in _snake)
                    _spriteBatch.Draw(_pixel, new Rectangle((int)s.X, (int)s.Y, size, size), Color.Green);

            // Překážky
            foreach (var o in _obstacles)
                _spriteBatch.Draw(_pixel, new Rectangle((int)o.X, (int)o.Y, size, size), Color.Blue);

            // Speciální nepřítel
            _specialEnemy.Draw(_spriteBatch, _pixel);

            // Štít
            if (_shieldActive)
                _spriteBatch.Draw(_pixel, new Rectangle((int)_shieldPos.X, (int)_shieldPos.Y, size, size), Color.LightCyan * 0.5f);

            _spriteBatch.End();
        }
    }
}
