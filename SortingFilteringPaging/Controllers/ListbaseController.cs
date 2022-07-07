using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace SortingFilteringPaging.Controllers
{
    public class ListBaseController : Controller
    {
        protected readonly ILogger _logger;
        protected readonly IHttpContextAccessor _httpContextAccessor;

        public ListBaseController(
          IHttpContextAccessor httpContextAccessor
        )
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public ListBaseController(
          IHttpContextAccessor httpContextAccessor,
          ILogger<ListBaseController> logger
          ) : this(httpContextAccessor)
        {
            _logger = logger;
        }
        protected void SetTotalCountHeader(int count)
        {
            this.Response.Headers.Add("X-Total-Count", count.ToString());
            this.Response.Headers.Add("Access-Control-Expose-Headers", "X-Total-Count");
        }

        private object GetDateTimeOrIntVal(string v)
        {
            DateTime tmpDate;
            int tmpInt;
            if (DateTime.TryParse(v, out tmpDate))
            {
                return tmpDate;
            }
            else if (int.TryParse(v, out tmpInt))
            {
                return tmpInt;
            }
            else
            {
                throw new ArgumentException();
            }
        }
        protected List<Filter> GetFilter()
        {
            List<Filter> filters = new List<Filter>();

            var lte = this.HttpContext.Request.Query.Where(x => x.Key.Contains("_lte"));
            var gte = this.HttpContext.Request.Query.Where(x => x.Key.Contains("_gte"));
            var codeFields = this.HttpContext.Request.Query.Where(x => x.Key.Contains("__"));
            var textFields = this.HttpContext.Request.Query.Where(x => x.Key.Contains("_like"));

            if (lte != null)
            {
                foreach (var item in lte)
                {
                    filters.Add(new Filter
                    {
                        PropertyName = item.Key.Remove(item.Key.IndexOf("_lte")),
                        Operation = Op.LessThanOrEqual,
                        Value = GetDateTimeOrIntVal(item.Value)
                    });
                    Debug.WriteLine("ListBaseController filter " + filters[0].PropertyName);
                }
            }

            if (gte != null)
            {
                foreach (var item in gte)
                {
                    var filter = new Filter
                    {
                        PropertyName = item.Key.Remove(item.Key.IndexOf("_gte")),
                        Operation = Op.GreaterThanOrEqual,
                        // Value = DateTime.Parse(item.Value)
                        Value = GetDateTimeOrIntVal(item.Value)
                    };
                    filters.Add(filter);
                }
            }

            if (codeFields != null)
            {
                foreach (var item in codeFields)
                {
                    int val;
                    if (int.TryParse(item.Value, out val))
                    {
                        filters.Add(new Filter
                        {
                            PropertyName = item.Key.Remove(item.Key.IndexOf("__")),
                            Operation = Op.Equals,
                            Value = val
                        });
                    }
                    else
                    {
                        filters.Add(new Filter
                        {
                            PropertyName = item.Key,
                            Operation = Op.Equals,
                            Value = item.Value
                        });
                    }
                }
            }

            if (textFields != null)
            {
                // _logger.LogDebug
                foreach (var item in textFields)
                {
                    filters.Add(new Filter
                    {
                        PropertyName = item.Key.Remove(item.Key.IndexOf("_")),
                        Operation = Op.Contains,
                        Value = item.Value[0]
                    });
                }
            }
            foreach (var filter in filters)
            {
                Console.WriteLine("Controller filter.PropertyName {0}, value : {1}, Type: {2}", filter.PropertyName, filter.Value, filter.Value?.GetType());
            }
            return filters;
        }
    }
}