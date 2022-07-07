from utilityFunctions import *
import pandas
import sys

# suppress all warnings
import warnings
warnings.filterwarnings("ignore")

# %%
try:
    # get run arguments
    args = pandas.read_csv(sys.argv[1], header=None, index_col=0)[1]
    args["override"] = eval(args["override"])
    uploadable_data = pandas.read_csv(f"{args['data_path']}\\uploadable_data.csv", index_col=0)
    ### I read dummy predictions
    # TODO: read real predictions 
    pred_df = pandas.read_csv('C:/Users/buein/OneDrive - Bar-Ilan University/שנה ג/פרוייקט שנתי/mouse_tracking/cv/videos/examples/testing_project_deepethogram/DATA/odor28/odor28_predictions.csv',
                              index_col=0).drop('background', axis=1).astype(bool)
    pred_df.columns = pred_df.columns.map(lambda s: "is_" + s.replace(' ', '_'))

    
    save_path = args["video_path"][:args["video_path"].rindex("\\")]
    video_name = args["video_path"].split("\\")[-1].split(".")[0]
    uploadable_data.to_csv(f"{save_path}\\{video_name}_processed_data.csv") # TODO: check if this is okay

    if len(uploadable_data) != len(pred_df):
        raise Exception("features and path files are not in the same length. Are you sure they were generated for the same video?")


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