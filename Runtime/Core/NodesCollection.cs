using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UIManagement.Core
{
    [CreateAssetMenu(fileName = "NodesCollection", menuName = "UIManagement/Nodes Collection", order = 0)]
    public class NodesCollection : ScriptableObject
    {
        public string Id = "NodeCollection";
        public List<UINode> UINodes = new List<UINode>();
        public List<string> UIViews = new List<string>();

        public static List<NodesCollection> _instances = new List<NodesCollection>();
        public static List<NodesCollection> Instances
        {
            get
            {
                if (_instances == null)
                {
                    UpdateInstances();
                }
                return _instances;
            }
        }

        public static void UpdateInstances()
        {
            _instances = Resources.LoadAll<NodesCollection>("").ToList();
        }

        private void Reset()
        {
            // TODO: Удалять все дочерние ассеты
        }
    }
}