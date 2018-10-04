using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WizardMobile.Core;

namespace WizardMobile.Uwp.GamePage
{
    class GamePageNavigationParams
    {
        public enum NavigationType { NEW_GAME, RESUME_GAME };

        public NavigationType StartNavigationType { get; set; }
        public GameContextDto PrevGameContextDto { get; set; }
    }
}
