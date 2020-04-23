namespace cw3.Mappers
{
    public interface IMapper<Tin, Tout>
    {
        Tout Map(Tin data);
    }
}