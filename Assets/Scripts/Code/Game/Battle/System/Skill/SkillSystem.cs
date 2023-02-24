using System;
using System.Collections.Generic;

namespace TaoTie
{
    public class SkillSystem:IManager
    {
        public static SkillSystem Instance { get; private set; }

        private Dictionary<int, List<ISkillWatcher>> allSteps;
        
        private Dictionary<string, List<IConditionWatcher>> allWatchers;
        
        #region override

        public void Init()
        {
            Instance = this;
            {
                this.allSteps = new Dictionary<int, List<ISkillWatcher>>();

                var types = AttributeManager.Instance.GetTypes(TypeInfo<SkillWatcherAttribute>.Type);
                for (int j = 0; j < types.Count; j++)
                {
                    Type type = types[j];
                    object[] attrs = type.GetCustomAttributes(TypeInfo<SkillWatcherAttribute>.Type, false);

                    for (int i = 0; i < attrs.Length; i++)
                    {
                        SkillWatcherAttribute attribute = (SkillWatcherAttribute) attrs[i];
                        ISkillWatcher obj = (ISkillWatcher) Activator.CreateInstance(type);
                        if (!this.allSteps.ContainsKey(attribute.SkillStepType))
                        {
                            this.allSteps.Add(attribute.SkillStepType, new List<ISkillWatcher>());
                        }

                        this.allSteps[attribute.SkillStepType].Add(obj);
                    }
                }
            }
            {
                this.allWatchers = new Dictionary<string, List<IConditionWatcher>>();

                var types = AttributeManager.Instance.GetTypes(TypeInfo<ConditionWatcherAttribute>.Type);
                for (int j = 0; j < types.Count; j++)
                {
                    Type type = types[j];
                    object[] attrs = type.GetCustomAttributes(TypeInfo<ConditionWatcherAttribute>.Type, false);

                    for (int i = 0; i < attrs.Length; i++)
                    {
                        ConditionWatcherAttribute attribute = (ConditionWatcherAttribute) attrs[i];
                        IConditionWatcher obj = (IConditionWatcher) Activator.CreateInstance(type);
                        if (!this.allWatchers.ContainsKey(attribute.ConditionType))
                        {
                            this.allWatchers.Add(attribute.ConditionType, new List<IConditionWatcher>());
                        }

                        this.allWatchers[attribute.ConditionType].Add(obj);
                    }
                }
            }
        }


        public void Destroy()
        {
            Instance = null;
        }

        #endregion
        
        public void DoSkillStep(int type,SkillPara para)
        {
            List<ISkillWatcher> list;
            if (!this.allSteps.TryGetValue(type, out list))
            {
                return;
            }
            for (int i = 0; i < list.Count; i++)
            {
                ISkillWatcher numericWatcher = list[i];
                numericWatcher.Run(para);
            }
        }
        
        public bool CheckCondition(string type,SkillPara para)
        {
            List<IConditionWatcher> list;
            if (!this.allWatchers.TryGetValue(type, out list))
            {
                return false;
            }

            bool res = true;
            for (int i = 0; i < list.Count; i++)
            {
                IConditionWatcher numericWatcher = list[i];
                res &= numericWatcher.Run(para);
            }

            return res;
        }
    }
}