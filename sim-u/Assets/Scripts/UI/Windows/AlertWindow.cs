using UnityEngine;

namespace UI
{
    public abstract class AlertWindow : Window
    {
        private GameObject _fullScreenBlock;

        /// <summary>
        /// Alert windows block out all the UI behind them.
        /// </summary>
        /// <param name="data"></param>
        public override void Open(object data)
        {
            _fullScreenBlock = UIFactory.GenerateFullScreenFade("AlertWindowBlock", transform.parent);
            transform.SetParent(_fullScreenBlock.transform);
        }

        /// <summary>
        /// Close the alert window.
        /// This will destroy itself.
        /// </summary>
        public override void Close()
        {
            Destroy(_fullScreenBlock);
        }
    }
}
