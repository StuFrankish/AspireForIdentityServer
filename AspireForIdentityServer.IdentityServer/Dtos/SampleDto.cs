namespace IdentityServer.Dtos;

public class SampleDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime GeneratedDate { get; set; } = DateTime.Now;
}
