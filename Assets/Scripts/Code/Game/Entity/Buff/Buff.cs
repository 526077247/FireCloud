using UnityEngine;

namespace TaoTie
{
    public class Buff:Entity,IEntity<BuffHolderComponent,int,long,long>,IEntity<BuffHolderComponent,int,long,bool,long>
    {
        [Timer(TimerType.RemoveBuff)]
        public class RemoveBuffTimer: ATimer<Buff>
        {
            public override void Run(Buff self)
            {
                self.RemoveBuff();
            }
        }

        public int ConfigId { get; private set; }
        public BuffConfig Config => BuffConfigCategory.Instance.Get(ConfigId);
        
        /// <summary>
        /// 持续到什么时间
        /// </summary>
        public long Timestamp;
        
        public long TimerId;

        public BuffHolderComponent Holder { get; private set; }

        /// <summary>
        /// 来源
        /// </summary>
        public long FromId;

        #region override

        public override EntityType Type => EntityType.Buff;

        public void Init(BuffHolderComponent holder, int id,long timestamp,long sourceId)
        {
            Holder = holder;
            HandleAddLogic(id, timestamp, sourceId);
        }
        
        public void Init(BuffHolderComponent holder,int id,long timestamp,bool ignoreLogic,long sourceId)
        {
            Holder = holder;
            HandleAddLogic(id, timestamp, sourceId,ignoreLogic);
        }

        public void Destroy()
        {
            Holder = null;
            ConfigId = default;
            Timestamp = default;
            FromId = default;
            GameTimerManager.Instance.Remove(ref TimerId);
        }

        #endregion

        
        /// <summary>
        /// 处理添加buff
        /// </summary>
        /// <param name="id"></param>
        /// <param name="timestamp"></param>
        /// <param name="sourceId"></param>
        /// <param name="ignoreLogic"></param>
        public void HandleAddLogic(int id,long timestamp,long sourceId,bool ignoreLogic=false)
        {
            Log.Info("添加BUFF id="+id);
            this.ConfigId = id;
            this.Timestamp = timestamp;
            this.FromId = sourceId;
            var unit = this.Holder.GetParent<Unit>();
            if (!ignoreLogic)//忽略逻辑处理
            {
                for (int i = 0; i < this.Config.Type.Length; i++)
                {
                    if (this.Config.Type[i] == BuffType.Attribute)
                    {
                        this.AddComponent<BuffAttributeComponent,int>(this.ConfigId);
                    }
                    else if (this.Config.Type[i] == BuffType.ActionControl)
                    {
                        this.AddComponent<BuffActionControlComponent,int>(this.ConfigId);
                    }
                    else if (this.Config.Type[i] == BuffType.Bleed)
                    {
                        this.AddComponent<BuffBleedComponent,int>(this.ConfigId);
                    }
                    else if (this.Config.Type[i] == BuffType.Chant)
                    {
                        
                    }
                }
            }

            if(timestamp>=0)
                this.TimerId = GameTimerManager.Instance.NewOnceTimer(timestamp, TimerType.RemoveBuff, this);
        }

        private void HandleRemoveLogic()
        {
            for (int i = 0; i < this.Config.Type.Length; i++)
            {
                if (this.Config.Type[i] == BuffType.Attribute) //结束后是否移除加成（0:是）
                {
                    this.AddComponent<BuffAttributeComponent,int>(this.ConfigId);
                }
                else if (this.Config.Type[i] == BuffType.ActionControl)
                {
                    this.RemoveComponent<BuffActionControlComponent>();
                }
                else if (this.Config.Type[i] == BuffType.Bleed)
                {
                    this.RemoveComponent<BuffBleedComponent>();
                }
                else if (this.Config.Type[i] == BuffType.Chant)
                {

                }
            }
        }
        private void RemoveBuff()
        {
            try
            {
                if(this==null||this.IsDispose) return;
                this.Holder.Remove(this.Id);
            }
            catch (System.Exception e)
            {
                Log.Error($"move timer error: {this.Id}\n{e}");
            }
        }

        /// <summary>
        /// 刷新时间
        /// </summary>
        /// <param name="timestamp"></param>
        public void RefreshTime(long timestamp)
        {
            if(timestamp<=this.Timestamp) return;
            if (this.Timestamp >= 0)
            {
                GameTimerManager.Instance.Remove(ref this.Timestamp);
            }
            this.Timestamp = timestamp;
            this.TimerId = GameTimerManager.Instance.NewOnceTimer(this.Timestamp, TimerType.RemoveBuff, this);
        }
        
        #region Event
        
        /// <summary>
        /// 造成伤害前
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="target"></param>
        /// <param name="info"></param>
        public void BeforeDamage(Unit attacker,Unit target,DamageInfo info)
        {
            foreach ((_,var item) in Components)
            {
                if (item is IDamageBuffWatcher watcher)
                {
                    watcher.BeforeDamage(attacker,target,this,info);
                }
            }
        }
        /// <summary>
        /// 造成伤害后
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="target"></param>
        /// <param name="info"></param>
        public void AfterDamage(Unit attacker,Unit target,DamageInfo info)
        {
            foreach ((_,var item) in Components)
            {
                if (item is IDamageBuffWatcher watcher)
                {
                    watcher.AfterDamage(attacker,target,this,info);
                }
            }
        }
        /// <summary>
        /// 当添加其他buff前
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="target"></param>
        /// <param name="otherBuffId"></param>
        /// <returns></returns>
        public bool BeforeAddBuff(Unit attacker, Unit target, int otherBuffId)
        {
            bool res = true;
            foreach ((_,var item) in Components)
            {
                if (item is IAddBuffWatcher watcher)
                {
                    watcher.BeforeAdd(attacker,target,this,otherBuffId,ref res);
                }
            }
            return true;
        }
        /// <summary>
        /// 当添加其他buff后
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="target"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public void AfterAddBuff(Unit attacker, Unit target, Buff other)
        {
            foreach ((_,var item) in Components)
            {
                if (item is IAddBuffWatcher watcher)
                {
                    watcher.AfterAdd(attacker,target,this,other);
                }
            }
        }
        
        /// <summary>
        /// 移除其他buff前
        /// </summary>
        /// <param name="target"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool BeforeRemoveBuff(Unit target,Buff other)
        {
            bool res = true;
            foreach ((_,var item) in Components)
            {
                if (item is IRemoveBuffWatcher watcher)
                {
                    watcher.BeforeRemove(target,this,other,ref res);
                }
            }
            return true;
        }
        /// <summary>
        /// 移除其他buff后
        /// </summary>
        /// <param name="target"></param>
        /// <param name="other"></param>
        public void AfterRemoveBuff(Unit target,Buff other)
        {
            foreach ((_,var item) in Components)
            {
                if (item is IRemoveBuffWatcher watcher)
                {
                    watcher.AfterRemove(target,this,other);
                }
            }
        }
        /// <summary>
        /// 当移动
        /// </summary>
        /// <param name="target"></param>
        /// <param name="before"></param>
        public void AfterMove(Unit target,Vector3 before)
        {
            foreach ((_,var item) in Components)
            {
                if (item is IMoveBuffWatcher watcher)
                {
                    watcher.AfterMove(target,this,before);
                }
            }
        }
        #endregion
        
    }
}