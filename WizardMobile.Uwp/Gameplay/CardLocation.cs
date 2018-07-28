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
    public class CardLocation
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
        public static CardLocation PLAYER_2 = new CardLocation(new Point(0, 0));
        public static CardLocation PLAYER_3 = new CardLocation(new Point(0, 0));
        public static CardLocation PLAYER_4 = new CardLocation(new Point(0, 0));
        public static CardLocation PLAYER_1_STAGING = new CardLocation(new Point(0, 0));
        public static CardLocation PLAYER_2_STAGING = new CardLocation(new Point(0, 0));
        public static CardLocation PLAYER_3_STAGING = new CardLocation(new Point(0, 0));
        public static CardLocation PLAYER_4_STAGING = new CardLocation(new Point(0, 0));
    }
}
