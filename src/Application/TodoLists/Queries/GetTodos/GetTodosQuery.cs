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
            .Include(x => x.Items) // Items ile birlikte yükle
            .AsQueryable();

            if (request.Filter != null && !string.IsNullOrEmpty(request.Filter.Value))
            {
                // Sadece ilgili Tag'e sahip olan Items'leri filtrele
                query = query
      .Select(todoList => new TodoList
      {
          Id = todoList.Id,
          Title = todoList.Title,
          Colour = todoList.Colour,
          Items = todoList.Items
              .Where(item => item.Tag == request.Filter.Value)
              .ToList() // Yeni bir liste oluşturup atıyoruz
      }); // Boş olan listeleri filtrele

            }

            query = query.AsNoTracking();


            return new TodosVm
            {
                PriorityLevels = Enum.GetValues(typeof(PriorityLevel))
                    .Cast<PriorityLevel>()
                    .Select(p => new PriorityLevelDto { Value = (int)p, Name = p.ToString() })
                    .ToList(),

                Lists = await query
                    .ProjectTo<TodoListDto>(_mapper.ConfigurationProvider)
                    .OrderBy(t => t.Title)
                    .ToListAsync(cancellationToken)
            };
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

}
