using System;
using Common;
using UniRx;

namespace UniEasy.Net
{
    public static class INetworkSystemExtensions
    {
        public static void OnEvent(this INetworkSystem eventSystem, RequestCode requestCode, Action<string> action)
        {
            eventSystem.Receive(requestCode, data =>
            {
                Observable.ReturnUnit().ObserveOnMainThread().Subscribe(_ =>
                {
                    action?.Invoke(data);
                });
            });
        }
    }
}
