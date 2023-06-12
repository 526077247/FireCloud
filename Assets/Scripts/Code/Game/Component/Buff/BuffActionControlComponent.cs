namespace TaoTie
{
    public class BuffActionControlComponent:Component,IComponent<int>
    {
        public int ConfigId { get; private set; }
        public BuffActionControlConfig Config => BuffActionControlConfigCategory.Instance.Get(ConfigId);
        
        #region override
        
        public void Init(int p1)
        {
            ConfigId = p1;
            AddBuffActionControl();
        }

        public void Destroy()
        {
            RemoveBuffActionControl();
            ConfigId = default;
        }        

        #endregion
        
        /// <summary>
        /// 添加行为禁制
        /// </summary>
        public void AddBuffActionControl()
        {
            var buffComp = this.GetParent<Buff>().Holder;
            if (this.Config.ActionControl != null)
            {
                for (int i = 0; i < this.Config.ActionControl.Length; i++)
                {
                    var type = this.Config.ActionControl[i];
                    if (!buffComp.ActionControls.ContainsKey(type)||buffComp.ActionControls[type]==0)
                    {
                        Messager.Instance.Broadcast(buffComp.Id,MessageId.ActionControlActiveChange,type,true);
                    }
                    else
                    {
                        buffComp.ActionControls[type]++;
                    }
                }
            }
        }

        /// <summary>
        /// 移除行为禁制
        /// </summary>
        public void RemoveBuffActionControl()
        {
            var buffComp = this.GetParent<Buff>().Holder;
            if (this.Config.ActionControl != null)
            {
                for (int i = 0; i < this.Config.ActionControl.Length; i++)
                {
                    var type = this.Config.ActionControl[i];
                    if (buffComp.ActionControls.ContainsKey(type)&&buffComp.ActionControls[type]>0)
                    {
                        buffComp.ActionControls[type]--;
                        if (buffComp.ActionControls[type] == 0)
                        {
                            Messager.Instance.Broadcast(buffComp.Id,MessageId.ActionControlActiveChange,type,false);
                        }
                    }
                }
            }
        }
    }
}