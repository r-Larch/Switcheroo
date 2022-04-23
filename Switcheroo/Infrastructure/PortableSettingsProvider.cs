/*
 * Switcheroo - The incremental-search task switcher for Windows.
 * http://www.switcheroo.io/
 * Copyright 2009, 2010 James Sulak
 * Copyright 2014 Regin Larsen
 *
 * Switcheroo is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * Switcheroo is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with Switcheroo.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Windows.Forms;
using System.Xml;


namespace Switcheroo {
    // From https://github.com/crdx/PortableSettingsProvider
    // MIT License
    public sealed class PortableSettingsProvider : SettingsProvider, IApplicationSettingsProvider {
        private const string RootNodeName = "settings";
        private const string LocalSettingsNodeName = "localSettings";
        private const string GlobalSettingsNodeName = "globalSettings";
        private const string ClassName = "PortableSettingsProvider";
        private XmlDocument _xmlDocument;

        private string FilePath => Path.Combine(Path.GetDirectoryName(Application.ExecutablePath)!, $"{ApplicationName}.settings");

        private XmlNode LocalSettingsNode {
            get {
                var settingsNode = GetSettingsNode(LocalSettingsNodeName);
                var machineNode = settingsNode.SelectSingleNode(Environment.MachineName.ToLowerInvariant());

                if (machineNode == null) {
                    machineNode = RootDocument.CreateElement(Environment.MachineName.ToLowerInvariant());
                    settingsNode.AppendChild(machineNode);
                }

                return machineNode;
            }
        }

        private XmlNode GlobalSettingsNode => GetSettingsNode(GlobalSettingsNodeName);

        private XmlNode RootNode => RootDocument.SelectSingleNode(RootNodeName);

        private XmlDocument RootDocument {
            get {
                if (_xmlDocument == null) {
                    _xmlDocument = new XmlDocument();
                    try {
                        _xmlDocument.Load(FilePath);
                    }
                    catch (Exception) {
                        // Ignore
                    }

                    if (_xmlDocument.SelectSingleNode(RootNodeName) != null) {
                        return _xmlDocument;
                    }

                    _xmlDocument = GetBlankXmlDocument();
                }

                return _xmlDocument;
            }
        }

        public override string ApplicationName {
            get => Path.GetFileNameWithoutExtension(Application.ExecutablePath);
            set { }
        }

        public override string Name => ClassName;

        public void Reset(SettingsContext context)
        {
            LocalSettingsNode.RemoveAll();
            GlobalSettingsNode.RemoveAll();

            _xmlDocument.Save(FilePath);
        }

        public SettingsPropertyValue GetPreviousVersion(SettingsContext context, SettingsProperty property)
        {
            // do nothing
            return new SettingsPropertyValue(property);
        }

        public void Upgrade(SettingsContext context, SettingsPropertyCollection properties)
        {
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(Name, config);
        }

        public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection)
        {
            foreach (SettingsPropertyValue propertyValue in collection)
                SetValue(propertyValue);

            try {
                RootDocument.Save(FilePath);
            }
            catch (Exception) {
                /*
                 * If this is a portable application and the device has been
                 * removed then this will fail, so don't do anything. It's
                 * probably better for the application to stop saving settings
                 * rather than just crashing outright. Probably.
                 */
            }
        }

        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection collection)
        {
            var values = new SettingsPropertyValueCollection();

            foreach (SettingsProperty property in collection) {
                values.Add(new SettingsPropertyValue(property) {
                    SerializedValue = GetValue(property)
                });
            }

            return values;
        }

        private void SetValue(SettingsPropertyValue propertyValue)
        {
            var targetNode = IsGlobal(propertyValue.Property)
                ? GlobalSettingsNode
                : LocalSettingsNode;

            var settingNode = targetNode.SelectSingleNode($"setting[@name='{propertyValue.Name}']");

            if (settingNode != null)
                settingNode.InnerText = propertyValue.SerializedValue.ToString();
            else {
                settingNode = RootDocument.CreateElement("setting");

                var nameAttribute = RootDocument.CreateAttribute("name");
                nameAttribute.Value = propertyValue.Name;

                settingNode.Attributes.Append(nameAttribute);
                settingNode.InnerText = propertyValue.SerializedValue.ToString();

                targetNode.AppendChild(settingNode);
            }
        }

        private string GetValue(SettingsProperty property)
        {
            var targetNode = IsGlobal(property) ? GlobalSettingsNode : LocalSettingsNode;
            var settingNode = targetNode.SelectSingleNode($"setting[@name='{property.Name}']");

            if (settingNode == null)
                return property.DefaultValue != null ? property.DefaultValue.ToString() : string.Empty;

            return settingNode.InnerText;
        }

        private bool IsGlobal(SettingsProperty property)
        {
            foreach (DictionaryEntry attribute in property.Attributes) {
                if ((Attribute) attribute.Value is SettingsManageabilityAttribute)
                    return true;
            }

            return false;
        }

        private XmlNode GetSettingsNode(string name)
        {
            var settingsNode = RootNode.SelectSingleNode(name);

            if (settingsNode == null) {
                settingsNode = RootDocument.CreateElement(name);
                RootNode.AppendChild(settingsNode);
            }

            return settingsNode;
        }

        public XmlDocument GetBlankXmlDocument()
        {
            var blankXmlDocument = new XmlDocument();
            blankXmlDocument.AppendChild(blankXmlDocument.CreateXmlDeclaration("1.0", "utf-8", string.Empty));
            blankXmlDocument.AppendChild(blankXmlDocument.CreateElement(RootNodeName));

            return blankXmlDocument;
        }
    }
}
