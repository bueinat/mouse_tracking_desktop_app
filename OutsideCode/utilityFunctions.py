# %%
import numpy as np
import cv2
import pandas
import imutils
from numpy_ext import rolling_apply
import datetime

import os
import shutil
import sys
import mongoengine as mnge
from utilityFunctions import *

import scipy.ndimage as ndimage
import scipy.ndimage.filters as filters

# suppress all warnings
import warnings
warnings.filterwarnings("ignore")

# mainly taken from [here](https://www.pyimagesearch.com/2015/05/25/basic-motion-detection-and-tracking-with-python-and-opencv/).
# this code roughly finds the path in which the rat went. It goes over the frames and tries to detect from the edges where the rat is


def video_to_frames(video_path, frames_path, video_name=None, include_video_name=False):
    # create filename if doesn't exist and capture whole video
    if video_name == None:
        video_name = video_path.split('/')[-1].split('.')[0]
    vidcap = cv2.VideoCapture(video_path)
    os.makedirs(frames_path, exist_ok=True)
    count = 0

    # make sure the video wasn't extracted already
    success, image = vidcap.read()
    frame_name = f"{frames_path}\\{video_name}_frame{count}.jpg" if include_video_name else f"{frames_path}\\frame{count}.jpg"
    if os.path.exists(frame_name):
        raise Exception('the video was already extracted')

    # write all frames to images
    while success:
        frame_name = f"{frames_path}\\{video_name}_frame{count}.jpg" if include_video_name else f"{frames_path}\\frame{count}.jpg"
        s = cv2.imwrite(frame_name, image)
        if not s:
            raise Exception(f'frame {count} was not written')
        success, image = vidcap.read()
        count += 1
# %%


def get_arena_limits(gray):
    thresh = cv2.threshold(
        gray, 0, 255, cv2.THRESH_OTSU + cv2.THRESH_BINARY)[1]

    # Find contour and sort by contour area
    cnts = cv2.findContours(thresh, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
    cnts = cnts[0] if len(cnts) == 2 else cnts[1]
    cnts = sorted(cnts, key=cv2.contourArea, reverse=True)

    # Find bounding box and extract ROI
    for c in cnts:
        x, y, w, h = cv2.boundingRect(c)
        break
    return {'ymin': y, 'ymax': y+h, 'xmin': x, 'xmax': x+w}


# %%
def find_contours(gray, alims, firstFrame):
    # compute the absolute difference between the current frame and first frame
    gray = gray[alims['ymin']:alims['ymax'], alims['xmin']:alims['xmax']]
    frameDelta = cv2.absdiff(firstFrame, gray)
    thresh = cv2.threshold(frameDelta, 25, 255, cv2.THRESH_BINARY)[1]

    # dilate the thresholded image to fill in holes, then find contours on thresholded image
    thresh = cv2.dilate(thresh, None, iterations=2)
    cnts = cv2.findContours(
        thresh.copy(), cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
    cnts = imutils.grab_contours(cnts)
    return cnts

# %%


def rat_path(video_path):
    vs = cv2.VideoCapture(video_path)
    firstFrame = None
    alims = None
    frame_num = 0
    rat_rects = {}
    frames = []

    # loop over the frames of the video
    while True:
        # grab the current frame and initialize the occupied / unoccupied text
        frame = vs.read()[1]
        frame_num += 1
        # if the frame could not be grabbed, then we have reached the end of the video
        if frame is None:
            break

        # resize the frame, convert it to grayscale, and blur it
        frame = imutils.resize(frame, width=500)
        frames.append(frame.copy())
        gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
        gray = cv2.GaussianBlur(gray, (21, 21), 0)

        # if the first frame is None, initialize it
        if firstFrame is None:
            alims = get_arena_limits(gray)
            firstFrame = gray[alims['ymin']:alims['ymax'],
                              alims['xmin']:alims['xmax']]
            continue

        cnts = find_contours(gray, alims, firstFrame)
        # find the rat
        for c in cnts:
            # if the contour is too small, ignore it
            if cv2.contourArea(c) < 0.1:
                continue
            # compute the bounding box for the contour
            (x, y, w, h) = cv2.boundingRect(c)
            rat_rects[frame_num] = {'x': x, 'y': y,
                                    'w': w, 'h': h, 'm': (w + h) / 2}

    # cleanup the camera and close any open windows
    vs.release()
    cv2.destroyAllWindows()

    return frames, rat_rects, alims


def alternative_rat_path(video_path):
    vs = cv2.VideoCapture(video_path)
    prevFrame = None
    frame_num = 0
    nose_pos = {}
    frames = []
    maxvals = {}
    edited_frames = []

    # loop over the frames of the video
    while True:
        # grab the current frame and initialize the occupied / unoccupied text
        frame = vs.read()[1]
        frame_num += 1
        # if the frame could not be grabbed, then we have reached the end of the video
        if frame is None:
            break
        
        k, sigma = 9, 10
        frames.append(frame.copy())
        frame = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
        frame = frame - np.mean(frame)
        frame = cv2.GaussianBlur(frame, (k, k), sigma)
        edited_frames.append(frame)
        
        # if the first frame is None, initialize it
        if prevFrame is None:
            prevFrame = frame
            continue

        diff = frame - prevFrame
        tail = np.unravel_index(diff.argmax(), diff.shape)
        head = np.unravel_index(diff.argmin(), diff.shape)
#         try:
#         if len(nose_pos) == 0 or dist(head, nose_pos[list(nose_pos.keys())[-1]]) < 100:
        nose_pos[frame_num] = head
        maxvals[frame_num] = diff.min()
#         except:
#             nose_pos[frame_num] = head
        prevFrame = frame.copy()

    # cleanup the camera and close any open windows
    vs.release()
    cv2.destroyAllWindows()

    return frames, edited_frames, nose_pos, maxvals

def find_closest_local_minima(diff_frames, last_point, neighborhood_size=5, threshold=1.5, maxval=-1,
                              return_all=False):
    data = - np.where(diff_frames < maxval, diff_frames, 0)
    
    data_max = filters.maximum_filter(data, neighborhood_size)
    data_min = filters.minimum_filter(data, neighborhood_size)
    maxima = (data == data_max)
    diff = ((data_max - data_min) > threshold)
    maxima[diff == 0] = 0
    
    labeled, num_objects = ndimage.label(maxima)
    slices = ndimage.find_objects(labeled)
    x, y = [], []
    for dy, dx in slices:
        x.append((dx.start + dx.stop - 1) / 2)
        y.append((dy.start + dy.stop - 1) / 2)
    
    if len(x) == 0:
        if return_all:
            return x, y, np.NaN
        return np.NaN, np.NaN
    
    imin = np.argmin((np.array(x) - last_point[1]) ** 2 + (np.array(y) - last_point[0]) ** 2)
    if return_all:
        return x, y, imin
    return int(x[imin]), int(y[imin])

def find_closest_local_minima_wrapper(diff_frames, last_point, neighborhood_size=5, init_threshold=1.5, maxval=-1,
                              return_all=False):
    thresholds = [init_threshold]
    thr_step = 0.1
    # threshold = init_threshold
    x, y, imin = find_closest_local_minima(diff_frames, last_point, neighborhood_size, thresholds[-1], maxval, True)
    while len(x) not in range(3, 11):
        # print(thresholds[-1], end=", ")
        if len(x) > 10:
            new_threshold = thresholds[-1] + thr_step
        elif len(x) < 3:
            new_threshold = thresholds[-1] - thr_step
        if new_threshold in thresholds:
            thr_step /= 2
        else:
            thresholds.append(new_threshold)
            x, y, imin = find_closest_local_minima(diff_frames, last_point, neighborhood_size,
                                                   thresholds[-1], maxval, True)
    return find_closest_local_minima(diff_frames, last_point, neighborhood_size, thresholds[-1], maxval, return_all)

# %%
def aireal_dist(dfx, dfy):
    return np.sqrt((dfx.iloc[0] - dfx.iloc[-1]) ** 2 + (dfy.iloc[0] - dfy.iloc[-1]) ** 2)


# %%
# those are just for introducing purposes
# running the full classes ends up with errors because they don't know each other
class Analysis(mnge.Document):
    timestep = mnge.ListField(mnge.IntField())
    
class Video(mnge.Document):
    registered_date = mnge.DateTimeField(default=datetime.datetime.now)
    
#%%
class Analysis(mnge.Document):
    timestep = mnge.ListField(mnge.IntField(), required=True)
    
    x = mnge.ListField(mnge.FloatField(), required=True)
    y = mnge.ListField(mnge.FloatField(), required=True)
    vx = mnge.ListField(mnge.FloatField(), required=True)
    vy = mnge.ListField(mnge.FloatField(), required=True)
    ax = mnge.ListField(mnge.FloatField(), required=True)
    ay = mnge.ListField(mnge.FloatField(), required=True)
    curviness = mnge.ListField(mnge.FloatField(), required=True)
    
    path = mnge.ListField(mnge.StringField(required=True))
    is_sniffing = mnge.ListField(mnge.BooleanField(default=False))
    is_drinking = mnge.ListField(mnge.BooleanField(default=False))
    is_nose_casting = mnge.ListField(mnge.BooleanField(default=False))
    video = mnge.ReferenceField(Video)
    
    meta = {
        'db_alias': 'core',
        'collection': 'analysis'
    }

# %%


class Video(mnge.Document):
    registered_date = mnge.DateTimeField(default=datetime.datetime.now)
    name = mnge.StringField(required=True)
    length = mnge.IntField(required=True)
    description = mnge.StringField(required=True)
    link_to_data = mnge.StringField(required=True)
    analysis = mnge.ReferenceField(Analysis)
    
    meta = {
        'db_alias': 'core',
        'collection': 'videos'
    }
