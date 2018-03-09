using UnityEngine;

[System.Serializable]
public class VolumeSettings
{
    public int x;
    public int y;
    public int z;
    public int channels;
    public int timepoints;

    public static VolumeSettings CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<VolumeSettings>(jsonString);
    }

    // Given JSON input:
    // {"name":"Dr Charles","lives":3,"health":0.8}
    // this example will return a PlayerInfo object with
    // name == "Dr Charles", lives == 3, and health == 0.8f.
}
