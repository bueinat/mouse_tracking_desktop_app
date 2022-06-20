import pandas
from utilityFunctions import *
# import sys

# suppress all warnings
import warnings
warnings.filterwarnings("ignore")

# %%

try:
    # get run arguments
    FUNCTION_NAME = 'alternative_rat_path'

    args = pandas.read_csv(sys.argv[1], header=None, index_col=0)[1]
    args["override"] = eval(args["override"])
    frames_path = f"{args['data_path']}\\frames"

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
    uploadable_data = raw_data[['x', 'y', 'vx', 'vy', 'ax', 'ay']]
    uploadable_data.loc[:, 'curviness'] = raw_data.adist / raw_data.rdist
    uploadable_data['path'] = [f"{frames_path}\\frame{i}.jpg"
                                for i in uploadable_data.index]

    uploadable_data.to_csv(f"{args['data_path']}\\uploadable_data.csv")
    print("success")

except Exception as e:
    if str(e).startswith("message"):
        print(e)
    else:
        exc_type, exc_obj, exc_tb = sys.exc_info()
        print(f"error in line {exc_tb.tb_lineno}: {e.__class__.__name__}: {e}")