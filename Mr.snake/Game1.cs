using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Mr.snake
{   
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D _pixel;
        private Vector2 _redPos;
        private List<Vector2> _snake = new List<Vector2>();
        private Vector2 _dir = new Vector2(1, 0); // výchozí směr doprava
        private const int velikostctverecku = 40;
        private const float rychlost = 200f;

        private bool _wasColliding = false;
        private Random _rnd = new Random();

        private KeyboardState _previousKeyboardState; // Přidáno pro sledování předchozího stavu klávesnice

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
            _snake.Add(new Vector2(300, 100)); // startovní pozice hlavy hada
            _redPos = RandomRedPosition();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _pixel = new Texture2D(GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        private Vector2 RandomRedPosition()
        {
            int maxX = (_graphics.PreferredBackBufferWidth - velikostctverecku) / velikostctverecku;
            int maxY = (_graphics.PreferredBackBufferHeight - velikostctverecku) / velikostctverecku;
            return new Vector2(
                _rnd.Next(0, maxX + 1) * velikostctverecku,
                _rnd.Next(0, maxY + 1) * velikostctverecku
            );
        }

        protected override void Update(GameTime gameTime)
        {
            var kstate = Keyboard.GetState();
            if (kstate.IsKeyDown(Keys.Escape))
                Exit();

            // --- Ovládání šipek ---
            Vector2 move = Vector2.Zero;
            if (kstate.IsKeyDown(Keys.Up)) move = new Vector2(0, -1);
            else if (kstate.IsKeyDown(Keys.Down)) move = new Vector2(0, 1);
            else if (kstate.IsKeyDown(Keys.Left)) move = new Vector2(-1, 0);
            else if (kstate.IsKeyDown(Keys.Right)) move = new Vector2(1, 0);

            // Pokud je stisknutá šipka, nastav směr
            if (move != Vector2.Zero)
                _dir = move;

            // Pohyb hada pouze pokud je nějaká šipka stisknutá
            if (move != Vector2.Zero)
            {
                float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
                Vector2 newHead = _snake[0] + _dir * rychlost * delta;

                // Okraje obrazovky
                newHead.X = MathHelper.Clamp(newHead.X, 0, _graphics.PreferredBackBufferWidth - velikostctverecku);
                newHead.Y = MathHelper.Clamp(newHead.Y, 0, _graphics.PreferredBackBufferHeight - velikostctverecku);

                // Posuň tělo
                for (int i = _snake.Count - 1; i > 0; i--)
                    _snake[i] = _snake[i - 1];
                _snake[0] = newHead;

                // --- Kolize s červeným čtverečkem ---
                Rectangle rectRed = new Rectangle((int)_redPos.X, (int)_redPos.Y, velikostctverecku, velikostctverecku);
                Rectangle rectHead = new Rectangle((int)_snake[0].X, (int)_snake[0].Y, velikostctverecku, velikostctverecku);

                bool isColliding = rectRed.Intersects(rectHead);

                if (isColliding && !_wasColliding)
                {
                    // Prodloužení hada o více segmentů (např. 3)
                    for (int i = 0; i < 3; i++)
                        _snake.Add(_snake[_snake.Count - 1]);
                    // Nová pozice červeného čtverečku
                    _redPos = RandomRedPosition();
                }
                _wasColliding = isColliding;
            }
            else
            {
                _wasColliding = false; // Reset kolize, když se had nehýbe
            }

            _previousKeyboardState = kstate;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin();

            // Červený čtvereček
            _spriteBatch.Draw(
                _pixel,
                new Rectangle((int)_redPos.X, (int)_redPos.Y, velikostctverecku, velikostctverecku),
                Color.Red);

            // Had (zelený čtvereček a jeho tělo)
            for (int i = 0; i < _snake.Count; i++)
            {
                _spriteBatch.Draw(
                    _pixel,
                    new Rectangle((int)_snake[i].X, (int)_snake[i].Y, velikostctverecku, velikostctverecku),
                    Color.Green);
            }

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}