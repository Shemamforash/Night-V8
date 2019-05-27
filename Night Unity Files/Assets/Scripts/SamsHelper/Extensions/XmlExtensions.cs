using System;
using System.Globalization;
using System.Xml;
using UnityEngine;

namespace Extensions
{
	public static class XmlExtensions
	{
		public static XmlNode CreateChild(this XmlNode parent, string tagName)
		{
			try
			{
				XmlDocument parentDocument;
				if (parent is XmlDocument document) parentDocument = document;
				else parentDocument                                = parent.OwnerDocument;

				if (parentDocument.Null()) throw new Exception("ParentDocumentNotSetException");
				XmlNode newNode = parentDocument.CreateElement(tagName);
				parent.AppendChild(newNode);
				return newNode;
			}
			catch (XmlException e)
			{
				Debug.Log(tagName);
				throw e;
			}
		}

		public static void CreateChild(this XmlNode parent, string tagName, float value)
		{
			XmlNode newNode = parent.CreateChild(tagName);
			newNode.InnerText = value.ToString(CultureInfo.InvariantCulture);
		}

		public static void CreateChild(this XmlNode parent, string tagName, int value)
		{
			XmlNode newNode = parent.CreateChild(tagName);
			newNode.InnerText = value.ToString(CultureInfo.InvariantCulture);
		}

		public static void CreateChild(this XmlNode parent, string tagName, long value)
		{
			XmlNode newNode = parent.CreateChild(tagName);
			newNode.InnerText = value.ToString(CultureInfo.InvariantCulture);
		}

		public static void CreateChild(this XmlNode parent, string tagName, bool value)
		{
			XmlNode newNode = parent.CreateChild(tagName);
			newNode.InnerText = value.ToString(CultureInfo.InvariantCulture);
		}

		public static void CreateChild(this XmlNode parent, string tagName, string value)
		{
			XmlNode newNode = parent.CreateChild(tagName);
			newNode.InnerText = value ?? "";
		}

		public static XmlNode GetChild(this XmlNode root, string nodeName)
		{
			if (root.Name == nodeName) return root;
			XmlNode node = root.SelectSingleNode(nodeName);
			if (node.NotNull()) return node;
			Debug.Log("Node not found "                    + nodeName);
			throw new Exception("Xml Node Does Not Exist " + nodeName);
		}

		public static string ParseAttribute(this XmlNode root, string attributeName)
		{
			if (root.Attributes.Null()) throw new Exception("Node Has No Attributes Exception " + root.Name);
			return root.Attributes[attributeName].Value;
		}

		public static string ParseString(this XmlNode root, string name) => GetChild(root, name).InnerText;

		public static int ParseInt(this XmlNode root, string nodeName) => int.Parse(ParseString(root, nodeName), CultureInfo.InvariantCulture.NumberFormat);

		public static float ParseFloat(this XmlNode root, string nodeName) => float.Parse(ParseString(root, nodeName), CultureInfo.InvariantCulture.NumberFormat);

		public static long ParseLong(this XmlNode root, string nodeName) => long.Parse(ParseString(root, nodeName), CultureInfo.InvariantCulture.NumberFormat);

		public static bool ParseBool(this XmlNode root, string nodeName) => ParseString(root, nodeName).ToLower() == "true";
	}
}