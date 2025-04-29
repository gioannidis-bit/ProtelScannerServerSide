using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Mrz;
using Mrz.Types;
using PassToProtel.XmlDocsDescr;
using PassToProtel.XmlExecution;

namespace ProtelScannerServerSide.Classes;

public class DocsDescrParser
{
	public List<DocDescr> GetDocsDescrList(string xmlFile)
	{
		List<DocDescr> res = new List<DocDescr>();
		new List<Scenario>();
		if (!File.Exists(xmlFile))
		{
			MessageBox.Show("File " + xmlFile + " not found !");
			return res;
		}
		try
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(xmlFile);
			foreach (XmlNode document in xmlDocument.DocumentElement.SelectNodes("/DOCUMENTS/DOCUMENT"))
			{
				DocDescr des = new DocDescr();
				des.ROWS = readIntAttr(document, "ROWS");
				des.ROWSIZE = readIntAttr(document, "ROWSIZE");
				des.COUNTRY = readStringAttr(document, "COUNTRY");
				des.TYPE = readDocTypeAttr(document, "TYPE");
				des.FIRSTNAME = getElemenDescr(document, "FIRSTNAME");
				des.LASTNAME = getElemenDescr(document, "LASTNAME");
				des.DOCUMENTNO = getElemenDescr(document, "DOCUMENTNO");
				des.SEX = getElemenDescr(document, "SEX");
				des.BIRTHDATE = getElemenDescr(document, "BIRTHDATE");
				des.EXPIRATIONDATE = getElemenDescr(document, "EXPIRATIONDATE");
				des.ISSUINGCOUNTRY = getElemenDescr(document, "ISSUINGCOUNTRY");
				des.NATIONALITY = getElemenDescr(document, "NATIONALITY");
				res.Add(des);
			}
		}
		catch (Exception ex)
		{
			Utils.Log("XmlSettings GetScenarios : " + ex.Message);
		}
		return res;
	}

	private BaseField getElemenDescr(XmlNode document, string elementName)
	{
		BaseField res = new BaseField();
		XmlNode node = document.SelectSingleNode(elementName);
		if (node == null)
		{
			return res;
		}
		res.CHECKDIGIT = readStringAttr(node, "CHECKDIGIT");
		res.CHECKDIGIT_POS = -1;
		if (IsNumeric(res.CHECKDIGIT))
		{
			res.CHECKDIGIT_POS = ToNumber(res.CHECKDIGIT);
		}
		res.START = readStringAttr(node, "START");
		res.START_POS = -1;
		if (IsNumeric(res.START))
		{
			res.START_POS = ToNumber(res.START);
		}
		res.SEARCHFROM = Math.Max(readIntAttr(node, "SEARCHFROM"), 0);
		res.END = readStringAttr(node, "END");
		res.END_POS = -1;
		if (IsNumeric(res.END))
		{
			res.END_POS = ToNumber(res.END);
		}
		res.ROW = readIntAttr(node, "ROW");
		res.FIELD_NAME = elementName;
		return res;
	}

	public static bool IsNumeric(string Expression)
	{
		double retNum;
		return double.TryParse(Expression, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out retNum);
	}

	public static int ToNumber(string Expression)
	{
		int retNum = -1;
		int.TryParse(Expression, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out retNum);
		return retNum;
	}

	private string readStringAttr(XmlNode node, string attrName)
	{
		string res = "";
		if (node.Attributes[attrName] != null)
		{
			res = node.Attributes[attrName].Value;
		}
		return res;
	}

	private COMBO_SEARCH readComboSearchAttr(XmlNode node)
	{
		COMBO_SEARCH res = COMBO_SEARCH.BEGIN;
		if (node.Attributes["COMBO_SEARCH"] != null && node.Attributes["COMBO_SEARCH"].Value.ToUpper() == "EXACT")
		{
			res = COMBO_SEARCH.EXACT;
		}
		return res;
	}

	private ACCESS readAccessAttr(XmlNode node)
	{
		ACCESS res = ACCESS.RANDOM;
		if (node.Attributes["ACCESS"] != null && node.Attributes["ACCESS"].Value.ToUpper() == "SEQUENTIAL")
		{
			res = ACCESS.SEQUENTIAL;
		}
		return res;
	}

	private int readIntAttr(XmlNode node, string attrName)
	{
		int res = 0;
		if (node.Attributes[attrName] != null)
		{
			string str = node.Attributes[attrName].Value;
			try
			{
				res = Convert.ToInt32(str, 10);
			}
			catch (FormatException ex)
			{
				res = -1;
				Utils.Log(ex.Message);
			}
		}
		return res;
	}

	private MrzDocumentCode.DocumentCode readDocTypeAttr(XmlNode node, string attrName)
	{
		MrzDocumentCode.DocumentCode res = MrzDocumentCode.DocumentCode.Passport;
		if (node.Attributes[attrName] != null)
		{
			switch (node.Attributes[attrName].Value)
			{
			case "P":
				res = MrzDocumentCode.DocumentCode.Passport;
				break;
			case "ID":
				res = MrzDocumentCode.DocumentCode.IDCard;
				break;
			case "C":
				res = MrzDocumentCode.DocumentCode.CCard;
				break;
			case "IDOLDGERMAN":
				res = MrzDocumentCode.DocumentCode.IDCard_Old_German;
				break;
			}
		}
		return res;
	}
}
