using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class UIHudView:UIBaseView,IOnCreate,IOnDisable
    {
        public static string PrefabPath => "UIGame/UIBattle/Prefabs/UIHudView.prefab";
        private readonly LinkedList<HudInfo> _huds = new ();
        public void OnCreate()
        {
        }

        public void OnDisable()
        {
            for (var node = _huds.First; node!=null; node=node.Next)
            {
                node.Value.Dispose();
            }
            _huds.Clear();
        }

        public void ShowHud(HudInfo info)
        {
            info.OnAttach(this.GetTransform());
            _huds.AddLast(info);
        }

        public void Update()
        {
            if(_huds==null) return;
            for (var node = _huds.First; node != null;)
            {
                var next = node.Next;
                if (!RefreshHud(node.Value))
                {
                    _huds.Remove(node);
                }
                node = next;
            }
        }

        bool RefreshHud(HudInfo info)
        {
            if (info.config.LifeTime>0 && GameTimerManager.Instance.GetTimeNow() > info.disposeTime)
            {
                info.Dispose();
                return false;
            }

            if (info.hud != null)
            {
                Vector2 pt =  Camera.main.WorldToScreenPoint(info.GetWorldPos())*UIManager.Instance.ScreenSizeflag;
                info.hud.anchoredPosition = pt+info.offset;
            }
            return true;
        }
    }
}