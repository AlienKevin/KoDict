using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KoDict.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private Dict Dict = new Dict();
    [BindProperty(SupportsGet = true)]
    public string? Word { get; set; }
    public Dict.Entry? Entry;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }
    public async Task OnGetAsync()
    {
        if (!string.IsNullOrEmpty(Word))
        {
            this.Entry = this.Dict.entries.Where(s => s.word == Word).FirstOrDefault();
        }
    }
}
