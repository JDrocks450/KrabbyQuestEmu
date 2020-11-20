using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinkyFile.Installation
{
    public struct InstallationCompletionInfo
    {
        public string StepName;
        public string CurrentTask;
        public double PercentComplete;
        public TimeSpan ElapsedTime;
        public string StandardOutput;
    }
}
