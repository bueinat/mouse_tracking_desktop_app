# %% [markdown]
# # Computer Vision
#
# 16/11/2021
# read images from video and analyze them.

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
    # get run arguments
    args = pandas.read_csv(sys.argv[1], header=None, index_col=0)[1]

    args["override"] = eval(args["override"])
    FUNCTION_NAME = 'alternative_rat_path'
    MAXD = 10

    os.makedirs(args["cache_path"], exist_ok=True)
    video_name = args["video_path"].split('\\')[-1].split('.')[0]
    data_path = f"{args['cache_path']}\\{video_name}"
    working_path = f"@WORKING_PATH\\{video_name}"
    frames_path = u"{}\\{}\\frames".format(args['cache_path'], video_name) # os.path.join(f"{args['cache_path']}\\{video_name}", u"frames")


    # create a folder for cache
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

    # copy video to cache and extract frames
    if not os.path.exists(os.path.join(args["video_path"], data_path)) or args["override"]:
        shutil.copy2(args["video_path"], data_path)
    if not os.path.exists(frames_path) or len(os.listdir(frames_path)) == 0 or args["override"]:
        nframes = video_to_frames(args["video_path"], frames_path, override=args['override'])
    else:
        nframes = len(os.listdir(frames_path))
    print(f"nframes: {nframes}")


    # extracting data from video
    if FUNCTION_NAME == 'rat_path':
        frames, rat_rects, alims = rat_path(args["video_path"])

        # show track of rat in time
        raw_data = pandas.DataFrame(rat_rects).T
   
    elif FUNCTION_NAME == 'alternative_rat_path':
        frames, eframes, nose_pos, max_vals = alternative_rat_path(args["video_path"])
        get_raw_data(nose_pos, max_vals, eframes)

    raw_data = analyze_raw_data(raw_data)
    
    # create uploadabe data
    uploadabale_data = raw_data[['x', 'y', 'vx', 'vy', 'ax', 'ay']]
    uploadabale_data.loc[:, 'curviness'] = raw_data.adist / raw_data.rdist
    frames_path = f"{working_path}\\frames"
    print(f"frames_path: {frames_path}")
    print(uploadabale_data.index[0])
    uploadabale_data['path'] = [f"{frames_path}\\frame{i}.jpg"
                                for i in uploadabale_data.index]

    ### I read dummy predictions
    # TODO: read real predictions 
    pred_df = pandas.read_csv('C:/Users/buein/OneDrive - Bar-Ilan University/שנה ג/פרוייקט שנתי/mouse_tracking/cv/videos/examples/testing_project_deepethogram/DATA/odor28/odor28_predictions.csv',
                              index_col=0).drop('background',1).astype(bool)
    pred_df.columns = pred_df.columns.map(lambda s: "is_" + s.replace(' ', '_'))

    if len(uploadabale_data) != len(pred_df):
        raise Exception("features and path files are not in the same length. Are you sure they were generated for the same video?")

    uploadabale_data = pandas.concat([uploadabale_data, pred_df], axis=1)


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
    video.name = video_name
    frame_rate = 45
    total_length = nframes / frame_rate
    td = datetime.timedelta(seconds=total_length)
    minutes, seconds = divmod(td.seconds, 60)
    millis = round(td.microseconds/1000, 0)
    video.length = f"{seconds:02}:{int(millis) // 10}"
    video.nframes = len(os.listdir(frames_path))
    video.modification_date = datetime.datetime.now

    # TODO: enable passing this as an argument
    video.description = "dummy video\nthis is just meant for testing."
    video.link_to_data = f"{working_path}\\{video_name}"

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