namespace TaoTie
{
    public class Player:Unit,IEntity<int>
    {
        #region override

        public override EntityType Type => EntityType.Player;

        public void Init(int configId)
        {
            ConfigId = configId;
            AddCommonUnitComponent();
        }

        public void Destroy()
        {
            ConfigId = default;
        }

        #endregion
        
    }
}