using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace FurniToolkit
{
    class Item
    {
        public int ID;
        public int Revision;
        public int DefaultDir;
        public char Type;
        public string ClassName;
        public int ColorIndex;
        public int TileSizeX;
        public int TileSizeY;
        public int TileSizeZ;
        public List<string> Colors;
        public string Title;
        public string Description;
        public string AdURL;
        public int CatalogPageID;

        public int OfferID;
        public int RentOfferID;

        public bool Buyout;
        public bool RentBuyout;

        public string CustomParams;
        public int SpecialType;
        public bool BuildersClub;

        public bool CanStandOn;
        public bool CanSitOn;
        public bool CanLayOn;


        public Item(XmlNode node)
        {
            ID = int.Parse(node.Attributes["id"].InnerText);
            ClassName = node.Attributes["classname"].InnerText;
            Type = node.ParentNode.Name == "roomitemtypes" ? 's' : 'i';
            Colors = new List<string>();
            foreach (XmlNode child in node.ChildNodes)
            {
                switch (child.Name)
                {
                    case "revision":
                        Revision = int.Parse(child.InnerXml);
                        break;
                    case "defaultdir":
                        DefaultDir = int.Parse(child.InnerXml);
                        break;
                    case "xdim":
                        TileSizeX = int.Parse(child.InnerXml);
                        break;
                    case "ydim":
                        TileSizeY = int.Parse(child.InnerXml);
                        break;
                    case "color":
                        Colors.Add(child.InnerXml);
                        break;
                    case "name":
                        Title = child.InnerXml;
                        break;
                    case "description":
                        Description = child.InnerXml;
                        break;
                    case "adurl":
                        AdURL = child.InnerXml;
                        break;
                    case "offerid":
                        OfferID = int.Parse(child.InnerXml);
                        break;
                    case "buyout":
                        Buyout = child.InnerXml == "1";
                        break;
                    case "rentofferid":
                        RentOfferID = int.Parse(child.InnerXml);
                        break;
                    case "rentbuyout":
                        RentBuyout = child.InnerXml == "1";
                        break;
                    case "bc":
                        BuildersClub = child.InnerXml == "1";
                        break;
                    case "customparams":
                        CustomParams = child.InnerXml;
                        break;
                    case "specialtype":
                        SpecialType = int.Parse(child.InnerXml);
                        break;
                    case "canstandon":
                        CanStandOn = child.InnerXml == "1";
                        break;
                    case "cansiton":
                        CanSitOn = child.InnerXml == "1";
                        break;
                    case "furnitype":
                        return;
                }
            }

            if (ClassName.Contains('*'))
            {
                ColorIndex = int.Parse(ClassName.Split('*')[1]);
                ClassName = ClassName.Split('*')[0];
            }
            else
                ColorIndex = -1;
        }
        public Item(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
                return;

            data = Regex.Replace(data, @"/\[{1,}/mg", "");
            data = Regex.Replace(data, @"/\[{1,}/mg", "");
            List<string> splitted = data.Split('"').ToList();
            //RemoveOnValue(splitted, ",", true);
            splitted.RemoveAll(s => s.Trim() == ",");
            splitted.RemoveAt(0);
            splitted.RemoveAt(splitted.Count - 1);

            Type = splitted[0][0];
            ID = int.Parse(splitted[1]);
            ClassName = splitted[2];
            if (ClassName.Contains('*'))
            {
                ColorIndex = int.Parse(ClassName.Split('*')[1]);
            }
            else
                ColorIndex = 0;

            Revision = int.Parse(splitted[3]);
            TileSizeX = string.IsNullOrWhiteSpace(splitted[4]) ? 1 : int.Parse(splitted[4]);
            TileSizeY = string.IsNullOrWhiteSpace(splitted[5]) ? 1 : int.Parse(splitted[5]);
            TileSizeZ = string.IsNullOrWhiteSpace(splitted[6]) ? 0 : int.Parse(splitted[6]);
            Colors = splitted[7].Split(',').ToList();
            Title = splitted[8];
            Description = splitted[9];

            // Fix if invalid stuff
            TileSizeX = Math.Max(TileSizeX, 1);
            TileSizeY = Math.Max(TileSizeY, 1);

            // Set some default values
            SpecialType = 1;
            OfferID = -1;
            RentOfferID = -1;
            CustomParams = "";

            // We usually have corruped stuff below, so we use try, catch
            try
            {
                AdURL = splitted[10];
                if (splitted.Count > 17)
                {
                    OfferID = int.Parse(splitted[11]);
                    Buyout = bool.Parse(splitted[12]);
                    RentOfferID = int.Parse(splitted[13]);
                    RentBuyout = bool.Parse(splitted[14]);

                    CustomParams = splitted[15];
                    SpecialType = int.Parse(splitted[16]);
                    BuildersClub = bool.Parse(splitted[17]);

                    if (Type != 'i')
                    {
                        CanStandOn = bool.Parse(splitted[18]);
                        CanSitOn = bool.Parse(splitted[19]);
                        CanLayOn = bool.Parse(splitted[20]);
                    }
                }
                else if (splitted.Count > 13)
                {
                    CatalogPageID = int.Parse(splitted[11]);
                    OfferID = int.Parse(splitted[12]);
                }
            }
            catch (Exception x)
            {
                //Console.WriteLine("Partly corruped item: '{0}'", FullName);
            }
        }
    }
}
