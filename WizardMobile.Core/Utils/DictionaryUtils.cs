using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace WizardMobile.Core.Utils
{
    static class DictionaryUtils
    {
        public static Dictionary<Tkey, TValue> ToDictionary<Tkey, TValue>(this IReadOnlyDictionary<Tkey, TValue> readonlyDictionary)
        {
            var dictionary = new Dictionary<Tkey, TValue>();
            foreach (KeyValuePair<Tkey, TValue> keyValuePair in readonlyDictionary)
                dictionary[keyValuePair.Key] = keyValuePair.Value;
            return dictionary;
        }
    }
}
