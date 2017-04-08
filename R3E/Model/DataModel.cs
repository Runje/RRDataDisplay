using R3E.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static R3E.Constant;

namespace R3E.Model
{
    public class DataModel
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The actual data to display.
        /// </summary>
        public DisplayData ActualData { get; set; }

        /// <summary>
        /// The actual shared data from R3E.
        /// </summary>
        public Shared ActualShared { get; private set; }

        /// <summary>
        /// The last shared data from R3E.
        /// </summary>
        public Shared LastShared;

        public DataModel()
        {
            resetAll();
        }

        /// <summary>
        /// Resets all values of ActualData to invalid data.
        /// </summary>
        private void resetAll()
        {
            ActualData = new DisplayData();
        }

        public void UpdateFromR3E(Shared shared)
        {
            if (shared.Player.GameSimulationTime == LastShared.Player.GameSimulationTime)
            {
                // game is paused
                return;
            }

            ActualShared = shared;
            if (isStartOfNewSession())
            {
                log.Debug("Start of new session " + LastShared.SessionType + " --> " + shared.SessionType);
            }

            if (isNewStint() && !isRace())
            {
                log.Debug("Start of new stint");
            }

            if (isStartOfNewLap())
            {
                log.Debug("Start of new Lap");
            }

            LastShared = shared;
        }

        

        /// <summary>
        /// Whether a new lap is starting.
        /// </summary>
        /// <returns></returns>
        private bool isStartOfNewLap()
        {
            if (ActualShared.LapDistance == DisplayData.INVALID_POSITIVE)
            {
                // not on track
                return false;
            }

            bool newLap = ActualShared.LapDistance < LastShared.LapDistance;
            
            return newLap;
        }

        private bool isRace()
        {
            return (Session)ActualShared.SessionType == Session.Race;
        }

        private bool isNewStint()
        {
            return LastShared.GameInMenus != 0 && LastShared.GamePaused == 0 && (ActualShared.GameInMenus == 0 || LastShared.GamePaused != 0);
        }

        /// <summary>
        /// Determines if a new session(Training, Qualy, Race) has started.
        /// </summary>
        /// <returns></returns>
        private bool isStartOfNewSession()
        {
            // first session begins first if real new session begins, so that the data are still visible in menu
            return (Session) ActualShared.SessionType != Session.Unavailable  && LastShared.SessionType != ActualShared.SessionType;
        }
    }
}
