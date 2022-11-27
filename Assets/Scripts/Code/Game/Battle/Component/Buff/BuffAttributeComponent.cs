namespace TaoTie
{
    public class BuffAttributeComponent:Component,IComponent<int>
    {
        public int ConfigId { get; private set; }
        public BuffAttrConfig Config => BuffAttrConfigCategory.Instance.Get(ConfigId);
        
        #region override
        
        public void Init(int p1)
        {
            ConfigId = p1;
            AddBuffAttrValue();
        }

        public void Destroy()
        {
            if(this.Config.IsRemove == 0)
                RemoveBuffAttrValue();
            ConfigId = default;
        }        
        
        #endregion
        
        /// <summary>
        /// 添加BUFF属性加成
        /// </summary>
        private void AddBuffAttrValue()
        {
            if (this.Config.AttributeType != null)
            {
                var unit = this.GetParent<Buff>().Holder.GetParent<Unit>();
                var numc = unit.GetComponent<NumericComponent>();
                if (numc != null)
                {
                    for (int i = 0; i < this.Config.AttributeType.Length; i++)
                    {
                        if (NumericType.Map.TryGetValue(this.Config.AttributeType[i], out var attr))
                        {
                            if (this.Config.AttributeAdd != null && this.Config.AttributeAdd.Length > i)
                                numc.Set(attr * 10 + 2, numc.GetAsInt(attr * 10 + 2) + this.Config.AttributeAdd[i]);
                            if (this.Config.AttributePct != null && this.Config.AttributePct.Length > i)
                                numc.Set(attr * 10 + 3, numc.GetAsInt(attr * 10 + 3) + this.Config.AttributePct[i]);
                            if (this.Config.AttributeFinalAdd != null && this.Config.AttributeFinalAdd.Length > i)
                                numc.Set(attr * 10 + 4,
                                    numc.GetAsInt(attr * 10 + 4) + this.Config.AttributeFinalAdd[i]);
                            if (this.Config.AttributeFinalPct != null && this.Config.AttributeFinalPct.Length > i)
                                numc.Set(attr * 10 + 5,
                                    numc.GetAsInt(attr * 10 + 5) + this.Config.AttributeFinalPct[i]);
                        }
                        else
                        {
                            Log.Info("BuffConfig属性没找到 【" + this.Config.AttributeType[i]+"】");
                        }
                    }
                }
                else
                {
                    Log.Error("添加BUFF id= " + unit.Id + " 时没找到 NumericComponent 组件");
                }
            }
        }
        /// <summary>
        /// 移除BUFF属性加成
        /// </summary>
        private void RemoveBuffAttrValue()
        {
            if (this.Config.AttributeType != null)
            {
                var unit = this.GetParent<Buff>().Holder.GetParent<Unit>();
                var numc = unit.GetComponent<NumericComponent>();
                if (numc != null)
                {
                    for (int i = 0; i < this.Config.AttributeType.Length; i++)
                    {
                        if (NumericType.Map.TryGetValue(this.Config.AttributeType[i], out var attr))
                        {
                            if (this.Config.AttributeAdd != null && this.Config.AttributeAdd.Length > i)
                                numc.Set(attr * 10 + 2, numc.GetAsInt(attr * 10 + 2) - this.Config.AttributeAdd[i]);
                            if (this.Config.AttributePct != null && this.Config.AttributePct.Length > i)
                                numc.Set(attr * 10 + 3, numc.GetAsInt(attr * 10 + 3) - this.Config.AttributePct[i]);
                            if (this.Config.AttributeFinalAdd != null && this.Config.AttributeFinalAdd.Length > i)
                                numc.Set(attr * 10 + 4,
                                    numc.GetAsInt(attr * 10 + 4) - this.Config.AttributeFinalAdd[i]);
                            if (this.Config.AttributeFinalPct != null && this.Config.AttributeFinalPct.Length > i)
                                numc.Set(attr * 10 + 5,
                                    numc.GetAsInt(attr * 10 + 5) - this.Config.AttributeFinalPct[i]);
                        }
                        else
                        {
                            Log.Info("BuffConfig属性没找到 【" + this.Config.AttributeType[i]+"】");
                        }
                    }
                }
                else
                {
                    Log.Error("移除BUFF id= " + this.ConfigId + " 时没找到 NumericComponent 组件");
                }
                
            }
        }

    }
}