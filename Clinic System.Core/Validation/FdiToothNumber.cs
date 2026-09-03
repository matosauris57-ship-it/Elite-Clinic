namespace Clinic_System.Core.Validation;

public enum FdiToothKind
{
    CentralIncisor = 1,
    LateralIncisor = 2,
    Canine = 3,
    FirstPremolar = 4,
    SecondPremolar = 5,
    FirstMolar = 6,
    SecondMolar = 7,
    ThirdMolar = 8
}

public static class FdiToothNumber
{
    public static bool IsValid(int toothNumber)
    {
        var quadrant = toothNumber / 10;
        var position = toothNumber % 10;

        return quadrant switch
        {
            >= 1 and <= 4 => position is >= 1 and <= 8,
            >= 5 and <= 8 => position is >= 1 and <= 5,
            _ => false
        };
    }

    public static bool IsPermanent(int toothNumber) =>
        IsValid(toothNumber) && toothNumber / 10 is >= 1 and <= 4;

    public static int Quadrant(int toothNumber) =>
        IsValid(toothNumber)
            ? toothNumber / 10
            : throw new ArgumentOutOfRangeException(nameof(toothNumber), "El número dental no es una notación FDI válida.");

    public static int Position(int toothNumber) =>
        IsValid(toothNumber)
            ? toothNumber % 10
            : throw new ArgumentOutOfRangeException(nameof(toothNumber), "El número dental no es una notación FDI válida.");

    public static bool IsUpper(int toothNumber)
    {
        var q = Quadrant(toothNumber);
        return q is 1 or 2 or 5 or 6;
    }

    public static bool IsRight(int toothNumber)
    {
        var q = Quadrant(toothNumber);
        return q is 1 or 4 or 5 or 8;
    }

    public static bool IsAnterior(int toothNumber) => Position(toothNumber) is >= 1 and <= 3;

    public static bool IsPosterior(int toothNumber) => !IsAnterior(toothNumber);

    public static bool IsPremolar(int toothNumber) =>
        IsPermanent(toothNumber) && Position(toothNumber) is 4 or 5;

    public static bool IsMolar(int toothNumber)
    {
        var position = Position(toothNumber);
        return IsPermanent(toothNumber) ? position is >= 6 and <= 8 : position is 4 or 5;
    }

    public static FdiToothKind Kind(int toothNumber)
    {
        var position = Position(toothNumber);
        if (!IsPermanent(toothNumber) && position is 4 or 5)
            return position == 4 ? FdiToothKind.FirstMolar : FdiToothKind.SecondMolar;

        return (FdiToothKind)position;
    }

    public static string Describe(int toothNumber)
    {
        if (!IsValid(toothNumber))
            return $"Pieza {toothNumber}";

        var arch = IsUpper(toothNumber) ? "superior" : "inferior";
        var side = IsRight(toothNumber) ? "derecho" : "izquierdo";
        var deciduous = IsPermanent(toothNumber) ? "" : " temporal";
        return $"{KindLabel(Kind(toothNumber))}{deciduous} {arch} {side}";
    }

    public static string KindLabel(FdiToothKind kind) => kind switch
    {
        FdiToothKind.CentralIncisor => "Incisivo central",
        FdiToothKind.LateralIncisor => "Incisivo lateral",
        FdiToothKind.Canine => "Canino",
        FdiToothKind.FirstPremolar => "Primer premolar",
        FdiToothKind.SecondPremolar => "Segundo premolar",
        FdiToothKind.FirstMolar => "Primer molar",
        FdiToothKind.SecondMolar => "Segundo molar",
        FdiToothKind.ThirdMolar => "Tercer molar",
        _ => kind.ToString()
    };

    /// <summary>Molars and upper first premolars typically present furcation anatomy.</summary>
    public static bool HasFurcation(int toothNumber)
    {
        if (!IsValid(toothNumber))
            return false;
        if (IsMolar(toothNumber))
            return true;
        return IsPermanent(toothNumber) && IsUpper(toothNumber) && Position(toothNumber) == 4;
    }
}
