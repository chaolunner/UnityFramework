using System.Linq;
using System.IO;
#if (NETFX_CORE || NET_4_6 || NET_STANDARD_2_0 || UNITY_WSA_10_0)
using System;
#endif
using UniRx;

namespace UniEasy
{
    public partial class EasyWriter
    {
        private string FilePath = "Defaults";
        private static ReactiveDictionary<string, ReactiveWriter> dict;

        public EasyWriter(string path)
        {
            FilePath = path;
#if UNITY_EDITOR
            var directoryName = Path.GetDirectoryName(path);
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
#endif
            if (dict == null)
            {
                dict = new ReactiveDictionary<string, ReactiveWriter>();
            }

            if (!dict.ContainsKey(path))
            {
                DeserializeAsync<EasyDictionary<string, EasyObject>>(path).Subscribe(o =>
                {
                    dict.Add(path, new ReactiveWriter(path, o));
                });
            }
        }

        public IObservable<ReactiveWriter> OnAdd()
        {
            return dict.ObserveAdd()
                .StartWith(dict.Select(w => new DictionaryAddEvent<string, ReactiveWriter>(w.Key, w.Value)))
                .Where(w => w.Key.Equals(FilePath)).Select(w => w.Value);
        }

        public IObservable<ReactiveWriter> OnRemove()
        {
            return dict.ObserveRemove().Where(w => w.Key.Equals(FilePath)).Select(w => w.Value);
        }
    }
}
