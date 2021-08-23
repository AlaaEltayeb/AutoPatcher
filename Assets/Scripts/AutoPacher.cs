using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoPacher : MonoSingleton<AutoPacher>
{
    #region Vars
    float localVersion;
    float remoteVersion;
    #endregion

    #region Methods
    // Start is called before the first frame update
    void Start()
    {
        localVersion = PlayerPrefs.GetFloat(nameof(localVersion), 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    #endregion
}
