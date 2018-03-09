# 3DMicroscopyVolumeViewer
This is my Unity3D project built to visualize 4D datasets (3D + time) in VR.

# Project Status
For the moment I have built a FPS version. I will move to implementing the HTC Vive headset soon. 
The menus and possibilities are quite limited for now. 

# TO DO list
- Load the volume data into RAM using a multi threading approach with the Volume Cache shared between threads.
- Add a capacity to Add cubes into the Room. Each Added cube would be another dataset. 
- Add a capacity to expand a TimePoint from Low Res to High Res data if available. 
- Change the Shader from 1 color to 2 or 3 colors depending on the dataset.
  - Change the texture3D type based on Channel Number from 1 Alpha8 -> 2 RG8 -> 3 RGB24 -> 4 RGBA32 
  - Modify the shader from sending back the pixel value based on the sum of alpha to a transfer function of sum of alpha per color channel. Each channel giving the color of a pixel in an RGB setting. 
- Ameliorate the Shader RayMarching ... There are a lot of artefacts that are quite ugly. It looks like the Alpha Blending is just not ... performing well ? Or maybe the number of Rays ? ... 
- Clean the unity project and remove all that is not used or needed !

# Usage 
The basic usage is to launch the executable, and then select your dataset. Wait for it to load, and then Enjoy ! 

**This repo Contains a TEST_TUBE dataset in Assets/Textures folder, if you just want to try the tool but do not have data, just copy it to the correct folder (see: Where do I put my data ?)**

**The Build folder contains pre built executables for Linux, Windows and Mac OS**

## What type of data can I load.
While in theory any form of data could be loaded if someone took the time to build a converter from DATATYPE -> Texture3D, I have implemented a 8Bit RAW to Texture3D. 
Each timepoint needs to be it's own file. 
You can save RAW files using FIJI. 

## Where do I put my data ?
I am using the dataPath option of Unity to store the microscopy data. Basically it is the folder where the Executable is located ! 

**To make sure you know the exact path, the Splash Screen will display on the top left corner the path on your system! Make sure to check it out if you have issues**

## Dataset definition
- Each dataset must be encapsulated in a folder.
- Each channel / timepoint combo must be named as follow:
  - `c{%01d}_t{%05d}.raw`
  - In english, no padding on the channeln index, and a 5 zero padding on the time points. **Example: c0_t00132.raw** 
- Each dataset must contain a `settings.json` containing the metadata of the dataset.

## What is Settings.json ?
**IMPORTANT: RAW format does not carry headers giving the dimension of the Cube, thus a settings.json must accompany the data!**
This file contains a few things, first it contains the cube dimention, second it contains the number of channel and timepoints. It is very easy and looks like this:


```json
{
"x":143,
"y":164,
"z":122,
"channels":2,
"timepoints":9
}
```
  
