using System.Collections;

namespace TaoTie
{
    public class HudSystem : IManager,IUpdateManager
    {
        public void Init()
        {
            Messager.Instance.AddListener<Unit,Unit,DamageInfo>(0,MessageId.AfterCombatUnitGetDamage,OnAttackHit);
            PreloadLoadAsset().Coroutine();
        }
        /// <summary>
        /// preload一些常用hud到pool
        /// </summary>
        /// <returns></returns>
        private async ETTask PreloadLoadAsset()
        {
            using (ListComponent<string> paths = ListComponent<string>.Create())
            {
                for (int i = 0; i < HudDataConfigCategory.Instance.GetAllList().Count; i++)
                {
                    paths.Add(HudDataConfigCategory.Instance.GetAllList()[i].ResName);
                }

                await GameObjectPoolManager.Instance.LoadDependency(paths);
            }
            
        }

        public void Destroy()
        {
            Messager.Instance.RemoveListener<Unit,Unit,DamageInfo>(0,MessageId.AfterCombatUnitGetDamage,OnAttackHit);
        }

        public void Update()
        {
            var hudView =  UIManager.Instance.GetWindow<UIHudView>(1);
            if (hudView != null)
            {
                hudView.Update();
            }
        }

        private void OnAttackHit(Unit from,Unit to,DamageInfo info)
        {
            var hudView =  UIManager.Instance.GetWindow<UIHudView>();
            var conf = DamageTextConfigCategory.Instance.Get(info.Value > 0 ? 1 : 0);
            if (hudView!=null&&conf!=null)
            {
                
                DamageHudInfo hudInfo = DamageHudInfo.Create(conf.Id,info.HitPos,info.Value);
                if (hudInfo.config.Type == 0)
                {
                    hudView.ShowHud(hudInfo);
                }
            }
        }
    }
}