using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Product.Ops
{
    /// <summary>
    /// Ops portfolio object
    /// </summary>
    public class Portfolio
    {
        /// <summary>
        /// The id of the portfolio
        /// </summary>
        [JsonProperty("id")]
        public string ID { get; set; }

        /// <summary>
        /// The name of the portfolio
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The Code of the portfolio
        /// </summary>
        [JsonProperty("code")]
        public string Code { get; set; }

        /// <summary>
        /// The status of the portfolio
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; set; }

        /// <summary>
        /// The parent id of the portfolio
        /// </summary>
        [JsonProperty("parent_asset_id")]
        public string ParentAssetId { get; set; }

        /// <summary>
        /// The asset type of the portfolio
        /// </summary>
        [JsonProperty("asset_type")]
        public AssetType AssetType { get; set; }

        /// <summary>
        /// The list of properties assigned to the parent portfolio
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<Portfolio> Properties { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Portfolio()
        {
            Properties = new List<Portfolio>();
        }

        /// <summary>
        /// Is the portfolio assigned to the user
        /// </summary>
        public bool IsAssigned { get; set; }

        /// <summary>
        /// The UPFM property instance id
        /// </summary>
        public string InstanceId { get; set; }
    }



    /// <summary>
    /// Used to store asset information
    /// </summary>
    public class AssetType
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("system_name")]
        public string SystemName { get; set; }

    }

    /// <summary>
    /// Helpers used to build a tree view of Ops portfolios
    /// </summary>
    public static class Helpers
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodes"></param>
        /// <returns></returns>
        public static Portfolio BuildTree(this List<Portfolio> nodes)
        {
            if (nodes == null)
            {
                throw new ArgumentNullException("nodes");
            }
            
            return new Portfolio().BuildTree(nodes);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        /// <param name="nodes"></param>
        /// <returns></returns>
        public static Portfolio BuildTree(this Portfolio root, List<Portfolio> nodes)
        {
            if (nodes.Count == 0) { return root; }

            var children = root.FetchChildren(nodes).ToList();
            root.Properties.AddRange(children);
            root.RemoveChildren(nodes);

            for (int i = 0; i < children.Count; i++)
            {
                children[i] = children[i].BuildTree(nodes);
                if (nodes.Count == 0) { break; }
            }

            return root;
        }

        /// <summary>
        /// Used to build a tree view of Portfolios
        /// </summary>
        /// <param name="root"></param>
        /// <param name="nodes"></param>
        /// <returns></returns>
        public static IEnumerable<Portfolio> FetchChildren(this Portfolio root, List<Portfolio> nodes)
        {
            return nodes.Where(n => n.ParentAssetId == root.ID);
        }

        /// <summary>
        /// Used to remove child nodes
        /// </summary>
        /// <param name="root"></param>
        /// <param name="nodes"></param>
        public static void RemoveChildren(this Portfolio root, List<Portfolio> nodes)
        {
            foreach (var node in root.Properties)
            {
                nodes.Remove(node);
            }
        }

    }
}
    