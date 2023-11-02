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

import pandas as pd
import matplotlib.pyplot as plt
import numpy as np
import math
import cv2
import os

def pngs(px, py, dx, dy, file_path, size, title=None, xlabel=None, ylabel=None):
    for i in range(0, size):
        plt.cla()
        xpoint = np.array(px[i])
        ypoint = np.array(py[i])
        plt.plot(xpoint, ypoint, color = 'teal')
        origin = np.array([xpoint, ypoint])
        xpoint = np.array(dx[i])
        ypoint = np.array(dy[i])
        plt.quiver(*origin, xpoint, ypoint, color=['b'])
        idx = "{:0>3d}".format(i)
        filename = file_path + idx
        plt.xlim(0, 10)
        plt.ylim(3, 10)
        plt.title(title)
        plt.savefig(filename)



def euler_from_quaternion(x1, y1, z1, w1):
    """
    Convert a quaternion into euler angles (roll, pitch, yaw)
    roll is rotation around x in radians (counterclockwise)
    pitch is rotation around y in radians (counterclockwise)
    yaw is rotation around z in radians (counterclockwise
    """
    # for x, y, z, w, in x1, y1, z1, w1:
    size = x1.shape[0]
    rx = np.zeros(size)
    ry = np.zeros(size)
    rz = np.zeros(size)
    for i in range(0, size):
        x = x1[i]
        y = y1[i]
        z = z1[i]
        w = w1[i]
        t0 = +2.0 * (w * x + y * z)
        t1 = +1.0 - 2.0 * (x * x + y * y)
        roll = math.atan2(t0, t1)

        t2 = +2.0 * (w * y - z * x)
        t2 = +1.0 if t2 > +1.0 else t2
        t2 = -1.0 if t2 < -1.0 else t2
        pitch = math.asin(t2)

        t3 = +2.0 * (w * z + x * y)
        t4 = +1.0 - 2.0 * (y * y + z * z)
        yaw = math.atan2(t3, t4)
        
        
        yawMatrix = np.matrix([
            [math.cos(yaw), -math.sin(yaw), 0],
            [math.sin(yaw), math.cos(yaw), 0],
            [0, 0, 1]
        ])

        pitchMatrix = np.matrix([
        [math.cos(pitch), 0, math.sin(pitch)],
        [0, 1, 0],
        [-math.sin(pitch), 0, math.cos(pitch)]
        ])

        rollMatrix = np.matrix([
        [1, 0, 0],
        [0, math.cos(roll), -math.sin(roll)],
        [0, math.sin(roll), math.cos(roll)]
        ])

        R = yawMatrix * pitchMatrix * rollMatrix

        theta = math.acos(((R[0, 0] + R[1, 1] + R[2, 2]) - 1) / 2)
        if (math.sin(theta)==0): multi = 0
        else : multi = 1 / (2 * math.sin(theta))

        rx[i] = multi * (R[2, 1] - R[1, 2]) * theta
        ry[i] = multi * (R[0, 2] - R[2, 0]) * theta
        rz[i] = multi * (R[1, 0] - R[0, 1]) * theta
    return rx, ry, rz

def pngToVideo(imageDir, savePath, frameRate):


    imageFolder = imageDir
    videoName = savePath

    images = [img for img in os.listdir(imageFolder) if img.endswith(".png")]
    frame = cv2.imread(os.path.join(imageFolder, images[0]))
    height, width, layers = frame.shape


    video = cv2.VideoWriter(videoName, 0, frameRate, (width,height))

    images.sort()
    for image in images:

        video.write(cv2.imread(os.path.join(imageFolder, image)))

    cv2.destroyAllWindows()
    video.release()

def visualizeCSV(inputPath, savingPath, startingFrame, length, frameRate):
    df = pd.read_csv(inputPath)
    df = df.replace(r'^\s*$', np.nan, regex=True)
    df = df.astype(float)
    
    endingFrame = startingFrame + length
    draw_range = df.iloc[startingFrame:endingFrame]
    headx, heady, headz = euler_from_quaternion(draw_range['Headq_x'].values, draw_range['Headq_y'].values,
                                                draw_range['Headq_z'].values, draw_range['Headq_w'].values)
    # x, y, z
    rightx, righty, rightz = euler_from_quaternion(draw_range['Rightq_x'].values, draw_range['Rightq_y'].values,
                                                draw_range['Rightq_z'].values, draw_range['Rightq_w'].values)
    leftx, lefty, leftz = euler_from_quaternion(draw_range['Leftq_x'].values, draw_range['Leftq_y'].values,
                                                draw_range['Leftq_z'].values, draw_range['Leftq_w'].values)
    
    p = 'head'
    p = os.path.join(savingPath, p)
    os.mkdir(p)
    savepath = p + '/'
    pngs(draw_range['Headp_x'].values, 
        draw_range['Headp_z'].values, 
        headx, headz, savepath, length, title='HMD', xlabel='Headp_x', ylabel='Headp_z')
    videoName = p + '.avi'
    pngToVideo(savepath, videoName, frameRate)


    p = 'left'
    p = os.path.join(savingPath, p)
    os.mkdir(p)
    savepath = p + '/'
    pngs(draw_range['Leftp_x'].values, 
        draw_range['Leftp_z'].values, 
        leftx, leftz, savepath, length, title='Left Hand', xlabel='Leftp_x', ylabel='Leftp_z')
    videoName = p + '.avi'
    pngToVideo(savepath, videoName, frameRate)
    
    p = 'right'
    p = os.path.join(savingPath, p)
    os.mkdir(p)
    savepath = p + '/'
    pngs(draw_range['Rightp_x'].values, 
        draw_range['Rightp_z'].values, 
        rightx, rightz, savepath, length, title='Right Hand', xlabel='Rightp_x', ylabel='Rightp_z')
    videoName = p + '.avi'
    pngToVideo(savepath, videoName, frameRate)

    p = 'eye'
    p = os.path.join(savingPath, p)
    os.mkdir(p)
    savepath = p + '/'
    pngs(draw_range['Eye_orix'].values, 
        draw_range['Eye_oriz'].values, 
        draw_range['Eye_dirx'].values, 
        draw_range['Eye_dirz'].values, savepath, length, title='Eye', xlabel='Eye_orix', ylabel='Eye_oriz')
    videoName = p + '.avi'
    pngToVideo(savepath, videoName, frameRate)