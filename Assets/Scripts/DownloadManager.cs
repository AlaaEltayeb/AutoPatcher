using Firebase.Storage;
using System;
using System.IO;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android;
using UnityEngine.Networking;
using System.Collections.Generic;

//using Task = System.Threading.Tasks.Task;
//using CancellationToken = System.Threading.CancellationToken;

public class DownloadManager : MonoSingletonPersistent<DownloadManager>
{
    #region Vars
    [SerializeField] Image progressBar;
    [SerializeField] TextMeshProUGUI msg;

    string downloadPath;

    FirebaseStorage storageInstance;
    StorageReference storage_ref;
    #endregion

    #region Properties
    public List<AssetBundle> Bundles { get; set; } = new List<AssetBundle>();
    #endregion

    #region Methods
    // Start is called before the first frame update
    void Start()
    {
#if PLATFORM_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
            Permission.RequestUserPermission(Permission.ExternalStorageRead);

        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
#endif

        InitStorage();

        downloadPath = Path.Combine(Application.persistentDataPath, "Versions");
    }

    public void InitStorage()
    {
        storageInstance = FirebaseStorage.DefaultInstance;
        storage_ref = storageInstance.GetReferenceFromUrl("gs://autopatcher-47edb.appspot.com/");
    }

    #region Download New Files
    public void DownloadFile()
    {
        msg.text = "Downloading";

        if (!Directory.Exists(downloadPath))
            Directory.CreateDirectory(downloadPath);

        StartCoroutine(IDownloadFile());
    }

    IEnumerator IDownloadFile()
    {
        bool finished = false;
        string result = "";

        StorageReference reference = storage_ref.Child($"version{AutoPacher.Instance.LocalProjectVersion + 1}");

        reference.GetDownloadUrlAsync().ContinueWith(task =>
        {
            if (!task.IsFaulted && !task.IsCanceled)
            {
                result = task.Result.ToString();
                finished = true;
            }
        });

        while (!finished)
            yield return null;

        using (UnityWebRequest webRequest = UnityWebRequestAssetBundle.GetAssetBundle(result))
        {
            webRequest.downloadHandler = new DownloadHandlerFile(Path.Combine(downloadPath, $"version{AutoPacher.Instance.LocalProjectVersion + 1}"));

            UnityWebRequestAsyncOperation op = webRequest.SendWebRequest();

            while (!op.isDone)
            {
                progressBar.fillAmount = webRequest.downloadProgress;

                yield return null;
            }
        }

        finished = false;

        progressBar.fillAmount = 1;
        AutoPacher.Instance.LocalProjectVersion++;

        if (AutoPacher.Instance.LocalProjectVersion == AutoPacher.Instance.RemoteProjectVersion)
        {
            msg.text = "Finished downloading, loading game scene";
            LoadLocalAssetBundle();
        }
        else
        {
            msg.text = "Checking game version";
            DownloadFile();
        }
    }

    public void LoadLocalAssetBundle()
    {
        msg.text = "Loading Assets";

        for (int i = 0; i < AutoPacher.Instance.LocalProjectVersion; i++)
        {
            AssetBundle bundle = AssetBundle.LoadFromFile(Path.Combine(downloadPath, $"version{i + 1}"));
            Bundles.Add(bundle);
        }

        msg.text = "Assets Loaded";
    }
    #endregion
    #endregion
}
