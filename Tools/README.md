This folder contains the tools we implement for collecting the dataset.
# realsense.py
The script used for recording the RGBD videos with Intel RealSense Depth Camera. After finishing the recording, 4 files will be generated: 
1. filename.mp4: RGB video
2. filename_depth.mp4: Depth video
3. filename.txt: timestamps
4. filename.npy: a NumPy array of depth information. it is a NumPy array of size time * height * width.

# visualize.py
The script is used for visualizing the sensor data. It reads the CSV files and plots the sensor data into images and concatenates a sequence of images into videos. To use the visualizer, call *visualize CSV(inputPath, savingPath, startingFrame, length, frameRate)*

* inputPath: the input trajectory CSV file's path. ('x.csv')
* savingPath: this function will generate 4 videos and 4 folders in savingPath.
* startingFrame: the starting frame of the visualized video. Corresponding to the index of the input trajectory CSV files.
* length: the length of the visualized video, united in the frame.
* frameRate: the frame rate of the visualized video.
The generated videos are the bird's eye's view video of HMD, two hand controllers, and gaze data.

# sensor logger
This folder contains two scripts, **Tracking.cs** and **TrackingObj.cs**.
Tracking.cs is provided for capturing the sensor data without interactable objects. It outputs the sensor data in a CSV file. TrackingObj.cs is provided for capturing the sensor data with interactable objects. It outputs the sensor data in a JSON file. To use these two scripts, import the scripts into your Unity project, and add the scripts as components of your XR Origin, then Unity will build the project. 