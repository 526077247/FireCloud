using UnityEngine;
namespace TaoTie
{
    public abstract class Unit:Entity
    {
        #region 基础数据

        
        public int ConfigId { get; protected set; } //配置表id
        
        public UnitConfig Config => UnitConfigCategory.Instance.Get(this.ConfigId);

        private Vector3 position; //坐标

        public Vector3 Position
        {
            get => this.position;
            set
            {
                var oldPos = this.position;
                this.position = value;
                Messager.Instance.Broadcast(Id,MessageId.ChangePositionEvt,this,oldPos);
            }
        }
        
        public Vector3 Forward
        {
            get => this.Rotation * Vector3.forward;
            set => this.Rotation = Quaternion.LookRotation(value, Vector3.up);
        }

        private Quaternion rotation;
        public Quaternion Rotation
        {
            get => this.rotation;
            set
            {
                var oldRot = this.rotation;
                this.rotation = value;
                Messager.Instance.Broadcast(Id,MessageId.ChangeRotationEvt,this,oldRot);
            }
        }

        
        #endregion

        protected void AddCommonUnitComponent()
        {
            AddComponent<GameObjectHolderComponent>();
            AddComponent<BuffHolderComponent>();
            var numericComponent = AddComponent<NumericComponent>();

            #region 临时数据

            numericComponent.Set(NumericType.SpeedBase, 6f); // 速度是6米每秒
            numericComponent.Set(NumericType.AOIBase, 2); // 视野2格
            numericComponent.Set(NumericType.HpBase, 1000); // 生命1000
            numericComponent.Set(NumericType.MaxHpBase, 1000); // 最大生命1000
            numericComponent.Set(NumericType.LvBase,1); //1级
            numericComponent.Set(NumericType.ATKBase,100); //100攻击
            numericComponent.Set(NumericType.DEFBase,500); //500防御

            #endregion
            AddComponent<SkillHolderComponent>();
            AddComponent<SpellComponent>();
            AddComponent<MoveComponent>();
        }
    }
}