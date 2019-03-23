using System.Collections.Generic;
using System;
using UniRx;

namespace UniEasy.ECS
{
    public interface IGroup : IDisposable
    {
        IEventSystem EventSystem { get; set; }

        IPool EntityPool { get; set; }

        string Name { get; set; }

        ReactiveCollection<IEntity> Entities { get; set; }

        Type[] Components { get; set; }
    }
}
