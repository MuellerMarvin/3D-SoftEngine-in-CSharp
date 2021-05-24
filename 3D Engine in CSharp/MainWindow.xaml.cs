using EngineObjects;
using SharpDX;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace _3D_Engine_in_CSharp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Device Device;
        private Mesh Mesh;
        private readonly Camera Cam = new Camera();

        public MainWindow()
        {
            InitializeComponent();

            this.FrontBuffer.Stretch = Stretch.UniformToFill;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Define backbuffer resolution
            WriteableBitmap bmp = new WriteableBitmap(640, 480, 0, 0, PixelFormats.Bgra32, null);

            // create device
            this.Device = new Device(bmp);

            // assign buffer to image control
            this.FrontBuffer.Source = bmp;

            // mesh creation
            this.Mesh = new Mesh("Cube", new Vector3[8]);
            this.Mesh.Vertices[0] = new Vector3(-1, 1, 1);
            this.Mesh.Vertices[1] = new Vector3(1, 1, 1);
            this.Mesh.Vertices[2] = new Vector3(-1, -1, 1);
            this.Mesh.Vertices[3] = new Vector3(-1, -1, -1);
            this.Mesh.Vertices[4] = new Vector3(-1, 1, -1);
            this.Mesh.Vertices[5] = new Vector3(1, 1, -1);
            this.Mesh.Vertices[6] = new Vector3(1, -1, 1);
            this.Mesh.Vertices[7] = new Vector3(1, -1, -1);

            // camera creation
            this.Cam.Position = new Vector3(0, 0, 10);
            this.Cam.Target = Vector3.Zero;

            // Register to the rendering loop, now that the window is loaded
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }


        /// <summary>
        /// Render Loop
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            // Clear screen
            this.Device.Clear(0, 0, 0, 255);

            // rotate mesh slightly
            this.Mesh.Rotation = new Vector3(this.Mesh.Rotation.X + 0.01f, this.Mesh.Rotation.Y + 0.01f, this.Mesh.Rotation.Z);

            // Render the image
            this.Device.Render(this.Cam, this.Mesh);

            // display the image
            this.Device.Present();
        }
    }
}
