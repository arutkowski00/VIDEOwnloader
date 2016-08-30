namespace VIDEOwnloader.Base.Video
{
    public interface IVideoInfoResultItem
    {
        string Extractor { get; set; }
        string ExtractorKey { get; set; }
        string Id { get; set; }
        string Title { get; set; }
        string WebpageUrl { get; set; }
        string WebpageUrlBasename { get; set; }
    }
}