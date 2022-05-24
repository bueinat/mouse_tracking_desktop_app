# %% [markdown]
# # Computer Vision
#
# 16/11/2021
# read images from video and analyze them.
#

# TODO:
# * allow user to add input (like video path)
# * change the archive path
# * show error in dialog
# * manage to also add the suffix
# * start with frame0

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

# %%
try:
    if len(sys.argv) == 1:
        VIDEO_PATH = 'videos/examples/Odor28.avi'
    else:
        VIDEO_PATH = sys.argv[1]
    FUNCTION_NAME = 'alternative_rat_path'
    MAXD = 10
    VIDEO_NAME = VIDEO_PATH.split('\\')[-1].split('.')[0]

    # os.chdir('cv')
    archive_path = f".\\archive"
    os.makedirs(archive_path, exist_ok=True)
    video_name = VIDEO_PATH.split('\\')[-1].split('.')[0]
    data_path = f"{archive_path}\\{video_name}"
    frames_path = f".\\archive\\{video_name}\\frames"
    print(f"archive path: {os.getcwd()}")
    print(f"video path: {os.getcwd()}\\archive\\{video_name}")
    video_path = f"{os.getcwd()}\\archive\\{video_name}"
    print(f"nframes: {len(os.listdir(frames_path))}")
    print("video id: 625862ccddd13c6e2add1ec3")
    try:
        os.mkdir(data_path)
    except FileExistsError:
        raise FileExistsError(
            f'a video named {video_name} already exists in archive. You can use it or give the new video a different name')
    shutil.copy2(VIDEO_PATH, data_path)
    frames_path = f".\\archive\\{video_name}\\frames"
    nframes = video_to_frames(VIDEO_PATH, frames_path)
    print(f"frames path: {os.getcwd()}{frames_path[1:]}")
    print(f"nframes: {nframes}")

    # %%
    # extracting data from video
    if FUNCTION_NAME == 'rat_path':
        frames, rat_rects, alims = rat_path(VIDEO_PATH)

        # show track of rat in time
        raw_data = pandas.DataFrame(rat_rects).T
   
    elif FUNCTION_NAME == 'alternative_rat_path':
        frames, eframes, nose_pos, max_vals = alternative_rat_path(VIDEO_PATH)
        dfnose = pandas.DataFrame(nose_pos, index=['y', 'x']).T
        # dfnose.y = frames[0].shape[0] - dfnose.y
        dfnose = pandas.concat([dfnose, pandas.Series(max_vals)], axis=1)
        dfnose = dfnose[['x', 'y', 0]].rename(columns={0: 'minvals'})
        dfnose.index.name = 'timestep'
        dfnose['d'] = np.sqrt(dfnose.x.diff() ** 2 + dfnose.y.diff() ** 2)

        # cut the first frames
        first_appearance = dfnose[dfnose.d < MAXD].index[0]
        dfnose.loc[:first_appearance - 1, 'd'] = 100

        # if the mouse is too far, set its position to the previous
        dfnose.loc[dfnose.eval(f'd > {MAXD}'), ['x', 'y']] = np.NaN
        dfnose = dfnose.fillna(method='ffill').fillna(method='bfill')
        dfnose['d'] = np.sqrt(dfnose.x.diff() ** 2 + dfnose.y.diff() ** 2).fillna(0)
    
        dfnose['replace'] = False
        dfnose.loc[dfnose.query(f'd > {MAXD}').index, 'replace'] = True
        # print((dfnose.query(f'd > {MAXD}').reset_index()['timestep'].diff().dropna() == 1).any())
    
        dfnose['new_x'] = dfnose.x
        dfnose['new_y'] = dfnose.y

        for idx in dfnose.query('replace').index:
            if idx < dfnose.index[-1]:
                new_point = find_closest_local_minima_wrapper(eframes[idx] - eframes[idx-1], nose_pos[idx-1],
                                                            init_threshold=1)
                dfnose.loc[idx, 'new_x'] = new_point[0]
                dfnose.loc[idx, 'new_y'] = new_point[1]

        dfnose['new_d'] = np.sqrt(dfnose.new_x.diff() ** 2 + dfnose.new_y.diff() ** 2).fillna(0)
    
        dfnose['real_x'] = dfnose.x
        dfnose.loc[dfnose.d > dfnose.new_d, 'x'] = dfnose.new_x
        dfnose['real_y'] = dfnose.y
        dfnose.loc[dfnose.d > dfnose.new_d, 'y'] = dfnose.new_y
        dfnose['real_d'] = np.sqrt(dfnose.real_x.diff() ** 2 + dfnose.real_y.diff() ** 2).fillna(0)
    
        raw_data = dfnose[['real_x', 'real_y']].rename(columns={'real_x': 'x', 'real_y': 'y'})

    raw_data.index.name = 'timestep'
    raw_data['time'] = raw_data.index
    path = raw_data.set_index('x').y

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
    dists = np.sqrt(raw_data.x.diff() ** 2 + raw_data.y.diff() ** 2)

    win_size = 100

    raw_data["adist"] = rolling_apply(
        aireal_dist, win_size, raw_data.x, raw_data.y)
    raw_data["rdist"] = raw_data.r.rolling(win_size).sum()

    raw_data = raw_data.fillna(method='backfill')

    # %% [markdown]
    # ### Upload data to server

    # %%
    # the commented line is used for server storage, but we prefer saving on this computer.
    # cluster = "mongodb+srv://john:1234@cluster0.9txls.mongodb.net/real_test?retryWrites=true&w=majority" 
    # in order to run this, you have first to run MongoDB server using:
    # `C:\Program Files\MongoDB\Server\5.0\bin\mongo.exe`
    cluster = "mongodb://127.0.0.1:27017/test_dbs"    
    mnge.register_connection(alias='core', host=cluster)

    # %%
    video = Video()
    video.name = VIDEO_PATH.split('\\')[-1] # VIDEO_NAME # VIDEO_PATH.split('/')[-1]
    # video.length = VideoFileClip(VIDEO_PATH).duration
    video.length = 10.1
    video.nframes = len(os.listdir(frames_path))

    working_path = os.getcwd()
    video.description = "dummy video\nthis is just meant for testing."
    video.link_to_data = f"{working_path}\\{data_path[2:]}"
    video.save()

    print(f"video id: {video.id}")

    # %%
    uploadabale_data = raw_data[['x', 'y', 'vx', 'vy', 'ax', 'ay']]
    uploadabale_data.loc[:, 'curviness'] = raw_data.adist / raw_data.rdist
    uploadabale_data['path'] = [f"{working_path}\\{frames_path[2:]}\\frame{i}.jpg"
                                for i in uploadabale_data.index]
    uploadabale_data.index -= 1
    
    # uploadabale_data.index -= 1
    # uploadabale_data['path'] = [f"{working_path}\\{frames_path[2:]}\\frame{i-uploadabale_data.index[0]}.jpg"
    #                             for i in uploadabale_data.index]

    ### I read dummy predictions
    pred_df = pandas.read_csv('C:/Users/buein/OneDrive - Bar-Ilan University/שנה ג/פרוייקט שנתי/mouse_tracking/cv/videos/examples/testing_project_deepethogram/DATA/odor28/odor28_predictions.csv',
                              index_col=0).drop('background',1).astype(bool)[1:]
    pred_df.columns = pred_df.columns.map(lambda s: "is_" + s.replace(' ', '_'))
    # pred_df.index += 1

    if len(uploadabale_data) != len(pred_df):
        raise Exception("features and path files are not in the same length. Are you sure they were generated for the same video?")

    uploadabale_data = pandas.concat([uploadabale_data, pred_df], axis=1)

    # %%
    video = Video.objects(id=video.id).first()
    ana = Analysis()
    ana.timestep = list(uploadabale_data.index)

    for c in uploadabale_data.columns:
        exec(f"ana.{c} = list(uploadabale_data['{c}'])")

    ana.video = video.id
    video.analysis = ana
    ana.save()
    video.update(analysis=ana.id)
    video.save()
except Exception as e:
    print(f"error: {e.__class__.__name__}: {e}")