import sys
import os
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

    # create feaures list
    project_config_path = u"{}\\project_config.yaml".format(args['de_project_path'])
    project_config = deepethogram.projects.load_config(project_config_path)
    features_list = project_config.project.class_names[1:]

    # create documents
    Video, Analysis, FeatureItem = get_documents_classes(features_list)
    
    class FeatureItem(mnge.EmbeddedDocument):
        for feature in features_list:
            exec(f"{feature} = mnge.BooleanField(default=False)")
        
        meta = {
            'db_alias': 'core',
            'collection': 'features'
        }
    class Video(mnge.Document):
        registration_date = mnge.DateTimeField(default=datetime.datetime.now)
        modification_date = mnge.DateTimeField()
        name = mnge.StringField(required=True)
        nframes = mnge.IntField(reqiured=True)
        length = mnge.StringField(required=True)
        description = mnge.StringField(required=True)
        link_to_data = mnge.StringField(required=True)
        analysis = mnge.ReferenceField('Analysis')
    
        meta = {
            'db_alias': 'core',
            'collection': 'videos'
        }
    
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
        features = mnge.ListField(mnge.EmbeddedDocumentField(FeatureItem))
    
        meta = {
            'db_alias': 'core',
            'collection': 'analysis'
        }

    # needed paths
    args["data_path"], video_path = add_video_to_de_project(args)
    video_name = video_path.split('\\')[-1].split('.')[0]
    print(f"video name = {video_name}")
    frames_path = u"{}\\.frames".format(args['data_path'])
    
    print("FindRatFeatures") 
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
            print("message: (1/3) feature extractor")
            cfg = feature_extractor_inference_config(project_path=args['de_project_path'], video_path=args["data_path"])
            feature_extractor_inference(cfg)

            # sequence model
            print("message: (2/3) sequence model")
            cfg = sequence_inference_config(project_path=args['de_project_path'], video_path=args["data_path"])
            sequence_inference(cfg)

            # post process
            print("message: (3/3) post process and predict")
            cfg = deepethogram.configuration.make_postprocessing_cfg(project_path=args['de_project_path'])
            
            deepethogram.postprocessing.postprocess_and_save(cfg)

            # get predictions
            record = deepethogram.projects.get_records_from_datadir(os.path.join(args['de_project_path'], 'DATA'))[video_name]
            predictions_filename = os.path.join(os.path.dirname(record['rgb']), record['key'] + '_predictions.csv') 
    else:
        # allow case when there's no cuda to work
        run_num = args['video_path'].split("\\")[-1].split(".")[0]
        if "6" in run_num:
            predictions_filename = "C:/Users/buein/OneDrive - Bar Ilan University/תואר ראשון/שנה ג/פרוייקט שנתי/mouse_tracking/cv/videos/examples/testing_project_deepethogram" + f"/DATA/{run_num}/{run_num}_predictions.csv"
        else:
            predictions_filename = "C:/Users/buein/OneDrive - Bar Ilan University/תואר ראשון/שנה ג/פרוייקט שנתי/mouse_tracking/cv/videos/examples/odor28_features.csv"
        # handle the case where CUDA is not available
        print("message: no CUDA hardware exists. Can't run DeepEthogram.")
        # TODO: raise exception

    
    print("FindRatPath")
    # copy video to cache and extract frames
    nframes = extract_video(args, video_name, frames_path)
    dscript_detect = run_inference(frames_path, nframes)
    uploadable_data = process_inference(dscript_detect, frames_path)

    # combine all gathered data together
    pred_df = pandas.read_csv(predictions_filename, index_col=0).drop('background', axis=1).astype(bool)
    video_name = args["video_path"].split("\\")[-1].split(".")[0]
    uploadable_data = pandas.concat([uploadable_data, pred_df], axis=1).dropna()
    uploadable_data.to_csv(f"{args['data_path']}\\processed_data.csv")

    print("SaveToDataBase")
    # %%
    # the commented line is used for server storage, but we prefer saving on this computer.
    # cluster = "mongodb+srv://john:1234@cluster0.9txls.mongodb.net/real_test?retryWrites=true&w=majority" 
    # in order to run this, you have first to run MongoDB server using:
    # `C:\Program Files\MongoDB\Server\6.0\bin\mongo.exe`
    mnge.register_connection(alias='core', host=args["connection_string"])
    video = create_video_object(video_name, nframes, args["video_path"], Video())
    
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
        if c not in features_list:
            exec(f"ana.{c} = list(uploadable_data['{c}'])")

    for i, r in uploadable_data[features_list].iterrows():
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
    #try:
    #    run(args)
    #except Exception as e:
    #    print(f"error: {e.__class__.__name__}: {e}")
    #print("exit")
    sys.exit(0)