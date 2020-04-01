using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Karamtara_Application.Models
{
    public class ProductGroupModel
    {
        public int ProductGroupId { get; set; }
        public string ProductGroupName { get; set; }
        public string ProductGroupCode { get; set; }
        public string Summary { get; set; }
        public string LineVoltage { get; set; }
        public string GroupType { get; set; }
        public string UTS { get; set; }
        public string BundleType { get; set; }
        public string Conductor { get; set; }
        public int ProductGroupTypeId { get; set; }
        public int LineVoltageId { get; set; }
        public int UtsValueId { get; set; }
        public int BundleTypeId { get; set; }
        public int ConductorTypeId { get; set; }
        public DateTime CreateDate { get; set; }
        public List<ProductGroupTypeMS> ProductGroupTypeList { get; set; }
        public List<LineVoltageMS> LineVoltageList { get; set; }
        public List<UTSMS> UtsValueList { get; set; }
        public List<BundleTypeMS> BundleTypeList { get; set; }
        public List<ConductorTypeMaster> ConductorTypeList { get; set; }
        public int Quantity { get; set; }
        public decimal UnitGrWt { get; set; }
        public decimal UnitNetWt { get; set; }
        public string Size { get; set; }
        public string DrawingNo { get; set; }

        //pending
        public List<ProductGroupListModel> ProductGroupList { get; set; }
        public List<AssemblyMasterModel> AssemblyList { get; set; }
        public List<SubAssemblyListModel> SubAssemblyList { get; set; }
        public List<ComponentModel> ComponentList { get; set; }
        public List<MasterModel> MasterList { get; set; }
        
    }

    public class ProductGroupTypeMS
    {
        public int Id { get; set; }
        public string ProductGroupType { get; set; }
    }

    public class LineVoltageMS
    {
        public int Id { get; set; }
        public string LineVoltage { get; set; }
    }

    public class UTSMS
    {
        public int Id { get; set; }
        public string UTSValue { get; set; }
    }

    public class BundleTypeMS
    {
        public int Id { get; set; }
        public string BundleType { get; set; }
    }

    public class ConductorTypeMaster
    {
        public int Id { get; set; }
        public string ConductorType { get; set; }
    }
}