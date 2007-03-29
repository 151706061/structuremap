using System.Xml;
using StructureMap.Graph;
using StructureMap.Graph.Configuration;
using StructureMap.Source;

namespace StructureMap.Configuration
{
    public class ProfileAndMachineParser
    {
        private readonly IGraphBuilder _builder;
        private readonly XmlNode _structureMapNode;
        private readonly XmlMementoCreator _creator;

        public ProfileAndMachineParser(IGraphBuilder builder, XmlNode structureMapNode, XmlMementoCreator creator)
        {
            _builder = builder;
            _structureMapNode = structureMapNode;
            _creator = creator;
        }

        public void Parse()
        {
            XmlNode defaultProfileNode = _structureMapNode.Attributes.GetNamedItem(XmlConstants.DEFAULT_PROFILE);
            if (defaultProfileNode != null)
            {
                _builder.DefaultManager.DefaultProfileName = defaultProfileNode.InnerText;
            }

            foreach (XmlElement profileElement in findNodes(XmlConstants.PROFILE_NODE))
            {
                string profileName = profileElement.GetAttribute(XmlConstants.NAME);
                _builder.AddProfile(profileName);

                writeOverrides(profileElement,
                               delegate(string fullName, string defaultKey) { _builder.OverrideProfile(fullName, defaultKey); }, profileName);
            }

            foreach (XmlElement machineElement in findNodes(XmlConstants.MACHINE_NODE))
            {
                string machineName = machineElement.GetAttribute(XmlConstants.NAME);
                string profileName = machineElement.GetAttribute(XmlConstants.PROFILE_NODE);

                _builder.AddMachine(machineName, profileName);

                writeOverrides(machineElement,
                               delegate(string fullName, string defaultKey) { _builder.OverrideMachine(fullName, defaultKey); }, machineName);
            }
        }


        private delegate void WriteOverride(string fullTypeName, string defaultKey);

        private void writeOverrides(XmlElement parentElement, WriteOverride function, string profileName)
        {
            foreach (XmlElement overrideElement in parentElement.SelectNodes(XmlConstants.OVERRIDE))
            {
                processOverrideElement(function, overrideElement, profileName);
            }
        }

        private void processOverrideElement(WriteOverride function, XmlElement overrideElement, string profileName)
        {
            string fullName = overrideElement.GetAttribute(XmlConstants.TYPE_ATTRIBUTE);

            XmlElement instanceElement = (XmlElement) overrideElement.SelectSingleNode(XmlConstants.INSTANCE_NODE);
            if (instanceElement == null)
            {
                string defaultKey = overrideElement.GetAttribute(XmlConstants.DEFAULT_KEY_ATTRIBUTE);
                function(fullName, defaultKey);
            }
            else
            {
                createOverrideInstance(fullName, instanceElement, function, profileName);
            }
        }

        private void createOverrideInstance(string fullName, XmlElement instanceElement, WriteOverride function,
                                            string profileName)
        {
            string key = Profile.InstanceKeyForProfile(profileName);
            InstanceMemento memento = _creator.CreateMemento(instanceElement);
            memento.InstanceKey = key;

            TypePath familyPath = _builder.LocateOrCreateFamilyForType(fullName);
            _builder.RegisterMemento(familyPath, memento);

            function(fullName, key);
        }

        private XmlNodeList findNodes(string nodeName)
        {
            return _structureMapNode.SelectNodes(nodeName);
        }
    }
}