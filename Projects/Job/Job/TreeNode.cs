using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Job
{
    [Gtk.TreeNode(ListOnly = true)]
    public class TreeNode : Gtk.TreeNode
    {
        //string values of node (taking text from nodes)
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
                //make some transformation of text from argument
                List<String> list = parse(value);
                int i = 0;
                for(; i < list.Count; i++) { 
                    if(i < authors.Count) {
                        authors[i].InnerText = list[i];//if less than change value
                    }
                    else {
                        XmlNode newAuthor = createAuthorNode(list[i]);//else add new author
                        rootNode.AppendChild(newAuthor);
                        authors.Add(newAuthor);
                    }
                }
                if(i < authors.Count){ 
                    for(int k = authors.Count-1; k >= i; k--) { //removing authors num of authors became less than was
                        rootNode.RemoveChild(authors[k]);
                        authors.RemoveAt(k);
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

        //nodes, document(for creating new node of author) and main node to rempve or add new nodes
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
            //initial nodes and string values
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

        private String makeAuthors() { //making string of authors from nodes
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
            XmlNode newAuthor = document.CreateNode("element", "author", ""); //author with name s
            newAuthor.InnerText = s;
            return newAuthor;
        }

        private List<String> parse(String value) { //parsing string of authors to list
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
