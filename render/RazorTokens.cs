using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;
using System.Web.Routing;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Collections;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.WebControls;
using NBrightCore.common;
using NBrightCore.providers;
using NBrightCore.render;
using NBrightDNN;
using NBrightDNN.render;
using Nevoweb.DNN.NBrightBuy.Components;
using RazorEngine.Templating;
using RazorEngine.Text;
using NBrightCore.images;
using System.IO;
using DotNetNuke.Entities.Users;
using NBrightBuy.render;
using Nevoweb.DNN.NBrightBuy;
using Nevoweb.DNN.NBrightBuy.Components.Interfaces;
using Nevoweb.DNN.NBrightBuy.Providers.QtyPromo;

namespace NBrightBuy.QtyPromo.render
{
    public class QtyPromoRazorTokens<T> : NBrightBuyRazorTokens<T>
    {

        public IEncodedString QtyPromoFromPrice(int productId)
        {
            return new RawString(NBrightBuyUtils.FormatToStoreCurrency(PromoUtils.CalcQtyPromoFromPrice(PortalSettings.Current.PortalId,productId)));
        }

        public IEncodedString QtyPromoDescription(int productId,string qtypromoref = "")
        {
            var qtydescritpion = "";
            var objCtrl = new NBrightBuyController();
            var prodData = new ProductData(productId, Utils.GetCurrentCulture());
            if (qtypromoref != "")
            {
                var qtypromo = objCtrl.GetByGuidKey(PortalSettings.Current.PortalId, -2, "QTYPROMO", qtypromoref);
                if (bulkBuyOn(prodData,qtypromo))
                {
                    var qtyInfo = objCtrl.GetData(qtypromo.ItemID, "QTYPROMOLANG", Utils.GetCurrentCulture());
                    qtydescritpion = qtyInfo.GetXmlProperty("genxml/lang/genxml/textbox/description");
                }
            }
            else
            {
                var qtypromolist = objCtrl.GetList(PortalSettings.Current.PortalId, -2, "QTYPROMO");
                foreach (var q in qtypromolist)
                {
                    if (bulkBuyOn(prodData, q))
                    {
                        var qtyInfo = objCtrl.GetData(q.ItemID, "QTYPROMOLANG", Utils.GetCurrentCulture());
                        qtydescritpion = qtyInfo.GetXmlProperty("genxml/lang/genxml/textbox/description");
                        break;
                    }
                }
            }

            return new RawString(qtydescritpion);
        }

        public IEncodedString HasQtyPromo(int productId, string qtypromoref = "")
        {
            var objCtrl = new NBrightBuyController();
            var prodData = new ProductData(productId, Utils.GetCurrentCulture());
            if (qtypromoref != "")
            {
                var qtypromo = objCtrl.GetByGuidKey(PortalSettings.Current.PortalId, -2, "QTYPROMO", qtypromoref);
                if (bulkBuyOn(prodData, qtypromo))
                {
                        return new RawString("true");
                }
            }
            else
            {
                var qtypromolist = objCtrl.GetList(PortalSettings.Current.PortalId, -2, "QTYPROMO");
                foreach (var q in qtypromolist)
                {
                    if (bulkBuyOn(prodData, q))
                    {
                            return new RawString("true");
                    }
                }
            }
            return new RawString("false");
        }

        private bool bulkBuyOn(ProductData prodData, NBrightInfo qtypromo)
        {
            if (qtypromo != null && prodData != null)
            {
                if (!qtypromo.GetXmlPropertyBool("genxml/checkbox/disabled"))
                {
                    var typeselect = qtypromo.GetXmlProperty("genxml/radiobuttonlist/typeselect");
                    var catgroupid = qtypromo.GetXmlProperty("genxml/dropdownlist/catgroupid");
                    var propgroupid = qtypromo.GetXmlProperty("genxml/dropdownlist/propgroupid");
                    var groupid = catgroupid;
                    if (typeselect == "prop") groupid = propgroupid;
                    var gCat = CategoryUtils.GetCategoryData(groupid, Utils.GetCurrentCulture());
                    if (gCat != null)
                    {
                        if (prodData.HasProperty(gCat.CategoryRef) || prodData.IsInCategory(gCat.CategoryRef))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

    }
}
