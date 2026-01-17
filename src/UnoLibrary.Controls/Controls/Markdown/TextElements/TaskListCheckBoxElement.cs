using Markdig.Extensions.TaskLists;

namespace Symptum.UI.Markdown.TextElements;

public class TaskListCheckBoxElement : IAddChild
{
    private TaskList _taskList;
    private SContainer _container = new();

    public STextElement TextElement => _container;

    public TaskListCheckBoxElement(TaskList taskList)
    {
        _taskList = taskList;
        FontIcon icon = new()
        {
            FontSize = 16,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Glyph = taskList.Checked ? "\uE73A" : "\uE739"
        };
        _container.UIElement = icon;
    }

    public void AddChild(IAddChild child) { }
}
