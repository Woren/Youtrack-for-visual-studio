using AutoMapper;
using YouTrack.REST.API.Models.Enterprise;
using YouTrack.REST.API.Models.Standard;

namespace YouTrack.REST.API.Mappings.Converters
{
    public class EnterpriseRepositoryTypeConverter : ITypeConverter<EnterpriseRepository, Repository>
    {
        public Repository Convert(EnterpriseRepository source, Repository destination, ResolutionContext context)
        {
            return new Repository()
            {
                Id = source.Id,
                Scm = source.Scm,
                Links = source.Links.MapTo<Links>(),
                IsPrivate = !source.IsPublic,
                ForkPolicy = source.Forkable != null && source.Forkable.Value ? "allow_forks" : "no_forks",
                //Owner = new User() { Username = source.Project.Key },
                Name = source.Name
            };
        }
    }
}