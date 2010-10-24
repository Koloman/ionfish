﻿using System.Drawing;
using Core.Commands;
using Graphics;
using Graphics.Cameras;
using Graphics.Materials;
using Graphics.Primitives;
using Input;
using Math;
using SlimDX;
using SlimDX.Direct2D;
using SlimDX.Direct3D10;
using SlimDX.DirectWrite;
using SlimDX.DXGI;
using Matrix = Math.Matrix;
using Vector4 = Math.Vector4;

namespace Sandbox
{
    public class WriteDemo : Game
    {
        private Keyboard mKeyboard;
        private InputCommandBinder mInputCommandBinder;
        private Camera mCamera;
        private Material mMaterial;
        private MeshMaterialBinding mQuadBinding;
        private Texture mTexture;
        private TextFormat mTextFormat;
        private RenderTarget mWindowRenderTarget;
        private SolidColorBrush mBrush;

        private const string ESCAPE = "escape";
        private const string TAKE_SCREENSHOT = "take screenshot";
        private const string MOVE_FORWARD = "move forward";
        private const string MOVE_BACKWARD = "move backward";
        private const string STRAFE_LEFT = "strafe left";
        private const string STRAFE_RIGHT = "strafe right";
        private const string UP = "up";
        private const string DOWN = "down";

        protected override void Initialize()
        {
            mMaterial = new Material("texture.fx", Window.Device);
            mQuadBinding = new MeshMaterialBinding(Window.Device, mMaterial, new Quad(Window.Device, new Vector4(0.6f, 0.6f, 0.6f, 0)));
            var description = new Texture2DDescription
            {
                ArraySize = 1,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = Format.R8G8B8A8_UNorm,
                Height = 1024,
                Width = 1024,
                MipLevels = 1,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                OptionFlags = ResourceOptionFlags.None
            };
            var texture = new Texture2D(Window.Device, description);
            mTexture = new Texture(Window.Device, texture);
            var surface = texture.AsSurface();
            var factory2D = new SlimDX.Direct2D.Factory(SlimDX.Direct2D.FactoryType.SingleThreaded);
            var renderTargetProperties = new RenderTargetProperties
            {
                PixelFormat = new PixelFormat(Format.Unknown, AlphaMode.Premultiplied),
                Usage = RenderTargetUsage.None,
                HorizontalDpi = 96,
                VerticalDpi = 96,
                MinimumFeatureLevel = FeatureLevel.Direct3D10,
                Type = RenderTargetType.Default
            };
            mWindowRenderTarget = RenderTarget.FromDXGI(factory2D, surface,
                renderTargetProperties);

            mBrush = new SolidColorBrush(mWindowRenderTarget, new Color4(1, 1, 1));

            var factory = new SlimDX.DirectWrite.Factory(SlimDX.DirectWrite.FactoryType.Shared);
            mTextFormat = factory.CreateTextFormat("Consola", SlimDX.DirectWrite.FontWeight.Normal,
                SlimDX.DirectWrite.FontStyle.Normal, FontStretch.Normal, 100, "en-us");
            mTextFormat.TextAlignment = TextAlignment.Center;
            mTextFormat.ParagraphAlignment = ParagraphAlignment.Center;

            mKeyboard = new Keyboard();

            var stand = new OrbitingStand {Radius = 7, Azimuth = Constants.HALF_PI};
            var lens = new PerspectiveProjectionLens();
            mCamera = new Camera(stand, lens);

            var commands = new CommandManager();
            commands.Add(ESCAPE, Exit);
            commands.Add(TAKE_SCREENSHOT, Window.TakeScreenshot);

            mInputCommandBinder = new InputCommandBinder(commands, mKeyboard);
            mInputCommandBinder.Bind(Button.Escape, ESCAPE);
            mInputCommandBinder.Bind(Button.PrintScreen, TAKE_SCREENSHOT);

            commands.Add(MOVE_BACKWARD, () => stand.Radius += Frametime);
            commands.Add(MOVE_FORWARD, () => stand.Radius -= Frametime);
            commands.Add(STRAFE_RIGHT, () => stand.Azimuth -= Frametime);
            commands.Add(STRAFE_LEFT, () => stand.Azimuth += Frametime);
            commands.Add(UP, () => stand.Declination += Frametime);
            commands.Add(DOWN, () => stand.Declination -= Frametime);

            mInputCommandBinder.Bind(Button.W, MOVE_FORWARD);
            mInputCommandBinder.Bind(Button.S, MOVE_BACKWARD);
            mInputCommandBinder.Bind(Button.D, STRAFE_RIGHT);
            mInputCommandBinder.Bind(Button.A, STRAFE_LEFT);
            mInputCommandBinder.Bind(Button.R, UP);
            mInputCommandBinder.Bind(Button.F, DOWN);
        }

        protected override void OnFrame()
        {
            mKeyboard.Update();
            mInputCommandBinder.Update();

            RenderQuad();
        }

        void RenderQuad()
        {
            var world = Matrix.Identity;
            mMaterial.SetWorldViewProjectionMatrix(mCamera.ViewProjectionMatrix * world);
            mMaterial.SetTexture(mTexture);

            mWindowRenderTarget.BeginDraw();
            mWindowRenderTarget.Transform = Matrix3x2.Identity;
            mWindowRenderTarget.Clear(new Color4(0, 0, 0, 0));
            var layoutRectangle = new RectangleF(0, 0, 1024, 1024);
            mWindowRenderTarget.DrawText(string.Format("{0:0000} fps", 1.0 / Frametime),
                mTextFormat, layoutRectangle, mBrush);
            mWindowRenderTarget.EndDraw();

            mQuadBinding.Draw();
        }

        public override void Dispose()
        {
            base.Dispose();
            mTexture.Dispose();
        }       
    }
}