namespace TaoTie
{
    public class Monster:Unit,IEntity<int>
    {
        #region override
        public override EntityType Type => EntityType.Monster;
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