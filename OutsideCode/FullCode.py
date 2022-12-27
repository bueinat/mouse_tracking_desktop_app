import sys
import os
import shutil
import datetime

import pandas
import numpy

import torch
import mongoengine as mnge
from utilityFunctions import *

# suppress all warnings
import warnings
warnings.filterwarnings("ignore")

def create_data_path_directory():
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

def extract_video():
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

def run_inference(frames_path):
    PATH = "C:/ProgramData/MouseApp/yolov5/models/best_trained_model.pt"
    model = torch.hub.load('ultralytics/yolov5', 'custom', path=PATH)  # local model

    dscript_detect = {}
    for i, filename in enumerate(os.listdir(frames_path)):
        dscript_detect[int(filename.split(".")[0][5:])] =  model(f"{frames_path}/{filename}")
        if i % 10 == 0:
            print(f"progress: {i}/{nframes}")
    print(f"progress: {nframes}/{nframes}")
    return dscript_detect

def process_inference(dscript_detect):
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
try:
    # get run arguments
    args = pandas.read_csv(sys.argv[1], header=None, index_col=0)[1]
    args["override"] = eval(args["override"])

    print("ExtractVideo")
    # create needed folders
    video_name = args["video_path"].split('\\')[-1].split('.')[0] 
    frames_path = u"{}\\frames".format(args['data_path'])

    # create a folder for cache
    create_data_path_directory()
    
    # copy video to cache and extract frames
    nframes = extract_video()

    print("FindRatPath")

    # dscript_detect = run_inference(frames_path)
    # uploadable_data = process_inference(dscript_detect)

    # # save data to csv
    # uploadable_data.to_csv(f"{args['data_path']}\\uploadable_data.csv")
    
    print("FindRatFeatures") 
    run_num = args['video_path'].split("\\")[-1].split(".")[0]
    try:
        pred_df = pandas.read_csv("C:\\ProgramData\\MouseApp\\Predictions\\{run_num}_predictions.csv",
                                index_col=0).drop('background', axis=1).astype(bool)
    except:
        pred_df = pandas.read_csv("C:\\ProgramData\\MouseApp\\Predictions\\odor 24_predictions.csv",
                                index_col=0).drop('background', axis=1).astype(bool)
    pred_df.columns = pred_df.columns.map(lambda s: "is_" + s.replace(' ', '_'))

    save_path = args["video_path"][:args["video_path"].rindex("\\")]
    video_name = args["video_path"].split("\\")[-1].split(".")[0]
    uploadable_data.to_csv(f"{save_path}\\{video_name}_processed_data.csv") # TODO: check if this is okay

    if len(uploadable_data) != len(pred_df):
        pred_df = pandas.DataFrame(numpy.zeros((len(uploadable_data), len(pred_df.columns))), columns=pred_df.columns).astype(bool)
        print("message: features and path files are not in the same length. No support of features exists in such case.")
        # raise Exception("features and path files are not in the same length. Are you sure they were generated for the same video?")

    uploadable_data = pandas.concat([uploadable_data, pred_df], axis=1)
    uploadable_data.to_csv(f"{args['data_path']}\\uploadable_data.csv")
    uploadable_data.to_csv(f"{save_path}\\{video_name}_processed_data.csv")
    
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

    uploadabale_data = pandas.read_csv(f"{args['data_path']}\\uploadable_data.csv", index_col=0)
    ana = Analysis()
    ana.timestep = list(uploadabale_data.index)
    for c in uploadabale_data.columns:
        exec(f"ana.{c} = list(uploadabale_data['{c}'])")

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
    sys.exit(0)


except Exception as e:
    if str(e).startswith("message"):
        print(e)
        sys.exit(0)
    else:
        exc_type, exc_obj, exc_tb = sys.exc_info()
        print(f"error in line {exc_tb.tb_lineno}: {e.__class__.__name__}: {e}")
        sys.exit(0)