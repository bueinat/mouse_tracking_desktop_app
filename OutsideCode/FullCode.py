import sys
import os
import shutil
import datetime

import pandas
import numpy as np
from numpy_ext import rolling_apply

import cv2
from PIL import Image

import torch
import mongoengine as mnge
# from utilityFunctions import *

import deepethogram # import configuration, postprocessing, projects
from deepethogram.sequence.inference import sequence_inference
from deepethogram.feature_extractor.inference import feature_extractor_inference

# suppress all warnings
import warnings
warnings.filterwarnings("ignore")

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
    is_noseCasting = mnge.ListField(mnge.BooleanField(default=False))
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
def aireal_dist(dfx, dfy):
    return np.sqrt((dfx.iloc[0] - dfx.iloc[-1]) ** 2 + (dfy.iloc[0] - dfy.iloc[-1]) ** 2)

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

def create_data_path_directory(args, video_name):
    try:
        os.makedirs(args["data_path"])
        print("created new directory")
        args["override"] = False
    except Exception as e:
        if args["override"]:
            print(f"message: note: overriding {video_name} which already existed in the archive.")
        else:
            mnge.register_connection(alias='core', host=args["connection_string"])
            videoq = Video.objects(link_to_data=args["video_path"], analysis__exists=1).order_by('-registered_date')
            # return an already existing video
            if len(videoq) > 0:
                video_id = videoq.first().id
                analysis_id = videoq.first().analysis.id
                print(f"message: loading existing data of {video_name} which already exists in the cache.")
                print(f"video id: {video_id}")
                print("success")
                exit(0)
            else:
                os.makedirs(args["data_path"], exist_ok=True)

def extract_video(args, video_name, frames_path):
    if not os.path.exists(os.path.join(args["data_path"], video_name)) or args["override"]:
        shutil.copy2(args["video_path"], args["data_path"])
    if not os.path.exists(frames_path) or len(os.listdir(frames_path)) == 0 or args["override"]:
        nframes = video_to_frames(args["video_path"], frames_path, override=args['override'])
    else:
        nframes = len(os.listdir(frames_path))
    print(f"nframes: {nframes}")
    print(f"override: {args['override']}")
    if nframes == 0:
        raise Exception("error occured when trying to extract video")
    return nframes

def run_inference(frames_path, nframes):
    PATH = "C:/ProgramData/MouseApp/yolov5/models/best_trained_model.pt"
    model = torch.hub.load('ultralytics/yolov5', 'custom', path=PATH)  # local model

    dscript_detect = {}
    for i, filename in enumerate(os.listdir(frames_path)):
        dscript_detect[int(filename.split(".")[0][5:])] =  model(f"{frames_path}/{filename}")
        if i % 100 == 0:
            print(f"progress: {i}/{nframes}")
    print(f"progress: {nframes}/{nframes}")
    return dscript_detect

def process_inference(dscript_detect, frames_path):
    dff = pandas.concat({key: dscript_detect[key].pandas().xyxy[0] for key in dscript_detect.keys()})
    dff.insert(0, 'confidence', dff.pop('confidence'))
    dff = dff.sort_values("confidence").query("name == 'nose'").groupby(level=0).last()
    dff["x"] = (dff.xmin + dff.xmax) / 2
    dff["y"] = (dff.ymin + dff.ymax) / 2
    dff["width"] = dff.xmax - dff.xmin
    dff["height"] = dff.ymax - dff.ymin

    df = dff[["x", "y", "width", "height"]]
    df.index.name = "timestep"
    df = df.reindex(np.arange(df.index.min(), df.index.max() + 1)).interpolate(method="pchip")
    raw_data = df.reindex(np.arange(0, len(dscript_detect))).fillna(method="bfill").fillna(method="ffill")[["x", "y"]]

    raw_data = analyze_raw_data(raw_data)
    
    # create uploadabe data
    raw_data['path'] = [f"{frames_path}\\frame{i}.jpg"
                                    for i in raw_data.index]

    uploadable_data = raw_data[['x', 'y', 'vx', 'vy', 'ax', 'ay', 'curviness', 'path']]
    return uploadable_data

def create_video_object(video_name, nframes, video_path):
    video = Video()
    video.name = video_name
    frame_rate = 45
    total_length = nframes / frame_rate
    td = datetime.timedelta(seconds=total_length)
    minutes, seconds = divmod(td.seconds, 60)
    millis = round(td.microseconds/1000, 0)
    video.length = f"{seconds:02}:{int(millis) // 10}"
    video.nframes = nframes
    video.modification_date = datetime.datetime.now

    # TODO: enable passing this as an argument
    video.description = "dummy video\nthis is just meant for testing."
    video.link_to_data = video_path
    return video

# %%
def feature_extractor_inference_config(project_path):
    preset = 'deg_f'
    cfg = deepethogram.configuration.make_feature_extractor_inference_cfg(project_path=project_path, preset=preset)
    cfg.feature_extractor.weights = 'latest'
    cfg.flow_generator.weights = 'latest'

    cfg.inference.overwrite = True
    # make sure errors are thrown
    cfg.inference.ignore_error = False
    cfg.compute.num_workers = 2
    return cfg

def sequence_inference_config(project_path):
    ncpus = 4
    cfg = deepethogram.configuration.make_sequence_inference_cfg(project_path)
    cfg.sequence.weights = 'latest'
    cfg.compute.num_workers = ncpus
    cfg.inference.overwrite = True
    cfg.inference.ignore_error = False
    return cfg

def run(args):
    print("ExtractVideo")
    # create needed folders
    video_name = args["video_path"].split('\\')[-1].split('.')[0] 
    frames_path = u"{}\\frames".format(args['data_path'])

    # create a folder for cache
    create_data_path_directory(args, video_name)
    
    # copy video to cache and extract frames
    nframes = extract_video(args, video_name, frames_path)

    print("FindRatPath")
    dscript_detect = run_inference(frames_path, nframes)
    uploadable_data = process_inference(dscript_detect, frames_path)
    
    print("FindRatFeatures") 
    if torch.cuda.is_available():
        # if not override, get predictions if exist
        pname = f"{args['de_project_path']}\\DATA\\{video_name}\\{video_name}_predictions.csv"
        print("predname", pname)
        if not args["override"] and os.path.exists(pname):
            predictions_filename = pname
            print(f"message: loading existing predictions of {video_name} which already exist in DeepEthogram.")
        else:
            # add video to project
            project_config = deepethogram.projects.load_config(f"{args['de_project_path']}\\project_config.yaml")
            try:
                # TODO: add only if doesn't exist, and take care of the difference in override
                deepethogram.projects.add_video_to_project(project_config, args["video_path"], mode='copy')
            except:
                shutil.rmtree(f"{args['de_project_path']}\\DATA\\{video_name}")
            # feature extractor
            cfg = feature_extractor_inference_config(project_path=args['de_project_path'])
            feature_extractor_inference(cfg)
            # sequence model
            cfg = sequence_inference_config(project_path=args['de_project_path'])
            sequence_inference(cfg)
            # post process
            cfg = deepethogram.configuration.make_postprocessing_cfg(project_path=args['de_project_path'])
            deepethogram.postprocessing.postprocess_and_save(cfg)
            # get predictions
            key = args["video_path"].split('\\')[-1].split(".")[0]
            record = deepethogram.projects.get_records_from_datadir(os.path.join(args['de_project_path'], 'DATA'))[key]
            predictions_filename = os.path.join(os.path.dirname(record['rgb']), record['key'] + '_predictions.csv') 
    else:
        # handle the case where CUDA is not available
        print("message: no CUDA hardware exists. Can't run DeepEthogram.")
    
    pred_df = pandas.read_csv(predictions_filename, index_col=0).drop('background', axis=1).astype(bool)
    if pred_df.columns[0].endswith("ing"):
        pred_df.columns = pred_df.columns.map(lambda s: "is_" + s.replace(' ', '_'))
    else:
        pred_df.columns = pred_df.columns.map(lambda s: "is_" + s.replace(' ', '_') + "ing")

    # save_path = args["video_path"][:args["video_path"].rindex("\\")]
    video_name = args["video_path"].split("\\")[-1].split(".")[0]
    uploadable_data = pandas.concat([uploadable_data, pred_df], axis=1)
    uploadable_data.to_csv(f"{args['data_path']}\\processed_data.csv")

    print("SaveToDataBase")
    # %%
    # the commented line is used for server storage, but we prefer saving on this computer.
    # cluster = "mongodb+srv://john:1234@cluster0.9txls.mongodb.net/real_test?retryWrites=true&w=majority" 
    # in order to run this, you have first to run MongoDB server using:
    # `C:\Program Files\MongoDB\Server\5.0\bin\mongo.exe`
    mnge.register_connection(alias='core', host=args["connection_string"])
    video = create_video_object(video_name, nframes, args["video_path"])
    
    update_video = Video.objects(link_to_data=args["video_path"], analysis__exists=1).order_by('-registered_date')
    if args["override"] and len(update_video) > 0:
        update_video.update_one(set__length=video.length,
                                set__nframes=video.nframes,
                                set__modification_date=video.modification_date,
                                set__description=video.description,
                                set__name=video.name)
        video_id = update_video.first().id 
    else:
        video.save()
        video_id = video.id
    print(f"video id: {video_id}")

    # %%
    video = Video.objects(id=video_id).first()
    
    ana = Analysis()
    ana.timestep = list(uploadable_data.index)
    for c in uploadable_data.columns:
        exec(f"ana.{c} = list(uploadable_data['{c}'])")

    if args["override"]:
        try:
            video.analysis.delete()
        except Exception as e:
            print(f"{e.__class__.__name__}: {e}")

    ana.video = video_id
    video.analysis = ana
    ana.save()
    video.update(analysis=ana.id)
    video.save()
    print(f"analysis id: {ana.id}")
    print("success")

if __name__ == '__main__':
    # %%
    args = pandas.read_csv(sys.argv[1], header=None, index_col=0)[1]
    args["override"] = eval(args["override"])
    run(args)
    print("exit")
    sys.exit(0)