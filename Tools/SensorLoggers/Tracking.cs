// Copyright 2023 Yu-Szu Wei <weiyousz0328@gmail.com>, Xing Wei 
// <weixing@buaa.edu.cn>, Xing-Yi Zheng <sharren89776@gapp.nthu.edu.tw>,
// Cheng-Hsin Hsu <chsu@cs.nthu.edu.tw>, Chenyang Yang <cyyang@buaa.edu.cn>

// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at

    // http://www.apache.org/licenses/LICENSE-2.0

// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Net;
using System.Net.Sockets;
using Tobii.G2OM;
using System.Collections;
using System.Collections.Generic;
using Tobii.XR;
using UnityEngine;

using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils;

using System.Text;
using System.IO;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

public class Tracking : MonoBehaviour
{
    private GameObject leftHand;
    private GameObject rightHand;
    public XRNode inputSource;
    public string outputCsv1 = "user_0.csv";
    private XROrigin rig;
    private FileStream fs1;
    private StreamWriter sw1;
    private float t;
    public int pid;
    int frame_id;
    // ConcurrentQueue<KeyValuePair<string, Texture2D>> cq;
    // ConcurrentQueue<KeyValuePair<string, byte[]>> cq;
    // List<System.Threading.Thread> threads;
    // bool isEnd = false;

    // Start is called before the first frame update
    void Start()
    {
        // cq = new ConcurrentQueue<KeyValuePair<string, Texture2D>>();
        // cq = new ConcurrentQueue<string>();
        // cq = new ConcurrentQueue<KeyValuePair<string, byte[]>>();
        // threads = new List<System.Threading.Thread>();
        // for(int i=0; i<10; i++){
        //     threads.Add(new Thread(ThreadWorkSaveFrame));
        // }

        // for (int i=0; i<threads.Count; i++){
        //     threads[i].Start();
        // }
        pid = 0;
        frame_id = 0;
        rig = GetComponent<XROrigin>();
        leftHand = GameObject.Find("/XR Origin/Camera Offset/LeftHand Controller");
        rightHand = GameObject.Find("/XR Origin/Camera Offset/RightHand Controller");
        fs1 = new FileStream(outputCsv1, FileMode.Create);
        sw1 = new StreamWriter(fs1);
        sw1.WriteLine("T,Headp_x,Headp_y,Headp_z,Headq_x,Headq_y,Headq_z,Headq_w,Leftp_x,Leftp_y,Leftp_z,Leftq_x,Leftq_y,Leftq_z,Leftq_w,Left_grip,Left_primary,Left_primary_x,Left_primary_y,Left_second,Left_second_x,Left_second_y,Rightp_x,Rightp_y,Rightp_z,Rightq_x,Rightq_y,Rightq_z,Rightq_w,Right_grip,Right_primary,Right_primary_x,Right_primary_y,Right_second,Right_second_x,Right_second_y,Eye_ori,Eye_dir,Eye_con,ntp_time,Pid");
    }

    // void ThreadWorkSaveFrame(){
    //     while(!cq.IsEmpty || !isEnd){
    //         KeyValuePair<string, byte[]> item;
    //         // KeyValuePair<string, Texture2D> item;
    //         // string item;
    //         bool isSuccessful = cq.TryDequeue(out item);
    //         if(isSuccessful){
    //             // ScreenCapture.CaptureScreenshot($"{frame_id}.png");
    //             string path = item.Key;
    //             // Texture2D screenImage = item.Value;
    //             byte[] rawData = item.Value;

    //             // Convert to png(Expensive)
    //             // byte[] imageBytes = screenImage.EncodeToPNG();

    //             // store image as raw Texture2D
    //             // byte[] rawData = screenImage.GetRawTextureData();
    //             // Debug.Log($"{path} is store in memory!");
    //             // UnityEngine.Object.Destroy(screenImage);

    //             File.WriteAllBytes(path, rawData);
    //             Debug.Log($"{path} is store in disk!");
    //             rawData = null;
    //         }
    //     }
    // }

    void FixedUpdate()
    {
        bool left_grip = false;
        bool left_primary = false;
        Vector2 left_primary_v = Vector2.zero;
        bool right_grip = false;
        bool right_primary = false;
        Vector2 right_primary_v = Vector2.zero;

        bool left_secondary = false;
        Vector2 left_secondary_v = Vector2.zero;
        bool right_secondary = false;
        Vector2 right_secondary_v = Vector2.zero;

        // get the key stroks of the controllers
        var leftHandDevices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.LeftHand, leftHandDevices);

        if(leftHandDevices.Count == 1)
        {
            UnityEngine.XR.InputDevice device = leftHandDevices[0];
            // Debug.Log(string.Format("Device name '{0}' with role '{1}'", device.name, device.role.ToString()));
            bool gripValue;
            Vector2 primary2DAxisValue;
            if (leftHandDevices[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out gripValue) && gripValue)
            {
                left_grip = gripValue;
                // Debug.Log("Left grip button is pressed.");
            }

            if (leftHandDevices[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out primary2DAxisValue) && primary2DAxisValue!=Vector2.zero)
            {
                left_primary = true;
                left_primary_v = primary2DAxisValue;
                // Debug.Log("left touchpad"+primary2DAxisValue);
            }
        }

        var rightHandDevices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.RightHand, rightHandDevices);

        if(rightHandDevices.Count == 1)
        {
            UnityEngine.XR.InputDevice device = rightHandDevices[0];
            // Debug.Log(string.Format("Device name '{0}' with role '{1}'", device.name, device.role.ToString()));
            bool gripValue;
            Vector2 primary2DAxisValue;
            if (rightHandDevices[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out gripValue) && gripValue)
            {
                right_grip = gripValue;
                // Debug.Log("Right grip button is pressed.");
            }

            if (rightHandDevices[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out primary2DAxisValue) && primary2DAxisValue!=Vector2.zero)
            {
                right_primary = true;
                right_primary_v = primary2DAxisValue;
                // Debug.Log("right touchpad"+primary2DAxisValue);
            }
        }

        // Debug.Log("left_grip"+left_grip);
        // Debug.Log("left_primary"+left_primary);
        // Debug.Log("left_primary_v"+left_primary_v);
        // Debug.Log("left_primary_v"+left_primary_v.x);
        // Debug.Log("left_primary_v"+left_primary_v.y);
        // Debug.Log("right_grip"+right_grip);
        // Debug.Log("right_primary"+right_primary);
        // Debug.Log("right_primary_v"+right_primary_v);
        // Debug.Log("right_primary_v"+right_primary_v.x);
        // Debug.Log("right_primary_v"+right_primary_v.y);
        

        Quaternion head_q = rig.Camera.transform.rotation;
        Vector3 head_p = rig.Camera.transform.position;
        Quaternion left_q = leftHand.transform.rotation;
        Vector3 left_p = leftHand.transform.position;
        Quaternion right_q = rightHand.transform.rotation;
        Vector3 right_p = rightHand.transform.position;

        // get device time
        DateTime date = DateTime.Now;
        DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        TimeSpan diff = date.ToUniversalTime() - origin;
        double total_second =  Math.Floor(diff.TotalSeconds);

        // Get eye tracking data in world space
        var eyeTrackingData = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.World);

        // float t = eyeTrackingData.Timestamp;

        // Check if gaze ray is valid
        if(eyeTrackingData.GazeRay.IsValid){
            // The origin of the gaze ray is a 3D point
            var rayOrigin = eyeTrackingData.GazeRay.Origin;

            // The direction of the gaze ray is a normalized direction vector
            var rayDirection = eyeTrackingData.GazeRay.Direction;
            if(eyeTrackingData.ConvergenceDistanceIsValid){
                float convDistance = eyeTrackingData.ConvergenceDistance;
                sw1.WriteLine($"{t},{head_p.x},{head_p.y},{head_p.z},{head_q.x},{head_q.y},{head_q.z},{head_q.w},{left_p.x},{left_p.y},{left_p.z},{left_q.x},{left_q.y},{left_q.z},{left_q.w},{left_grip},{left_primary},{left_primary_v.x},{left_primary_v.y},{left_secondary},{left_secondary_v.x},{left_secondary_v.y},{right_p.x},{right_p.y},{right_p.z},{right_q.x},{right_q.y},{right_q.z},{right_q.w},{right_grip},{right_primary},{right_primary_v.x},{right_primary_v.y},{right_secondary},{right_secondary_v.x},{right_secondary_v.y},{rayOrigin},{rayDirection},{convDistance},{total_second},{pid}");
            }
            else{
                bool convDistance = false;
                sw1.WriteLine($"{t},{head_p.x},{head_p.y},{head_p.z},{head_q.x},{head_q.y},{head_q.z},{head_q.w},{left_p.x},{left_p.y},{left_p.z},{left_q.x},{left_q.y},{left_q.z},{left_q.w},{left_grip},{left_primary},{left_primary_v.x},{left_primary_v.y},{left_secondary},{left_secondary_v.x},{left_secondary_v.y},{right_p.x},{right_p.y},{right_p.z},{right_q.x},{right_q.y},{right_q.z},{right_q.w},{right_grip},{right_primary},{right_primary_v.x},{right_primary_v.y},{right_secondary},{right_secondary_v.x},{right_secondary_v.y},{rayOrigin},{rayDirection},{convDistance},{total_second},{pid}");
            }
        }
        else{
            bool rayOrigin = false;
            bool rayDirection = false;
            if(eyeTrackingData.ConvergenceDistanceIsValid){
                float convDistance = eyeTrackingData.ConvergenceDistance;
                sw1.WriteLine($"{t},{head_p.x},{head_p.y},{head_p.z},{head_q.x},{head_q.y},{head_q.z},{head_q.w},{left_p.x},{left_p.y},{left_p.z},{left_q.x},{left_q.y},{left_q.z},{left_q.w},{left_grip},{left_primary},{left_primary_v.x},{left_primary_v.y},{left_secondary},{left_secondary_v.x},{left_secondary_v.y},{right_p.x},{right_p.y},{right_p.z},{right_q.x},{right_q.y},{right_q.z},{right_q.w},{right_grip},{right_primary},{right_primary_v.x},{right_primary_v.y},{right_secondary},{right_secondary_v.x},{right_secondary_v.y},{rayOrigin},{rayDirection},{convDistance},{total_second},{pid}");
            }
            else{
                bool convDistance = false;
                sw1.WriteLine($"{t},{head_p.x},{head_p.y},{head_p.z},{head_q.x},{head_q.y},{head_q.z},{head_q.w},{left_p.x},{left_p.y},{left_p.z},{left_q.x},{left_q.y},{left_q.z},{left_q.w},{left_grip},{left_primary},{left_primary_v.x},{left_primary_v.y},{left_secondary},{left_secondary_v.x},{left_secondary_v.y},{right_p.x},{right_p.y},{right_p.z},{right_q.x},{right_q.y},{right_q.z},{right_q.w},{right_grip},{right_primary},{right_primary_v.x},{right_primary_v.y},{right_secondary},{right_secondary_v.x},{right_secondary_v.y},{rayOrigin},{rayDirection},{convDistance},{total_second},{pid}");
            }
        }

        t += Time.fixedDeltaTime;

        
        // cq.Enqueue(new KeyValuePair<string, Texture2D> (path, screenImage));
        // cq.Enqueue(path);
        // StartCoroutine(RecordFrame());
        // StartCoroutine(ReadScreenPixels(frame_id));
        // frame_id++;
    }

    // IEnumerator ReadScreenPixels(int fid){
    //     yield return new WaitForEndOfFrame();
    //     string path = $"{fid}.png";
    //     // string path = $"{frame_id}.t2D";
    //     Texture2D screenImage = new Texture2D(Screen.width, Screen.height);
    //     // Get Image from screen
    //     screenImage.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
    //     screenImage.Apply();

    //     // Convert to png(Expensive)
    //     byte[] imageBytes = screenImage.EncodeToPNG();
    //     Debug.Log($"{path} is store in memory!");

    //     // byte[] rawData = screenImage.GetRawTextureData();
    //     // Debug.Log($"{path} is store in memory!");
    //     // UnityEngine.Object.Destroy(screenImage);
    //     cq.Enqueue(new KeyValuePair<string, byte[]> (path, imageBytes));
    // }

    // IEnumerator RecordFrame(){
    //     yield return new WaitForEndOfFrame();
    //     var texture = ScreenCapture.CaptureScreenshotAsTexture();
    //     Debug.Log($"Frame {frame_id} is captured as texture!");
        
    //     string path = $"{frame_id}.png";
    //     // Texture2D screenImage = new Texture2D(Screen.width, Screen.height);
    //     // screenImage.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
    //     // screenImage.Apply();
    //     cq.Enqueue(new KeyValuePair<string, Texture2D> (path, texture));

    //     UnityEngine.Object.Destroy(texture);
    //     frame_id ++;
    // }

    void OnApplicationQuit() 
    {
        // isEnd = true;
        // for(int i=0;i<10;i++){
        //     threads[i].Join();
        // }
        sw1.Close();
    }
}