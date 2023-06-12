using System.Collections.Generic;

namespace TaoTie
{
    public class SkillHolderComponent: Component,IComponent,IComponent<List<int>>
    {
        public DictionaryComponent<int, long> IdSkillMap;

        #region override

        public void Init()
        {
            IdSkillMap = DictionaryComponent<int, long>.Create();
        }

        public void Init(List<int> p1)
        {
            Init();
            for (int i = 0; i < p1.Count; i++)
            {
                AddSkill(p1[i]);
            }
        }

        public void Destroy()
        {
            foreach ((_,var id) in IdSkillMap)
            {
                parent.Parent.Remove(id);
            }
            IdSkillMap.Dispose();
            IdSkillMap = null;
        }

        #endregion
        
        /// <summary>
        /// 添加技能
        /// </summary>
        /// <param name="configId"></param>
        /// <returns></returns>
        public SkillAbility AddSkill(int configId)
        {
            if (!this.IdSkillMap.ContainsKey(configId))
            {
                var skill = parent.Parent.CreateEntity<SkillAbility, int>(configId);
                this.IdSkillMap.Add(configId, skill.Id);
            }
            return parent.Parent.Get<SkillAbility>(this.IdSkillMap[configId]);
        }
        
        public bool TryGetSkillAbility(int configId,out SkillAbility skill)
        {
            if (this.IdSkillMap.ContainsKey(configId))
            {
                skill = parent.Parent.Get<SkillAbility>(this.IdSkillMap[configId]);
                return true;
            }
            skill = null;
            return false;
        }

        public SkillAbility GetSkill(int configId)
        {
            TryGetSkillAbility(configId, out var res);
            return res;
        }
    }
}