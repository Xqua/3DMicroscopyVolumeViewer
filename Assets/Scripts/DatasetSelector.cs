using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DatasetSelector : MonoBehaviour {
  public GameObject loadingScreen;
  public Dropdown datasetChooser;
  public string Dataset;
  public List<string> dirs;

  // Make global
  public static DatasetSelector Instance {
      get;
      set;
  }

  void Awake() {
      DontDestroyOnLoad(transform.gameObject);
      Instance = this;
  }

  void Start () {
    datasetChooser.ClearOptions();
    string texturePath = Path.Combine(Application.persistentDataPath, "Textures");
    if (!Directory.Exists(texturePath))
             {
                 Directory.CreateDirectory(texturePath);
             }
    List<string> tmpdirs = new List<string>(Directory.GetDirectories(texturePath));
    foreach (var dir in tmpdirs)
    {
      print(Path.GetFileName(dir));
      dirs.Add(Path.GetFileName(dir));
    }
    datasetChooser.AddOptions(dirs);
  }

  public void Set () {
    Dataset = dirs[datasetChooser.value];
    print(Dataset);
  }

}
