using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace VolumeRendering
{

    public class VolumeRenderingController : MonoBehaviour {

        [SerializeField] protected VolumeRendering volume;
        [SerializeField] protected Slider sliderXMin, sliderXMax, sliderYMin, sliderYMax, sliderZMin, sliderZMax;
        [SerializeField] protected Slider sliderRotX, sliderRotY, sliderRotZ, sliderScale;
        [SerializeField] protected Slider sliderThreshold, sliderIntensity;
        [SerializeField] protected Slider sliderTimepoint, sliderRate;
        [SerializeField] protected Dropdown dropChannel;

        void Start ()
        {
            const float threshold = 0.025f;

            dropChannel.onValueChanged.AddListener((v) => {
              volume.channel = (int)v;
              volume.changeTexture();
            });

            sliderXMin.onValueChanged.AddListener((v) => {
                volume.sliceXMin = sliderXMin.value = Mathf.Min(v, volume.sliceXMax - threshold);
            });
            sliderXMax.onValueChanged.AddListener((v) => {
                volume.sliceXMax = sliderXMax.value = Mathf.Max(v, volume.sliceXMin + threshold);
            });

            sliderYMin.onValueChanged.AddListener((v) => {
                volume.sliceYMin = sliderYMin.value = Mathf.Min(v, volume.sliceYMax - threshold);
            });
            sliderYMax.onValueChanged.AddListener((v) => {
                volume.sliceYMax = sliderYMax.value = Mathf.Max(v, volume.sliceYMin + threshold);
            });

            sliderZMin.onValueChanged.AddListener((v) => {
                volume.sliceZMin = sliderZMin.value = Mathf.Min(v, volume.sliceZMax - threshold);
            });
            sliderZMax.onValueChanged.AddListener((v) => {
                volume.sliceZMax = sliderZMax.value = Mathf.Max(v, volume.sliceZMin + threshold);
            });

            sliderRotX.onValueChanged.AddListener((v) => {
                volume.rotx = sliderRotX.value;
            });
            sliderRotY.onValueChanged.AddListener((v) => {
                volume.roty = sliderRotY.value;
            });
            sliderRotZ.onValueChanged.AddListener((v) => {
                volume.rotz = sliderRotZ.value;
            });

            sliderScale.onValueChanged.AddListener((v) => {
                volume.Scale(sliderScale.value);
            });

            sliderThreshold.onValueChanged.AddListener((v) => {
                volume.threshold = sliderThreshold.value;
            });
            sliderIntensity.onValueChanged.AddListener((v) => {
                volume.intensity = sliderIntensity.value;
            });

            sliderTimepoint.onValueChanged.AddListener((v) => {
                volume.timepoint = (int)sliderTimepoint.value;
                volume.changeTexture();
            });

            sliderRate.onValueChanged.AddListener((v) => {
                volume.rate = sliderRate.value;
            });
        }

        void Update ()
        {
          sliderTimepoint.value = volume.timepoint;
          sliderTimepoint.maxValue = volume.maxtimepoint;
        }

        public void Play()
        {
          volume.play = !volume.play;
        }

        public void Quit()
        {
          Application.Quit();
        }
    }

}
