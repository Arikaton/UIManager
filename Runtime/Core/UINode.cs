using UnityEngine;

namespace UIManagement.Core
{
    public class UINode : ScriptableObject
    {
        public NodesCollection Collection;
        public string Id;
        public string[] ViewIds;
    }
}