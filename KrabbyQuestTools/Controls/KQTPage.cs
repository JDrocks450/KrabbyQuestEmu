using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace KrabbyQuestTools.Controls
{
    public class KQTPage : Page
    {
        public virtual bool OnClosing()
        {
            return true;
        } 
        public virtual void OnActivated()
        {

        }
    }
}
