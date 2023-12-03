namespace CustomBackend.Infra.Dtos.Result
{
    public class PagedResult<T>
    {
        public PagedResult()
        {
            Page = 1;
            PagesAvailable = 1;
            Items = new T[0];
        }

        public PagedResult(int page, int pageSizeRequested) : this()
        {
            Page = page;
            PageSizeRequested = pageSizeRequested;
        }

        public PagedResult(int page, int pagesAvailable, int pageSizeRequested, int pageSizeResult, int itemsAvailable, IEnumerable<T> items) : this()
        {
            Page = page;
            PagesAvailable = pagesAvailable;
            PageSizeRequested = pageSizeRequested;
            PageSizeResult = pageSizeResult;
            ItemsAvailable = itemsAvailable;
            Items = items?.ToArray();
        }

        public int Page { get; set; }
        public int PagesAvailable { get; set; }
        public int PageSizeRequested { get; set; }
        public int PageSizeResult { get; set; }
        public int ItemsAvailable { get; set; }
        public string OrderColumn { get; set; }
        public bool OrderAscending { get; set; }
        public T[] Items { get; set; }

        public PagedResult<TResult> To<TResult>(Func<T, TResult> selector) => new PagedResult<TResult>(Page, PagesAvailable, PageSizeRequested, PageSizeResult, ItemsAvailable, Items.Select(selector));

        public async Task<PagedResult<TResult>> ToAsync<TResult>(Func<T, Task<TResult>> selector)
        {
            var items = new List<TResult>();

            foreach (var item in Items)
                items.Add(await selector(item));

            return new PagedResult<TResult>(Page, PagesAvailable, PageSizeRequested, PageSizeResult, ItemsAvailable, items);
        }

        public PagedResult<TResult> ToParallel<TResult>(Func<T, TResult> selector) => new PagedResult<TResult>(Page, PagesAvailable, PageSizeRequested, PageSizeResult, ItemsAvailable, Items.AsParallel().Select(selector));

        public async Task<PagedResult<TResult>> ToAsyncParallel<TResult>(Func<T, Task<TResult>> selector) => new PagedResult<TResult>(
            Page,
            PagesAvailable,
            PageSizeRequested,
            PageSizeResult,
            ItemsAvailable,
            await Task.WhenAll(Items.AsParallel().Select(selector))
        );
    }
}
