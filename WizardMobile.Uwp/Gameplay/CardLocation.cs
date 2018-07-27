using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace WizardMobile.Uwp.Gameplay
{
    // strongly typed enum representing playable locations on the game board
    // TODO needs a reference to application for screen size and to subscribe to screen resize event so that it can dynamically resize
    class CardLocation
    {
        private CardLocation(Point position)
        {
            Position = position;            
        }

        public Point Position { get; }


        // ENUM INSTANCES
        // TODO fill in correct positions
        public static CardLocation CENTER = new CardLocation(new Point(0, 0));
        public static CardLocation LEFT_CENTER = new CardLocation(new Point(0, 0));
        public static CardLocation RIGHT_CENTER = new CardLocation(new Point(0, 0));
        public static CardLocation DISCARD = new CardLocation(new Point(0, 0));
        public static CardLocation PLAYER_1 = new CardLocation(new Point(0, 0));
        public static CardLocation CENTER = new CardLocation(new Point(0, 0));
        public static CardLocation CENTER = new CardLocation(new Point(0, 0));
        public static CardLocation CENTER = new CardLocation(new Point(0, 0));
        public static CardLocation CENTER = new CardLocation(new Point(0, 0));

        CENTER,
            LEFT_CENTER,
            RIGHT_CENTER,
            DISCARD,
            PLAYER_1,
            PLAYER_2,
            PLAYER_3,
            PLAYER_4,
            PLAYER_1_STAGING,
            PLAYER_2_STAGING,
            PLAYER_3_STAGING,
            PLAYER_4_STAGING
    }
}
