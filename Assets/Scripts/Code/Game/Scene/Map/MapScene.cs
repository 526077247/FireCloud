using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{
    public class MapScene:SceneManagerProvider,IScene
    {
        public Player Self;
        public Monster Monster;
        private UILoadingView win;
        private string[] dontDestroyWindow = {"UILoadingView"};
        public List<string> scenesChangeIgnoreClean;
        
        public string[] GetDontDestroyWindow()
        {
            return dontDestroyWindow;
        }

        public List<string> GetScenesChangeIgnoreClean()
        {
            return scenesChangeIgnoreClean;
        }
        public async ETTask OnCreate()
        {
            scenesChangeIgnoreClean = new List<string>();
            scenesChangeIgnoreClean.Add(UILoadingView.PrefabPath);
            await ETTask.CompletedTask;
        }

        public async ETTask OnEnter()
        {
            win = await UIManager.Instance.OpenWindow<UILoadingView>(UILoadingView.PrefabPath);
            win.SetProgress(0);
        }

        public async ETTask OnLeave()
        {
            await ETTask.CompletedTask;
            RemoveManager<EntityManager>();
        }

        public async ETTask OnPrepare()
        {
            await ETTask.CompletedTask;
        }

        public async ETTask OnComplete()
        {
            await ETTask.CompletedTask;
        }

        public async ETTask SetProgress(float value)
        {
            win.SetProgress(value);
            await ETTask.CompletedTask;
        }

        public string GetScenePath()
        {
            return "Scenes/MapScene/Map.unity";
        }

        public async ETTask OnSwitchSceneEnd()
        {
            await UIManager.Instance.DestroyWindow<UILoadingView>();
            win = null;
            var em = RegisterManager<EntityManager>();
            Self = em.CreateEntity<Player,int>(1);
            Self.Position = new Vector3(2, 0, 0);
            Monster = em.CreateEntity<Monster,int>(1);
            Monster.Position = new Vector3(-2, 0, 0);
            Self.Rotation = Quaternion.LookRotation(Vector3.left);
            Monster.Rotation = Quaternion.LookRotation(Vector3.right);

            for (int i = 0; i < 4; i++)
            {
                Self.GetComponent<SkillHolderComponent>().AddSkill(1001 + i);
            }
            
            await UIManager.Instance.OpenWindow<UIHudView>(UIHudView.PrefabPath,UILayerNames.GameBackgroudLayer);
            await UIManager.Instance.OpenWindow<UIBattleView,MapScene>(UIBattleView.PrefabPath,this);
        }
    }
}