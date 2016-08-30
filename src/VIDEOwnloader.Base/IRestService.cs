using System.Threading.Tasks;

namespace VIDEOwnloader.Base
{
    public interface IAsyncRestService<in TReq, TRes>
    {
        Task<TRes> GetResponseAsync(TReq request);
    }

    public interface IRestService<in TReq, TRes> : IAsyncRestService<TReq, TRes>
    {
        TRes GetResponse(TReq request);
    }
}