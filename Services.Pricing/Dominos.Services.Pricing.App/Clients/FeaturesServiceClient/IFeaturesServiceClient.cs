using System.Threading.Tasks;
using Dominos.Services.Features.Api.v1.Features;

namespace Dominos.Services.Pricing.App.Clients.FeaturesServiceClient
{
    public interface IFeaturesServiceClient
    {
        Task<GetFeaturesResponse> GetFeaturesAsync(GetFeaturesRequest request);
    }
}
