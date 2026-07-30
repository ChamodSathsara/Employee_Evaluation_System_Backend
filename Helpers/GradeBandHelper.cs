namespace EmployeeEvaluationSystem.Helpers;

// Maps a Final Score to a Grade using the default bands from the SRS
// (96-100=A+, 90-95=A, 80-89=B, 70-79=C, 50-69=D, below 50=E).
// TODO: replace hard-coded bands with a configurable lookup (see Configurations/).
public static class GradeBandHelper
{
    public static string ResolveGrade(decimal finalScore) => finalScore switch
    {
        >= 96 => "A+",
        >= 90 => "A",
        >= 80 => "B",
        >= 70 => "C",
        >= 50 => "D",
        _ => "E"
    };
}
