using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizardMobile.Uwp.Gameplay
{
    public class UniqueCard
    {
        public UniqueCard(string name)
        {
            Name = name;
            Id = Guid.NewGuid().ToString();
        }

        public string Name { get; set; }
        public string Id { get; }

        public Card ToEngineCard()
        {

        }
    }
}
