using System;
using System.Collections.Generic;
using ProtoBuf;

namespace TaoTie
{
    [ProtoContract]
    [Config]
    public partial class DamageTextConfigCategory : ProtoObject, IMerge
    {
        public static DamageTextConfigCategory Instance;
		
        
        [ProtoIgnore]
        private Dictionary<int, DamageTextConfig> dict = new Dictionary<int, DamageTextConfig>();
        
        [ProtoMember(1)]
        private List<DamageTextConfig> list = new List<DamageTextConfig>();
		
        public DamageTextConfigCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            DamageTextConfigCategory s = o as DamageTextConfigCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            for(int i =0 ;i<list.Count;i++)
            {
                DamageTextConfig config = list[i];
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public DamageTextConfig Get(int id)
        {
            this.dict.TryGetValue(id, out DamageTextConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (DamageTextConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, DamageTextConfig> GetAll()
        {
            return this.dict;
        }
        public List<DamageTextConfig> GetAllList()
        {
            return this.list;
        }
        public DamageTextConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class DamageTextConfig: ProtoObject
	{
		/// <summary>对应HudData表id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>伤害或加血</summary>
		[ProtoMember(2)]
		public int IsDamage { get; set; }
		/// <summary>颜色</summary>
		[ProtoMember(3)]
		public string Color { get; set; }

	}
}
