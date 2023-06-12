namespace TaoTie
{
    public class BuffBleedComponent:Component,IComponent<int>
    {
        [Timer(TimerType.TryBleed)]
        public class TryBleedTimer:ATimer<BuffBleedComponent>
        {
            public override void Run(BuffBleedComponent t)
            {
                t.TryBleed();
            }
        }
        public int ConfigId { get; private set; }
        public BuffBleedConfig Config => BuffBleedConfigCategory.Instance.Get(ConfigId);
        /// <summary>
        /// 上次掉血时间
        /// </summary>
        public long LastBleedTime;

        private long Timer;

        #region override
        
        public void Init(int p1)
        {
            ConfigId = p1;
            LastBleedTime = GameTimerManager.Instance.GetTimeNow();
            HandleBleed();
            this.Timer = GameTimerManager.Instance.NewRepeatedTimer(100, TimerType.TryBleed, this);
        }

        public void Destroy()
        {
            TryBleed();
            ConfigId = default;
        }        

        #endregion
        
        private void TryBleed()
        {
            var timeNow = GameTimerManager.Instance.GetTimeNow();
            var deltaTime = timeNow - this.LastBleedTime;
            while (deltaTime >= this.Config.CD)
            {
                deltaTime -= this.Config.CD;
                this.LastBleedTime += this.Config.CD;
                this.HandleBleed();
            }
        }
        
        public void HandleBleed()
        {
            var buff = this.GetParent<Buff>();
            var target = buff.Holder.GetParent<Unit>();
            var from = target?.Parent.Get<Unit>(buff.FromId);
            if (from != null)
            {
                FormulaConfig formula = FormulaConfigCategory.Instance.Get(this.Config.FormulaId);
                if (formula!=null)
                {
                    FormulaStringFx fx = FormulaStringFx.Get(formula.Formula);
                    NumericComponent f = from.GetComponent<NumericComponent>();
                    NumericComponent t = target.GetComponent<NumericComponent>();
                    float value = fx.GetData(f, t);
                    BattleHelper.Damage(from,target,value,target.Position);
                }
            }
        }
    }
}