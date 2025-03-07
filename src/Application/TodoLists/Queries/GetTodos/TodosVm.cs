namespace Todo_App.Application.TodoLists.Queries.GetTodos;

public class TodosVm
{
    public IList<PriorityLevelDto> PriorityLevels { get; set; } = new List<PriorityLevelDto>();

    public IList<TodoListDto> Lists { get; set; } = new List<TodoListDto>();
    public IList<TagListDto> TagLists { get; set; } = new List<TagListDto>();
}

public class TagListDto
{
    public string Tag { get; set; }
    public int Count { get; set; }
}