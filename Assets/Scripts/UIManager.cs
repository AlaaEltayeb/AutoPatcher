using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    #region Vars
    [SerializeField] MeshRenderer cube;
    #endregion

    #region Methods
    public void LoadMat(string matName)
    {
        if (DownloadManager.Instance.Bundles.Count > 0)
        {
            foreach (var item in DownloadManager.Instance.Bundles)
            {
                Material mat = item.LoadAsset<Material>(matName);
                if (mat)
                {
                    cube.material = mat;
                    break;
                }
            }
        }
    }


    #endregion
}
