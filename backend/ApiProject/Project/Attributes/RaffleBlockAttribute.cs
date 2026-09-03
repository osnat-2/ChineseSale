using Microsoft.AspNetCore.Mvc.Filters;

namespace Project.Attributes
{
    /// <summary>
    /// Attribute to block raffles/lottery operations
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class RaffleBlockAttribute : Attribute, IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // Logic to execute before the action runs
            // Can be used to check if raffles are blocked or other validation
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Logic to execute after the action runs
        }
    }
}
