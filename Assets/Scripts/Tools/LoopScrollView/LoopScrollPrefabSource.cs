using UnityEngine;
using System.Collections;
using System;
using SG;

namespace UnityEngine.UI
{
    [Serializable]
    public class LoopScrollPrefabSource
    {
        public string prefabName;
        public string prefabPath = "";
        public int poolSize = 0;
        public GameObject prefabObj;

        private bool inited = false;
        public Action<Transform> ResetItemAction = (t) => { Debug.Log("reset " + t.gameObject.name); };

        public virtual GameObject GetObject()
        {
            if (prefabObj != null && string.IsNullOrEmpty(prefabName) == true )
            {
                prefabName = prefabObj.name;
            }

            if (!inited)
            {
                ResourceManager.Instance.InitPool(this, poolSize);
                inited = true;
            }
            return ResourceManager.Instance.GetObjectFromPool(this);
        }

        public virtual void ReturnObject(Transform go)
        {
            ResetItemAction(go);
            ResourceManager.Instance.ReturnObjectToPool(go.gameObject);
        }
    }
}
