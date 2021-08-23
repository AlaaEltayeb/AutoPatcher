using Firebase;
using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirebaseManager : MonoBehaviour
{
    #region Vars
    DatabaseReference dBReference;
    [SerializeField] string databaseUri;
    #endregion

    #region Methods
    private void Start()
    {
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

            FirebaseApp.DefaultInstance.Options.DatabaseUrl = new Uri(databaseUri);
            dBReference = FirebaseDatabase.DefaultInstance.RootReference;

            finished = true;
        });

        while (!finished)
            yield return null;

        StartCoroutine(GetRemoteVersion());
    }

    IEnumerator GetRemoteVersion()
    {
        bool finished = false;
        string result = "";
        string key = "";

        dBReference.Child("ProjectVersion").GetValueAsync().ContinueWith(task =>
        {
            result = task.Result.GetRawJsonValue();
            key = task.Result.Key;

            print(result);
            print(key);

            finished = true;
        });

        while (!finished)
            yield return null;
    }
    #endregion
}
