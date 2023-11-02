# TrajCollecting
## About this dataset
This dataset is a 6DoF VR dataset collecting the users' movement data and some videos of the users with four 3D virtual worlds, i.e., City, Nature landscape, Room, and Art gallery. Due to data loss, 7 subjects with the subject id 11, 12, 15, 17, 23, 24, and 25, are removed from this dataset. The file naming format is *{scene}_{U}* followed by the file extension, in which scene represents the current *{scene}* name and *{U}* represents the subject ID.
The dataset is accepted by ACM MMSys'23 conference, the paper is available at https://dl.acm.org/doi/10.1145/3587819.3592557.

### Questionnaire
The Questionnaire folder includes the demographic and experience questionnaires as two pdf files and an answer folder. The answers to the questionnaires are placed in the answers folder. The inputs from all subjects are saved in CSV files within the answers folder.

### City, Nature, Office, and Gallery
The City, Nature, Office, and Gallery folders contain the sensor data, i.e., trajectory data and physical RGBD video, of each scene. Each folder contains two folders, Trajectory and Physical, which are for the trajectory data and the physical videos. The download links for each scene and the objects used in each scene are provided below:
* City: https://assetstore.unity.com/packages/3d/environments/urban/real-new-york-city-vol-2-222827#description 
    * Cars: https://assetstore.unity.com/packages/3d/vehicles/land/hq-racing-car-model-no-1203-139221
* Nature: https://assetstore.unity.com/packages/3d/environments/fantasy/fantasy-forest-environment-free-demo-35361
    * Plants: https://assetstore.unity.com/packages/3d/environments/fantasy/glowing-forest-79686
* Office: https://assetstore.unity.com/packages/3d/environments/free-medieval-room-131004
    * Cookies: https://assetstore.unity.com/packages/3d/props/food/christmas-cookies-breakable-105913
* Gallery: https://assetstore.unity.com/packages/3d/environments/art-gallery-museum-188756
If you have any concerns about how to reconstruct the scene, please feel free to contact us.

#### Trajectory
##### CSV file
The loss data & outliers are left as nan in the file.
The headers are explained as follows:
* T: time elapsed in Unity
* Headp_x, Headp_y, Headp_z: Coordinate of HMD position.
* Headq_x, Headq_y, Headq_z, Headq_w: Quaternion of HMD orientation.
* Leftp_x, Leftp_y, Leftp_z: Coordinate of Left controller position.
* Leftq_x, Leftq_y, Leftq_z, Leftq_w: Quaternion of Left controller orientation.
* Left_grip: Represents whether the grip button on the left controller is pressed. 1 indicates true, nan indicates false.
* Left_primary: Represents whether the left controller primary touchpad is tapped. 1 indicates true, nan indicates false.
* Left_primary_x, Left_primary_y: Coordinate the tapped position on the left controller's primary touchpad.
* Left_second: Represents whether the left controller's second touchpad is tapped. 1 indicates true, nan indicates false.
* Left_second_x, Left_second_y: Coordinate the tapped position on the left controller's second touchpad.
* Eye_orix, Eye_oriy, Eye_oriz: The eye position. Collected by Tobii SDK.
* Eye_dirx, Eye_diry, Eye_dirz: The gaze orientation. Collected by Tobii SDK.
* Eye_con: The convergence distance of the eye. Collected by Tobii SDK.
* ntp_time: The timestamp when each sample is recorded.
* Pid: The id of the subject to whom tractory belongs.

The right controller sensor data are represented the same as the left controller sensor data but replaced left to right.

##### JSON file
The JSON files can be used for reconstructing the HMD view. The sensor data are the same in the CSV file format, but with the positions and orientations of the objects. The representation of the positions and orientations of the objects are the same as the HMD positions and orientations. Note that we do not remove the outliers in the JSON files due to the continuity of the frames while rendering the HMD view.

#### Physical RGBD
This folder contains the RGB videos and depth videos of each subject in the physical world when they are exploring the scenes.
The timestamps of each frame in the videos are stored in .txt files.
This folder is too large to store on Github, you can download it with the link: http://snoopy.cs.nthu.edu.tw/datasets/VRPrivacyMMSys23.tar.bz2

### Tools
This folder contains the tools we implement for collecting the dataset.
