using Karamtara_Application.Models.Tender;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Karamtara_Application.Models
{
    public class TestMasterModel
    {
        public string TestName { get; set; }
        public string TestDescription { get; set; }
        public string TestComments { get; set; }
        public List<MasterModel> ProductList { get; set; }
        public List<TestModel> TestList { get; set; }
        public List<TestModel> DummyList { get; set; }
        public List<LineVoltageMS> KVLineList { get; set; }
        public List<ProductGroupTypeMS> TypeList { get; set; }
        public List<UTSMS> UtsList { get; set; }
        public List<BundleTypeMS> BundleList { get; set; }
        public int KvLineId { get; set; }
        public int TypeId { get; set; }
        public int BundleId { get; set; }
        public int UtsId { get; set; }
        
        //extra data for relation with BOM
        public string ProjectName { get; set; }
        public string EPCCustomerName { get; set; }
        public string EndCustomerName { get; set; }
        public int BomId { get; set; }
        public int RevNo { get; set; }
        public int TenderId { get; set; }
        public int TenderRevisionId { get; set; }
        public string ProductName { get; set; }
        public string TechnicalName { get; set; }
        public string Code { get; set; }
        public string CatalogueNo { get; set; }
        public int ProductId { get; set; }
        public int ParentId { get; set; }
        public int ProdType { get; set; }
        public int ComponentId { get; set; }
        public int SubAssemblyId { get; set; }
        public int AssemblyId { get; set; }
        public int ProductGroupId { get; set; }
        public List<IntTenderDetails> IntTndValues { get; set; }
        public decimal IncrementByPercentage { get; set; }
    }
}