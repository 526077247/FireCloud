using System;
using System.Collections.Generic;
using ProtoBuf;

namespace TaoTie
{
    [ProtoContract]
    [Config]
    public partial class HudDataConfigCategory : ProtoObject, IMerge
    {
        public static HudDataConfigCategory Instance;
		
        
        [ProtoIgnore]
        private Dictionary<int, HudDataConfig> dict = new Dictionary<int, HudDataConfig>();
        
        [ProtoMember(1)]
        private List<HudDataConfig> list = new List<HudDataConfig>();
		
        public HudDataConfigCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            HudDataConfigCategory s = o as HudDataConfigCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            for(int i =0 ;i<list.Count;i++)
            {
                HudDataConfig config = list[i];
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public HudDataConfig Get(int id)
        {
            this.dict.TryGetValue(id, out HudDataConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (HudDataConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, HudDataConfig> GetAll()
        {
            return this.dict;
        }
        public List<HudDataConfig> GetAllList()
        {
            return this.list;
        }
        public HudDataConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class HudDataConfig: ProtoObject
	{
		/// <summary>id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>资源路径</summary>
		[ProtoMember(2)]
		public string ResName { get; set; }
		/// <summary>存在时间（单位ms，-1无限）</summary>
		[ProtoMember(3)]
		public int LifeTime { get; set; }
		/// <summary>附加类型（0：ui，1：世界）</summary>
		[ProtoMember(4)]
		public int Type { get; set; }

	}
}
