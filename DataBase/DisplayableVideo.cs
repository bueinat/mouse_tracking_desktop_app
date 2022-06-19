using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mouse_tracking_web_app.DataBase
{
    public class DisplayableVideo
    {
        public enum State { ExtractVideo, FindRatPath, FindRatFeatues, SaveToDataBase, Successful, Failed };
        public Video VideoItem { get; set; }
        public State ProcessingState { get; set; }
        public string VideoID { get; set; }

        public string ReducedName { get; set; }
    }
}
