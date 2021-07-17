using System;
using Gtk;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.IO;
using System.Xml.Xsl;
using System.Text.RegularExpressions;

public partial class MainWindow : Gtk.Window
{
    private NodeStore store;
    private XmlDocument document;
    private Job.TreeNode chosenNode;
    public MainWindow() : base(Gtk.WindowType.Toplevel)
    {
        Build();
        document = new XmlDocument();
        document.Load("help/basic.xml"); //load empty xml file with only bookstore tag
        store = new NodeStore(typeof(Job.TreeNode)); //something like model
        mainTable.NodeStore = store;
        typeof(NodeView).GetField("store", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(mainTable, store); //strange thing but help to solute broblem with null store
        //init some cell renderes for cells
        CellRendererText textOfBook = new CellRendererText();
        CellRendererText textOfAuthor = new CellRendererText();
        CellRendererText textOfCategory = new CellRendererText();
        CellRendererText textOfPrice = new CellRendererText();
        CellRendererText textOfYear = new CellRendererText();
        textOfBook.Underline = Pango.Underline.Low;
        //add columns with name
        mainTable.AppendColumn("Книга", textOfBook, "text", 0).Clickable = true;
        mainTable.AppendColumn("Автор", textOfAuthor, "text", 1).Clickable = true;
        mainTable.AppendColumn("Категория", textOfCategory, "text", 2).Clickable = true;
        mainTable.AppendColumn("Цена", textOfPrice, "text", 3).Clickable = true;
        mainTable.AppendColumn("Год", textOfYear, "text", 4).Clickable = true;
        //pretty uncomfortable thing but coudn't think out any better way to add these listeners
        textOfBook.Editable = true;
        textOfBook.Edited += (object o, EditedArgs args) =>
        {
            var node = store.GetNode(new TreePath(args.Path)) as Job.TreeNode;
            node.Name = args.NewText;
            mainTable.ColumnsAutosize();
        };

        textOfAuthor.Editable = true;
        textOfAuthor.Edited += (object o, EditedArgs args) =>
        {
            var node = store.GetNode(new TreePath(args.Path)) as Job.TreeNode;
            node.Authors = args.NewText;
            mainTable.ColumnsAutosize();
        };

        textOfCategory.Editable = true;
        textOfCategory.Edited += (object o, EditedArgs args) =>
        {
            var node = store.GetNode(new TreePath(args.Path)) as Job.TreeNode;
            node.Category = args.NewText;
            mainTable.ColumnsAutosize();
        };

        textOfPrice.Editable = true;
        textOfPrice.Edited += (object o, EditedArgs args) =>
        {
            var node = store.GetNode(new TreePath(args.Path)) as Job.TreeNode;
            try
            {
                double l = Convert.ToDouble(args.NewText);
                node.Price = args.NewText;
            }
            catch (Exception)
            { }

            mainTable.ColumnsAutosize();
        };

        textOfYear.Editable = true;
        textOfYear.Edited += (object o, EditedArgs args) =>
        {
            var node = store.GetNode(new TreePath(args.Path)) as Job.TreeNode;

            try
            {
                int l = Convert.ToInt32(args.NewText);
                node.Year = args.NewText;
            }
            catch (Exception) { }

            mainTable.ColumnsAutosize();
        };
        mainTable.NodeSelection.Mode = SelectionMode.Single;
        mainTable.Selection.Changed += OnMainTableSelectionChanged;
    }

    protected void OnDeleteEvent(object sender, DeleteEventArgs a)
    {
        Application.Quit();
        a.RetVal = true;
    }

    protected void OnLoadButtonClicked(object sender, EventArgs e) //load file with choser dialog
    {
        FileChooserDialog fileDialog = new FileChooserDialog("Choose file", this,
        FileChooserAction.Open, "Cancel", ResponseType.Cancel,
         "Open", ResponseType.Accept);
        FileFilter fl = new FileFilter();
        fl.AddPattern("*.xml");
        fileDialog.Filter = fl;
        fileDialog.SetCurrentFolder(".");
        if (fileDialog.Run() == (int)ResponseType.Accept)
        {
            document = new XmlDocument();
            document.Load(fileDialog.Filename); // load documnt
            update(); //update cells in table
        }
        fileDialog.Destroy();
    }

    void update() //updating cells in table
    {
        if (document != null)
        {
            XmlElement root = document.DocumentElement;
            store.Clear();
            foreach (XmlNode node in root.ChildNodes) //go throw all nodes and add it to store
            {
                store.AddNode(new Job.TreeNode(node, document)); //node can show the inner of nodes
            }
        }
    }

    protected void OnMainTableSelectionChanged(object o, System.EventArgs args)//remember the chosen node to removing
    {
        chosenNode = (Job.TreeNode)mainTable.NodeSelection.SelectedNode;
    }

    protected void OnAddButtonClicked(object sender, EventArgs e)
    {
        List<XmlNode> nodes = new List<XmlNode>(); //create new empty node of book
        XmlNode n = document.CreateNode("element", "book", "");
        XmlAttribute atr = document.CreateAttribute("category");
        n.Attributes.Append(atr);
        XmlAttribute ar = document.CreateAttribute("lang");
        ar.Value = "en";
        XmlNode auth = document.CreateNode("element", "author", "");

        XmlNode title = document.CreateNode("element", "title", "");
        title.InnerText = "example";
        title.Attributes.Append(ar);
        XmlNode year = document.CreateNode("element", "year", "");
        XmlNode price = document.CreateNode("element", "price", "");
        nodes.Add(auth);
        nodes.Add(title);
        nodes.Add(year);
        nodes.Add(price);
        foreach (XmlNode xm in nodes) //I think there is better method of creating node than this one but anyway..
        {
            n.AppendChild(xm);
        }
        document.DocumentElement.AppendChild(n);
        store.AddNode(new Job.TreeNode(n, document));
    }

    protected void OnSaveButtonClicked(object sender, EventArgs e) //like loading
    {
        FileChooserDialog fileDialog = new FileChooserDialog("Choose file", this,
        FileChooserAction.Save, "Cancel", ResponseType.Cancel,
         "Save", ResponseType.Accept);
        FileFilter fl = new FileFilter();
        fl.AddPattern("*.xml");
        fileDialog.Filter = fl;
        fileDialog.SetCurrentFolder(".");
        if (fileDialog.Run() == (int)ResponseType.Accept)
        {
            if (document != null)
            {
                String name = fileDialog.Filename;
                if (!endsWith(name, ".xml")) name += ".xml";//it makes saving a bit easier
                using (var sw = new StreamWriter(name))
                {
                    document.Save(sw); //but save xml document here
                    sw.Close();
                }
            }

        }
        fileDialog.Destroy();
    }

    protected void OnHtmlButtonClicked(object sender, EventArgs e)
    {
        FileChooserDialog fileDialog = new FileChooserDialog("Choose file", this,
        FileChooserAction.Save, "Cancel", ResponseType.Cancel,
         "Save", ResponseType.Accept);
        FileFilter fl = new FileFilter();
        fl.AddPattern("*.html");
        fileDialog.Filter = fl;
        fileDialog.SetCurrentFolder(".");
        if (fileDialog.Run() == (int)ResponseType.Accept)
        {
            if (document != null)
            {
                String name = fileDialog.Filename;
                if (!endsWith(name, ".html")) name += ".html";
                using (var sw = new StreamWriter(name))
                {
                    String s = makeHtml(); //making html with using xstl
                    sw.WriteLine(s); //write to file
                    sw.Close();

                }
            }

        }
        fileDialog.Destroy();
    }

    private String makeHtml()
    {
        XslCompiledTransform transform = new XslCompiledTransform();
        string xsltString = File.ReadAllText("help/transform.xsl"); //load xstl file from help folder
        using (XmlReader reader = XmlReader.Create(new StringReader(xsltString)))
        {
            transform.Load(reader);
        }
        StringWriter results = new StringWriter();
        using (XmlReader reader = XmlReader.Create(new StringReader(document.InnerXml)))
        {
            transform.Transform(reader, null, results); //transform it to html text
        }
        return results.ToString();
    }

    protected void OnBrowswerButtonClicked(object sender, EventArgs e)
    {
        if (document != null)
        {
            using (var sw = new StreamWriter("tmp/tmpFile.html"))
            {
                String s = makeHtml(); //making html and open it in browser
                sw.WriteLine(s);
                sw.Close();
                System.Diagnostics.Process.Start("tmp/tmpFile.html");
            }
        }

    }

    protected void OnRemoveButtonClicked(object sender, EventArgs e)//removing chosen node
    {
        if(document != null && chosenNode != null) {
            document.DocumentElement.RemoveChild(chosenNode.rootNode);
            store.RemoveNode(chosenNode);
            chosenNode = null;
        }
    }

    private Boolean endsWith(String value, String end) {

        string pattern = "/.*\\" + end;
        return Regex.IsMatch(value, pattern);
    }
}
