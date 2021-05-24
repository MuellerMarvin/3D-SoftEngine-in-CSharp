using System;
using SharpDX;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows;

namespace EngineObjects
{
    class Camera
    {
        public Vector3 Position { get; set; }
        public Vector3 Target { get; set; }
    }
    class Mesh
    {
        public string Name { get; set; }
        public Vector3[] Vertices { get; private set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }

        public Mesh(string name, Vector3[] vertices)
        {
            this.Name = name;
            this.Vertices = vertices;
        }

        /// <summary>
        /// Create a cube with all corners consisting of either a 1 or a -1, sidelength 2
        /// </summary>
        /// <returns></returns>
        public static Mesh CreateStandardCube()
        {
            return CreateStandardCube("Cube");
        }

        /// <summary>
        /// Create a cube with all corners consisting of either a 1 or a -1, sidelength 2
        /// </summary>
        /// <returns></returns>
        public static Mesh CreateStandardCube(string name)
        {
            Vector3[] vertices = new Vector3[8];
            vertices[0] = new Vector3(-1, 1, 1);
            vertices[1] = new Vector3(1, 1, 1);
            vertices[2] = new Vector3(-1, -1, 1);
            vertices[3] = new Vector3(-1, -1, -1);
            vertices[4] = new Vector3(-1, 1, -1);
            vertices[5] = new Vector3(1, 1, -1);
            vertices[6] = new Vector3(1, -1, 1);
            vertices[7] = new Vector3(1, -1, -1);
            return new Mesh(name, vertices);
        }
    }

    class Device
    {
        public byte[] BackBuffer;
        public WriteableBitmap Bitmap;

        public Device(WriteableBitmap writeableBitmap)
        {
            this.Bitmap = writeableBitmap;

            // Width * Height * RGBA
            this.BackBuffer = new byte[writeableBitmap.PixelWidth * writeableBitmap.PixelHeight * 4];
        }

        /// <summary>
        /// Clear screen with a specified RGBA color
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="a"></param>
        public void Clear(byte r, byte g, byte b, byte a)
        {
            for (int i = 0; i < this.BackBuffer.Length; i += 4)
            {
                BackBuffer[i] = b;
                BackBuffer[i + 1] = g;
                BackBuffer[i + 2] = r;
                BackBuffer[i + 3] = a;
            }
        }

        /// <summary>
        /// Flush back buffer to the front buffer
        /// </summary>
        public void Present()
        {
            try
            {
                // lock bitmap-backbuffer to apply changes
                this.Bitmap.Lock();

                unsafe
                {
                    // Write pixels to the Front Buffer
                    this.Bitmap.WritePixels(new Int32Rect(0,0, this.Bitmap.PixelWidth, this.Bitmap.PixelHeight), this.BackBuffer, this.Bitmap.PixelWidth * 4, 0);
                }
            }
            finally
            {
                // unlock bitmap-backbuffer to display changes
                this.Bitmap.Unlock();
            }
        }

        public void PutPixel(int x, int y, Color4 color)
        {
            int index = (x + y * this.Bitmap.PixelWidth) * 4;

            this.BackBuffer[index] = (byte)(color.Blue * 255);
            this.BackBuffer[index + 1] = (byte)(color.Green * 255);
            this.BackBuffer[index + 2] = (byte)(color.Red * 255);
            this.BackBuffer[index + 3] = (byte)(color.Alpha * 255);
        }

        public void DrawLine(Vector2 point1, Vector2 point2)
        {
            var distance = (point2 - point1).Length();

            if (distance < 2)
                return;


            // Find the middle point between first & second point
            Vector2 middlePoint = point1 + (point2 - point1) / 2;
            // We draw this point on screen
            DrawPoint(middlePoint);
            // Recursive algorithm launched between first & middle point
            // and between middle & second point
            DrawLine(point1, middlePoint);
            DrawLine(middlePoint, point2);
        }

        public Vector2 Project(Vector3 vector, Matrix transMat)
        {
            // Transform coordinate
            Vector3 point = Vector3.TransformCoordinate(vector, transMat);

            // Move center of coordinate system from the center of the screen to the top left
            // so that X: 0, Y: 0 is at the top left, not in the center

            float x = point.X * this.Bitmap.PixelWidth + this.Bitmap.PixelWidth / 2.0f;
            float y = point.Y * this.Bitmap.PixelHeight + this.Bitmap.PixelHeight / 2.0f;
            return (new Vector2(x, y));
        }

        public void DrawPoint(Vector2 point)
        {
            // Clip what's visible on the screen
            if (point.X >= 0 && point.Y >= 0 && point.X < this.Bitmap.PixelWidth && point.Y < this.Bitmap.PixelHeight)
            {
                // Draw the pixel as a yellow point
                PutPixel((int)point.X, (int)point.Y, new Color4(1, 1, 0, 1));
            }
        }

        public void Render(Camera camera, params Mesh[] meshes)
        {
            Matrix viewMatrix = Matrix.LookAtLH(camera.Position, camera.Target, Vector3.UnitY);
            Matrix projectionMatrix = Matrix.PerspectiveFovRH(0.78f, (float)this.Bitmap.PixelWidth / this.Bitmap.PixelHeight, 0.01f, 1.0f);

            foreach (Mesh mesh in meshes)
            {
                Matrix worldMatrix = Matrix.RotationYawPitchRoll(mesh.Rotation.Y, mesh.Rotation.X, mesh.Rotation.Z) * Matrix.Translation(mesh.Position);

                var transformMatrix = worldMatrix * viewMatrix * projectionMatrix;

                Vector2[] points = new Vector2[mesh.Vertices.Length];

                for(int i = 0; i < mesh.Vertices.Length; i++)
                {
                    points[i] = Project(mesh.Vertices[i], transformMatrix);
                    DrawPoint(points[i]);
                }

                for (int i = 0; i < points.Length; i++)
                {
                    for (int j = 0; j < points.Length; j++)
                    {
                        DrawLine(points[i], points[j]);
                    }
                }
            }
        }
    }
}
