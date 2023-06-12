using System;
using UnityEngine;

namespace TaoTie
{
    public class BuffHolderComponent: Component,IComponent
    {
        public ListComponent<long> AllBuff;
        public DictionaryComponent<int, long> Groups;
        public DictionaryComponent<int, int> ActionControls;

        #region override

        public void Init()
        {
            AllBuff = ListComponent<long>.Create();
            Groups = DictionaryComponent<int, long>.Create();
            ActionControls = DictionaryComponent<int, int>.Create();
            Messager.Instance.AddListener<Unit,Vector3>(Id,MessageId.ChangePositionEvt,AfterMove);
        }

        public void Destroy()
        {
            Messager.Instance.RemoveListener<Unit,Vector3>(Id,MessageId.ChangePositionEvt,AfterMove);
            for (int i = 0; i < AllBuff.Count; i++)
            {
                parent.Parent.Remove(AllBuff[i]);
            }
            AllBuff.Dispose();
            Groups.Dispose();
            ActionControls.Dispose();
            AllBuff = null;
            Groups = null;
            ActionControls = null;
        }

        #endregion
        
        /// <summary>
        /// 添加BUFF
        /// </summary>
        /// <param name="id"></param>
        /// <param name="timestamp"></param>
        /// <param name="sourceId"></param>
        /// <returns></returns>
        public Buff AddBuff(int id,long timestamp,long sourceId)
        {
            bool canAdd = true;
            var source = this.parent.Parent.Get<Unit>(sourceId);
            for (int i = 0; i < this.AllBuff.Count; i++)
            {
                var basebuff = this.Get(this.AllBuff[i]);
                if (!basebuff.BeforeAddBuff(source, this.GetParent<Unit>(), id))
                {
                    canAdd = false;
                }
            }

            if (canAdd)
            {
                BuffConfig conf = BuffConfigCategory.Instance.Get(id);
                if (this.Groups.ContainsKey(conf.Group))
                {
                    var oldId = this.Groups[conf.Group];
                    var old = this.Get(oldId);
                    if (old.Config.Priority > conf.Priority)
                    {
                        Log.Info("添加BUFF失败，优先级" + old.Config.Id + " > " + conf.Id);
                        return null; //优先级低
                    }

                    if (old.ConfigId == conf.Id)
                    {
                        Log.Info("相同buff,更新时间");
                        old.RefreshTime(timestamp);
                        return old;
                    }
                    else
                    {
                        Log.Info("优先级高或相同，替换旧的");
                        this.Remove(this.Groups[conf.Group]);
                    }
                }

                Buff buff = this.parent.Parent.CreateEntity<Buff,BuffHolderComponent, int, long, long>(this,id, timestamp, sourceId);
                this.Groups[conf.Group] = buff.Id;
                this.AllBuff.Add(buff.Id);
                for (int i = 0; i < this.AllBuff.Count; i++)
                {
                    var basebuff = this.Get(this.AllBuff[i]);
                    basebuff.AfterAddBuff(source, this.GetParent<Unit>(), buff);
                }
                parent.GetComponent<GameObjectHolderComponent>().AddEffect(buff.Config.EffectId).Coroutine();
                return buff;
            }

            return null;
        }
        /// <summary>
        /// 通过Buff的唯一Id取
        /// </summary>

        /// <param name="id"></param>
        /// <returns></returns>
        public Buff Get(long id)
        {
            Buff buff = this.parent.Parent.Get<Buff>(id);
            return buff;
        }
        /// <summary>
        /// 通过Buff配置表的id取
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Buff GetByConfigId(int id)
        {
            BuffConfig config = BuffConfigCategory.Instance.Get(id);
            if (this.Groups.ContainsKey(config.Group))
            {
                Buff buff = this.Get(this.Groups[config.Group]);
                if (buff.ConfigId == id)
                {
                    return buff;
                }
            }

            return null;
        }

        /// <summary>
        /// 非自动移除BUFF
        /// </summary>
        /// <param name="id"></param>
        /// <param name="force"></param>
        public void RemoveByOther(long id,bool force = false)
        {
            this.Remove(id, force);
        }
        /// <summary>
        /// 通过Buff的唯一Id移除
        /// </summary>
        /// <param name="id"></param>
        /// <param name="force"></param>
        public bool Remove(long id,bool force = false)
        {
            Buff buff = this.Get(id);
            if(buff==null) return true;
            bool canRemove = true;
            for (int i = 0; i < this.AllBuff.Count; i++)
            {
                var basebuff = this.Get(this.AllBuff[i]);
                if (!basebuff.BeforeRemoveBuff(this.GetParent<Unit>(), buff))
                {
                    canRemove= false;
                }
            }

            if (canRemove||force)
            {
                this.Groups.Remove(buff.Config.Group);
                this.AllBuff.Remove(id);
                for (int i = 0; i < this.AllBuff.Count; i++)
                {
                    var basebuff = this.Get(this.AllBuff[i]);
                    basebuff.AfterRemoveBuff(this.GetParent<Unit>(), buff);
                }
                parent.GetComponent<GameObjectHolderComponent>().RemoveEffect(buff.Config.EffectId);
                buff.Dispose();
                return true;
            }
            return false;
        }
        /// <summary>
        /// 通过Buff配置表的id移除buff
        /// </summary>
        /// <param name="id"></param>
        public void RemoveByConfigId(int id)
        {
            BuffConfig config = BuffConfigCategory.Instance.Get(id);
            if (this.Groups.ContainsKey(config.Group))
            {
                Buff buff = this.Get(this.Groups[config.Group]);
                if (buff.ConfigId == id)
                {
                    this.Groups.Remove(buff.Config.Group);
                    buff?.Dispose();
                }
            }
        }
        
        /// <summary>
        /// 造成伤害前
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="target"></param>
        /// <param name="damage"></param>
        public void BeforeDamage(Unit attacker,Unit target,DamageInfo damage)
        {
            for (int i = 0; i < this.AllBuff.Count; i++)
            {
                var buff = this.Get(this.AllBuff[i]);
                buff.BeforeDamage(attacker,target,damage);
            }
        }
        
        /// <summary>
        /// 造成伤害后
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="target"></param>
        /// <param name="damage"></param>
        public void AfterDamage(Unit attacker,Unit target,DamageInfo damage)
        {
            for (int i = 0; i < this.AllBuff.Count; i++)
            {
                var buff = this.Get(this.AllBuff[i]);
                buff.AfterDamage(attacker,target,damage);
            }
        }
        
        /// <summary>
        /// 移动后
        /// </summary>
        /// <param name="target"></param>
        /// <param name="before"></param>
        public void AfterMove(Unit target,Vector3 before)
        {
            for (int i = 0; i < this.AllBuff.Count; i++)
            {
                var buff = this.Get(this.AllBuff[i]);
                buff.AfterMove(target,before);
            }
        }

        /// <summary>
        /// 展示所有BUFF
        /// </summary>
        public void ShowAllBuffView()
        {
            foreach ((_,var id) in this.Groups)
            {
                parent.GetComponent<GameObjectHolderComponent>().RemoveEffect(this.Get(id).Config.EffectId);
            }
        }
        
        /// <summary>
        /// 隐藏所有BUFF效果
        /// </summary>
        public void HideAllBuffView()
        {
            foreach ((_,var id) in this.Groups)
            {
                parent.GetComponent<GameObjectHolderComponent>().RemoveEffect(this.Get(id).Config.EffectId);
            }
        }

    }
}