using Firebase;
using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirebaseManager : MonoSingletonPersistent<FirebaseManager>
{
    #region Vars
    FirebaseDatabase database;
    DatabaseReference dBReference;

    [SerializeField] string databaseUri;
    #endregion

    #region Properties
    public DatabaseReference DBReference { get => dBReference; set => dBReference = value; }
    #endregion

    #region Methods
    private void Start()
    {
        PlayerPrefs.DeleteAll();
        Init();
    }

    public void Init()
    {
        StartCoroutine(IntializeFirebase());
    }

    IEnumerator IntializeFirebase()
    {
        bool finished = false;

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.Exception != null)
            {
                return;
            }

            finished = true;
        });

        while (!finished)
            yield return null;

        database = FirebaseDatabase.GetInstance(databaseUri);
        DBReference = database.RootReference;

        StartCoroutine(GetRemoteVersion());
    }

    IEnumerator GetRemoteVersion()
    {
        bool finished = false;
        string result = "";
        string key = "";

        DBReference.Child("ProjectVersion").GetValueAsync().ContinueWith(task =>
        {
            result = task.Result.GetRawJsonValue();
            key = task.Result.Key;

            finished = true;
        });

        while (!finished)
            yield return null;

        AutoPacher.Instance.RemoteProjectVersion = float.Parse(result);
        if (AutoPacher.Instance.RemoteProjectVersion > AutoPacher.Instance.LocalProjectVersion)
            DownloadManager.Instance.InitStorage();
    }
    #endregion
}
