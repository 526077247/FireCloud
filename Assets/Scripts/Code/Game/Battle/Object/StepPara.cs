using System;

namespace TaoTie
{
    public class StepPara<T> : IDisposable
    {
        public T Value;

        public static StepPara<T> Create(T value)
        {
            var res = ObjectPool.Instance.Fetch<StepPara<T>>();
            res.Value = value;
            return res;
        }

        public void Dispose()
        {
            Value = default;
            ObjectPool.Instance.Recycle(this);
        }
    }

}