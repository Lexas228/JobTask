using System;
using Gtk;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.IO;
using System.Xml.Xsl;

public partial class MainWindow : Gtk.Window
{
    private NodeStore store;
    private XmlDocument document;
    private Job.TreeNode chosenNode;
    public MainWindow() : base(Gtk.WindowType.Toplevel)
    {
        Build();
        document = new XmlDocument();
        document.Load("help/basic.xml");
        store = new NodeStore(typeof(Job.TreeNode));
        mainTable.NodeStore = store;
        typeof(NodeView).GetField("store", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(mainTable, store);
        CellRendererText textOfBook = new CellRendererText();
        CellRendererText textOfAuthor = new CellRendererText();
        CellRendererText textOfCategory = new CellRendererText();
        CellRendererText textOfPrice = new CellRendererText();
        CellRendererText textOfYear = new CellRendererText();
        textOfBook.Underline = Pango.Underline.Low;

        mainTable.AppendColumn("Книга", textOfBook, "text", 0).Clickable = true;
        mainTable.AppendColumn("Автор", textOfAuthor, "text", 1).Clickable = true;
        mainTable.AppendColumn("Категория", textOfCategory, "text", 2).Clickable = true;
        mainTable.AppendColumn("Цена", textOfPrice, "text", 3).Clickable = true;
        mainTable.AppendColumn("Год", textOfYear, "text", 4).Clickable = true;

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

    protected void OnLoadButtonClicked(object sender, EventArgs e)
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
            document.Load(fileDialog.Filename);
            update();
        }
        fileDialog.Destroy();
    }

    void update()
    {
        if (document != null)
        {
            XmlElement root = document.DocumentElement;
            store.Clear();
            foreach (XmlNode node in root.ChildNodes)
            {
                store.AddNode(new Job.TreeNode(node, document));
            }
        }
    }

    protected void OnMainTableSelectionChanged(object o, System.EventArgs args)
    {
        chosenNode = (Job.TreeNode)mainTable.NodeSelection.SelectedNode;
    }

    protected void OnAddButtonClicked(object sender, EventArgs e)
    {
        List<XmlNode> nodes = new List<XmlNode>();
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
        foreach (XmlNode xm in nodes)
        {
            n.AppendChild(xm);
        }
        document.DocumentElement.AppendChild(n);
        store.AddNode(new Job.TreeNode(n, document));
    }

    protected void OnSaveButtonClicked(object sender, EventArgs e)
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
                using (var sw = new StreamWriter(fileDialog.Filename))
                {
                    document.Save(sw);
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
                using (var sw = new StreamWriter(fileDialog.Filename))
                {
                    String s = makeHtml();
                    sw.WriteLine(s);
                    Console.WriteLine(s);
                    sw.Close();

                }
            }

        }
        fileDialog.Destroy();
    }

    private String makeHtml()
    {
        XslCompiledTransform transform = new XslCompiledTransform();
        string xsltString = File.ReadAllText("help/transform.xsl");
        using (XmlReader reader = XmlReader.Create(new StringReader(xsltString)))
        {
            transform.Load(reader);
        }
        StringWriter results = new StringWriter();
        using (XmlReader reader = XmlReader.Create(new StringReader(document.InnerXml)))
        {
            transform.Transform(reader, null, results);
        }
        return results.ToString();
    }

    protected void OnBrowswerButtonClicked(object sender, EventArgs e)
    {
        if (document != null)
        {
            using (var sw = new StreamWriter("tmp/tmpFile.html"))
            {
                String s = makeHtml();
                sw.WriteLine(s);
                sw.Close();
                System.Diagnostics.Process.Start("tmp/tmpFile.html");
            }
        }

    }

    protected void OnRemoveButtonClicked(object sender, EventArgs e)
    {
        if(document != null && chosenNode != null) {
            document.DocumentElement.RemoveChild(chosenNode.rootNode);
            store.RemoveNode(chosenNode);
            chosenNode = null;
        }
    }
}
