using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFUI.Models
{
    internal class TimeModel
    {

        DateTime custom;
        DateTime epoch = new DateTime(1980, 1, 6, 0, 0, 0);
        public bool UseCustom { get; set; }

        public TimeModel()
        {

        }

        public uint GetGPSMillis()
        {
            uint millis = (uint)(GetTimeSpan().TotalMilliseconds % 604800000);
            return millis;
        }

        public uint GetGPSWeek()
        {
            uint weeks = (uint)(GetTimeSpan().TotalDays / 7);
            return weeks;
        }

        private TimeSpan GetTimeSpan()
        {
            DateTime curr = UseCustom ? custom : DateTime.UtcNow;
            return curr - epoch;
        }

        /*
         * update custom datetime
         */
        public void UpdateTime(TimeSpan elapsed)
        {
            custom = custom.AddMilliseconds(elapsed.TotalMilliseconds);
        }

        /*
         * set custom datetime
         */
        public void SetDateTime(string input)
        {
            custom = DateTime.Parse(input);
        }

        public void SetDateTime(DateTime dateTime)
        {
            custom = dateTime;
        }

        public string GetNow()
        {
            if (UseCustom)
            {
                return custom.ToString();
            }
            else
            {
                return GetUtcNow();
            }
        }

        public string GetUtcNow()
        {
            return DateTime.UtcNow.ToString();
        }

    }
}
