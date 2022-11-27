using System;
using System.Collections.Generic;

namespace TaoTie
{
    public class SkillStepLuaManager:IManager,ISkillStepManager
    {
        public static SkillStepLuaManager Instance { get; private set; }
        private DictionaryComponent<string, List<int>> TimeLine;
        private DictionaryComponent<string, List<int>> StepType;
        private DictionaryComponent<string, List<object[]>> Params;
        #region override

        public void Init()
        {
            Instance = this;
            this.Params = DictionaryComponent<string, List<object[]>>.Create();
            this.StepType = DictionaryComponent<string, List<int>>.Create();
            this.TimeLine = DictionaryComponent<string, List<int>>.Create();
        }

        public void Destroy()
        {
            Instance = null;
            this.Params.Dispose();
            this.StepType.Dispose();
            this.TimeLine.Dispose();
        }

        #endregion
        
        public List<int> GetTimeLine(int skillId,string group)
        {
            return GetSkillStepTimeLine(skillId, group);
        }
        
        public List<int> GetStepType(int skillId,string group)
        {
            return GetSkillStepType(skillId, group);
        }
        
        public List<object[]> GetParas(int skillId,string group)
        {
            return GetSkillStepParas(skillId, group);
        }
        
        public List<int> GetSkillStepTimeLine(int skillId,string group)
        {
            var key = skillId + group;
            if (!this.TimeLine.ContainsKey(key))
            {
                List<int> timeline = this.TimeLine[key] = new List<int>();
                var steptype = this.StepType[key] = new List<int>();
                var paras = this.Params[key] = new List<object[]>();
                var func = XLuaManager.Instance.GetGlobalFunc("TryGetSkillConfig");
                var res = func.Call(skillId, group, timeline,steptype,paras);
                return timeline;
            }
            else
            {
                return this.TimeLine[key];
            }
        }
        
        public List<int> GetSkillStepType(int skillId,string group)
        {
            var key = skillId + group;
            if (!this.StepType.ContainsKey(key))
            {
                List<int> timeline = this.TimeLine[key] = new List<int>();
                var steptype = this.StepType[key] = new List<int>();
                var paras = this.Params[key] = new List<object[]>();
                var func = XLuaManager.Instance.GetGlobalFunc("TryGetSkillConfig");
                var res = func.Call(skillId, group, timeline,steptype,paras);
                
                return steptype;
            }
            else
            {
                return this.StepType[key];
            }
        }
        
        public List<object[]> GetSkillStepParas(int skillId,string group)
        {
            var key = skillId + group;
            if (!this.Params.ContainsKey(key))
            {
                List<int> timeline = this.TimeLine[key] = new List<int>();
                var steptype = this.StepType[key] = new List<int>();
                var paras = this.Params[key] = new List<object[]>();
                var func = XLuaManager.Instance.GetGlobalFunc("TryGetSkillConfig");
                var res = func.Call(skillId, group, timeline,steptype,paras);
                
                return paras;
            }
            else
            {
                return this.Params[key];
            }
            
        }
    }
}