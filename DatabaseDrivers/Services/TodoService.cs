namespace TodoApi.Services
{
    public class TodoService : ITodoService
    {
        private readonly ITodoService _todoService;



        public TodoService(ITodoService todoService)
        {
            _todoService = todoService;
        }
    }

}
