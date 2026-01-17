using System.Collections.ObjectModel;

namespace Symptum.UI.Markdown;

public class DocumentOutline
{
    private Stack<DocumentNode> _nodeStack = new();

    public ObservableCollection<DocumentNode> Nodes { get; private set; } = [];

    public Dictionary<string, Action?> IdNavigateCollection { get; } = [];

    public void PushNode(DocumentNode node)
    {
        if (_nodeStack.TryPeek(out DocumentNode? _node)) // Gets the last node.
        {
            // Checks if the last node is higher in level than the new one (lower number => higher level)
            if (_node.Level < node.Level && _node.Level < DocumentLevel.Heading6)
            {
                _node.Children.Add(node); // If so add it as a child node.
                _nodeStack.Push(node); // Put this as the last node.
                IdNavigateCollection.TryAdd(node.Id, node.Navigate);
            }
            else
            {
                _nodeStack.Pop(); // Removes the node because it's lower in level than the new one.
                PushNode(node); // Recursively check for a best suit parent node and add to it.
            }
        }
        else
        {
            // Add the node to stack if it's empty
            _nodeStack.Push(node);
            Nodes.Add(node);
            IdNavigateCollection.TryAdd(node.Id, node.Navigate);
        }
    }

    public void Clear()
    {
        Nodes.Clear();
        _nodeStack.Clear();
        IdNavigateCollection.Clear();
    }
}

public class DocumentNode
{
    public string? Title { get; set; }

    public string? Id { get; set; }

    public DocumentLevel Level { get; set; }

    public ObservableCollection<DocumentNode> Children { get; private set; } = [];

    public Action? Navigate { get; set; }

    public override string ToString()
    {
        return $"{Title} {{#{Id}}} (H{(int)Level})";
    }
}

public enum DocumentLevel
{
    Heading1 = 1,
    Heading2,
    Heading3,
    Heading4,
    Heading5,
    Heading6,
    Quote,
    Table,
}
