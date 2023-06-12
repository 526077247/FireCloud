namespace TaoTie
{
    public class Skill:Unit,IEntity<int>
    {

        #region override
        public override EntityType Type => EntityType.Skill;
        public void Init(int configId)
        {
            ConfigId = configId;
            AddComponent<GameObjectHolderComponent>();
        }

        public void Destroy()
        {
            ConfigId = default;
        }

        #endregion

    }
}