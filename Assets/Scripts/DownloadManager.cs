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
    [SerializeField] Image _img;
    [SerializeField] Image progressBar;
    [SerializeField] TextMeshProUGUI msg;

    string downloadPath;

    FirebaseStorage storageInstance;
    StorageReference storage_ref;

    [SerializeField] MeshRenderer cube;
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

        downloadPath = Path.Combine(Application.persistentDataPath, "Versions");

        //downloadPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        //downloadPath = "Assets/StreamingAssets";

        Directory.CreateDirectory(downloadPath);

        StartCoroutine(IDownloadFile());
    }

    IEnumerator IDownloadFile()
    {
        bool finished = false;
        string result = "";

        //print(downloadPath);
        StorageReference reference = storage_ref.Child($"version{AutoPacher.Instance.LocalProjectVersion + 1}");

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
            webRequest.downloadHandler = new DownloadHandlerFile(Path.Combine(downloadPath, $"version{AutoPacher.Instance.LocalProjectVersion + 1}"));

            UnityWebRequestAsyncOperation op = webRequest.SendWebRequest();

            while (!op.isDone)
            {
                progressBar.fillAmount = webRequest.downloadProgress;

                yield return null;
            }

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(webRequest.error);
            }
            else
            {
                Debug.Log("download success");

                AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(webRequest);
                cube.material = bundle.LoadAsset<Material>("CubeColor");
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

    public void LoadLocalAssetBundle()
    {

        //AssetBundle bundle = bunde DownloadHandlerAssetBundle.GetContent(webRequest);
        //cube.material = bundle.LoadAsset<Material>("CubeColor");
    }
    #endregion
    #endregion
}
