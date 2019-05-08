using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sharpex2D;
using Sharpex2D.Rendering;
using Sharpex2D.Surface;

namespace TankGuiObserver
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new MainForm());

            var renderTarget = RenderTarget.Create();
            renderTarget.Window.Title = $"TankGuiObserver {Application.ProductVersion}";
            renderTarget.Window.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            renderTarget.Window.SurfaceLayout = new SurfaceLayout(true, false, true);
            renderTarget.Window.SurfaceStyle = SurfaceStyle.Fullscreen;

            SGL.Initialize(new Configurator(
                new BackBuffer(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height),
                new GuiObserver(), renderTarget));

        }
    }
}
