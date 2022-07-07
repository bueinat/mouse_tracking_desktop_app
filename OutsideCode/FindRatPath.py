import torch
import pandas
from utilityFunctions import *
import sys
import os
import matplotlib.pyplot as plt
import matplotlib.image as mpimg

# suppress all warnings
import warnings
warnings.filterwarnings("ignore")

# %%

try:
        # get run arguments
    FUNCTION_NAME = 'yolov5_algorithm'

    args = pandas.read_csv(sys.argv[1], header=None, index_col=0)[1]
    args["override"] = eval(args["override"])
    frames_path = f"{args['data_path']}\\frames"

    print("init")

    # extracting data from video
    if FUNCTION_NAME == 'rat_path':
        frames, rat_rects, alims = rat_path(args["video_path"])

        # show track of rat in time
        raw_data = pandas.DataFrame(rat_rects).T
   
    elif FUNCTION_NAME == 'alternative_rat_path':
        frames, eframes, nose_pos, max_vals = alternative_rat_path(args["video_path"])
        raw_data = get_raw_data(nose_pos, max_vals, eframes)

    elif FUNCTION_NAME == 'yolov5_algorithm':
        PATH = "C:/ProgramData/MouseApp/yolov5/models/best_trained_model.pt"
        model = torch.hub.load('ultralytics/yolov5', 'custom', path=PATH)  # local model
        dscript_detect = {int(filename.split(".")[0][5:]): model(f"{frames_path}/{filename}")
                                    for filename in os.listdir(frames_path)}

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

    # save data to csv
    uploadable_data.to_csv(f"{args['data_path']}\\uploadable_data.csv")
    print("success")

except Exception as e:
    if str(e).startswith("message"):
        print(e)
    else:
        exc_type, exc_obj, exc_tb = sys.exc_info()
        print(f"error in line {exc_tb.tb_lineno}: {e.__class__.__name__}: {e}")