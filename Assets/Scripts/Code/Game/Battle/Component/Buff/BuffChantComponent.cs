using UnityEngine;

namespace TaoTie
{
    public class BuffChantComponent:Component,IComponent<int>,IDamageBuffWatcher,IMoveBuffWatcher
    {
        public int ConfigId { get; private set; }
        public BuffChantConfig Config => BuffChantConfigCategory.Instance.Get(ConfigId);
        
        #region override
        
        public void Init(int p1)
        {
            ConfigId = p1;

        }

        public void Destroy()
        {
            GetParent<Buff>().Holder.GetParent<Unit>().GetComponent<SpellComponent>().Interrupt();
            ConfigId = default;
        }        

        #endregion

        #region Event

        public void BeforeDamage(Unit attacker, Unit target, Buff buff, DamageInfo info)
        {
            
        }

        public void AfterDamage(Unit attacker, Unit target, Buff buff, DamageInfo info)
        {
            if (info.Value>0 && Config.DamageInterrupt == 1)
            {
                var sc = target.GetComponent<SpellComponent>();
                if (sc != null&&sc.CanInterrupt())
                {
                    var bc = target.GetComponent<BuffHolderComponent>();
                    bc.RemoveByOther(buff.Id);
                }
            }
        }

        public void AfterMove(Unit target, Buff buff, Vector3 before)
        {
            if (Config.MoveInterrupt == 1)
            {
                var sc = target.GetComponent<SpellComponent>();
                if (sc != null&&sc.CanInterrupt() && Vector3.SqrMagnitude(target.Position-before)>0.01)
                {
                    var bc = target.GetComponent<BuffHolderComponent>();
                    bc.RemoveByOther(buff.Id);
                }
            }
        }

        #endregion
    }
}