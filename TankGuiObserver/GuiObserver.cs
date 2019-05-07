using Sharpex2D;
using Sharpex2D.GameService;

namespace TankGuiObserver
{
    public class GuiObserver : Game
    {
        public override EngineConfiguration OnInitialize(LaunchParameters launchParameters)
        {
            return new EngineConfiguration(new Sharpex2D.Rendering.DirectX11.DirectXRenderDevice(), null);
            //return new EngineConfiguration(new GDIRenderDevice(), null);
        }

        public override void OnLoadContent()
        {
            GameComponentManager.Add(SceneManager);
            GameComponentManager.Add(FpsMeter.Instance);
            SceneManager.AddScene(new LoginScene());
            SceneManager.AddScene(new ObserverScene());
            SceneManager.ActiveScene = SceneManager.Get<LoginScene>();
        }
    }
}