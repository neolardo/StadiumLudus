/// <summary>
/// Represents a type of sound effect.
/// </summary>
public enum SFX
{
    // hit
    HitOnFlesh = 0,
    HitOnWood = 1,
    HitOnStone = 2,
    GuardHit = 3,

    // death
    MaleDeath = 5,
    FemaleDeath = 6,

    // step
    StepOnDirt = 10,
    StepOnStone = 11,
    StepOnSand = 12,

    // slash
    Slash = 20,

    // projectile
    ShortbowFire = 30,
    CrossbowFire = 31,
    CrossbowReload = 32,

    // fountain
    FountainUse = 40,
    FountainIdle = 41,
    
    // buff
    BuffUse = 42,
    BuffIdle = 43,

    // menu
    MenuButtonHover = 50,
    MenuProceed = 51,
    MenuBack = 52,
    MenuButtonClick = 53,
    MenuButtonClickAlt = 54,
    CannotPerformSkillOrAttack = 55,

    // skills
    Smoke = 60,
    GroundSlamStart = 61,
    GroundSlamEnd = 62,
    GroundSlamCracking = 63,
    TrapActivate = 64,
    TrapDeactivate = 65,
    Whirlwind = 66,
    Dash = 67,

    // win and lose
    Win = 70,
    Lose = 71,

}
