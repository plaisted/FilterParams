# FilterParams
Helper library for turning http query strings into IQueryable queries.

FilterParams is under active development.

The goal is to provide a simple and easy to implement alternative to standards such as OData or GraphQL allowing REST(like) endpoints to be queried and filtered based on properties of the resource.  The implementation should be as easy as adding a parameter of type Filter<T> to to an ASPCORE controller method. FilterParams will then parse the HTTP query string and create the appropriate LINQ expression allowing efficient queries to EF (or other ORMs). The query parameters will be in a format similar to `http://server/test?Id=gt:5&string=like:text` where multiple properties of the object may be filtered. Complex queries will be supported such as:
    
`http://server/test?(Id=gt:5&or:String=like:text)&Number=gte:10`.


### Example controller setup
```csharp
    public class TestController : Controller
    {
        //ctor not shown
        
        [HttpGet]
        public async Task<IActionResult> Get(Filter<TestType> filter)
        {
            //_db.TestTypes is EF IQueryable DbSet<TestType>
            return Ok(filter.Apply(_db.TestTypes).ToList());
        }
    }
    
    public class TestType
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public string String { get; set; }
    }
```

### Query syntax

Statements are formatted as:

`{Property}`=`{op}`:`{value}`

where `{Property}` is the property on type `T` of `IQueryable<T>`, `{op}` is one of the value operators listed below, and `{value}` is the value for the statement comparison.

Value operators:
* Equal (eq)
* Not (ne)
* Like (lk, like)
* GreaterThan (gt)
* GreaterThanEqual (gte)
* LessThan (lt)
* LessThanEqual (lte)

Multiple statements may be included using:
* AND (&)
* OR (&or:)

If a value contains characters &, (, ), or \\ they should be escaped using \\.
