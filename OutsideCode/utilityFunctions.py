# %%
import numpy as np
import cv2
from numpy_ext import rolling_apply
import datetime

import os
import shutil
import mongoengine as mnge
from PIL import Image

# suppress all warnings
import warnings
warnings.filterwarnings("ignore")

# mainly taken from [here](https://www.pyimagesearch.com/2015/05/25/basic-motion-detection-and-tracking-with-python-and-opencv/).
# this code roughly finds the path in which the rat went. It goes over the frames and tries to detect from the edges where the rat is


def video_to_frames(video_path, frames_path, video_name=None, include_video_name=False, override=False):
    # create filename if doesn't exist and capture whole video
    if video_name == None:
        video_name = video_path.split('/')[-1].split('.')[0]
    vidcap = cv2.VideoCapture(video_path)
    if os.path.exists(frames_path) and len(os.listdir(frames_path)) != 0:
        print("frames directory already exists")
        if override:
            shutil.rmtree(frames_path)
            os.makedirs(frames_path)
        else:
             raise FileExistsError(f"cache contains extracted frames already and override is false")
    else:
        os.makedirs(frames_path, exist_ok=True)
    count = 0

    # make sure the video wasn't extracted already
    success, image = vidcap.read()
    frame_name = u"{}\\frame{}.jpg".format(frames_path, count)
    if os.path.exists(frame_name):
        raise Exception('the video was already extracted')

    # write all frames to images
    while success:
        frame_name = u"{}\\frame{}.jpg".format(frames_path, count)
        Image.fromarray(image).save(frame_name)
        success, image = vidcap.read()
        count += 1
    return count


# %%
def analyze_raw_data(raw_data):
    raw_data.index.name = 'timestep'
    raw_data['time'] = raw_data.index
    # path = raw_data.set_index('x').y

    # %%
    raw_data['vx'] = raw_data.x.diff() / raw_data.time.diff()
    raw_data['vy'] = raw_data.y.diff() / raw_data.time.diff()
    raw_data['r_tot'] = np.sqrt((raw_data.x - raw_data.x.iloc[0]) ** 2 + (raw_data.y - raw_data.y.iloc[0]) ** 2)
    raw_data['r'] = np.sqrt(raw_data.x.diff() ** 2 + raw_data.y.diff() ** 2)
    raw_data['v'] = np.sqrt(raw_data.vx ** 2 + raw_data.vy ** 2)

    raw_data['ax'] = raw_data.vx.diff() / raw_data.time.diff()
    raw_data['ay'] = raw_data.vy.diff() / raw_data.time.diff()
    raw_data['a'] = np.sqrt(raw_data.ax ** 2 + raw_data.ay ** 2)

    # %%
    win_size = 100

    raw_data["adist"] = rolling_apply(aireal_dist, win_size, raw_data.x, raw_data.y)
    raw_data["rdist"] = raw_data.r.rolling(win_size).sum()
    raw_data['curviness'] = (raw_data.adist / raw_data.rdist).shift(-win_size // 2)

    raw_data = raw_data.fillna(method='bfill').fillna(method='ffill')
    return raw_data # , path

# %%
def aireal_dist(dfx, dfy):
    return np.sqrt((dfx.iloc[0] - dfx.iloc[-1]) ** 2 + (dfy.iloc[0] - dfy.iloc[-1]) ** 2)

# %%
# those are just for introducing purposes
# running the full classes ends up with errors because they don't know each other
class Analysis(mnge.Document):
    timestep = mnge.ListField(mnge.IntField())
    
class Video(mnge.Document):
    registration_date = mnge.DateTimeField(default=datetime.datetime.now)
    
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
    registration_date = mnge.DateTimeField(default=datetime.datetime.now)
    modification_date = mnge.DateTimeField()
    name = mnge.StringField(required=True)
    nframes = mnge.IntField(reqiured=True)
    length = mnge.StringField(required=True)
    description = mnge.StringField(required=True)
    link_to_data = mnge.StringField(required=True)
    analysis = mnge.ReferenceField(Analysis)
    
    meta = {
        'db_alias': 'core',
        'collection': 'videos'
    }
