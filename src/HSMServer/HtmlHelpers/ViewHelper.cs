﻿using HSMServer.Model.ViewModel;
using Microsoft.AspNetCore.Html;
using System.Text;

namespace HSMServer.HtmlHelpers
{
    public static class ViewHelper
    {
        public static HtmlString CreateTreeWithLists(TreeViewModel model)
        {
            StringBuilder result = new StringBuilder();

            result.Append(TreeHelper.CreateTree(model));
            result.Append(ListHelper.CreateFullLists(model));

            return new HtmlString(result.ToString());
        }

        //public static string CreateStringTreeWithLists(TreeViewModel model)
        //{
        //    StringBuilder result = new StringBuilder();

        //    result.Append(TreeHelper.CreateTree(model));
        //    result.Append(ListHelper.CreateFullLists(model));

        //    return result.ToString();
        //}
    }
}