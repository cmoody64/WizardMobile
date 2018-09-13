using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WizardMobile.Uwp.Common;

namespace WizardMobile.Uwp.GamePage
{
    public interface IAnimationProvider
    {
        void QueueAnimationRequest(NamedAnimationRequest animation);
        void QueueAnimationRequests(IEnumerable<NamedAnimationRequest> animations);
        Task<bool> RunQueuedAnimations();
    }
}
