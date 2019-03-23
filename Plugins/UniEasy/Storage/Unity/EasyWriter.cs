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
        private static ReactiveDictionary<string, ReactiveWriter> records;

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
            if (records == null)
            {
                records = new ReactiveDictionary<string, ReactiveWriter>();
            }

            if (!records.ContainsKey(path))
            {
                DeserializeAsync<EasyDictionary<string, EasyObject>>(path).Subscribe(o =>
                {
                    records.Add(path, new ReactiveWriter(path, o));
                });
            }
        }

        public IObservable<ReactiveWriter> OnAdd()
        {
            return records.ObserveAdd()
                .StartWith(records.Select(w => new DictionaryAddEvent<string, ReactiveWriter>(w.Key, w.Value)))
                .Where(w => w.Key.Equals(FilePath)).Select(w => w.Value);
        }

        public IObservable<ReactiveWriter> OnRemove()
        {
            return records.ObserveRemove().Where(w => w.Key.Equals(FilePath)).Select(w => w.Value);
        }
    }
}
