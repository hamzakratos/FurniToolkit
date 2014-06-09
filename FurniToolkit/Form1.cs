using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using FurniToolkit.Properties;

namespace FurniToolkit
{
    public partial class Form1 : Form
    {
        public const string CONVERT_FILENAME = "furnidata_converted.xml";
        public const string SYNC_FILENAME = "furnidata_synced.xml";

        private string _sourceFilePath;
        private string _syncFilePath;
        private string _convertFilePath;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                _sourceFilePath = openFileDialog1.FileName;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                _syncFilePath = openFileDialog2.FileName;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (openFileDialog3.ShowDialog() == DialogResult.OK)
            {
                _convertFilePath = openFileDialog3.FileName;
            }
        }

        // Sync button
        private async void button3_Click(object sender, EventArgs e)
        {
            _sourceFilePath = textBox1.Text;
            _syncFilePath = textBox2.Text;
            if (string.IsNullOrWhiteSpace(_sourceFilePath) || string.IsNullOrWhiteSpace(_syncFilePath))
            {
                MessageBox.Show(Resources.Form1_button4_Click_Some_invalid_file_paths);
                return;
            }

            button3.Enabled = false;
            await Task.Factory.StartNew(delegate
            {
                List<Item> sourceItems = LoadFurnidata(_sourceFilePath);
                List<Item> syncItems = LoadFurnidata(_syncFilePath);

                foreach (Item item in syncItems)
                {
                    if (!sourceItems.Any(i => i.ID == item.ID && i.Type == item.Type))
                        sourceItems.Add(item);
                }
                SaveFurnidata(sourceItems, SYNC_FILENAME);
            });
            button3.Enabled = true;
            MessageBox.Show("Done syncing!");
        }

        // Convert button
        private async void button4_Click(object sender, EventArgs e)
        {
            _convertFilePath = textBox3.Text;
            if (string.IsNullOrWhiteSpace(_convertFilePath))
            {
                MessageBox.Show(Resources.Form1_button4_Click_Some_invalid_file_paths);
                return;
            }

            button4.Enabled = false;
            await Task.Factory.StartNew(delegate
            {
                List<Item> items = new List<Item>();
                string data = "";
                using (WebClient c = new WebClient())
                    data = c.DownloadString(_convertFilePath);

                string[] chunks = Regex.Split(data, "\n\r{1,}|\n{1,}|\r{1,}", RegexOptions.Multiline);
                foreach (string chunk in chunks)
                {
                    MatchCollection collection = Regex.Matches(chunk, @"\[+?((.)*?)\]");
                    foreach (Match item in collection)
                    {
                        items.Add(new Item(item.Value));
                    }
                }
                SaveFurnidata(items, CONVERT_FILENAME);
            });
            button4.Enabled = true;
            MessageBox.Show(Resources.Form1_button4_Click_Done_converting_);
        }

        private List<Item> LoadFurnidata(string path)
        {
            List<Item> items = new List<Item>();

            XmlDocument xml = new XmlDocument();
            xml.Load(path);
            XmlNodeList itemNodes = xml.SelectNodes("//furnitype");

            foreach (XmlNode node in itemNodes)
            {
                Item item = new Item(node);
                if (!items.Any(i => i.ID == item.ID && i.Type == item.Type))
                    items.Add(item);
            }
            //XmlReader reader = XmlReader.Create(path);
            //char currentType = 's';
            //while (reader.Read())
            //{
            //    if (reader.NodeType != XmlNodeType.Element)
            //        continue;

            //    if (reader.Name == "wallitemtypes")
            //        currentType = 'i';


            //    if (reader.Name == "furnitype")
            //    {
            //        Item item = new Item(reader, currentType);
            //        if (item.ID == 13)
            //        {
                        
            //        }
            //        if(!items.ContainsKey(item.ID))
            //            items.Add(item.ID, item);
            //    }
            //}
            return items;
        }

        private void SaveFurnidata(List<Item> items, string path)
        {
            List<Item> floorItems = items.Where(t => t.Type == 's').ToList();
            List<Item> wallItems = items.Where(t => t.Type == 'i').ToList();

            using (XmlWriter writer = XmlWriter.Create(path))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("furnidata");

                writer.WriteStartElement("roomitemtypes");
                foreach (Item i in floorItems)
                {
                    writer.WriteStartElement("furnitype");
                    writer.WriteAttributeString("id", i.ID.ToString());
                    writer.WriteAttributeString("classname", i.ClassName);

                    writer.WriteElementString("revision", i.Revision.ToString());
                    writer.WriteElementString("defaultdir", "0");
                    writer.WriteElementString("xdim", i.TileSizeX.ToString());
                    writer.WriteElementString("ydim", i.TileSizeY.ToString());

                    writer.WriteStartElement("partcolors");
                    foreach (string color in i.Colors)
                    {
                        writer.WriteStartElement("color");
                        writer.WriteString(color);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();

                    writer.WriteElementString("name", i.Title);
                    writer.WriteElementString("description", i.Description);
                    writer.WriteElementString("adurl", i.AdURL);
                    writer.WriteElementString("offerid", i.OfferID.ToString());
                    writer.WriteElementString("buyout", Convert.ToInt32(i.Buyout).ToString());
                    writer.WriteElementString("rentofferid", i.RentOfferID.ToString());
                    writer.WriteElementString("rentbuyout", Convert.ToInt32(i.RentBuyout).ToString());
                    writer.WriteElementString("bc", Convert.ToInt32(i.BuildersClub).ToString());
                    writer.WriteElementString("customparams", i.CustomParams);
                    writer.WriteElementString("specialtype", i.SpecialType.ToString());
                    writer.WriteElementString("canstandon", Convert.ToInt32(i.CanStandOn).ToString());
                    writer.WriteElementString("cansiton", Convert.ToInt32(i.CanSitOn).ToString());
                    writer.WriteElementString("canlayon", Convert.ToInt32(i.CanLayOn).ToString());
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();

                writer.WriteStartElement("wallitemtypes");
                foreach (Item i in wallItems)
                {
                    writer.WriteStartElement("furnitype");
                    writer.WriteAttributeString("id", i.ID.ToString());
                    writer.WriteAttributeString("classname", i.ClassName);

                    writer.WriteElementString("revision", i.Revision.ToString());
                    //writer.WriteElementString("defaultdir", "0");
                    //writer.WriteElementString("xdim", i.TileSizeX.ToString());
                    //writer.WriteElementString("ydim", i.TileSizeY.ToString());

                    //writer.WriteStartElement("partcolors");
                    //foreach (string color in i.Colors)
                    //{
                    //    writer.WriteStartElement("color");
                    //    writer.WriteString(color);
                    //    writer.WriteEndElement();
                    //}
                    //writer.WriteEndElement();

                    writer.WriteElementString("name", i.Title);
                    writer.WriteElementString("description", i.Description);
                    writer.WriteElementString("adurl", i.AdURL);
                    writer.WriteElementString("offerid", i.OfferID.ToString());
                    writer.WriteElementString("buyout", Convert.ToInt32(i.Buyout).ToString());
                    writer.WriteElementString("rentofferid", i.RentOfferID.ToString());
                    writer.WriteElementString("rentbuyout", Convert.ToInt32(i.RentBuyout).ToString());
                    writer.WriteElementString("bc", Convert.ToInt32(i.BuildersClub).ToString());
                    // writer.WriteElementString("customparams", i.CustomParams.ToString());
                    writer.WriteElementString("specialtype", i.SpecialType.ToString());
                    //writer.WriteElementString("canstandon", Convert.ToInt32(i.CanStandOn).ToString());
                    //writer.WriteElementString("cansiton", Convert.ToInt32(i.CanSitOn).ToString());
                    //writer.WriteElementString("canlayon", Convert.ToInt32(i.CanLayOn).ToString());
                    writer.WriteEndElement();
                    //var objects = new object[]
                    //{
                    //    i.ID, i.Type, i.Name, i.ColorIndex, i.Revision, i.TileSizeX, i.TileSizeY, i.TileSizeZ,
                    //    String.Join(",", i.Colors), i.Title, i.Description, i.AdURL, i.CatalogPageID, i.OfferID
                    //};
                    //builder.AppendLine(
                    //    string.Format(
                    //        "ID: {0}, TYPE: {1}, NAME: {2}, COLORINDEX: {3}, REVISION: {4}, TILESIZEX: {5}, TILESIZEY: {6}, TILESIZEZ: {7}, COLORS: {8}, TITLE: {9}, DESCRIPTION: {10}, ADURL: {11}, CATALOGPAGEID: {12}, OFFERID: {13}",
                    //        objects));
                }
                writer.WriteEndElement();

                writer.WriteEndDocument();
            }
        }
    }
}
