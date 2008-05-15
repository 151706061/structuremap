using System.Collections.Generic;
using System.Configuration;
using System.Xml;

namespace StructureMap.Configuration
{
    public class StructureMapConfigurationSection : IConfigurationSectionHandler
    {
        #region IConfigurationSectionHandler Members

        public object Create(object parent, object configContext, XmlNode section)
        {
            IList<XmlNode> allNodes = parent as IList<XmlNode>;
            if (allNodes == null)
            {
                allNodes = new List<XmlNode>();
            }
            allNodes.Add(section);
            return allNodes;
        }

        #endregion

        public static IList<XmlNode> GetStructureMapConfiguration()
        {
            IList<XmlNode> nodes = ConfigurationSettings.GetConfig(XmlConstants.STRUCTUREMAP) as IList<XmlNode>;
            if (nodes == null)
            {
                // TODO -- need to get this into PluginGraph instead
                throw new StructureMapException(105, XmlConstants.STRUCTUREMAP);
            }
            return nodes;
        }
    }
}