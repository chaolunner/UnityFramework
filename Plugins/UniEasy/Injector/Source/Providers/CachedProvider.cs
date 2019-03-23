using System.Collections.Generic;
using System;

namespace UniEasy.DI
{
    public class CachedProvider : IProvider
    {
        readonly IProvider creator;

        List<object> instances;

        public CachedProvider(IProvider creator)
        {
            this.creator = creator;
        }

        public Type GetInstanceType(InjectContext context)
        {
            return creator.GetInstanceType(context);
        }

        public IEnumerator<List<object>> GetAllInstancesWithInjectSplit(InjectContext context)
        {
            if (instances != null)
            {
                yield return instances;
                yield break;
            }

            var runner = creator.GetAllInstancesWithInjectSplit(context);

            // First get instance
            bool hasMore = runner.MoveNext();

            instances = runner.Current;

            yield return instances;

            // Now do injection
            while (hasMore)
            {
                hasMore = runner.MoveNext();
            }
        }
    }
}
