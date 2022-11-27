using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace TaoTie
{
	public class UIBattleView : UIBaseView, IOnCreate, IOnEnable<MapScene>
	{
		public static string PrefabPath => "UIGame/UIBattle/Prefabs/UIBattleView.prefab";
		public UIButton Skill1;
		public UIButton Skill2;
		public UIButton Skill3;
		public UIButton Skill4;

		private MapScene Scene;
		private SkillHolderComponent SkillHolderComponent;
		private SpellComponent SpellComponent;
		#region override
		public void OnCreate()
		{
			this.Skill1 = this.AddComponent<UIButton>("Bg/Skill1");
			this.Skill2 = this.AddComponent<UIButton>("Bg/Skill2");
			this.Skill3 = this.AddComponent<UIButton>("Bg/Skill3");
			this.Skill4 = this.AddComponent<UIButton>("Bg/Skill4");
		}
		public void OnEnable(MapScene scene)
		{
			Scene = scene;
			SkillHolderComponent = Scene.Self.GetComponent<SkillHolderComponent>();
			SpellComponent =  Scene.Self.GetComponent<SpellComponent>();
			this.Skill1.SetOnClick(OnClickSkill1);
			this.Skill2.SetOnClick(OnClickSkill2);
			this.Skill3.SetOnClick(OnClickSkill3);
			this.Skill4.SetOnClick(OnClickSkill4);
		}
		#endregion

		#region 事件绑定
		public void OnClickSkill1()
		{
			SpellComponent.SpellWithPoint(SkillHolderComponent.GetSkill(1001),Scene.Monster.Position);
		}
		public void OnClickSkill2()
		{
			SpellComponent.SpellWithDirect(SkillHolderComponent.GetSkill(1002),Scene.Monster.Position);
		}
		public void OnClickSkill3()
		{
			SpellComponent.SpellWithDirect(SkillHolderComponent.GetSkill(1003),Scene.Monster.Position);
		}
		public void OnClickSkill4()
		{
			SpellComponent.SpellWithTarget(SkillHolderComponent.GetSkill(1004),Scene.Monster);
		}
		#endregion
	}
}
