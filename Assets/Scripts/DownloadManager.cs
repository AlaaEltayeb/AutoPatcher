using Firebase.Storage;
using System;
using System.IO;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android;
using UnityEngine.Networking;

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

    [SerializeField] Image _img;
    #endregion

    #region Properties
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
    }

    public void InitStorage()
    {
        storageInstance = FirebaseStorage.DefaultInstance;
        storage_ref = storageInstance.GetReferenceFromUrl("gs://autopatcher-47edb.appspot.com/");

        DownloadFile();
    }

    #region Download New Files
    void DownloadFile()
    {
        msg.text = "Downloading";

        downloadPath = Path.Combine(Application.persistentDataPath, "Versions", $"Version{ (AutoPacher.Instance.LocalProjectVersion + 1).ToString() }");

        downloadPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        Directory.CreateDirectory(downloadPath);

        StartCoroutine(IDownloadFile());
    }

    IEnumerator IDownloadFile()
    {
        bool finished = false;
        string result = "";

        //print(downloadPath);
        StorageReference reference = storage_ref.Child("version1");

        reference.GetDownloadUrlAsync().ContinueWith(task =>
        {
            if (!task.IsFaulted && !task.IsCanceled)
            {
                result = task.Result.ToString();
                finished = true;

                Debug.Log("Download URL: " + task.Result);
            }
            else
            {
                print(task.Exception);
            }
        });

        while (!finished)
            yield return null;

        using (UnityWebRequest webRequest = UnityWebRequestAssetBundle.GetAssetBundle(result))
        {
            //webRequest.downloadHandler = new DownloadHandlerFile(downloadPath);

            ////DownloadHandlerTexture texDl = new DownloadHandlerTexture(true);
            ////webRequest.downloadHandler = texDl;

            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
                Debug.LogError(webRequest.error);
            else
            {
                AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(webRequest);
            }
        }

        finished = false;

        progressBar.fillAmount = 1;
        AutoPacher.Instance.LocalProjectVersion++;

        if (AutoPacher.Instance.LocalProjectVersion == AutoPacher.Instance.RemoteProjectVersion)
        {
            msg.text = "Finished downloading, loading game scene";
        }
        else
        {
            msg.text = "Checking game version";
            DownloadFile();
        }
    }
    #endregion
    #endregion
}
