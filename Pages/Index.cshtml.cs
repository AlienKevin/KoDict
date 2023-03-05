﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KoDict;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private Dict Dict = new Dict();
    [BindProperty(SupportsGet = true)]
    public string? Word { get; set; }
    public Dict.Match? MatchedEntries;
    [BindProperty(SupportsGet = true)]
    public int? Index { get; set; }

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }
    public async Task OnGetAsync()
    {
        if (!string.IsNullOrEmpty(Word))
        {
            this.MatchedEntries = this.Dict.lookupWord(Word);
        }
    }
    public List<Dict.Entry> GetEntryGroup(string word)
    {
        return this.Dict.lookupWord(word).GetEntries();
    }
}
