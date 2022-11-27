using System;
using System.Collections.Generic;

namespace TaoTie
{
    public class SkillStepManager:IManager,ISkillStepManager
    {
        public static SkillStepManager Instance { get; private set; }
        private DictionaryComponent<int, List<int>> TimeLine;
        private DictionaryComponent<int, List<int>> StepType;
        private DictionaryComponent<int, List<object[]>> Params;
        #region override

        public void Init()
        {
            Instance = this;
            this.Params = DictionaryComponent<int, List<object[]>>.Create();
            this.StepType = DictionaryComponent<int, List<int>>.Create();
            this.TimeLine = DictionaryComponent<int, List<int>>.Create();
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
            var conf = SkillStepConfigCategory.Instance.GetSkillGroup(skillId, group);
            return GetSkillStepTimeLine(conf.Id);
        }
        
        public List<int> GetStepType(int skillId,string group)
        {
            var conf = SkillStepConfigCategory.Instance.GetSkillGroup(skillId, group);
            return GetSkillStepType(conf.Id);
        }
        
        public List<object[]> GetParas(int skillId,string group)
        {
            var conf = SkillStepConfigCategory.Instance.GetSkillGroup(skillId, group);
            return GetSkillStepParas(conf.Id);
        }
        
        public List<int> GetSkillStepTimeLine(int configId)
        {
            if (!this.TimeLine.ContainsKey(configId))
            {
                List<int> timeline = this.TimeLine[configId] = new List<int>();
                SkillStepConfig config = SkillStepConfigCategory.Instance.Get(configId);
                var type = config.GetType();
                for (int i = 0; i < config.ParaCount; i++)
                {
                    object timelineItem = null;
                    try
                    {
                        var triggerTime = type.GetProperty("TriggerTime" + i);
                        timelineItem = triggerTime.GetValue(config);
                        if(timelineItem!=null)
                            timeline.Add((int)timelineItem);
                        else
                            timeline.Add(0);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(configId+" Load Fail! at "+i+" values:"+timelineItem+"\r\n"+ex);
                    }
                }
                return timeline;
            }
            else
            {
                return this.TimeLine[configId];
            }
        }
        
        public List<int> GetSkillStepType(int configId)
        {
            if (!this.StepType.ContainsKey(configId))
            {
                var steptype = this.StepType[configId] = new List<int>();
                SkillStepConfig config = SkillStepConfigCategory.Instance.Get(configId);
                var type = config.GetType();
                for (int i = 0; i < config.ParaCount; i++)
                {
                    object steptypeItem = null;
                    try
                    {
                        var stepStyle = type.GetProperty("StepStyle" + i);
                        steptypeItem = stepStyle.GetValue(config);
                        if(steptypeItem!=null)
                            steptype.Add((int)steptypeItem);
                        else
                            steptype.Add(0);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(configId+" Load Fail! at "+i+" values:"+" "+steptypeItem+"\r\n"+ex);
                    }
                }

                return steptype;
            }
            else
            {
                return this.StepType[configId];
            }
        }
        
        public List<object[]> GetSkillStepParas(int configId)
        {
            if (!this.Params.ContainsKey(configId))
            {
                var paras = this.Params[configId] = new List<object[]>();
                SkillStepConfig config = SkillStepConfigCategory.Instance.Get(configId);
                var type = config.GetType();
                for (int i = 0; i < config.ParaCount; i++)
                {
                    object stepParameterItem = null;
                    try
                    {
                        var stepParameter = type.GetProperty("StepParameter" + i);
                        stepParameterItem = stepParameter.GetValue(config);
                        if (stepParameterItem != null)
                        {
                            var list = (string[]) stepParameterItem;
                            object[] temp = new object[list.Length];
                            for (int j = 0; j < temp.Length; j++)
                            {
                                temp[j] = list[j];
                            }
                            paras.Add(temp);
                        }
                        else
                            paras.Add(new object[0]);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(configId+" Load Fail! at "+i+" values:"+stepParameterItem+"\r\n"+ex);
                    }
                }

                return paras;
            }
            else
            {
                return this.Params[configId];
            }
            
        }
    }
}