using System.Collections;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour {
  public GameObject loadingScreen;
  public Slider progressBar;
  public Text progressText;
  public Text LogText;
  public bool loading = true;


  public void UpdateProgress (float progress) {
    progressBar.value = progress;
    progressText.text = string.Format("{0}%", progress*100);
  }

  public void LogMessage(string message) {
    LogText.text += message;
    LogText.text += "\n";
  }

  public void Close() {
    loading = false;
    loadingScreen.SetActive(false);
  }
}
