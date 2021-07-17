using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Job
{
    [Gtk.TreeNode(ListOnly = true)]
    public class TreeNode : Gtk.TreeNode
    {

       [Gtk.TreeNodeValue(Column = 0)]
        public String Name { get { return name.InnerText; }  set{ name.InnerText = value; } }
        [Gtk.TreeNodeValue(Column = 1)]
        public String Authors
        {
            get {
                return makeAuthors();
            }
            set
            {
                List<String> list = parse(value);
                int i = 0;
                for(; i < list.Count; i++) { 
                    if(i < authors.Count) {
                        authors[i].InnerText = list[i];
                    }
                    else {
                        XmlNode newAuthor = createAuthorNode(list[i]);
                        rootNode.AppendChild(newAuthor);
                        authors.Add(newAuthor);
                    }
                }
                int deleted = 0;
                if(i < authors.Count){
                    for(; i < authors.Count; i++) {
                        rootNode.RemoveChild(authors[i]);
                        authors.RemoveAt(i - deleted);
                        deleted++;
                    }
                }
            }
        }
        [Gtk.TreeNodeValue(Column = 2)]
        public String Category { get { return category.InnerText; } set { category.InnerText = value; } }
       
        [Gtk.TreeNodeValue(Column = 3)]
        public String Price { get { return price.InnerText; } set{ price.InnerText = value; } }
        [Gtk.TreeNodeValue(Column = 4)]
        public String Year { get { return year.InnerText; } set { year.InnerText = value; } }
        public XmlDocument document { get; set; }
        public XmlNode rootNode { get; set; }
        public List<XmlNode> authors { get; set; }
        public XmlNode name { get; set; }
        public XmlAttribute category { get; set; }
        public XmlNode price { get; set; }
        public XmlNode year { get; set; }
        public TreeNode(XmlNode node, XmlDocument document)
        {
            this.document = document;
            authors = new List<XmlNode>();
            foreach (XmlAttribute at in node.Attributes) {
                if (at.Name.Equals("category")){
                    category = at;
                    Category = at.Value;
                }
            }
            foreach (XmlNode xl in node.ChildNodes) {
                String s = xl.Name;
                if (s.Equals("author"))
                {
                    authors.Add(xl);
                }
                else if (s.Equals("title"))
                {
                    name = xl;
                    Name = xl.InnerText;
                }

                else if (s.Equals("price"))
                {
                    price = xl;
                    Price = xl.InnerText;
                }
                else if (s.Equals("year")) { 
                    year = xl;
                    Year = xl.InnerText;
               }
            }
            rootNode = node;
            Authors = makeAuthors();
        }

        private String makeAuthors() {
            StringBuilder sb = new StringBuilder();
            if (authors.Count > 0)
            {
                sb.Append(authors[0].InnerText).Append(";");
                for (int i = 1; i < authors.Count; i++)
                {
                    sb.Append("\n").Append(authors[i].InnerText).Append(";");
                }
            }
            return sb.ToString();
        }

        private XmlNode createAuthorNode(String s) {
            XmlNode newAuthor = document.CreateNode("element", "author", "");
            newAuthor.InnerText = s;
            return newAuthor;
        }

        private List<String> parse(String value) {
            List<String> str = new List<string>();
            StringBuilder sk = new StringBuilder();
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == '\n') continue;
                if (value[i] != ';')
                {
                    sk.Append(value[i]);
                }
                else
                {
                    if (sk.Length > 0)
                    {
                        str.Add(sk.ToString());
                        sk.Clear();
                    }
                }
            }
            if (sk.Length > 0)
            { 
                str.Add(sk.ToString());
            }
            return str;
    }
    }
}
