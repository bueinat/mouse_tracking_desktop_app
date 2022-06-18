import pandas
import os
import shutil
import sys
import mongoengine as mnge
from utilityFunctions import *

# suppress all warnings
import warnings
warnings.filterwarnings("ignore")

# %%
try:
    # get run arguments
    args = pandas.read_csv(sys.argv[1], header=None, index_col=0)[1]
    args["override"] = eval(args["override"])

    os.makedirs(args["cache_path"], exist_ok=True)
    video_name = args["video_path"].split('\\')[-1].split('.')[0]
    data_path = f"{args['cache_path']}\\{video_path}"
    working_path = f"@WORKING_PATH\\{args['video_path']}"
    frames_path = u"{}\\{}\\frames".format(args['cache_path'], args["video_path"])

    # create a folder for cache
    try:
        os.mkdir(data_path)
        args["override"] = False
    except Exception as e:
        if args["override"]:
            print("success")
            print(f"message: note: overriding {video_name} which already existed in the archive.")
        else:
            mnge.register_connection(alias='core', host=args["connection_string"])
            videoq = Video.objects(link_to_path=args["video_path"]).order_by('-registered_date')
            # return an already existing video
            if len(videoq) > 0:
                video_id = videoq.first().id
                print(f"video id: {video_id}")
                print("success")
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
    override = args["override"]
    print(f"override: {override}")
    print("success")

except Exception as e:
    if str(e).startswith("message"):
        print(e)
    else:
        print(f"error: {e.__class__.__name__}: {e}")