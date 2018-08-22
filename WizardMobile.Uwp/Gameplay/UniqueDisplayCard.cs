using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WizardMobile.Core;

namespace WizardMobile.Uwp.Gameplay
{
    public class UniqueDisplayCard
    {
        public UniqueDisplayCard(Card coreCard, bool isFaceUp = true)
        {
            CoreCard = coreCard;
            Id = Guid.NewGuid().ToString();
        }

        public Card CoreCard { get; }
        public bool IsFaceUp { get; set; }
        public string Id { get; }
        public string Name => CoreCard.Name;
        public string DisplayKey => IsFaceUp ? CoreCard.Name : BACK_OF_CARD_KEY;

        private static readonly string BACK_OF_CARD_KEY = "back_of_card";
    }
}
