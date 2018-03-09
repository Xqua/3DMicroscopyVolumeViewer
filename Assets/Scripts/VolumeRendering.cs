
using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;
//using UnityEditor;

namespace VolumeRendering
{

    [RequireComponent (typeof(MeshRenderer), typeof(MeshFilter))]
    public class VolumeRendering : MonoBehaviour {

	      public GameObject cube;

        [SerializeField] protected LevelLoader loader;

        [SerializeField] protected Shader shader;
        protected Material material;

        [SerializeField] Color color = Color.white;
        [Range(0f, 1f)] public int channel = 0;
        [Range(0, 10)] public int timepoint = 0;
        [Range(0f, 1f)] public float threshold = 0.5f;
      	[Range(0f, 1f)] public float rotx = 0.0f;
      	[Range(0f, 1f)] public float roty = 0.0f;
        [Range(0f, 1f)] public float rotz = 0.25f;
      	[Range(0.01f, 2f)] public float rate = 0.33f;
        [Range(1f, 10f)] public float intensity = 1.5f;
        [Range(0f, 1f)] public float sliceXMin = 0.0f, sliceXMax = 1.0f;
        [Range(0f, 1f)] public float sliceYMin = 0.0f, sliceYMax = 1.0f;
        [Range(0f, 1f)] public float sliceZMin = 0.0f, sliceZMax = 1.0f;

        public bool play = false;
        public int maxtimepoint = 10;
        private float lastTime = 0.0F;

        [SerializeField] protected Texture3D volume;
        [SerializeField] protected string DataPath;

        public static VolumeSettings settings;
        public Texture3D[,] Volumes;

        private DatasetSelector dataset = DatasetSelector.Instance;

        protected void Start () {

            // GameObject dataset = GameObject.Find("DataSet Selector");
            DataPath = Path.Combine("Textures", dataset.Dataset);
            material = new Material(shader);
            GetComponent<MeshFilter>().sharedMesh = Build();
            GetComponent<MeshRenderer>().sharedMaterial = material;

            settings = loadTex3DSettings(DataPath);
            maxtimepoint = settings.timepoints;
            Volumes = new Texture3D[settings.channels, settings.timepoints+1];

            StartCoroutine("Load");

            string tex = String.Format("c{0}_t{1:D5}.raw", 0, 0);
            string path = Path.Combine(Application.persistentDataPath, Path.Combine(DataPath , tex));
            volume = LoadRAW(path) as Texture3D;
            material.SetTexture("_Volume", volume);
            // material.SetTexture("_Volume", volume);
	          // cube = GameObject.Find("VolumeRendering");

            lastTime = Time.time;
            Scale(1.0f);
        }

        IEnumerator Load() {
          // Loading the TimeLapse into RAM for fluidity!
          float counter = 0f;
          for (int c = 0; c < settings.channels; c++) {
            loader.LogMessage(String.Format("Loading Channel: {0}", c));
            for (int t = 0; t <= settings.timepoints; t++) {
              string tex = String.Format("c{0}_t{1:D5}.raw", c, t);
              string path = Path.Combine(Application.persistentDataPath, Path.Combine(DataPath , tex));
              Volumes[c,t] = LoadRAW(path) as Texture3D;
              print(counter);
              print(settings.channels * (settings.timepoints+1));
              loader.UpdateProgress(counter / (settings.channels * (settings.timepoints+1)));
              counter += 1;
              yield return null;
            }
            loader.LogMessage(String.Format("Channel {0} has been succesfully loaded !", c));
          }
          loader.Close();
        }

        public void Scale(float f) {
            f = 100 / f;
            float sx = settings.x/f;
            float sy = settings.y/f;
            float sz = settings.z/f;
            cube.transform.localScale = new Vector3(sx, sy, sz);
        }

        private static VolumeSettings loadTex3DSettings(string path)
        {
          string filePath = Path.Combine(Application.persistentDataPath, Path.Combine(path , "settings.json"));
          print(filePath);
          // Read the json from the file into a string
          string dataAsJson = File.ReadAllText(filePath);
          // Pass the json to JsonUtility, and tell it to create a GameData object from it
          VolumeSettings settings = VolumeSettings.CreateFromJSON(dataAsJson);
          return settings;

        }

        public static Texture3D LoadRAW(string path)
        {
            int width = settings.x;
            int height = settings.y;
            int depth = settings.z;

            var max = width * height * depth;
            // var tex  = new Texture3D(width, height, depth, TextureFormat.ARGB32, false);
            var tex  = new Texture3D(width, height, depth, TextureFormat.Alpha8, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Trilinear;
            tex.anisoLevel = 0;

            using(var stream = new FileStream(path, FileMode.Open))
            {
                var len = stream.Length;
                if(len != max)
                {
                    Debug.LogWarning(path + " doesn't have required resolution");
                }

                int i = 0;
                Color[] colors = new Color[max];
                float inv = 1f / 255.0f;
                for(i = 0; i < stream.Length; i++)
                {
                    if(i == max)
                    {
                        break;
                    }
                    int v = stream.ReadByte();
                    float f = v * inv;
                    colors[i] = new Color(f, f, f, f);
                }
                tex.SetPixels(colors);
                tex.Apply();
            }
            return tex;
        }

        public void changeTexture() {
          volume = Volumes[channel, timepoint];
          material.SetTexture("_Volume", volume);
        }

        protected void Update () {
            material.SetColor("_Color", color);
            material.SetFloat("_Threshold", threshold);
            material.SetFloat("_Intensity", intensity);
            material.SetVector("_SliceMin", new Vector3(sliceXMin, sliceYMin, sliceZMin));
            material.SetVector("_SliceMax", new Vector3(sliceXMax, sliceYMax, sliceZMax));
	          cube.transform.eulerAngles = new Vector3(rotx * 360, roty * 360, rotz * 360);
            if (play) {
              var newtime = Time.time;
              if (newtime - lastTime > rate) {
                timepoint += 1;
                if (timepoint > maxtimepoint) {
                  timepoint = 0;
                }
                changeTexture();
                lastTime = Time.time;
              }
          }
        }


        Mesh Build() {
            var vertices = new Vector3[] {
                new Vector3 (-0.5f, -0.5f, -0.5f),
                new Vector3 ( 0.5f, -0.5f, -0.5f),
                new Vector3 ( 0.5f,  0.5f, -0.5f),
                new Vector3 (-0.5f,  0.5f, -0.5f),
                new Vector3 (-0.5f,  0.5f,  0.5f),
                new Vector3 ( 0.5f,  0.5f,  0.5f),
                new Vector3 ( 0.5f, -0.5f,  0.5f),
                new Vector3 (-0.5f, -0.5f,  0.5f),
            };
            var triangles = new int[] {
                0, 2, 1,
                0, 3, 2,
                2, 3, 4,
                2, 4, 5,
                1, 2, 5,
                1, 5, 6,
                0, 7, 4,
                0, 4, 3,
                5, 4, 7,
                5, 7, 6,
                0, 6, 7,
                0, 1, 6
            };

            var mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            return mesh;
        }

        void OnValidate()
        {
            Constrain(ref sliceXMin, ref sliceXMax);
            Constrain(ref sliceYMin, ref sliceYMax);
            Constrain(ref sliceZMin, ref sliceZMax);
        }

        void Constrain (ref float min, ref float max)
        {
            const float threshold = 0.025f;
            if(min > max - threshold)
            {
                min = max - threshold;
            } else if(max < min + threshold)
            {
                max = min + threshold;
            }
        }

        void OnDestroy()
        {
            Destroy(material);
        }

    }

}
