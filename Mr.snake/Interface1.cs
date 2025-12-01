using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mr.snake; // Assuming Game1 is defined in the Mr.snake namespace

namespace Mr.snake
{
    public interface ISpecialEnemy
    {
        void Activate();
        void Update(float dt, Microsoft.Xna.Framework.Vector2 snakeHead, Microsoft.Xna.Framework.Vector2 snakeDir, ref bool isBlinking, ref bool isVisible, ref List<Microsoft.Xna.Framework.Vector2> snake, float knockbackDist);
        void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Microsoft.Xna.Framework.Graphics.Texture2D pixel);
        float Speed { get; set; }
        int ScoreThreshold { get; }
    }
}
