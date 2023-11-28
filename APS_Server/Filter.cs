using ASP_Server.Services;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ASP_Server
{
    public class SetSessionFilter : Attribute, IActionFilter
    {
        private string _name;
        public SetSessionFilter(string name)
        {
            _name = name;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {

        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            Dictionary<string, string> dict = new();
            if (context.ModelState != null)
            {
                foreach (var item in context.ModelState)
                {
                    dict.Add(item.Key, item.Value.AttemptedValue);
                }
                context.HttpContext.Session.Set(_name, dict);
            }

        }
    }
}
