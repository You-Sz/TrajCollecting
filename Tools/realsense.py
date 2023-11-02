# Copyright 2023 Yu-Szu Wei <weiyousz0328@gmail.com>, Xing Wei 
# <weixing@buaa.edu.cn>, Xing-Yi Zheng <sharren89776@gapp.nthu.edu.tw>,
# Cheng-Hsin Hsu <chsu@cs.nthu.edu.tw>, Chenyang Yang <cyyang@buaa.edu.cn>

# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at

#     http://www.apache.org/licenses/LICENSE-2.0

# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

###############################################
##      Open CV and Numpy integration        ##
###############################################

import pyrealsense2 as rs
import numpy as np
import time
import cv2
import sys
import subprocess
import os
import math
import threading
from collections import namedtuple
#from rs_skeleton.skeletontracker import skeletontracker
#import rs_skeleton.util as cm

record_flag = False
cur_time = None
close_camera = False
activity_type = None
camera_ready = threading.Event()
option_name = set_option = depth_sensor = supported_options = None
alpha, colormap = 0.0425, 'COLORMAP_JET'
mode = 'depth'
depth_map = None

def render_ids_3d(render_image, skeletons_2d, depth_map, depth_intrinsic, joint_confidence, joint_writer=None):
    thickness = 1
    text_color = (255, 255, 255)
    rows, cols, channel = render_image.shape[:3]
    distance_kernel_size = 5
    skeleton_list = []
    # calculate 3D keypoints and display them
    '''
    for skeleton_index in range(len(skeletons_2d)):
        skeleton_2D = skeletons_2d[skeleton_index]
        joints_2D = skeleton_2D.joints
        did_once = False
        joint_list = None ## record joints
        for joint_index in range(len(joints_2D)):
            if did_once == False:
                
                cv2.putText(
                    render_image,
                    "id: " + str(skeleton_2D.id),
                    (int(joints_2D[joint_index].x), int(joints_2D[joint_index].y - 30)),
                    cv2.FONT_HERSHEY_SIMPLEX,
                    0.55,
                    text_color,
                    thickness,
                )
                
                joint_list = list()

                did_once = True
            # check if the joint was detected and has valid coordinate
            if skeleton_2D.confidences[joint_index] > joint_confidence:
                distance_in_kernel = []
                low_bound_x = max(
                    0,
                    int(
                        joints_2D[joint_index].x - math.floor(distance_kernel_size / 2)
                    ),
                )
                upper_bound_x = min(
                    cols - 1,
                    int(joints_2D[joint_index].x + math.ceil(distance_kernel_size / 2)),
                )
                low_bound_y = max(
                    0,
                    int(
                        joints_2D[joint_index].y - math.floor(distance_kernel_size / 2)
                    ),
                )
                upper_bound_y = min(
                    rows - 1,
                    int(joints_2D[joint_index].y + math.ceil(distance_kernel_size / 2)),
                )
                for x in range(low_bound_x, upper_bound_x):
                    for y in range(low_bound_y, upper_bound_y):
                        distance_in_kernel.append(depth_map.get_distance(x, y))
                median_distance = np.percentile(np.array(distance_in_kernel), 50)
                depth_pixel = [
                    int(joints_2D[joint_index].x),
                    int(joints_2D[joint_index].y),
                ]
                if median_distance > 0.3:
                    point_3d = rs.rs2_deproject_pixel_to_point(
                        depth_intrinsic, depth_pixel, median_distance
                    )
                    point_3d = np.round([float(point_3d[0]), float(-point_3d[1]), float(-point_3d[2])], 3)
                    point_str = [x for x in point_3d]
                    
                    cv2.putText(
                        render_image,
                        str(point_3d),
                        (int(joints_2D[joint_index].x), int(joints_2D[joint_index].y)),
                        cv2.FONT_HERSHEY_DUPLEX,
                        0.4,
                        text_color,
                        thickness,
                    )
                    
                    joint_list.append(point_str)
                else:
                    joint_list.append([])
            else:
                joint_list.append([])
        skeleton_list.append(joint_list)
        if joint_writer:
            joint_writer.write_skeleton(skeleton_list)
    '''

def get_depthcolor_map(color_image=None, depth_image=None):
    global alpha, colormap
    # Apply colormap on depth image (image must be converted to 8-bit per pixel first)
    depth_colormap = cv2.applyColorMap(cv2.convertScaleAbs(depth_image, alpha=alpha), getattr(cv2, colormap))
    depth_colormap_dim = depth_colormap.shape
    color_colormap_dim = color_image.shape

    # If depth and color resolutions are different, resize color image to match depth image for display
    if depth_colormap_dim != color_colormap_dim:
        resized_color_image = cv2.resize(color_image, dsize=(depth_colormap_dim[1], depth_colormap_dim[0]), interpolation=cv2.INTER_AREA)
        images = np.hstack((resized_color_image, depth_colormap))
    else:
        images = np.hstack((color_image, depth_colormap))

    # Show images
    # cv2.namedWindow('RealSense', cv2.WINDOW_AUTOSIZE)
    # cv2.imshow('RealSense', depth_colormap)
    # cv2.waitKey(1)
    return depth_colormap

def start_camera():
    global record_flag, cur_time, close_camera, depth_sensor, supported_options, mode, depth_map
    depth_writer = color_writer = joint_writer = None
    skeleton_trace = []

    config = rs.config()
    pipeline = rs.pipeline()
    config.enable_stream(rs.stream.depth, 640, 480, rs.format.z16, 30)
    config.enable_stream(rs.stream.color, 640, 480, rs.format.bgr8, 30)

    profile = pipeline.start(config)
    depth_sensor = profile.get_device().first_depth_sensor()

    depth_sensor.set_option(rs.option.gain, 16.)
    depth_sensor.set_option(rs.option.enable_auto_exposure, 1.)
    depth_sensor.set_option(rs.option.laser_power, 360.)
    depth_sensor.set_option(rs.option.visual_preset, 5.)
    
    print()
    supported_options = depth_sensor.get_supported_options()
    for option in supported_options:
        range = depth_sensor.get_option_range(option)
        print(f'{option}={depth_sensor.get_option(option)}\tdes={depth_sensor.get_option_value_description(option, depth_sensor.get_option(option))}\tdefault={range.default}\trange={range.min}-{range.max}\tstep={range.step}')
    print()
    supported_options = list(map(lambda x: str(x)[7:], supported_options))

    # # Create align object to align depth frames to color frames
    align = rs.align(rs.stream.color)
    # # Get the intrinsics information for calculation of 3D point
    # unaligned_frames = pipeline.wait_for_frames()
    # frames = align.process(unaligned_frames)
    # depth = frames.get_depth_frame()
    # depth_intrinsic = depth.profile.as_video_stream_profile().intrinsics

    # Initialize the cubemos api with a valid license key in default_license_dir()
    #skeletrack = skeletontracker(cloud_tracking_api_key="")
    joint_confidence = 0.2

    depth_map = np.empty((4000, 480, 640))
    idx = 0

    ## video writer
    fourcc = cv2.VideoWriter_fourcc(*"mp4v")
    window_name = "cubemos skeleton tracking with realsense D400 series"
    cv2.namedWindow(window_name, cv2.WINDOW_NORMAL + cv2.WINDOW_KEEPRATIO)
    camera_ready.set()
    try:
        while True:
            # Wait for a coherent pair of frames: depth and color
            unaligned_frames = pipeline.wait_for_frames()
            frames = align.process(unaligned_frames)
            depth_frame = frames.get_depth_frame() ## depth frame and intrinstics should update every frame, in case some depth info. are missing
            depth_intrinsic = depth_frame.profile.as_video_stream_profile().intrinsics
            color_frame = frames.get_color_frame()
            
            if not depth_frame or not color_frame:
                continue
            # if record_flag:
            #     # logf.write(str(int(time.time() * (10**5)))+'\n')
            #     logf.write(str(int(depth_frame.get_timestamp()))+'\n')
            # Convert images to numpy arrays
            depth_image = np.asanyarray(depth_frame.get_data())
            color_image = np.asanyarray(color_frame.get_data())
            depth_colormap = get_depthcolor_map(color_image, depth_image)

            #skeletons = skeletrack.track_skeletons(color_image)
            #cm.render_result(skeletons, color_image, joint_confidence)
            #color_image = cv2.cvtColor(color_image, cv2.COLOR_BGR2RGB)
            
            # check record or not
            if record_flag:
                if not (depth_writer and color_writer):
                    depth_writer = cv2.VideoWriter(
                        f'{activity_type}_depth.mp4', fourcc, 30, (640, 480), True)
                    color_writer = cv2.VideoWriter(
                        f'{activity_type}.mp4', fourcc, 30, (640, 480), True)
                    #joint_writer = cm.JointWriter(output_path = f'{activity_type}_skeleton_{cur_time}.json')

                #joint_writer.time_stamp = str(int(time.time() * (10**5)))
            else:
                depth_writer = color_writer = joint_writer = None

            '''render_ids_3d(
                color_image, skeletons, depth_frame, depth_intrinsic, joint_confidence, joint_writer
            )'''
            
            if record_flag and (depth_writer and color_writer):
                logf.write(str(int(depth_frame.get_timestamp()))+'\n')
                depth_writer.write(depth_colormap)
                # depth_map = np.append(depth_map, [depth_image], axis=0)
                depth_map[idx] = depth_image
                idx += 1
                color_writer.write(color_image)
                cv2.putText(color_image if mode == 'rgb' else depth_colormap, "Recording", (20,30), cv2.FONT_HERSHEY_DUPLEX, 0.6, (0, 0, 255), 1,)

            if close_camera:
                break
            
            cv2.imshow(window_name, color_image if mode == 'rgb' else depth_colormap)
            if cv2.waitKey(1) == 27:
                break
            
    finally:
        # Stop streaming
        pipeline.stop()
        cv2.destroyAllWindows()

def main():
    global record_flag, cur_time, close_camera, activity_type, logf, option_name, set_option, depth_sensor, supported_options, alpha, colormap, mode, depth_map
    mmwave_proc = None
    camera_thread = threading.Thread(target=start_camera)
    camera_thread.start()
    camera_ready.wait()
    while True:
        c = input('Start (s), Stop (t), Quit (q): ').replace('\n', '').replace(' ', '')
        if c == "s":
            if not mmwave_proc:
                activity_type = input("Type the filename:")
                cur_time = str(int(time.time() * (10**5)))
                #output_path = f'{activity_type}_mmw_{cur_time}.txt'
                #f = open(output_path, 'w')
                #mmwave_proc = subprocess.Popen(["rostopic", "echo", "/ti_mmwave/radar_scan"], stdout=f)
                video_log_path = f'{activity_type}.txt'
                logf = open(video_log_path, 'w')
                record_flag = True
                
        elif c == "t":
            record_flag = False
            if mmwave_proc:
                mmwave_proc.kill()
                mmwave_proc = None
                #f.close()
                logf.close()
                np.save(f'{activity_type}.npy', depth_map)
            else:
                print("No Running mmWave Process")

        elif c == "q":
            close_camera = True
            camera_thread.join()
            print("Quit")
            break

        elif c:
            if c.count("="):
                option_name, set_option = c[:c.index("=")], c[c.index("=")+1:]
                if option_name in supported_options:
                    depth_sensor.set_option(getattr(rs.option, option_name), float(set_option))
                    option = depth_sensor.get_option(getattr(rs.option, option_name))
                    print(f'Start (s), Stop (t), Quit (q): {option}, des={depth_sensor.get_option_value_description(getattr(rs.option, option_name), option)}')
                elif option_name == "alpha":
                    alpha = float(set_option)
                elif option_name == "colormap":
                    colormap = set_option
                elif option_name == "mode":
                    mode = set_option
            else:
                option_name = c
                if option_name in supported_options:
                    option = depth_sensor.get_option(getattr(rs.option, option_name))
                    print(f'Start (s), Stop (t), Quit (q): {option}, des={depth_sensor.get_option_value_description(getattr(rs.option, option_name), option)}')
                elif option_name == "alpha":
                    print(f'Start (s), Stop (t), Quit (q): {alpha}')
                elif option_name == "colormap":
                    print(f'Start (s), Stop (t), Quit (q): {colormap}')
                elif option_name == "mode":
                    print(f'Start (s), Stop (t), Quit (q): {mode}')


if __name__=="__main__":
    main()

# #%%
# import numpy as np
# depth_map = np.load('a01_s01_r01_depthmap_167135356384988.npy')
# print(depth_map.shape)
# for i in range(1811, 1999):
#     print(depth_map[i])