namespace DentalCare.Admin.Services;

public static class ToothSvgCatalog
{
    public static string FacialPath(int toothNumber) =>
        $"/odontogram/teeth/{TemplateName(toothNumber)}.svg";

    public static string? OcclusalPath(int toothNumber)
    {
        var position = toothNumber % 10;
        var deciduous = toothNumber / 10 is >= 5 and <= 8;

        if (!deciduous && position is 4 or 5)
            return "/odontogram/teeth/14_occl.svg";
        if (!deciduous && position is >= 6 and <= 8)
            return "/odontogram/teeth/16_occl.svg";
        if (deciduous && position == 4)
            return "/odontogram/teeth/14_occl.svg";
        if (deciduous && position == 5)
            return "/odontogram/teeth/16_occl.svg";
        return null;
    }

    public static bool FlipHorizontal(int toothNumber)
    {
        var quadrant = toothNumber / 10;
        return quadrant is 2 or 3 or 6 or 7;
    }

    public static string TemplateName(int toothNumber)
    {
        var position = toothNumber % 10;
        var quadrant = toothNumber / 10;
        if (quadrant is >= 5 and <= 8)
        {
            return position switch
            {
                1 or 2 => "11",
                3 => "13",
                4 => "14",
                _ => "16"
            };
        }

        return position switch
        {
            1 or 2 => "11",
            3 => "13",
            4 or 5 => "14",
            _ => "16"
        };
    }
}
