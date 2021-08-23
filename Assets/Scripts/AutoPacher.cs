using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoPacher : MonoSingleton<AutoPacher>
{
    #region Properties
    public float LocalProjectVersion
    {
        get
        {
            return PlayerPrefs.GetFloat(nameof(LocalProjectVersion), 0);
        }
        set
        {
            PlayerPrefs.SetFloat(nameof(LocalProjectVersion), value);
        }
    }

    public float RemoteProjectVersion { get; set; }
    #endregion
}
