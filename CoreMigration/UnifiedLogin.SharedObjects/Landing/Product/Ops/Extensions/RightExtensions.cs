
using System;
using System.Collections.Generic;
using UnifiedLogin.SharedObjects.Product.Ops;

namespace UnifiedLogin.SharedObjects.Landing.Product.Ops.Extensions
{
    public static class RightExtensions
    {
        /// <summary>
        /// Used to convert Rights to be used by the UI
        /// </summary>
        /// <param name="rightGroup">The list of roles to convert</param>
        /// <returns></returns>
        public static List<MainGroup> ToRightsFormatForClient(this RightGroup rightGroup)
        {
            if (rightGroup == null)
                return new List<MainGroup>();

            List<MainGroup> mnGrp = new List<MainGroup>();
            foreach (Group rgrp in rightGroup.GroupList)
            {
                if (string.IsNullOrEmpty(rgrp.ParentGroupId.Trim()))
                {
                    mnGrp.Add(new MainGroup
                    {
                        mainID = rgrp.GroupID,
                        mainName = rgrp.GroupName
                    });
                }
            }

            // adding subgroups to main group
            foreach (var item in mnGrp)
            {
                foreach (Group rgrp in rightGroup.GroupList)
                {
                    if (!string.IsNullOrEmpty(rgrp.ParentGroupId.Trim()))
                    {
                        if (item.mainID == rgrp.ParentGroupId)
                        {
                            if (item.subGroupList == null) { item.subGroupList = new List<SubGroup>(); }
                            item.subGroupList.Add(new SubGroup
                            {
                                subID = rgrp.GroupID,
                                subName = rgrp.GroupName,
                                rightsList = AddRights(rgrp.GroupID, rightGroup.ResponsibilityList, item.mainID == "7600" ? true : false)
                            });
                        }
                    }
                }
            }




            return mnGrp;
        }


        private static List<RightDetails> AddRights(string id, IList<OpsRight> rtList, bool iscompliance = false)
        {
            List<RightDetails> rights = new List<RightDetails>();

            foreach (var item in rtList)
            {
                if (item.GroupID == id)
                {
                    rights.Add(new RightDetails
                    {
                        rightID = item.GroupID,
                        right = item.Title,
                        description = item.Description,
                        isAssigned = IsAssigned(item.Value),
                        more = "",
                        value = item.Value,
                        name = item.Name,
                        isCompliance = iscompliance,
                        isWarnAssigned = IsWarnAssigned(item.Value, iscompliance),
                        warntext = iscompliance == true ? "Warn?" : ""
                    });
                }

            }

            return rights;
        }

        private static bool IsAssigned(string value)
        {
            switch (value)
            {
                case "": return false; break;
                case "0": return false; break;
                case "1": return true; break;
                case "-1": return true; break;
                default:
                    return false;
            }
        }

        private static bool IsWarnAssigned(string value, bool iscompliance)
        {
            if (iscompliance == false) { return false; }
            switch (value)
            {
                case "-1": return true; break;
                case "0": return false; break;
                case "1": return false; break;
                default:
                    return false;
            }
        }
    }
}








