using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Karamtara_Application.HelperClass
{
    public class Flags
    {
        public enum DocumentType
        {
            Doc = 0,
            Boq = 1,
            //Project Specification
            PS = 2,
            //other
            Oth = 3,
            //enquiry attachement
            EA = 4
        }

        public enum LoginStatus
        {
            None = 0,
            Success = 1,
            Failed = 2,
            WrongPassword = 3,
            UserNotExists = 4
        }

        public enum MasterTypes
        {
            GroupType = 0,
            LineVoltage = 1,
            ConductorType = 2,
            BundleType = 3,
            BundleSpacing = 4,
            UTSValue = 5
        }
    }
}