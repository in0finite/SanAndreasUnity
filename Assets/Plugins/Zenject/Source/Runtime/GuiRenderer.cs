using UnityEngine;

namespace Zenject
{
    public class GuiRenderer : MonoBehaviour
    {
        private GuiRenderableManager _renderableManager;

        [Inject]
        private void Construct(GuiRenderableManager renderableManager)
        {
            _renderableManager = renderableManager;
        }

        public void OnGUI()
        {
            _renderableManager.OnGui();
        }
    }
}