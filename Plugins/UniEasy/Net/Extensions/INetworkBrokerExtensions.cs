using System;
using Common;
using UniRx;

namespace UniEasy.Net
{
    public static class INetworkBrokerExtensions
    {
        public static void OnEvent(this INetworkBroker networkBroker, RequestCode requestCode, Action<string> action)
        {
            networkBroker.Receive(requestCode, data =>
            {
                Observable.ReturnUnit().ObserveOnMainThread().Subscribe(_ =>
                {
                    action?.Invoke(data);
                });
            });
        }
    }
}
