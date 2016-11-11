using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using NBrightCore.common;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components;

namespace Nevoweb.DNN.NBrightBuy.Providers.QtyPromo
{
    public static class PromoUtils
    {
        #region "Qty promo"


        public static NBrightInfo  CalcQtyPromo(int portalId, NBrightInfo cartItemInfo)
        {
            var objCtrl = new NBrightBuyController();
            var l = objCtrl.GetList(portalId, -1, "QtyPromo", "", "", 0, 0, 0, 0, Utils.GetCurrentCulture());
            foreach (var p in l)
            {
                cartItemInfo = CalcQtyPromoItem(p, cartItemInfo);
            }
            return cartItemInfo;
        }

        public static NBrightInfo CalcQtyPromoItem(NBrightInfo p, NBrightInfo cartItemInfo)
        {
            var objCtrl = new NBrightBuyController();
            var typeselect = p.GetXmlProperty("genxml/radiobuttonlist/typeselect");
            var catgroupid = p.GetXmlProperty("genxml/dropdownlist/catgroupid");
            var propgroupid = p.GetXmlProperty("genxml/dropdownlist/propgroupid");
            var promoname = p.GetXmlProperty("genxml/textbox/name");
            var amounttype = p.GetXmlProperty("genxml/radiobuttonlist/amounttype");
            var amount = p.GetXmlPropertyDouble("genxml/textbox/amount");
            var overwrite = p.GetXmlPropertyBool("genxml/checkbox/overwrite");
            var disabled = p.GetXmlPropertyBool("genxml/checkbox/disabled");
            var rangeList = p.GetXmlProperty("genxml/textbox/range");

            if (!disabled)
            {

                var productid = cartItemInfo.GetXmlPropertyInt("genxml/productid");

                if (productid > 0)
                {
                    var prodData = new ProductData(productid,Utils.GetCurrentCulture());
                    if (prodData.Exists)
                    {
                        var groupid = catgroupid;
                        if (typeselect == "prop") groupid = propgroupid;
                        var gCat = CategoryUtils.GetCategoryData(groupid, Utils.GetCurrentCulture());
                        if (gCat == null) return cartItemInfo; 

                        if (prodData.HasProperty(gCat.CategoryRef) || prodData.IsInCategory(gCat.CategoryRef))
                        {
                            var runcalc = true;
                            // build range Data
                            var rangeData = new List<RangeItem>();
                            var rl = rangeList.Split(new string[] {"\n", "\r\n"}, StringSplitOptions.RemoveEmptyEntries);
                            foreach (var s in rl)
                            {
                                var ri = s.Split('=');
                                if (ri.Count() == 2 && Utils.IsNumeric(ri[1]))
                                {
                                    var riV = ri[0].Split('-');
                                    if (riV.Count() == 2 && Utils.IsNumeric(riV[0]) && Utils.IsNumeric(riV[1]))
                                    {
                                        var rItem = new RangeItem();
                                        rItem.RangeLow = Convert.ToDouble(riV[0], CultureInfo.GetCultureInfo("en-US"));
                                        rItem.Cost = Convert.ToDouble(ri[1], CultureInfo.GetCultureInfo("en-US"));
                                        rItem.RangeHigh = Convert.ToDouble(riV[1], CultureInfo.GetCultureInfo("en-US"));
                                        rangeData.Add(rItem);
                                    }
                                }
                            }

                            var unitcost = cartItemInfo.GetXmlPropertyDouble("genxml/unitcost");
                            var promoprice = cartItemInfo.GetXmlPropertyDouble("genxml/promoprice");
                            var rangeValue = cartItemInfo.GetXmlPropertyDouble("genxml/qty");
                            var basecost = unitcost;

                            foreach (var i in rangeData)
                            {
                                if (rangeValue >= i.RangeLow && rangeValue < i.RangeHigh)
                                {
                                    double newsaleprice = 0;

                                    if (amounttype == "1")
                                    {
                                        // percentage discount
                                        newsaleprice = Math.Round(basecost - ((basecost/100)*i.Cost), 4);
                                    }
                                    else
                                    {
                                        // amount discount
                                        newsaleprice = basecost - i.Cost;
                                        if (newsaleprice < 0) newsaleprice = 0;
                                    }

                                    cartItemInfo.SetXmlPropertyDouble("genxml/promoprice", newsaleprice);
                                }
                            }
                        }
                    }
                }
            }
            return cartItemInfo;
        }

        private class RangeItem
        {
            public Double RangeLow { get; set; }
            public Double RangeHigh { get; set; }
            public Double Cost { get; set; }
        }


        #endregion

    }
}
