import sys
import os
import datetime
# import multiprocessing

import pandas

import torch
import mongoengine as mnge
from utilityFunctions import *

import deepethogram
from deepethogram.sequence.inference import sequence_inference
from deepethogram.feature_extractor.inference import feature_extractor_inference


# suppress all warnings
import warnings
warnings.filterwarnings("ignore")

def run(args):
    print("ExtractVideo")

    # needed paths
    args["data_path"], video_path = add_video_to_de_project(args)
    video_name = video_path.split('\\')[-1].split('.')[0] 
    frames_path = u"{}\\.frames".format(args['data_path'])
    
    
    # TODO: fix this switching (pretty sure getting them back to the way they were would be enough)
    print("FindRatPath") 
    if torch.cuda.is_available():
        # if not override, get labels / predictions if exist
        lname = f"{args['de_project_path']}\\DATA\\{video_name}\\{video_name}_labels.csv"
        pname = lname.replace("labels", "predictions")
        if not args["override"] and os.path.exists(lname):
            predictions_filename = lname
            print(f"message: loading existing predictions of {video_name} which already exist in DeepEthogram.")
        elif os.path.exists(pname):
            predictions_filename = pname
            print(f"message: loading existing predictions of {video_name} which already exist in DeepEthogram.")
        else:
    
            # feature extractor
            cfg = feature_extractor_inference_config(project_path=args['de_project_path'], video_path=args["data_path"])
            feature_extractor_inference(cfg)

            # sequence model
            cfg = sequence_inference_config(project_path=args['de_project_path'], video_path=args["data_path"])
            sequence_inference(cfg)

            # post process
            cfg = deepethogram.configuration.make_postprocessing_cfg(project_path=args['de_project_path'])
            deepethogram.postprocessing.postprocess_and_save(cfg)

            # get predictions
            record = deepethogram.projects.get_records_from_datadir(os.path.join(args['de_project_path'], 'DATA'))[video_name]
            predictions_filename = os.path.join(os.path.dirname(record['rgb']), record['key'] + '_predictions.csv') 
    else:
        # handle the case where CUDA is not available
        print("message: no CUDA hardware exists. Can't run DeepEthogram.")
    
    pred_df = pandas.read_csv(predictions_filename, index_col=0).drop('background', axis=1).astype(bool)
    if pred_df.columns[0].endswith("ing"):
        pred_df.columns = pred_df.columns.map(lambda s: "is_" + s.replace(' ', '_'))
    else:
        pred_df.columns = pred_df.columns.map(lambda s: "is_" + s.replace(' ', '_') + "ing")

        
    print("FindRatFeatures")
    
    # copy video to cache and extract frames
    nframes = extract_video(args, video_name, frames_path)

    dscript_detect = run_inference(frames_path, nframes)
    uploadable_data = process_inference(dscript_detect, frames_path)

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

    for c in uploadabale_data.columns:
        if c not in features_list:
            exec(f"ana.{c} = list(uploadabale_data['{c}'])")
        
    for i, r in uploadabale_data[features_list].iterrows():
        exec("ana.features.append(FeatureItem({}))".format(str(r.to_dict())[1:-1].replace('\'', '').replace(': ', '=')))

    if args["override"]:
        try:
            video.analysis.delete()
        except Exception as e:
            print(f"{e.__class__.__name__}: {e}")


    ana.video = video.id
    video.analysis = ana
    ana.save()

    video.update(analysis=ana.id)
    video.cascade_save()

    print(f"analysis id: {ana.id}")
    print("success")

if __name__ == '__main__':
    # %%
    args = pandas.read_csv(sys.argv[1], header=None, index_col=0)[1]
    args["override"] = eval(args["override"])
    run(args)
    print("exit")
    sys.exit(0)