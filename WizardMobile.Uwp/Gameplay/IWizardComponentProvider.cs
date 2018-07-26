using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizardMobile.Uwp.Gameplay
{
    public interface IWizardComponentProvider
    {
        GamePage.AnimationSession CreateAnimationSession();
        void SetMessageBoxText(string message);
        // TODO add set<text> methods
    }
}
