import datetime
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
    args["nframes"] = int(args["nframes"])
    video_name = args["video_path"].split('\\')[-1].split('.')[0]

    # %%
    # the commented line is used for server storage, but we prefer saving on this computer.
    # cluster = "mongodb+srv://john:1234@cluster0.9txls.mongodb.net/real_test?retryWrites=true&w=majority" 
    # in order to run this, you have first to run MongoDB server using:
    # `C:\Program Files\MongoDB\Server\5.0\bin\mongo.exe`
    mnge.register_connection(alias='core', host=args["connection_string"])

    # %%
    video = Video()
    video.name = video_name
    frame_rate = 45
    total_length = args["nframes"] / frame_rate
    td = datetime.timedelta(seconds=total_length)
    minutes, seconds = divmod(td.seconds, 60)
    millis = round(td.microseconds/1000, 0)
    video.length = f"{seconds:02}:{int(millis) // 10}"
    video.nframes = args["nframes"]
    video.modification_date = datetime.datetime.now

    # TODO: enable passing this as an argument
    video.description = "dummy video\nthis is just meant for testing."
    video.link_to_data = args["video_path"]

    update_video = Video.objects(link_to_data=args["video_path"], analysis__exists=1).order_by('-registered_date')
    print(args["override"], len(update_video))
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

except Exception as e:
    if str(e).startswith("message"):
        print(e)
    else:
        exc_type, exc_obj, exc_tb = sys.exc_info()
        print(f"error in line {exc_tb.tb_lineno}: {e.__class__.__name__}: {e}")