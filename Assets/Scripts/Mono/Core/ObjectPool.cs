using System;
using System.Collections.Generic;

namespace TaoTie
{
    public class ObjectPool: IDisposable
    {
        private readonly Dictionary<Type, Queue<object>> pool = new Dictionary<Type, Queue<object>>();
        
        public static ObjectPool Instance = new ObjectPool();

        private List<object> temp = new List<object>();

        private ObjectPool()
        {
        }

        public void Update()
        {
            for (int i = 0; i < temp.Count; i++)
            {
                var obj = temp[i];
                Type type = obj.GetType();
                Queue<object> queue = null;
                if (!pool.TryGetValue(type, out queue))
                {
                    queue = new Queue<object>();
                    pool.Add(type, queue);
                }
                queue.Enqueue(obj);
            }
            temp.Clear();
        }
        public T Fetch<T>() where T: class
        {
            return Fetch(TypeInfo<T>.Type) as T;
        }
        public object Fetch(Type type)
        {
            Queue<object> queue = null;
            if (!pool.TryGetValue(type, out queue))
            {
                return Activator.CreateInstance(type);
            }

            if (queue.Count == 0)
            {
                return Activator.CreateInstance(type);
            }
            return queue.Dequeue();
        }

        public void Recycle(object obj)
        {
            temp.Add(obj);
        }

        public void Dispose()
        {
            this.pool.Clear();
        }
    }
}