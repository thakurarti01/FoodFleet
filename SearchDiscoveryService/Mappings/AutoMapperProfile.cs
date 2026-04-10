using AutoMapper;
using SearchDiscoveryService.Models;
using SearchDiscoveryService.DTOs;

namespace SearchDiscoveryService.Mappings
{
	public class AutoMapperProfiles : Profile
	{
		public AutoMapperProfiles()
		{
			// Restaurant → SearchResultDTO
			CreateMap<Restaurant, SearchResultDTO>();

			// (Optional) MenuItem mapping if needed later
			CreateMap<MenuItem, SearchResultDTO>()
				.ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));
		}
	}
}