using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class SkillStepPara
    {
        public int Index;
        public object[] Paras;
        public int Interval;
        public int Count;//作用单位数
    }
    /// <summary>
    /// 其他地方不要持有SkillPara的引用！！
    /// </summary>
    public class SkillPara:IDisposable
    {
        public SpellComponent Parent { get; private set; }
        public Vector3 Position;
        public Quaternion Rotation;
        public long FromId;
        public Unit From => Parent.GetParent<Unit>().Parent.Get<Unit>(FromId);
        public long ToId;
        public Unit To => Parent.GetParent<Unit>().Parent.Get<Unit>(ToId);
        public List<int> CostId { get; }= new List<int>();
        public List<int> Cost  { get; }= new List<int>();
        public int SkillConfigId;

        public SkillAbility Ability =>
            Parent.GetParent<Unit>().GetComponent<SkillHolderComponent>().GetSkill(SkillConfigId);
        public string CurGroup;
        public int CurIndex;
        #region 步骤参数

        public MultiMap<string,SkillStepPara> GroupStepPara = new MultiMap<string,SkillStepPara>();

        #endregion

        public static SkillPara Create(SpellComponent parent)
        {
            var res = ObjectPool.Instance.Fetch(TypeInfo<SkillPara>.Type) as SkillPara;
            res.Parent = parent;
            return res;
        }
        
        public void Clear()
        {
            Position=Vector3.zero;
            Rotation = Quaternion.identity;
            FromId = default;
            ToId = default;
            CostId.Clear();
            Cost.Clear();
            SkillConfigId = default;
            GroupStepPara.Clear();
        }
        public void Dispose()
        {
            Clear();
            Parent = null;
            ObjectPool.Instance.Recycle(this);
        }

        public SkillStepPara SetParaStep(string group,int index)
        {
            var stepPara = new SkillStepPara();
            stepPara.Index = index;
            stepPara.Paras = null;
            stepPara.Interval = 0;
            var conf = SkillStepConfigCategory.Instance.GetSkillGroup(SkillConfigId, group);
            var para = SkillStepManager.Instance.GetSkillStepParas(conf.Id);
            if (para != null && index < para.Count)
            {
                stepPara.Paras = para[index];
            }

            var timeline = SkillStepManager.Instance.GetSkillStepTimeLine(conf.Id);
            if (timeline != null && index < timeline.Count)
            {
                stepPara.Interval = timeline[index];
            }
            stepPara.Count = 0;
            
            this.GroupStepPara.Add(group,stepPara);
            return stepPara;
        }
        
        public SkillStepPara GetSkillStepPara(string group, int index)
        {
            if (this.GroupStepPara.TryGetValue(group, out var steps))
            {
                if (steps.Count > index)
                {
                    return steps[index];
                }
            }
    
            return this.SetParaStep(group,index);
        }
        public SkillStepPara GetSkillStepPara(int index)
        {
            return this.GetSkillStepPara(this.CurGroup, index);
        }
        public SkillStepPara GetCurSkillStepPara()
        {
            return this.GetSkillStepPara(this.CurGroup, this.CurIndex);
        }
    }
}