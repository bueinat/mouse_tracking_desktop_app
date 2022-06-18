import pandas
from utilityFunctions import *
# import sys

# suppress all warnings
import warnings
warnings.filterwarnings("ignore")

FUNCTION_NAME == 'alternative_rat_path'

# %%

try:
    # get run arguments
    args = pandas.read_csv(sys.argv[1], header=None, index_col=0)[1]
    args["override"] = eval(args["override"])

    data_path = f"{args['cache_path']}\\{args['video_path']}"
    working_path = f"@WORKING_PATH\\{args['video_path']}"
    frames_path = f"{working_path}\\frames"

    # extracting data from video
    if FUNCTION_NAME == 'rat_path':
        frames, rat_rects, alims = rat_path(args["video_path"])

        # show track of rat in time
        raw_data = pandas.DataFrame(rat_rects).T
   
    elif FUNCTION_NAME == 'alternative_rat_path':
        frames, eframes, nose_pos, max_vals = alternative_rat_path(args["video_path"])
        raw_data = get_raw_data(nose_pos, max_vals, eframes)

    raw_data = analyze_raw_data(raw_data)
    
    # create uploadabe data
    uploadabale_data = raw_data[['x', 'y', 'vx', 'vy', 'ax', 'ay']]
    uploadabale_data.loc[:, 'curviness'] = raw_data.adist / raw_data.rdist
    uploadabale_data['path'] = [f"{frames_path}\\frame{i}.jpg"
                                for i in uploadabale_data.index]

    uploadable_data.to_csv(f"{data_path}\\uploadable_data.csv")
    print("success")

except Exception as e:
    if str(e).startswith("message"):
        print(e)
    else:
        print(f"error: {e.__class__.__name__}: {e}")