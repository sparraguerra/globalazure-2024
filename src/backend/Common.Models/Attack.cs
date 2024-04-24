namespace Common.Models;

public class Attack
{
    public Attack(int energy, Guid? numberOfAttack) => (Energy, NumberOfAttack) = (energy, numberOfAttack);

    public int Energy { get; set; }
    public Guid? NumberOfAttack { get; set; }
}
