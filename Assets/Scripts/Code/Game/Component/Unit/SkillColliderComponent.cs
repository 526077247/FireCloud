using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class SkillColliderComponent:Component,IComponent<SkillPara>,IComponent<SkillPara,long>,IComponent<SkillPara,Vector3>
    {
        [Timer(TimerType.SkillColliderRemove)]
        public class SkillColliderRemoveTimer: ATimer<Unit>
        {
            public override void Run(Unit self)
            {
                try
                {
                    self.Dispose();
                }
                catch (Exception e)
                {
                    Log.Error($"move timer error: {self.Id}\n{e}");
                }
            }
        }
        [Timer(TimerType.GenerateSkillCollider)]
        public class GenerateSkillColliderTimer: ATimer<SkillColliderComponent>
        {
            public override void Run(SkillColliderComponent self)
            {
                try
                {
                    self.GenerateSkillCollider();
                }
                catch (Exception e)
                {
                    Log.Error($"move timer error: {self.Id}\n{e}");
                }
            }
        }
        public int ConfigId;

        public SkillJudgeConfig Config => SkillJudgeConfigCategory.Instance.Get(ConfigId);
        
        /// <summary>
        /// 来源Id
        /// </summary>
        public long FromId { get; set; }
        public Unit FromUnit => Unit.Parent.Get<Unit>(FromId);
        /// <summary>
        /// 目标Id
        /// </summary>
        public long ToId{ get; set; }
        public Unit ToUnit => Unit.Parent.Get<Unit>(ToId);
        /// <summary>
        /// 目标位置
        /// </summary>
        public Vector3 Position{ get; set; }
        /// <summary>
        /// 创建逻辑触发器时间，非显示View的时间
        /// </summary>
        public long CreateColliderTime { get; set; }
        /// <summary>
        /// 创建View的时间
        /// </summary>
        public long CreateViewTime { get; set; }

        public Skill Unit => this.GetParent<Skill>();
        
        
        public List<int> CostId;
        public List<int> Cost;

        public string SkillGroup;

        public int Index;

        public int SkillConfigId;

        public SkillConfig SkillConfig => SkillConfigCategory.Instance.Get(SkillConfigId);
        
        #region override

        public void Init(SkillPara p1)
        {
            Awake(p1);
        }

        public void Init(SkillPara p1, long p2)
        {
            this.ToId = p2;
            Awake(p1);
        }

        public void Init(SkillPara p1, Vector3 p2)
        {
            this.Position = p2;
            Awake(p1);
        }

        public void Destroy()
        {
            this.Unit.GetComponent<GameObjectHolderComponent>().RemoveTrigger(this.Id);
        }

        #endregion
        
        void Awake(SkillPara para)
        {
            this.SkillGroup = para.CurGroup;
            this.Index = para.CurIndex;
            
            var stepPara = para.GetCurSkillStepPara();
            this.Cost = para.Cost;
            this.CostId = para.CostId;
            this.SkillConfigId = para.Ability.ConfigId;

            this.FromId = para.From.Id;
            
            if (StepParaHelper.TryParseInt(ref stepPara.Paras[0], out var colliderId))
            {
                this.ConfigId = colliderId;
                int deltaTime = 0;
                if (this.GetPara().Paras.Length >= 6)
                {
                    StepParaHelper.TryParseInt(ref stepPara.Paras[5], out deltaTime);
                }
                if (deltaTime <= 0)
                {
                    deltaTime = 1;//等下一帧
                }

                this.CreateViewTime = GameTimerManager.Instance.GetTimeNow();
                this.CreateColliderTime =this.CreateViewTime + deltaTime;
                this.OnCreate();
            }
            else
            {
                Log.Error("stepPara.Paras[0] Error! "+stepPara.Paras[0]);
            }
        }
        
        void OnCreate()
        {
            #region 添加触发器
            
            if (this.Config.ColliderShape == ColliderShape.None)
            {
                return;
            }
            else if (this.Config.ColliderShape == ColliderShape.Sphere||
                     this.Config.ColliderShape == ColliderShape.OBB)
            {
                if (this.CreateColliderTime <= TimeHelper.ServerNow())
                {
                    GenerateSkillCollider();
                }
                else
                {
                    GameTimerManager.Instance.NewOnceTimer(this.CreateColliderTime, TimerType.GenerateSkillCollider, this);
                }
            }
            else
            {
                Log.Error("碰撞体形状未处理" + this.Config.ColliderType);
                return;
            }

            #endregion

            GameTimerManager.Instance.NewOnceTimer(this.CreateViewTime + this.Config.Time, TimerType.SkillColliderRemove, this.Unit);
        }
        
        SkillStepPara GetPara()
        {
            var para = this.FromUnit?.GetComponent<SpellComponent>()?.Para;
            if (para != null && para.SkillConfigId == this.SkillConfigId)
            {
                return para.GetSkillStepPara(this.SkillGroup, this.Index);
            }

            var conf = SkillStepConfigCategory.Instance.GetSkillGroup(this.SkillConfigId, this.SkillGroup);
            return new SkillStepPara()
            {
                Index = this.Index,
                Paras = SkillStepManager.Instance.GetSkillStepParas(conf.Id)[this.Index]
            };
        }
        
        void GenerateSkillCollider()
        {
            var skillAOIUnit = this.Unit;
            if (skillAOIUnit == null||skillAOIUnit.IsDispose)
            {
                Log.Info("skillAOIUnit == null||skillAOIUnit.IsDisposed");
                return;
            }
            if (this.Config.ColliderShape == ColliderShape.OBB)
            {
                Vector3 par = new Vector3(this.Config.ColliderPara[0], this.Config.ColliderPara[1],
                    this.Config.ColliderPara[2]);
                skillAOIUnit.GetComponent<GameObjectHolderComponent>().AddOBBTrigger(
                    this.Id,par,TriggerType.Enter,EntityType.ALL,OnTrigger).Coroutine();
            }
            else if (this.Config.ColliderShape == ColliderShape.Sphere)
            {
                skillAOIUnit.GetComponent<GameObjectHolderComponent>().AddSphereTrigger(
                    this.Id,this.Config.ColliderPara[0],TriggerType.Enter,EntityType.ALL,OnTrigger).Coroutine();
            }
            else
            {
                Log.Error("碰撞体形状未处理" + this.Config.ColliderType);
                return;
            }

            if (this.Config.ColliderType == ColliderType.Immediate)
            {
                this.Unit.Dispose();
            }
        }

        public void OnTrigger(long id,TriggerType type,Vector3 hitPos)
        {
            var other = this.FromUnit.Parent.Get<Unit>(id);
            BattleHelper.OnCollider(hitPos,type,this.FromUnit,other,GetPara(),CostId,Cost,SkillConfig);
        }
    }
}