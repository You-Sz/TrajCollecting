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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using UnityEngine.XR.Interaction.Toolkit;
using Newtonsoft.Json;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils;
using Tobii.XR;

public class TrackingObj : MonoBehaviour
{
    private Dictionary<string, List<List<double>>> transforms;
    public string outputFn = "a.json";
    private double t;
    public int pid = 0;
    int frame_id = 0;

    // XR
    private GameObject leftHand;
    private GameObject rightHand;
    public XRNode inputSource;
    private XROrigin rig;
    // GameObject[] interactables;
    List<GameObject> interactables;
    int nInteractables;

    /*
    {
        "T": [[t0], [t1], ...] # Nx1
        "interableObject": [[x, y, z, qx, qy, qz, qw], ...], # Nx7
        "head": [[x, y, z, qx, qy, qz, qw], ...], # Nx7
        "left": ,
        "right": ,
    }
    */
    
    // Start is called before the first frame update
    void Start()
    {
        // interactables = GameObject.FindObjectsOfType (typeof(XRGrabInteractable));
        var temp_interactables = GameObject.FindObjectsOfType (typeof(XRGrabInteractable));
        nInteractables = temp_interactables.Length;
        // interactables = new List<GameObject>();
        // for(int i = 0; i < nInteractables; i++) {
        //     interactables.Add((GameObject)temp_interactables[i]);
        // }
        transforms = new Dictionary<string, List<List<double>>>();
        Debug.Log(nInteractables);

        interactables = new List<GameObject>();
        for(int i = 0; i < nInteractables; i++) {
            // Debug.Log(interactables[i].name);
            transforms.Add(temp_interactables[i].name, new List<List<double>>());
            interactables.Add(GameObject.Find(temp_interactables[i].name));
        }
        
        // XR
        transforms.Add("pid", new List<List<double>>());
        transforms["pid"].Add(new List<double>{pid});
        transforms.Add("T", new List<List<double>>());
        transforms.Add("ntp_time", new List<List<double>>());
        transforms.Add("head", new List<List<double>>());
        transforms.Add("left", new List<List<double>>());
        transforms.Add("right", new List<List<double>>());
        transforms.Add("eye", new List<List<double>>());
        rig = GetComponent<XROrigin>();
        leftHand = GameObject.Find("/XR Origin/Camera Offset/LeftHand Controller");
        rightHand = GameObject.Find("/XR Origin/Camera Offset/RightHand Controller");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // common
        transforms["T"].Add(new List<double>{t});
        // get device time
        DateTime date = DateTime.Now;
        DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        TimeSpan diff = date.ToUniversalTime() - origin;
        double total_second =  Math.Floor(diff.TotalSeconds);
        transforms["ntp_time"].Add(new List<double>{(double)total_second});

        recordInteractables();
        recordXRObjects();
        t += Time.fixedDeltaTime;
        frame_id++;
    }
    void recordInteractables()
    {
        for(int i = 0; i < nInteractables; i++) {
            // transforms.Add(interactables[i].name, new List<List<double>>());
            Vector3 p = interactables[i].transform.position;
            Quaternion q = interactables[i].transform.rotation;
            transforms[interactables[i].name].Add(new List<double>{p.x, p.y, p.z, q.x, q.y, q.z, q.w});
        }
    }
    void recordXRObjects()
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
            Debug.Log(string.Format("Device name '{0}' with role '{1}'", device.name, device.role.ToString()));
            bool gripValue;
            Vector2 primary2DAxisValue;
            if (leftHandDevices[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out gripValue) && gripValue)
            {
                left_grip = gripValue;
                Debug.Log("Left grip button is pressed.");
            }

            if (leftHandDevices[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out primary2DAxisValue) && primary2DAxisValue!=Vector2.zero)
            {
                left_primary = true;
                left_primary_v = primary2DAxisValue;
                Debug.Log("left touchpad"+primary2DAxisValue);
            }
        }

        var rightHandDevices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.RightHand, rightHandDevices);

        if(rightHandDevices.Count == 1)
        {
            UnityEngine.XR.InputDevice device = rightHandDevices[0];
            Debug.Log(string.Format("Device name '{0}' with role '{1}'", device.name, device.role.ToString()));
            bool gripValue;
            Vector2 primary2DAxisValue;
            if (rightHandDevices[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out gripValue) && gripValue)
            {
                right_grip = gripValue;
                Debug.Log("Right grip button is pressed.");
            }

            if (rightHandDevices[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out primary2DAxisValue) && primary2DAxisValue!=Vector2.zero)
            {
                right_primary = true;
                right_primary_v = primary2DAxisValue;
                Debug.Log("right touchpad"+primary2DAxisValue);
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
        transforms["head"].Add(new List<double>{head_p.x, head_p.y, head_p.z, head_q.x, head_q.y, head_q.z, head_q.w});

        Quaternion left_q = leftHand.transform.rotation;
        Vector3 left_p = leftHand.transform.position;
        Quaternion right_q = rightHand.transform.rotation;
        Vector3 right_p = rightHand.transform.position;

        // get device time
        // DateTime date = DateTime.Now;
        // DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        // TimeSpan diff = date.ToUniversalTime() - origin;
        // double total_second =  Math.Floor(diff.TotalSeconds);

        // Get eye tracking data in world space
        var eyeTrackingData = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.World);

        // float t = eyeTrackingData.Timestamp;
        var rayValid = eyeTrackingData.GazeRay.IsValid;
        var rayOrigin = eyeTrackingData.GazeRay.Origin;
        var rayDirection = eyeTrackingData.GazeRay.Direction;
        var convDistValid = eyeTrackingData.ConvergenceDistanceIsValid;
        double convDistance = eyeTrackingData.ConvergenceDistance;

        transforms["left"].Add(new List<double>{left_p.x, left_p.y, left_p.z, left_q.x, left_q.y, left_q.z, left_q.w, (left_grip) ? 1.0f:0.0f, (left_primary) ? 1.0f: 0.0f, left_primary_v.x,left_primary_v.y, (left_secondary) ? 1.0f:0.0f, left_secondary_v.x, left_secondary_v.y});

        transforms["right"].Add(new List<double>{right_p.x, right_p.y, right_p.z, right_q.x, right_q.y, right_q.z, right_q.w, (right_grip) ? 1.0f:0.0f, (right_primary) ? 1.0f:0.0f, right_primary_v.x,right_primary_v.y, (right_secondary) ? 1.0f:0.0f, right_secondary_v.x, right_secondary_v.y});

        transforms["eye"].Add(new List<double>{(rayValid) ? 1.0f:0.0f, rayOrigin.x, rayOrigin.y, rayOrigin.z, rayDirection.x, rayDirection.y, rayDirection.z, (convDistValid) ? 1.0f:0.0f, convDistance});
    }

    void OnApplicationQuit() 
    {
        StreamWriter sw;
        FileStream fs;
        string j = JsonConvert.SerializeObject(transforms);
        fs = new FileStream(outputFn, FileMode.Create);
        sw = new StreamWriter(fs);
        sw.WriteLine(j);
        sw.Close();
        Debug.Log("QUit");
    }
}
