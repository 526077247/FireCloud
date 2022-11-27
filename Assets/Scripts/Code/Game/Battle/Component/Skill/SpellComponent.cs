using System;
using UnityEngine;

namespace TaoTie
{
    public class SpellComponent:Component,IComponent
    {
        [Timer(TimerType.PlayNextSkillStep)]
        public class PlayNextSkillStepTimer: ATimer<SpellComponent>
        {
            public override void Run(SpellComponent self)
            {
                try
                {
                    self.PlayNextSkillStep(self.NextSkillStep);
                }
                catch (Exception e)
                {
                    Log.Error($"move timer error: {self.Id}\n{e}");
                }
            }
        }
        /// <summary>
        /// 当前步骤
        /// </summary>
        public int NextSkillStep;

        public int CurSkillConfigId;//当前技能Id
        /// <summary>
        /// 当前参数
        /// </summary>
        public SkillPara Para;

        public long Timer;
        public bool Enable { get; set; } = true;
        public int waitStep = SkillStepType.None;//等待的步骤
        #region override

        public void Init()
        {
            CurSkillConfigId = 0;
            Para = SkillPara.Create(this);
            Messager.Instance.AddListener<int,bool>(Id,MessageId.ActionControlActiveChange,ActionControlActiveChange);
        }

        public void Destroy()
        {
            Interrupt();
            Para.Dispose();
            Para = null;
            Messager.Instance.RemoveListener<int,bool>(Id,MessageId.ActionControlActiveChange,ActionControlActiveChange);
        }

        private void ActionControlActiveChange(int type, bool value)
        {
            if (type == ActionControlType.BanSpell)
            {
                this.SetEnable(value);
            }
        }
        #endregion

        /// <summary>
        /// 当前技能
        /// </summary>
        public SkillAbility GetSkill()
        {
            if (this.GetParent<Unit>().GetComponent<SkillHolderComponent>().TryGetSkillAbility(this.CurSkillConfigId, out var res))
            {
                return res;
            }
            return null;
        } 
        /// <summary>
        /// 设置是否可施法
        /// </summary>
        /// <param name="enable"></param>
        public void SetEnable(bool enable)
        {
            this.Enable = enable;
            if(!enable)
                this.Interrupt();
        }
        /// <summary>
        /// 打断
        /// </summary>
        public void Interrupt()
        {
            if (this.CanInterrupt())
            {
                this.ChangeGroup(this.Para.Ability.Config.InterruptGroup);
            }
        }

        /// <summary>
        /// 是否可打断
        /// </summary>
        /// <returns></returns>
        public bool CanInterrupt()
        {
            return this.CurSkillConfigId != 0&&this.Timer!=0;
        }
        /// <summary>
        /// 改变技能释放组
        /// </summary>
        /// <param name="group"></param>
        public void ChangeGroup(string group)
        {
            if (this.CurSkillConfigId != 0)
            {
                this.Para.CurGroup = group;
                if (this.Timer != 0)//在等待中
                {
                    GameTimerManager.Instance.Remove(ref this.Timer);
                    this.PlayNextSkillStep(0);
                }
                else//在PlayNextSkillStep的循环中
                {
                    this.NextSkillStep = -1;
                }
            }
        }
        /// <summary>
        /// 结束
        /// </summary>
        private void OnSkillPlayOver()
        {
            var skill = this.GetSkill();
            if (skill!=null)
            {
                skill.LastSpellOverTime = GameTimerManager.Instance.GetTimeNow();
            }
            this.CurSkillConfigId = 0;
        }
        /// <summary>
        /// 释放对目标技能
        /// </summary>
        /// <param name="spellSkill"></param>
        /// <param name="targetEntity"></param>
        public void SpellWithTarget(SkillAbility spellSkill, Unit targetEntity)
        {
            if (!this.Enable) return;
            if (this.CurSkillConfigId != 0)
                return;
            if(!spellSkill.CanUse())return;
            this.CurSkillConfigId = spellSkill.ConfigId;
            var nowpos = this.GetParent<Unit>().Position;
            var nowpos2 = targetEntity.Position;
            if (Vector2.Distance(new Vector2(nowpos.x, nowpos.z), new Vector2(nowpos2.x, nowpos2.z)) >
                spellSkill.Config.PreviewRange[0])
            {
                return;
            }
            this.Para.Clear();
            this.Para.FromId = this.Id;
            this.Para.SkillConfigId = spellSkill.ConfigId;
            this.Para.ToId = targetEntity.Id;
            this.Para.CurGroup = spellSkill.Config.DefaultGroup;

            this.GetSkill().LastSpellTime = TimeHelper.ServerNow();
            this.PlayNextSkillStep(0);
        }
        /// <summary>
        /// 释放对点技能
        /// </summary>
        /// <param name="spellSkill"></param>
        /// <param name="point"></param>
        public void SpellWithPoint(SkillAbility spellSkill, Vector3 point)
        {
            if (!this.Enable) return;
            if (this.CurSkillConfigId != 0)
                return;
            if(!spellSkill.CanUse())return;
            this.CurSkillConfigId = spellSkill.ConfigId;
            var nowpos = this.GetParent<Unit>().Position;
            if (Vector2.Distance(new Vector2(nowpos.x, nowpos.z), new Vector2(point.x, point.z)) >
                spellSkill.Config.PreviewRange[0])
            {
                var dir =new Vector3(point.x - nowpos.x,0, point.z - nowpos.z).normalized;
                point = nowpos + dir * spellSkill.Config.PreviewRange[0];
            }

            this.Para.Clear();
            this.Para.Position = point;
            this.Para.FromId = this.Id;
            this.Para.SkillConfigId = spellSkill.ConfigId;
            this.Para.CurGroup = spellSkill.Config.DefaultGroup;

            this.GetSkill().LastSpellTime = TimeHelper.ServerNow();
            this.PlayNextSkillStep(0);
        }
        /// <summary>
        /// 释放方向技能
        /// </summary>
        /// <param name="spellSkill"></param>
        /// <param name="point"></param>
        public void SpellWithDirect(SkillAbility spellSkill, Vector3 point)
        {
            if (!this.Enable) return;
            if (this.CurSkillConfigId != 0)
                return;
            if(!spellSkill.CanUse())return;
            this.CurSkillConfigId = spellSkill.ConfigId;
            var nowpos = this.GetParent<Unit>().Position;
            point = new Vector3(point.x, nowpos.y, point.z);
            var Rotation = Quaternion.LookRotation(point - nowpos,Vector3.up);
            
            this.Para.Clear();
            this.Para.Position = point;
            this.Para.Rotation = Rotation;
            this.Para.FromId = this.Id;
            this.Para.SkillConfigId = spellSkill.ConfigId;
            this.Para.CurGroup = spellSkill.Config.DefaultGroup;
            
            this.GetSkill().LastSpellTime = TimeHelper.ServerNow();
            this.PlayNextSkillStep(0);
        }
        /// <summary>
        /// 触发下一个技能触发点
        /// </summary>
        /// <param name="index"></param>
        public void PlayNextSkillStep(int index)
        {
            do
            {
                var stepType = this.GetSkill()?.GetStepType(this.Para.CurGroup);
                if (this.Para==null||this.CurSkillConfigId==0||stepType==null||
                    index >=stepType.Count)
                {
                    this.OnSkillPlayOver();
                    return;
                }
                var id = stepType[index];
                this.Para.SetParaStep(this.Para.CurGroup,index);
                this.Para.CurIndex = index;
                SkillSystem.Instance.DoSkillStep(id, this.Para);
                if (this.CheckPause()) return;
                index++;
            } 
            while (this.Para.GetSkillStepPara(index-1).Interval<=0);
            this.NextSkillStep = index;
            this.Timer = GameTimerManager.Instance.NewOnceTimer(GameTimerManager.Instance.GetTimeNow() +
                this.Para.GetSkillStepPara(index-1).Interval, TimerType.PlayNextSkillStep, this);
        }
        

        /// <summary>
        /// 步骤是否暂停等待中
        /// </summary>
        /// <returns></returns>
        bool CheckPause()
        {
            return this.waitStep != SkillStepType.None;
        }
        
        
        /// <summary>
        /// 等待步骤
        /// </summary>
        /// <param name="stepType"></param>
        public void WaitStep(int stepType)
        {
            if(this.CurSkillConfigId==0||this.waitStep == stepType) return;
            var index = this.NextSkillStep;
            var id = this.GetSkill().GetStepType(this.Para.CurGroup)[index];
            if (stepType == id)
            {
                this.waitStep = stepType;
            }
        }
        /// <summary>
        /// 等待步骤结束
        /// </summary>
        /// <param name="stepType"></param>
        public void WaitStepOver(int stepType)
        {
            if (this.waitStep == stepType)
            {
                this.waitStep = SkillStepType.None;
                var index = this.NextSkillStep+1;
                if (this.Para.GetSkillStepPara(index-1).Interval <= 0)
                {
                    this.PlayNextSkillStep(index);
                }
                else
                {
                    this.NextSkillStep = index;
                    this.Timer = GameTimerManager.Instance.NewOnceTimer(GameTimerManager.Instance.GetTimeNow()
                         + this.Para.GetSkillStepPara(index-1).Interval, TimerType.PlayNextSkillStep, this);
                }
                
            }
        }
    }
}