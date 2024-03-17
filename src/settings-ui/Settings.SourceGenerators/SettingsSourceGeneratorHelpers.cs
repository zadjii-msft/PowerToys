// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Xml;

internal static class SettingsSourceGeneratorHelpers
{
    public static XmlNode GetNode(this XmlDocument doc, string path)
    {
        XmlNamespaceManager namespaceManager = new XmlNamespaceManager(doc.NameTable);
        namespaceManager.AddNamespace("ns", "http://schemas.microsoft.com/PowerToys/FileActionsMenu/ModuleDefinition");

        return doc.SelectSingleNode("ns:" + path.Replace("/", "/ns:"), namespaceManager);
    }

    public static XmlNode GetNode(this XmlNode node, string path)
    {
        XmlNamespaceManager namespaceManager = new XmlNamespaceManager(node.OwnerDocument.NameTable);
        namespaceManager.AddNamespace("ns", "http://schemas.microsoft.com/PowerToys/FileActionsMenu/ModuleDefinition");

        return node.SelectSingleNode("ns:" + path.Replace("/", "/ns:"), namespaceManager);
    }

    public static XmlNodeList GetNodes(this XmlDocument doc, string path)
    {
        XmlNamespaceManager namespaceManager = new XmlNamespaceManager(doc.NameTable);
        namespaceManager.AddNamespace("ns", "http://schemas.microsoft.com/PowerToys/FileActionsMenu/ModuleDefinition");

        return doc.SelectNodes("ns:" + path.Replace("/", "/ns:"), namespaceManager);
    }

    public static XmlNodeList GetNodes(this XmlNode node, string path)
    {
        XmlNamespaceManager namespaceManager = new XmlNamespaceManager(node.OwnerDocument.NameTable);
        namespaceManager.AddNamespace("ns", "http://schemas.microsoft.com/PowerToys/FileActionsMenu/ModuleDefinition");

        return node.SelectNodes("ns:" + path.Replace("/", "/ns:"), namespaceManager);
    }
}
