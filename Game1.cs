using Car_Genetic_Algorithm.Graphics;
using Car_Genetic_Algorithm.Utlities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Car_Genetic_Algorithm.Evolution;
using Exception = System.Exception;

namespace Car_Genetic_Algorithm
{
    public class Game1 : Game
    {
        private Evolution.Evolution evolution;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private PrimitiveBatch _primitiveBatch;
        private FrameCounter _frameCounter = new FrameCounter();

        private int width = 1000;
        private int height = 1000;

        private Texture2D redcar;
        private SpriteFont font;

        private Course course;

        private Vector2 origin = new Vector2(20, 10);

        private int initializationIterations = 0;
        private bool paused = false;

        private int total_runs = 100;
        private int generations_per_run = 45;
        private int current_GA= 0;
        

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            IsFixedTimeStep = false;
            _graphics.SynchronizeWithVerticalRetrace = false;
        }

        protected override void Initialize()
        {
            Console.WriteLine("Running GA " + (current_GA + 1));
            Console.Write("GA " + (current_GA + 1) + " times|distances: ");
            course = new Course(100);
            Random rand = new Random();
            evolution = new Evolution.Evolution(500, rand);
            if (current_GA == 0)
            {
                _graphics.PreferredBackBufferWidth = width;
                _graphics.PreferredBackBufferHeight = height;
                _graphics.ApplyChanges();

                _primitiveBatch = new PrimitiveBatch(this);
                _primitiveBatch.Initialize();

                //EvolutionGrouping groupingTest = new EvolutionGrouping();
                //evolution = groupingTest.RunEvolutionGrouping(1);
                //Debug.WriteLine(evolution.fastestTime);
                //evolution.RunGenerations(initializationIterations, false);
                //Debug.WriteLine(evolution.fastestTime);
                base.Initialize();
            }
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            redcar = Content.Load<Texture2D>("car2");
            font = Content.Load<SpriteFont>("galleryFont");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            evolution.UpdateCars(true);
            
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Green);
            _primitiveBatch.Primitives.Clear();

            _spriteBatch.Begin();

            for (int i = 0; i < evolution.course.leftWall.Count - 2; i++)
            {
                List<Vector2> vertices = new List<Vector2> { evolution.course.leftWall[i] - evolution.furthest.Position, evolution.course.leftWall[i + 1] - evolution.furthest.Position, evolution.course.rightWall[i + 1] - evolution.furthest.Position, evolution.course.rightWall[i] - evolution.furthest.Position };
                if (Utility.PolygonInRange(vertices, width, height))
                    continue;
                Polygon polygon = new Polygon(vertices, new Color(45, 45, 45));
                _primitiveBatch.Primitives.Add(polygon);

                Primitives2D.DrawLine(_spriteBatch, evolution.course.leftWall[i] - evolution.furthest.Position, evolution.course.leftWall[i + 1] - evolution.furthest.Position, new Color(100, 100, 100), 3);
                Primitives2D.DrawLine(_spriteBatch, evolution.course.rightWall[i] - evolution.furthest.Position, evolution.course.rightWall[i + 1] - evolution.furthest.Position, new Color(100, 100, 100), 3);
            }

            _primitiveBatch.Draw(gameTime);

            for (int i=1; i< evolution.cars.Count(); i++)
                _spriteBatch.Draw(redcar, evolution.cars[i].Position + new System.Numerics.Vector2(500, 500) - evolution.furthest.Position, null, Color.White, evolution.cars[i].Rotation, origin, 1f, SpriteEffects.None, 0f);
            _spriteBatch.Draw(redcar, new System.Numerics.Vector2(500, 500), null, Color.White, evolution.furthest.Rotation, origin, 1f, SpriteEffects.None, 0f);

            _spriteBatch.DrawString(font, "Generations: " + evolution.generations, new Vector2(5, 5), Color.Black);
            _spriteBatch.DrawString(font, "Speed: " + evolution.furthest.Velocity.Length(), new Vector2(5, 30), Color.Black);
            _spriteBatch.DrawString(font, "Remaining: " + evolution.carsLeft, new Vector2(5, 55), Color.Black);
            _spriteBatch.DrawString(font, "Gas/brake: " + evolution.furthest.brake, new Vector2(5, 80), Color.Black);
            _spriteBatch.DrawString(font, "Fastest time: " + evolution.fastestTime, new Vector2(5, 105), Color.Black);
            _spriteBatch.DrawString(font, "Location input: " + (evolution.furthest.Position.Y + 500) / (course.length * course.interval), new Vector2(5, 130), Color.Black);


            _spriteBatch.End();

            base.Draw(gameTime);

            if (!paused)
                Thread.Sleep(5000);
            paused = true;
        }
    }
}
