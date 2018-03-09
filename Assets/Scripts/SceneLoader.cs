using System.Collections;
using System.IO;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour {
  public GameObject loadingScreen;
  public Slider progressBar;
  public Text progressText;
  public Text PathText;
  public DatasetSelector Dataset;

  void Start () {
    // PathText.text = Path.Combine(Application.persistentDataPath, "Textures");;
    PathText.text = Path.Combine(Application.dataPath, "Textures");;
  }

  public void LoadLevel (int sceneIndex) {
    if (Dataset.Dataset != "") {
      StartCoroutine(LoadAsyncronously(sceneIndex));
    }
    else {
      progressText.text = "Please select a dataset above ...";
    }
  }

  IEnumerator LoadAsyncronously (int sceneIndex) {
    AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
    operation.allowSceneActivation = true;

    loadingScreen.SetActive(true);

    while (!operation.isDone) {
      float progress = Mathf.Clamp01(operation.progress / .9f);
      Debug.Log(progress);
      progressBar.value = progress;
      progressText.text = "Reading Settings ...";
      yield return null;
    }
  }
}
