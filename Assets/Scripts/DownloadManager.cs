using Firebase.Storage;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Networking;
using UnityEngine.UI;

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

        downloadPath = Path.Combine(Application.persistentDataPath, "Versions", $"Version{ (AutoPacher.Instance.LocalProjectVersion + 1).ToString() }.png");
        Directory.CreateDirectory(downloadPath);

        StartCoroutine(IDownloadFile());
    }

    IEnumerator IDownloadFile()
    {
        bool finished = false;
        string result = "";

        //print(downloadPath);
        StorageReference reference = storage_ref.Child("Version1.png");
        reference.GetDownloadUrlAsync().ContinueWith(task =>
        {
            if (!task.IsFaulted && !task.IsCanceled)
            {
                result = task.Result.ToString();
                Debug.Log("Download URL: " + task.Result);
            }
            else
            {
                print(task.Exception);
            }
        });

        //Task task = storage_ref.Child("Version1.png").GetFileAsync(downloadPath, new StorageProgress<DownloadState>((DownloadState state) =>
        //{
        //    progressBar.fillAmount = ((float)state.BytesTransferred / (float)state.TotalByteCount);
        //}), CancellationToken.None);
        //task.ContinueWith(resultTask =>
        //{
        //    if (!resultTask.IsFaulted && !resultTask.IsCanceled)
        //    {
        //        finished = true;
        //        Debug.Log("Download finished.");
        //    }
        //});

        while (!finished)
            yield return null;

        using (UnityWebRequest webRequest = UnityWebRequest.Get(result))
        {
            yield return webRequest.SendWebRequest();
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
