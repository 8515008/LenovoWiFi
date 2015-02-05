using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lenovo.WiFi.Client.Model
{
    public interface IDeskbandPipeListener
    {
        void OnMouseEnter();

        void OnMouseLeave();

        void OnLButtonClick();

        void OnRButtonClick();

        void OnExit();
    }
}
