using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CDPHP.Bot.Survey {

    public enum InsuranceRequesteeType {
        Unknown = -1,
        ForJustMe = 0,
        ForMeAndMySpouse = 1,
        ForMeMySpouseAndKids = 2,
        MeAndMyKids = 3,
        JustMyKids = 4
    }

    public class InsuranceQuestionaire {

        //public InsuranceRequesteeType InsuranceRequesteeType { get; set; }

        public string InsuranceRequesteeType { get; set; }

        public int ZipCode { get; set; }

        public int FamilyMemberCount { get; set; }

        public int ChildCount { get; set; }

        public decimal HouseIncome { get; set; }

        public string BudgetPreference { get; set; }

    }
}
