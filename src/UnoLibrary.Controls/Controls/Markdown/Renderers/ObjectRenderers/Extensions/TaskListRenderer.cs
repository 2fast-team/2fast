using Markdig.Extensions.TaskLists;
using Symptum.UI.Markdown.TextElements;

namespace Symptum.UI.Markdown.Renderers.ObjectRenderers.Extensions;

public class TaskListRenderer : WinUIObjectRenderer<TaskList>
{
    protected override void Write(WinUIRenderer renderer, TaskList taskList)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        ArgumentNullException.ThrowIfNull(taskList);

        TaskListCheckBoxElement checkBox = new(taskList);
        renderer.WriteInline(checkBox);
    }
}
