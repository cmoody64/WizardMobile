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
        public UniqueDisplayCard(Card engineCard, bool isFaceUp = true)
        {
            EngineCard = engineCard;
            Id = Guid.NewGuid().ToString();
        }

        public Card EngineCard { get; }
        public bool IsFaceUp { get; set; }
        public string Id { get; }
        public string DisplayKey => IsFaceUp ? EngineCard.ToString() : BACK_OF_CARD_KEY;

        private static readonly string BACK_OF_CARD_KEY = "back_of_card";
    }
}
