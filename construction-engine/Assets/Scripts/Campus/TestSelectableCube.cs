using Common;

namespace Campus
{
    public class TestSelectableCube : Selectable
    {
        protected override void Start()
        {
            base.Start();
            OnSelect = () => { GameLogger.Info("Selected Cube!"); };
            OnDeselect = () => { GameLogger.Info("Deselected Cube!"); };
        }
    }
}
