﻿using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using OpenGL_Game.Components;
using OpenGL_Game.Systems;
using OpenGL_Game.Managers;
using OpenGL_Game.Objects;
using OpenTK.Graphics;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OpenGL_Game
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class MyGame : GameWindow
    {
        public Matrix4 view, projection;
        public Vector4 lightPosition = new Vector4(5, 1, 1, 0);
        private EntityManager entityManager;
        private SystemManager systemManager;
        private ModelManager modelManager;
        private ShaderManager shaderManager;
        private MaterialManager matManager;
        private TimerObject mTimer;
        private float mElaspedTime;
        
        public static MyGame gameInstance;

        public MyGame() 
            : base(1000, // Width
                800, // Height
                GraphicsMode.Default,
                "Component based tower",
                GameWindowFlags.Default,
                DisplayDevice.Default,
                3, // major
                3, // minor
                GraphicsContextFlags.ForwardCompatible)
        {
            gameInstance = this;
            mTimer = new TimerObject();
            entityManager = new EntityManager();
            systemManager = new SystemManager();
            matManager = new MaterialManager("Textures/TextureList.txt");
            shaderManager = new ShaderManager("Shaders/ShaderList.txt");
            modelManager = new ModelManager("Geometry/ModelList.txt");
        }
        /// <summary>
        /// reads in data from a json file. will have to update it so components are stored in an array on the JSON file to save the
        /// if statement
        /// </summary>
        private void CreateEntities()
        {
            Entity newEntity;
            using (StreamReader r = new StreamReader("Managers/SceneData.json"))
            {
                string json = r.ReadToEnd();
                List<SceneManager> gameData = JsonConvert.DeserializeObject<List<SceneManager>>(json);

                for (int i = 0; i < gameData.Count; i++)
                {
                    string name = gameData[i].Name;
                    string mat = gameData[i].Mat;
                    Vector3 pos = gameData[i].ConvertToVector(gameData[i].Location);
                    Vector3 rot = gameData[i].ConvertToVector(gameData[i].Rotation);

                    newEntity = new Entity(name);
                    if (name.Contains("SmallSphere"))
                    {
                        newEntity.AddComponent(new ComponentTransform(pos, new Vector3(1, 1, 1), rot, .3f));
                    }
                    else if (name.Contains("BigSphere"))
                    {
                        newEntity.AddComponent(new ComponentTransform(pos, new Vector3(1, 1, 1), rot, .5f));
                    }
                    else if (name.Contains("DoomSphere"))
                    {
                        newEntity.AddComponent(new ComponentTransform(pos, new Vector3(1, 1, 1), rot, .8f));
                    }
                    else
                    {
                        newEntity.AddComponent(new ComponentTransform(pos, new Vector3(1, 1, 1), rot, 1));
                    }
                    newEntity.AddComponent(new ComponentModel(modelManager.FindModel(name)));
                    newEntity.AddComponent(new ComponentMaterial(matManager.FindMaterial(mat)));

                    if (name.Contains("Cylinder"))
                    {
                        newEntity.AddComponent(new ComponentCollsion());
                    }
                    else if (name.Contains("Doom"))
                    {
                        newEntity.AddComponent(new ComponentDoomSphere());
                    }
                    else if (name.Contains("Sphere"))
                    {
                        if (!name.StartsWith("Doom"))
                        {
                            if (name.Contains("SmallSphere"))
                            {
                                newEntity.AddComponent(new ComponentPhysics(new Vector3(1, -1, 0), 4.8f, .3f));
                            }
                            else if (name.Contains("BigSphere"))
                            {
                                newEntity.AddComponent(new ComponentPhysics(new Vector3(1, -1, 0),7.8f, .5f));
                            }
                            newEntity.AddComponent(new ComponentPhysicsCollision());
                            newEntity.AddComponent(new ComponentWorldCollsion());
                            newEntity.AddComponent(new ComponentCylinderCollsion());
                            newEntity.AddComponent(new ComponentDoomCollsion());
                        }
                    }
                    if (name.Contains("Tower"))
                    {
                        newEntity.AddComponent(new ComponentBoxCollison());
                    }

                    entityManager.AddEntity(newEntity);
                }
            }
        }
        private void AddEntity()
        {
            Entity newEntity;
            newEntity = new Entity("SmallSphere");
            newEntity.AddComponent(new ComponentTransform(new Vector3(-0.6f, 4.0f, 0), new Vector3(1, 1, 1), new Vector3(0,0,0), .3f));
            newEntity.AddComponent(new ComponentModel(modelManager.FindModel("SmallSphere")));
            newEntity.AddComponent(new ComponentMaterial(matManager.FindMaterial("pbrShader")));
            newEntity.AddComponent(new ComponentPhysics(new Vector3(1, -1, 0), 4.8f, .3f));
            newEntity.AddComponent(new ComponentPhysicsCollision());
            newEntity.AddComponent(new ComponentWorldCollsion());
            newEntity.AddComponent(new ComponentCylinderCollsion());
            newEntity.AddComponent(new ComponentDoomCollsion());
            entityManager.AddEntity(newEntity);

            newEntity = new Entity("BigSphere");
            newEntity.AddComponent(new ComponentTransform(new Vector3(0.8f, 4.3f, 0.3f), new Vector3(1, 1, 1), new Vector3(0, 0, 0), .3f));
            newEntity.AddComponent(new ComponentModel(modelManager.FindModel("BigSphere")));
            newEntity.AddComponent(new ComponentMaterial(matManager.FindMaterial("pbrShader")));
            newEntity.AddComponent(new ComponentPhysics(new Vector3(1, -1, 0), 4.8f, .3f));
            newEntity.AddComponent(new ComponentPhysicsCollision());
            newEntity.AddComponent(new ComponentWorldCollsion());
            newEntity.AddComponent(new ComponentCylinderCollsion());
            newEntity.AddComponent(new ComponentDoomCollsion());
            entityManager.AddEntity(newEntity);


        }
        private void CreateSystems()
        {
            ISystem newSystem;

            newSystem = new SystemRender(shaderManager.FindShader("pbrShader"));
            systemManager.AddSystem(newSystem);

            newSystem = new SystemPhysics(entityManager.Entities());
            systemManager.AddSystem(newSystem);

            newSystem = new SystemBox(entityManager.Entities());
            systemManager.AddSystem(newSystem);

            newSystem = new SystemDoomSphere(entityManager.Entities());
            systemManager.AddSystem(newSystem);

            newSystem = new SystemCollsion(entityManager.Entities());
            systemManager.AddSystem(newSystem);

            systemManager.StartTimer();
        }
        /// <summary>
        /// Allows the game to setup the environment and matrices.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GL.Enable(EnableCap.DepthTest);
            GL.ClearColor(Color4.CornflowerBlue);
            GL.Enable(EnableCap.CullFace);
            view = Matrix4.CreateTranslation(1, -1, -12.5f);
           // Vector4.Transform(lightPosition, view);
            projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45), 800f / 480f, 0.01f, 100f);
            
            modelManager.BindGeometetry(shaderManager);
            CreateSystems();
            CreateEntities();
            mTimer.StartTimer();
           
        }
        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindVertexArray(0);

            matManager.DeleteTextures();
            shaderManager.DeleteShaders();
            modelManager.DeleteBuffers();
        }
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="e">Provides a snapshot of timing values.</param>
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            if (GamePad.GetState(1).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Key.Escape))
                Exit();
            mElaspedTime += mTimer.GetElapsedSeconds();
            if (mElaspedTime > 3)
            {
                mElaspedTime = 0;
                AddEntity();
            }
           
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="e">Provides a snapshot of timing values.</param>
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Viewport(0, 0, Width, Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            systemManager.ActionSystems(entityManager);

            GL.Flush();
            SwapBuffers();
        }

    }
}
