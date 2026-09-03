using Project.Models;

namespace Project
{
    public class Result<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public IEnumerable<T> Data { get; set; }
    }
}