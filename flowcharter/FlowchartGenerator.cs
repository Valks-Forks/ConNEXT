namespace Flowcharter.flowcharter;

using System.IO;
using Flowcharter.flowcharter.blocks;
using Flowcharter.shapes;

public partial class FlowchartGenerator : Node2D
{
    Block MostRecentParent;
    public readonly static PackedScene shapeScene = GD.Load<PackedScene>("res://shapes/shape.tscn");
    static readonly PackedScene functionScene = GD.Load<PackedScene>("res://flowcharter/blocks/function.tscn");
    static readonly PackedScene ifScene = GD.Load<PackedScene>("res://flowcharter/blocks/if.tscn");
    static readonly PackedScene elseScene = GD.Load<PackedScene>("res://flowcharter/blocks/else.tscn");
    static readonly PackedScene elifScene = GD.Load<PackedScene>("res://flowcharter/blocks/elif.tscn");
    static readonly PackedScene whileScene = GD.Load<PackedScene>("res://flowcharter/blocks/while.tscn");
    static readonly PackedScene forScene = GD.Load<PackedScene>("res://flowcharter/blocks/for.tscn");
    static readonly PackedScene classScene = GD.Load<PackedScene>("res://flowcharter/blocks/class.tscn");
    static readonly PackedScene withScene = GD.Load<PackedScene>("res://flowcharter/blocks/with.tscn");
    static readonly PackedScene tryScene = GD.Load<PackedScene>("res://flowcharter/blocks/try.tscn");
    static readonly PackedScene exceptScene = GD.Load<PackedScene>("res://flowcharter/blocks/except.tscn");
    static readonly PackedScene finallyScene = GD.Load<PackedScene>("res://flowcharter/blocks/finally.tscn");
    List<string> FileLines;
    List<Function> Functions = new();
    List<Class> Classes = new();
    EnvironmentUI UI;
    public override void _Ready()
    {
        UI = GetNode<EnvironmentUI>("CanvasLayer/Control");
        Start();
    }
    private void Start()
    {
        Read(@"flowcharter\test.py");
        MostRecentParent = new Function().Init(0, "START OF THE CODE");
        Functions.Add(MostRecentParent as Function);
        AddChild(MostRecentParent);
        for (int i = 0; i < FileLines.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(FileLines[i]) || string.IsNullOrEmpty(FileLines[i]))
                continue;
            Analyze(FileLines[i], i);
        }
        SeparateFunctions();
        UI.Update(Functions, Classes);
    }
    private void Read(string path)
    {
        FileLines = File.ReadAllLines(path).ToList<string>();
    }
    private void Analyze(string line, int index)
    {
        int leadingSpaces = line.LeadingSpaces();
        while (leadingSpaces < MostRecentParent.LeadingSpaces)
        {
            MostRecentParent = MostRecentParent.GetParent<Block>();
        }
        string trimmed = line.Trim();
        //GD.PrintT(index, line, MostRecentParent.Name);
        if (trimmed.Last() == ':')
        {
            Block block = null;
            if (trimmed.Length > 6)
                if (trimmed.Substring(0, 6) == "async ")
                    trimmed = trimmed.Remove(0, 6);
            if (trimmed.Substring(0, 3) == "if ")
            {
                block = ifScene.Instantiate<If>().Init(FileLines[index + 1].LeadingSpaces(), trimmed);
            }
            else if (trimmed.Substring(0, 4) is "try:" or "try ")
            {
                block = tryScene.Instantiate<Try>().Init(FileLines[index + 1].LeadingSpaces(), trimmed);
            }
            else if (trimmed.Substring(0, 4) == "def ")
            {
                block = functionScene.Instantiate<Function>().Init(FileLines[index + 1].LeadingSpaces(), trimmed);
                if (!Functions.Contains(block as Function))
                    Functions.Add(block as Function);
            }
            else if (trimmed.Substring(0, 4) == "for ")
            {
                block = forScene.Instantiate<For>().Init(FileLines[index + 1].LeadingSpaces(), trimmed);
            }
            else if (trimmed.Substring(0, 5) is "else:" or "else ")
            {
                block = elseScene.Instantiate<Else>().Init(FileLines[index + 1].LeadingSpaces(), trimmed);
            }
            else if (trimmed.Substring(0, 5) == "elif ")
            {
                block = elifScene.Instantiate<Elif>().Init(FileLines[index + 1].LeadingSpaces(), trimmed);
            }
            else if (trimmed.Substring(0, 5) == "with ")
            {
                block = withScene.Instantiate<With>().Init(FileLines[index + 1].LeadingSpaces(), trimmed);
            }
            else if (trimmed.Substring(0, 6) == "while ")
                block = whileScene.Instantiate<While>().Init(FileLines[index + 1].LeadingSpaces(), trimmed);
            else if (trimmed.Substring(0, 6) == "class ")
            {
                block = classScene.Instantiate<Class>().Init(FileLines[index + 1].LeadingSpaces(), trimmed);
                if (!Classes.Contains(block as Class))
                    Classes.Add(block as Class);
            }
            else if (trimmed.Substring(0, 7) is "except " or "except:")
                block = exceptScene.Instantiate<Except>().Init(FileLines[index + 1].LeadingSpaces(), trimmed);
            else if (trimmed.Substring(0, 8) is "finally " or "finally:")
                block = finallyScene.Instantiate<Finally>().Init(FileLines[index + 1].LeadingSpaces(), trimmed);
            MostRecentParent.AddChild(block);
            MostRecentParent = block;
            return;
        }
        MostRecentParent.AddChild(shapeScene.Instantiate<NewShape>().Init(trimmed, trimmed, NewShape.Shapes.PROCESS));
    }
    private void SeparateFunctions()
    {
        foreach (var c in Classes)
            c.Reparent(GetNode("Classes"), false);
        foreach (var f in Functions)
            f.Reparent(GetNode("Functions"), false);
        foreach (var f in Functions)
            f.Update();
        foreach (var c in Classes)
            c.Update();
    }
}