using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Todo_App.Application.Common.Interfaces;
using Todo_App.Domain.Entities;
using Todo_App.Domain.Enums;

namespace Todo_App.Application.TodoLists.Queries.GetTodos;

public class GetTodosQuery : IRequest<TodosVm>
{
    public TodoFilterDto? Filter { get; set; }
}


public class GetTodosQueryHandler : IRequestHandler<GetTodosQuery, TodosVm>
{
    public static List<TagListDto> TagListDtos { get; set; } = new List<TagListDto>();

    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    public GetTodosQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    public async Task<TodosVm> Handle(GetTodosQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _context.TodoLists
            .Include(x => x.Items)
            .AsQueryable();
            if(request.Filter.Tag != null && request.Filter.Tag != "")
            {
                var count = TagListDtos.Where(x => x.Tag == request.Filter.Tag).FirstOrDefault();
                if (count != null) count.Count += 1;
                else TagListDtos.Add(new TagListDto { Tag = request.Filter.Tag, Count = 1 });
            }

            //Filtre is not null control
            if (request.Filter != null && !string.IsNullOrEmpty(request.Filter.Title)) query = query.Where(x => x.Title.ToLower().Contains(request.Filter.Title.ToLower()));
            if (request.Filter != null && !string.IsNullOrEmpty(request.Filter.Tag))
            {
                //apply filtre
                query = query
      .Select(todoList => new TodoList
      {
          Id = todoList.Id,
          Title = todoList.Title,
          Colour = todoList.Colour,
          Items = todoList.Items
              .Where(item => item.Tag.ToLower().Contains(request.Filter.Tag.ToLower()))
              .ToList() 
      });

            }
            
            query = query.AsNoTracking();


            return new TodosVm
            {
                PriorityLevels = Enum.GetValues(typeof(PriorityLevel))
                    .Cast<PriorityLevel>()
                    .Select(p => new PriorityLevelDto { Value = (int)p, Name = p.ToString() })
                    .ToList(),

                Lists = await query
                    .ProjectTo<TodoListDto>(_mapper.ConfigurationProvider).Select(x=> new TodoListDto {
                        Id = x.Id,
                        Title = x.Title,
                        Colour = x.Colour,
                        Items = x.Items
             .Where(item => item.IsDeleted == false)
             .ToList()
                    })
                    .OrderBy(t => t.Title)
                    .ToListAsync(cancellationToken),

                TagLists = TagListDtos.OrderByDescending(x => x.Count).ToList()
            };
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

}
