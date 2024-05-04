using System.Text.Json.Serialization;

namespace AYI.Core.DTO;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$kind")]
[JsonDerivedType(typeof(SpringHasSprung), typeDiscriminator: "spring-has-sprung")]
public record AuxiliaryRsvpDataDto
{
	public sealed record SpringHasSprung : AuxiliaryRsvpDataDto
	{
		[JsonPropertyName("allergies")] public string? Allergies { get; set; }
		[JsonPropertyName("foodBeingBrought")] public string? FoodBeingBrought { get; set; }
	}
}
