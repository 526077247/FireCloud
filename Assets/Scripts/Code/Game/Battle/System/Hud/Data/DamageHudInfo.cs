using UnityEngine;
using UnityEngine.UI;

namespace TaoTie
{
    /// <summary>
    /// 伤害飘字
    /// </summary>
    public sealed class DamageHudInfo:FixedHudInfo
    {
        public DamageTextConfig configDamageTextItem => DamageTextConfigCategory.Instance.Get(configId);
        private string _number;
        public static DamageHudInfo Create(int id,Vector3 pos,float damage)
        {
            var res = ObjectPool.Instance.Fetch<DamageHudInfo>();
            res._number = ((int)damage).ToString();
            res.OnInit(id,pos);
            return res;
        }

        protected override void OnGameObjectLoad()
        {
            base.OnGameObjectLoad();
            var text = hud.GetComponentInChildren<Text>();
            if (text != null)
            {
                text.text = _number;
                if (ColorUtility.TryParseHtmlString(configDamageTextItem.Color, out var color))
                {
                    text.color = color;
                }
                else
                {
                    text.color = Color.white;
                }
               
            }
        }
        
        public override void Dispose()
        {
            base.Dispose();
            _number = default;
        }
    }
}