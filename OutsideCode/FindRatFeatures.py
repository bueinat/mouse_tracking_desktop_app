from utilityFunctions import *
import sys
import os
import numpy
import torch
import pandas

# suppress all warnings
import warnings
warnings.filterwarnings("ignore")

from deepethogram import configuration, postprocessing, projects
from deepethogram.sequence.inference import sequence_inference
from deepethogram.feature_extractor.inference import feature_extractor_inference

# %%
def feature_extractor_inference_config(project_path):
    preset = 'deg_f'
    cfg = configuration.make_feature_extractor_inference_cfg(project_path=project_path, preset=preset)
    cfg.feature_extractor.weights = 'latest'
    cfg.flow_generator.weights = 'latest'

    cfg.inference.overwrite = True
    # make sure errors are thrown
    cfg.inference.ignore_error = False
    cfg.compute.num_workers = 2
    return cfg

def sequence_inference_config(project_path):
    ncpus = 4
    cfg = configuration.make_sequence_inference_cfg(project_path)
    cfg.sequence.weights = 'latest'
    cfg.compute.num_workers = ncpus
    cfg.inference.overwrite = True
    cfg.inference.ignore_error = False
    return cfg

try:
    # get run arguments
    args = pandas.read_csv(sys.argv[1], header=None, index_col=0)[1]
    args["override"] = eval(args["override"])
    uploadable_data = pandas.read_csv(f"{args['data_path']}\\uploadable_data.csv", index_col=0)
    ### I read dummy predictions
    # TODO: read real predictions
    if torch.cuda.is_available():
        # add video to project
        project_config = projects.load_config(f"{args['de_project_path']}\\project_config.yaml")
        # TODO: add only if doesn't exist, and take care of the difference in override
        projects.add_video_to_project(project_config, args["video_path"], mode='copy')
        # feature extractor
        cfg = feature_extractor_inference_config(project_path=args['de_project_path'])
        feature_extractor_inference(cfg)
        # sequence model
        cfg = sequence_inference_config(project_path=args['de_project_path'])
        sequence_inference(cfg)
        # post process
        cfg = configuration.make_postprocessing_cfg(project_path=args['de_project_path'])
        postprocessing.postprocess_and_save(cfg)
        # get predictions
        key = args["video_path"].split('\\')[-1].split(".")[0]
        record = projects.get_records_from_datadir(os.path.join(args['de_project_path'], 'DATA'))[key]
        predictions_filename = os.path.join(os.path.dirname(record['rgb']), record['key'] + '_predictions.csv') 
    else:
        run_num = args['video_path'].split("\\")[-1].split(".")[0]
        if "6" in run_num:
            predictions_filename = "C:/Users/buein/OneDrive - Bar-Ilan University/שנה ג/פרוייקט שנתי/mouse_tracking/cv/videos/examples/testing_project_deepethogram" + f"/DATA/{run_num}/{run_num}_predictions.csv"
        else:
            predictions_filename = 'C:/Users/buein/OneDrive - Bar-Ilan University/שנה ג/פרוייקט שנתי/mouse_tracking/cv/videos/examples/testing_project_deepethogram/DATA/odor28/odor28_predictions.csv'
    
    pred_df = pandas.read_csv(predictions_filename, index_col=0).drop('background', axis=1).astype(bool)
    if pred_df.columns[0].endswith("ing"):
        pred_df.columns = pred_df.columns.map(lambda s: "is_" + s.replace(' ', '_'))
    else:
        pred_df.columns = pred_df.columns.map(lambda s: "is_" + s.replace(' ', '_') + "ing")

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
    print("success")

except Exception as e:
    if str(e).startswith("message"):
        print(e)
    else:
        exc_type, exc_obj, exc_tb = sys.exc_info()
        print(f"error in line {exc_tb.tb_lineno}: {e.__class__.__name__}: {e}")
