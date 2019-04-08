using Common;
using GameData;

namespace Campus
{
    public class Building : Selectable
    {
        private BuildingData _building;

        protected override void Start()
        {
            base.Start();

            OnSelect = () => { GameLogger.Info("Building '{0}' selected!", _building.Name); };
        }

        public void Initialize(BuildingData building)
        {
            _building = building;
        }
    }
}
