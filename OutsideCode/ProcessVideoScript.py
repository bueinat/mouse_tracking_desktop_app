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
    args = pandas.read_csv(sys.argv[1], header=None, index_col=0)[1]

    args["override"] = eval(args["override"])
    # OVERRIDE = eval(sys.argv[1])
    # CONNECTION_STRING = sys.argv[2]
    # args["video_path"] = sys.argv[3]
    # args["archive_path"] = sys.argv[4]
    FUNCTION_NAME = 'alternative_rat_path'
    MAXD = 10

    os.makedirs(args["archive_path"], exist_ok=True)
    video_name = args["video_path"].split('\\')[-1].split('.')[0]
    # data_path = os.path.join(args['archive_path'], video_name)
    data_path = f"{args['archive_path']}\\{video_name}"
    working_path = f"@WORKING_PATH\\{video_name}"
    frames_path = u"{}\\{}\\frames".format(args['archive_path'], video_name) # os.path.join(f"{args['archive_path']}\\{video_name}", u"frames")
    # frames_path = f"{args['archive_path']}\\{video_name}\\frames"

    # print(args["connection_string"])
    # print(f"archive path: {os.getcwd()}")
    # print(f"video path: {os.getcwd()}\\archive\\{video_name}")
    # print(f"nframes: {len(os.listdir(frames_path))}")
    # print("video id: 625862ccddd13c6e2add1ec3")
    
    # print(OVERRIDE, CONNECTION_STRING)
    # print(args["video_path"])
    # print(args["archive_path"], FUNCTION_NAME, VIDEO_NAME, data_path, frames_path)
    # if os.path.exists(data_path):

    try:
        os.mkdir(data_path)
        args["override"] = False
    except Exception as e:
        if args["override"]:
            print(f"message: note: overriding {video_name} which already existed in the archive.")
        else:
            mnge.register_connection(alias='core', host=args["connection_string"])
            print(Video.objects)
            print(len(Video.objects), video_name)
            videoq = Video.objects(name=video_name).order_by('-registered_date')
            # return an already existing video
            if len(videoq) > 0:
                video_id = videoq.first().id
                print(f"video id: {video_id}")
                raise FileExistsError(f"message: loading existing data of {video_name} which already exists in the archive.")
            else:
                os.makedirs(data_path, exist_ok=True)

    if not os.path.exists(os.path.join(args["video_path"], data_path)) or args["override"]:
        shutil.copy2(args["video_path"], data_path)
    if not os.path.exists(frames_path) or len(os.listdir(frames_path)) == 0 or args["override"]:
        nframes = video_to_frames(args["video_path"], frames_path, override=args['override'])
    else:
        nframes = len(os.listdir(frames_path))
    print(f"nframes: {nframes}")

    # raise Exception("done.")
    # %%
    # extracting data from video
    if FUNCTION_NAME == 'rat_path':
        frames, rat_rects, alims = rat_path(args["video_path"])

        # show track of rat in time
        raw_data = pandas.DataFrame(rat_rects).T
   
    elif FUNCTION_NAME == 'alternative_rat_path':
        frames, eframes, nose_pos, max_vals = alternative_rat_path(args["video_path"])
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
    print(raw_data.index[0])
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
    mnge.register_connection(alias='core', host=args["connection_string"])

    # %%
    video = Video()
    video.name = video_name # args["video_path"].split('\\')[-1]
    frame_rate = 45
    total_length = nframes / frame_rate
    td = datetime.timedelta(seconds=total_length)
    minutes, seconds = divmod(td.seconds, 60)
    millis = round(td.microseconds/1000, 0)
    video.length = f"{seconds:02}:{int(millis) // 10}"
    video.nframes = len(os.listdir(frames_path))
    video.modification_date = datetime.datetime.now

    video.description = "dummy video\nthis is just meant for testing."
    video.link_to_data = f"{working_path}\\{video_name}" # data_path

    update_video = Video.objects(name=video_name).order_by('-registered_date')
    print(args["override"], len(update_video))
    if args["override"] and len(update_video) > 0:
        update_video.update_one(set__length=video.length,
                                set__nframes=video.nframes,
                                set__modification_date=video.modification_date,
                                set__description=video.description,
                                set__link_to_data=video.link_to_data)
        video_id = update_video.first().id 
    else:
        video.save()
        video_id = video.id
    print(f"video id: {video_id}")

    # %%
    uploadabale_data = raw_data[['x', 'y', 'vx', 'vy', 'ax', 'ay']]
    uploadabale_data.loc[:, 'curviness'] = raw_data.adist / raw_data.rdist
    frames_path = f"{working_path}\\frames"
    print(f"frames_path: {frames_path}")
    print(uploadabale_data.index[0])
    uploadabale_data['path'] = [f"{frames_path}\\frame{i}.jpg"
                                for i in uploadabale_data.index]
    # print(uploadabale_data.path.values)
    # uploadabale_data.index -= 1
    
    # uploadabale_data.index -= 1
    # uploadabale_data['path'] = [f"{working_path}\\{frames_path[2:]}\\frame{i-uploadabale_data.index[0]}.jpg"
    #                             for i in uploadabale_data.index]

    ### I read dummy predictions
    # TODO: read real predictions 
    pred_df = pandas.read_csv('C:/Users/buein/OneDrive - Bar-Ilan University/שנה ג/פרוייקט שנתי/mouse_tracking/cv/videos/examples/testing_project_deepethogram/DATA/odor28/odor28_predictions.csv',
                              index_col=0).drop('background',1).astype(bool)
    pred_df.columns = pred_df.columns.map(lambda s: "is_" + s.replace(' ', '_'))
    # pred_df.index += 1

    if len(uploadabale_data) != len(pred_df):
        raise Exception("features and path files are not in the same length. Are you sure they were generated for the same video?")

    uploadabale_data = pandas.concat([uploadabale_data, pred_df], axis=1)

    # %%
    video = Video.objects(id=video_id).first()
    ana = Analysis()
    ana.timestep = list(uploadabale_data.index)

    for c in uploadabale_data.columns:
        exec(f"ana.{c} = list(uploadabale_data['{c}'])")

    if args["override"]:
        try:
            video.analysis.delete()
        except Exception:
            print(f"error: {e.__class__.__name__}: {e}")
            pass

    ana.video = video_id
    video.analysis = ana
    ana.save()
    video.update(analysis=ana.id)
    video.save()
    print(f"analysis id: {ana.id}")
except Exception as e:
    if str(e).startswith("message"):
        print(e)
    else:
        print(f"error: {e.__class__.__name__}: {e}")