namespace ExtendableCustomerApi.Model.Filter
{
    public class ComplexFilter
    {
       
            public string? SearchQuery { get; set; }
            public List<SimpleFilter>? Filters { get; set; } = new List<SimpleFilter>();
            public int PageIndex { get; set; } = 1;
            public int PageSize { get; set; } = 5;
            public string? Sort { get; set; } //key
            public string? Order { get; set; }//asc desc
        
    }
}
