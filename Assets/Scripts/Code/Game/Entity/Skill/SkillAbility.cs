using System.Collections.Generic;

namespace TaoTie
{
    public class SkillAbility:Entity,IEntity<int>
    {
        public int ConfigId { get; private set; }

        public SkillConfig Config => SkillConfigCategory.Instance.Get(ConfigId);
        
        public long LastSpellTime;//上次施法时间
        public long LastSpellOverTime;//上次施法完成时间

        public ISkillStepManager StepManager => SkillStepLuaManager.Instance;
        #region override
        public override EntityType Type => EntityType.SkillAbility;

        public void Init(int p1)
        {
            ConfigId = p1;
        }

        public void Destroy()
        {
            ConfigId = default;
        }

        #endregion
        /// <summary>
        /// 是否可用
        /// </summary>
        /// <returns></returns>
        public bool CanUse()
        {
            return true;
        }
        public List<int> GetTimeLine(string group)
        {
            return StepManager.GetTimeLine(ConfigId, group);
        }
        
        public List<int> GetStepType(string group)
        {
            return StepManager.GetStepType(ConfigId, group);
        }
        
        public List<object[]> GetParas(string group)
        {
            return StepManager.GetParas(ConfigId, group);
        }
    }
}