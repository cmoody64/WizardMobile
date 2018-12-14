using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WizardMobile.Uwp.Common;

namespace WizardMobile.Uwp.GamePage
{
    // abstracts positioning, transfering, and animation of the dealer button
    public class DealerButtonPosition
    {
        public DealerButtonPosition(ICanvasProvider canvasProvider, NormalizedPosition origin)
        {
            _canvasProvider = canvasProvider;
            _origin = origin;
            _isDisplayingButton = false;
        }

        public bool ShowButton()
        {
            // static flag ensures that among all button positions, the button is only shown once 
            if(!_hasButtonBeenShown)
            {
                _canvasProvider.ShowImage(BUTTON_IMAGE_KEY, _origin, 0/*orientation*/, 0.07/*scale factor*/);
                _isDisplayingButton = true;
                _hasButtonBeenShown = true;
                return true;
            }
            return false;
        }

        public bool TransferButton(DealerButtonPosition other)
        {
            if(_isDisplayingButton)
            {
                var transferAnimRequest = new NamedAnimationRequest()
                {
                    Destination = other._origin,
                    Duration = 0.3,
                    TargetElementName = BUTTON_IMAGE_KEY,
                    IsCenteredAtDestination = false                    
                };

                _canvasProvider.QueueAnimationRequest(transferAnimRequest);
                other._isDisplayingButton = true;
                this._isDisplayingButton = false;
                return true;
            }
            return false;
        }

        private ICanvasProvider _canvasProvider;
        private NormalizedPosition _origin;
        private bool _isDisplayingButton;

        private static readonly string BUTTON_IMAGE_KEY = "dealer_button";
        private static bool _hasButtonBeenShown = false;
    }
}
