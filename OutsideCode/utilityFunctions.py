# %%
import numpy as np
import cv2
import pandas
from numpy_ext import rolling_apply
import datetime
import torch

import os
import shutil
import mongoengine as mnge
from PIL import Image

import deepethogram

# suppress all warnings
import warnings
warnings.filterwarnings("ignore")


def create_data_path_directory(args, video_name):
    try:
        os.makedirs(args["data_path"])
        print("created new directory")
        args["override"] = False
    except Exception:
        if args["override"]:
            print(f"message: note: overriding {video_name} which already existed in the archive.")
        else:
            mnge.register_connection(alias='core', host=args["connection_string"])
            videoq = Video.objects(link_to_data=args["video_path"], analysis__exists=1).order_by('-registered_date')
            # return an already existing video
            if len(videoq) > 0:
                video_id = videoq.first().id
                # analysis_id = videoq.first().analysis.id
                print(f"message: loading existing data of {video_name} which already exists in the cache.")
                print(f"video id: {video_id}")
                print("success")
                exit(0)
            else:
                os.makedirs(args["data_path"], exist_ok=True)


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
    return raw_data


def add_video_to_de_project(args):
    """returns video name"""
    project_config_path = u"{}\\project_config.yaml".format(args['de_project_path'])
    project_config = deepethogram.projects.load_config(project_config_path)
    features_list = project_config.project.class_names[1:]
    Video, Analysis, FeatureItem = get_documents_classes(features_list)
    try:
        new_path = deepethogram.projects.add_video_to_project(project_config, args["video_path"], mode="copy")
        return os.path.dirname(new_path), new_path.split("\\")[-1]
    except ValueError as e:
        video_name = str(e).split("Directory ")[1].split(" already")[0]
        if args["override"]:
            print(f"message: note: overriding {video_name} which already existed in the archive.")
            shutil.rmtree(f"{args['de_project_path']}\\DATA\\{video_name}")
            new_path = deepethogram.projects.add_video_to_project(project_config, args["video_path"], mode="copy")
            return os.path.dirname(new_path), new_path.split("\\")[-1]
        else:
            mnge.register_connection(alias='core', host=args["connection_string"])
            videoq = Video.objects(link_to_data=args["video_path"], analysis__exists=1).order_by('-registered_date')
            # return an already existing video
            if len(videoq) > 0:
                video_id = videoq.first().id
                print(f"message: loading existing data of {video_name} which already exists in the cache.")
                print(f"video id: {video_id}")
                print("success")
                exit(0)
            else:
                # there was some kind of error
                shutil.rmtree(f"{args['de_project_path']}\\DATA\\{video_name}")
                new_path = deepethogram.projects.add_video_to_project(project_config, args["video_path"], mode="copy")
                return os.path.dirname(new_path), new_path.split("\\")[-1]


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
    print(raw_data['path'].head())

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
def feature_extractor_inference_config(project_path, video_path):
    preset = 'deg_f'
    cfg = deepethogram.configuration.make_feature_extractor_inference_cfg(project_path=project_path, preset=preset)
    cfg.feature_extractor.weights = 'latest'
    cfg.flow_generator.weights = 'latest'

    cfg.inference.overwrite = True
    # make sure errors are thrown
    cfg.inference.ignore_error = False
    cfg.compute.num_workers = 2

    # run inference only on the given file
    cfg.inference.directory_list = [video_path]
    return cfg

def sequence_inference_config(project_path, video_path):
    ncpus = 4
    cfg = deepethogram.configuration.make_sequence_inference_cfg(project_path)
    cfg.sequence.weights = 'latest'
    cfg.compute.num_workers = ncpus
    cfg.inference.overwrite = True
    cfg.inference.ignore_error = False

    # run inference only on the given file
    cfg.inference.directory_list = [video_path]
    return cfg

def flow_generator_train_config(project_path):
    # this function is actually not needed since this training is supposed to be done in the DE app.
    preset = 'deg_f'
    cfg = deepethogram.configuration.make_flow_generator_train_cfg(project_path, preset=preset)
    n_cpus = 2 # multiprocessing.cpu_count()
    cfg.compute.num_workers = n_cpus
    return cfg

def feature_extractor_train_config(project_path):
    preset = 'deg_f'
    cfg = deepethogram.configuration.make_feature_extractor_train_cfg(project_path, preset=preset)
    cfg.flow_generator.weights = 'latest'
    n_cpus = 2 # multiprocessing.cpu_count()
    cfg.compute.num_workers = n_cpus
    return cfg

# %%
# those are just for introducing purposes
# running the full classes ends up with errors because they don't know each other
class Analysis(mnge.Document):
    timestep = mnge.ListField(mnge.IntField())
    
class Video(mnge.Document):
    registration_date = mnge.DateTimeField(default=datetime.datetime.now)

class FeatureItem(mnge.EmbeddedDocument):
    dummy_field = mnge.BinaryField();


def get_FeatureItem(features_list):
    class FeatureItem(mnge.EmbeddedDocument):
        for feature in features_list:
            exec(f"{feature} = mnge.BooleanField(default=False)")
        
        meta = {
            'db_alias': 'core',
            'collection': 'features'
        }
    return FeatureItem

def get_Analysis(features_list):
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
        video = mnge.ReferenceField(Video)
        features = mnge.EmbeddedDocumentListField(get_FeatureItem(features_list))
    
        meta = {
            'db_alias': 'core',
            'collection': 'analysis'
        }
    return Analysis

def get_Video(features_list):
    class Video(mnge.Document):
        registration_date = mnge.DateTimeField(default=datetime.datetime.now)
        modification_date = mnge.DateTimeField()
        name = mnge.StringField(required=True)
        nframes = mnge.IntField(reqiured=True)
        length = mnge.StringField(required=True)
        description = mnge.StringField(required=True)
        link_to_data = mnge.StringField(required=True)
        analysis = mnge.ReferenceField(get_Analysis(features_list))
    
        meta = {
            'db_alias': 'core',
            'collection': 'videos'
        }
    return Video

def get_documents_classes(features_list):
    return get_Video(features_list), get_Analysis(features_list), get_FeatureItem(features_list)