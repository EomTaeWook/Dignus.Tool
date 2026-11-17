using System;
using System.Collections.Generic;

namespace Shared
{
    public struct EnumKey
    {
        public string Name { get; set; }

        public string FullName { get; set; }
    }
    public class EnumContainer
    {
        private readonly Dictionary<string, HashSet<string>> _enumValueContainer = new Dictionary<string, HashSet<string>>();
        private readonly Dictionary<string, string> _enumNamespaceDatas = new Dictionary<string, string>();
        private readonly List<string> _keys = new List<string>();
        public void AddNamespace(string key, string namepsace)
        {
            _enumNamespaceDatas.Add(key, namepsace);
        }
        public Dictionary<string, string> GetEnumNamespaceDatas()
        {
            return _enumNamespaceDatas;
        }
        public void Add(string key, string value)
        {
            if (_enumValueContainer.TryGetValue(key, out HashSet<string> hashSet) == false)
            {
                hashSet = new HashSet<string>();
                _enumValueContainer.Add(key, hashSet);
                _keys.Add(key);
            }
            hashSet.Add(value);
        }
        public string GetNamespace(string key)
        {
            if (_enumNamespaceDatas.TryGetValue(key, out string value) == false)
            {
                return string.Empty;
            }
            return value;
        }
        public HashSet<string> Get(string key)
        {
            if (_enumValueContainer.TryGetValue(key, out HashSet<string> hashSet) == false)
            {
                return null;
            }
            return hashSet;
        }
        public bool Contains(string key, string item)
        {
            if (_enumValueContainer.TryGetValue(key, out HashSet<string> hashSet) == false)
            {
                return false;
            }
            return hashSet.Contains(item);
        }
        public List<string> GetKeys()
        {
            return _keys;
        }
    }
}
