using R3E.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R3E.Model
{
    public class DataModel
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public DisplayData ActualData { get; set; }

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
            if (isStartOfNewSession(shared))
            {
                log.Debug("Start of new session");
            }
        }

        /// <summary>
        /// Determines if a new session(Training, Qualy, Race) has started.
        /// </summary>
        /// <param name="shared"></param>
        /// <returns></returns>
        private bool isStartOfNewSession(Shared shared)
        {
            // TODO
            return false;
        }
    }
}
